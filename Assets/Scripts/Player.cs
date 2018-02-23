using Com.LuisPedroFonseca.ProCamera2D;
using InControl;
using MonsterLove.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uween;
using UnityEngine.SceneManagement;
public enum PlayerState {
    Ground,
    Air,
    Squished,

    Dive,
    Hacking,
    Dead
}
public enum VisibilityLevel {
    Invisible,
    Low, 
    High
}

public class Player : PhysicsObject {
 

    //PLEASE REMEMBER NOT TO TOUCH THESE IN HERE
    public float fallGravityModifier = 0f;
    public float speed = 5f;
    public float runSpeed = 7.5f;
    public float squishSpeed = 2.5f;
    public float jumpSpeed = 5f;
    //testing morte shit
    
    public float squishPopHeight = 1.5f;
    public float pushSpeed = 2.5f;
    public float accel = 0.5f;
    
    public float diveSpeed = 10f;
    public float respawnTime = 1f;
    public bool visible = true;

    public float defaultFriction = 0.92f;
    public float squishFriction = 0.9f;
    public float squishBoost = 12f;
    
    //END FORBIDDEN ZONE

    private float originalJumpSpeed;
    private float friction = 0.95f;
    [HideInInspector]
    public Wearable wearing;
    public StateMachine<PlayerState> fsm;
    public Dictionary<string, float> animLengths = new Dictionary<string, float>();

    private GameObject sprite;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private CharacterActions input;
    private ContactFilter2D defaultContactFilter;

    private GameObject currentlyColliding;

    private bool pushing = false;

    private float sprintSoundTimer = 0;
    private float respawnTimer;
    private Vector2 respawnPos;

    private float hInput = 0f;
    private float prevHInput = 0f;

    private float jumps = 0;
    private float jumpLimit = 1;

    private float currentSpeed;

    private bool squished = false;

    private Enemy enemyColliding = null;

    private Computer currentComputer = null;
    private const string GRATE_TAG = "Grate";

    private float slimeGenCounter = 0;

    private bool isDead = false;


    const string RunAnim = "PlayerRun";
    const string JumpAnim = "PlayerJump";
    const string IdleAnim = "PlayerIdle";
    const string WalkAnim = "PlayerWalk";
    const string FallAnim = "PlayerFall";
    const string TurnAnim = "PlayerTurn";
    const string SquishAnim = "PlayerSquish";
    const string SquishWalkAnim = "PlayerSquishWalk";
    const string SquishIdleAnim = "PlayerSquishIdle";
    const string PushAnim = "PlayerPush";
    const string DiveAnim = "PlayerDive";
    const string DeadAnim = "PlayerDead";

    public List<string> acceptedCollisionTags = new List<string>{"Enemy", "Crate", "Button"};
    
    void Awake() {

        this.originalJumpSpeed = this.jumpSpeed;
        respawnPos = this.transform.position;
        fsm = StateMachine<PlayerState>.Initialize(this, PlayerState.Air);
        input = new CharacterActions();
        input.Left.AddDefaultBinding(Key.A);
        input.Left.AddDefaultBinding(Key.LeftArrow);
        input.Left.AddDefaultBinding(InputControlType.LeftStickLeft);

        input.Right.AddDefaultBinding(Key.D);
        input.Right.AddDefaultBinding(Key.RightArrow);
        input.Right.AddDefaultBinding(InputControlType.LeftStickRight);

        input.Down.AddDefaultBinding(Key.DownArrow);

        input.Down.AddDefaultBinding(Key.S);
        input.Down.AddDefaultBinding(InputControlType.LeftStickDown);

        input.Jump.AddDefaultBinding(Key.UpArrow);
        input.Jump.AddDefaultBinding(Key.W);
        input.Jump.AddDefaultBinding(Key.Space);

        input.Jump.AddDefaultBinding(InputControlType.Action1);

        input.Run.AddDefaultBinding(Key.Shift);
        input.Run.AddDefaultBinding(Key.Z);
        input.Run.AddDefaultBinding(InputControlType.Action2);

        sprite = transform.GetChild(0).gameObject;
        animator = GetComponent<Animator>();
        spriteRenderer = sprite.GetComponent<SpriteRenderer>();
        currentSpeed = speed;

        //animator.StartPlayback();

        foreach (AnimationClip ac in animator.runtimeAnimatorController.animationClips) {
            animLengths[ac.name] = ac.length;
        }
        

    }

    void Ground_Enter() {
        Squash();
        
    }

    void Ground_Update() {

        var currentAnim = "";

        if (hInput != 0) {
            currentAnim = WalkAnim;
        } 
        else {
            currentAnim = IdleAnim;
        }

        if (Mathf.Sign(hInput) != Mathf.Sign(velocity.x) && hInput != 0) {
            currentAnim = TurnAnim;
        }
        animator.Play(currentAnim);

        //END ANIMATION STUFF

        this.currentSpeed = this.speed;
        if (input.Jump.WasPressed) {
            Stretch();
            velocity.y = jumpSpeed;
        }

        this.currentGravityModifier = this.gravityModifier;

        var origin = transform.position + transform.up * 0.5f;
        var direction = Vector2.right * (spriteRenderer.flipX ? -1 : 1);
        var length = collider.bounds.size.x / 2 + 0.01f;
        pushing = false;
        RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
        int count = rb2d.Cast(direction, contactFilter, hitBuffer, 0.03f);
        for (int i = 0; i < count; i++) {
            var collider = hitBuffer[i].collider;
            pushing = true;
            if (collider.gameObject.CompareTag("Crate")) {
                var physicsObject = collider.GetComponent<PhysicsObject>();
                //no vertical movement plx
                physicsObject.velocity += (Vector2.right * this.accel * this.hInput) / 10;
            }
        }
        if (pushing) this.currentSpeed = this.pushSpeed;

        //STATE CHANGES
        if (!grounded) {
            fsm.ChangeState(PlayerState.Air);

        }
        if (input.Down.IsPressed) fsm.ChangeState(PlayerState.Squished);

        HandleLeftRightMovement();
    }

    void Ground_Exit() {
    }

    void Air_Update() {
        if (this.velocity.y > 0) animator.Play(JumpAnim);
        else animator.Play(FallAnim);

        if (input.Jump.WasReleased) {
            velocity.y *= 0.5f;
        }

        if (velocity.y > 0) this.currentGravityModifier = this.gravityModifier;
        else this.currentGravityModifier = this.fallGravityModifier;

        if (grounded) fsm.ChangeState(PlayerState.Ground);
        if (input.Down.WasPressed) {
            fsm.ChangeState(PlayerState.Dive);
        }

        HandleLeftRightMovement();
    }

    IEnumerator Dive_Enter() {
        animator.Play(FallAnim);
        velocity = Vector2.zero;
        currentGravityModifier = 0;
        Squash(0.1f);
        yield return new WaitForSeconds(0.1f);
        animator.Play(DiveAnim);
        currentGravityModifier = 1;
        velocity.y -= diveSpeed;
    }

    void Dive_Update() {
        if (grounded) {
            fsm.ChangeState(PlayerState.Squished);
        }
        if (enemyColliding != null) {
            fsm.ChangeState(PlayerState.Air);
        }


        if (currentlyColliding) {
            var comp = currentlyColliding.GetComponent<Computer>();
            if (comp != null) {
                currentComputer = comp;
                comp.StartHacking();
                fsm.ChangeState(PlayerState.Hacking);
            }   
            var button = currentlyColliding.GetComponent<Button>();
            if (button != null && !button.activated) {
                button.StartCoroutine(button.Activate());
                TweenSX.Add(sprite.gameObject, 0.3f, 1.5f).EaseOutBack().Then(() => { TweenSX.Add(sprite.gameObject, 0.2f, 1).EaseOutBack().Delay(0.2f); });
                TweenSY.Add(sprite.gameObject, 0.3f, 0.3f).EaseOutBack().Then(() => { TweenSY.Add(sprite.gameObject, 0.2f, 1).EaseOutBack().Delay(0.2f); });
                TweenNull.Add(gameObject, 0.4f).Then(() => { 
                    this.velocity.y += this.jumpSpeed * 0.75f; 
                    fsm.ChangeState(PlayerState.Air); 
                });
            }
        }   
    }

    void Hacking_Enter() {
        sprite.SetActive(false);
        this.transform.position = currentComputer.transform.position;
    }

    void Hacking_Update() {
        if (currentComputer.finishedHacking) {

            currentComputer = null;
            fsm.ChangeState(PlayerState.Air);
            velocity.y += jumpSpeed;
        }
    }

    void Hacking_Exit() {
        sprite.SetActive(true);
    }

    void Squished_Enter() {
        if (Mathf.Abs(hInput) > 0.01) this.velocity.x += squishBoost * Mathf.Sign(hInput);
        visible = false;
        if (this.wearing) {
            this.wearing.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            this.wearing.transform.parent = null;
            this.wearing = null;
        }
    }

    void Squished_Update() {
        if (!input.Down.IsPressed || input.Jump.WasPressed) {
            fsm.ChangeState(PlayerState.Air);
            if (this.grounded) velocity.y = squishPopHeight;
        }

        if (hInput != 0) {
            animator.Play(SquishWalkAnim);
        } else {
            animator.Play(SquishIdleAnim);
        }

        this.currentSpeed = this.squishSpeed;

        /*if (this.grounded) {
            this.slimeGenCounter += Time.deltaTime;
            if (this.slimeGenCounter > this.slimeGenInterval) {
                slimeGenCounter -= slimeGenInterval;
                var slime = Instantiate(this.slimePrefab);
                slime.transform.position = this.transform.position + Vector3.right * Random.Range(-0.3f, 0.3f) + Vector3.down * Random.Range(0.1f, 0.22f);
            }
        }*/

        //HandleLeftRightMovement();
    }

    void Squished_Exit() {
        
        if (enemyColliding && !isDead) {
            enemyColliding.Die();
        }
        
        if (currentlyColliding) {
            var wearable = currentlyColliding.GetComponent<Wearable>();
            if (wearable) {
                var rb2d = wearable.GetComponent<Rigidbody2D>();
                rb2d.bodyType = RigidbodyType2D.Static;
                wearable.transform.parent = this.sprite.transform;
                var wearableCollider = wearable.GetComponent<BoxCollider2D>();
                float offset = 0.2f;
                if (wearable.type == Wearable.WearableType.HELMET) offset = 0.3f;
                wearable.transform.position = (Vector2) transform.position - wearableCollider.offset + this.collider.offset + Vector2.up * offset;
                this.wearing = wearable;
            }
        }
    }

    void Dead_Enter() {
        isDead = true;
        this.velocity = Vector2.up * 10 + Vector2.right * Random.Range(-3f, 3f);
        
        this.collider.enabled = false;
        this.currentGravityModifier = this.gravityModifier * 2;

        ProCamera2D camera = Camera.main.GetComponent<ProCamera2D>();
        camera.FollowHorizontal = false;
        camera.FollowVertical = false;
        animator.Play(DeadAnim);
    }
    
    void Dead_Update() {
        this.transform.localEulerAngles += Vector3.forward * 3f;
        this.respawnTimer += Time.deltaTime;
        if (this.respawnTimer > this.respawnTime) {
             SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    protected override void ComputeVelocity() {

        if (this.fsm.State == PlayerState.Squished) this.friction = squishFriction;
        else this.friction = defaultFriction;
        if (hInput == 0 || fsm.State == PlayerState.Squished) this.velocity.x *= friction;
        if (this.fsm.State == PlayerState.Dead) return;
        if (enemyColliding && enemyColliding.fsm.State == EnemyState.Alert && fsm.State == PlayerState.Ground) {
            enemyColliding.fsm.ChangeState(EnemyState.Normal);
            isDead = true;
            fsm.ChangeState(PlayerState.Dead, StateTransition.ForceOverwrite);
        }

        //WEARING
        jumpSpeed = originalJumpSpeed;
        if (wearing) {
            if (wearing.type == Wearable.WearableType.BOX) {
                  this.jumpSpeed = 1.2f;
            }
        }
    }

    private void LateUpdate() {
    }
    protected override bool IsCollisionFit(Collider2D collider) {
        if (collider.tag == GRATE_TAG) {
            return (this.transform.position.y - 0.5f > collider.transform.position.y) && this.fsm.State != PlayerState.Squished && this.fsm.State != PlayerState.Dive;
        } else return true;
    }

    public void OnCollisionStay2D(Collision2D collision) {
        
        if (acceptedCollisionTags.IndexOf(collision.gameObject.tag) != -1) {
            this.currentlyColliding = collision.gameObject;
            var enemyComponent = this.currentlyColliding.GetComponent<Enemy>();
            if (enemyComponent != null) this.enemyColliding = enemyComponent;
        }

    }   

    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject == this.currentlyColliding) {
             if (this.currentlyColliding.GetComponent<Enemy>() != null) this.enemyColliding = null;
            this.currentlyColliding = null;
           
        }
    }

    public void Squash(float time = 0.1f, float returnDelay = 0f) {
        TweenSX.Add(sprite.gameObject, time, 1.4f).EaseOutBack().Then(() => { TweenSX.Add(sprite.gameObject, time, 1).EaseOutBack().Delay(returnDelay); });
        TweenSY.Add(sprite.gameObject, time, 0.5f).EaseOutBack().Then(() => { TweenSY.Add(sprite.gameObject, time, 1).EaseOutBack().Delay(returnDelay); });
    }

    public void Stretch() {
        TweenSX.Add(sprite.gameObject, 0.1f, 0.7f).EaseOutBack().Then(() => { TweenSX.Add(sprite.gameObject, 0.1f, 1).EaseOutBack(); });
        TweenSY.Add(sprite.gameObject, 0.1f, 1.2f).EaseOutBack().Then(() => { TweenSY.Add(sprite.gameObject, 0.1f, 1).EaseOutBack(); });
    }

    public void HandleLeftRightMovement() {
        hInput = input.Move.Value;

        spriteRenderer.flipX = hInput != 0 ? hInput < 0 : spriteRenderer.flipX;
        //this sorta weird setup  means that the player can still MOVE really fast (if propelled by external forces)
        //its just that he cant go super fast just by player input alone

        //force should be clamped so it doesnt let velocity extend past currentSpeed
        var force = accel * hInput;
        force = Mathf.Clamp(force + velocity.x, -currentSpeed, currentSpeed) - velocity.x;

        //basically making sure the adjusted force of the input doesn't act against the input itself
        if (Mathf.Sign(force) == Mathf.Sign(hInput)) velocity.x += force;


        if (Mathf.Abs(this.velocity.x) > currentSpeed) velocity.x *= 0.96f;
    }

}