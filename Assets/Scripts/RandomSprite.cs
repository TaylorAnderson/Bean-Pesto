using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSprite : MonoBehaviour
{
    public Sprite[] sprites;
    public float[] weights;
    private new SpriteRenderer renderer;
    // Use this for initialization
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = this.sprites[GetRandomWeightedIndex(weights)];
    }

    // Update is called once per frame
    void Update()
    {

    }

    public int GetRandomWeightedIndex(float[] weights)
    {
        if (weights == null || weights.Length == 0) return -1;

        float w;
        float t = 0;
        int i;
        for (i = 0; i < weights.Length; i++)
        {
            w = weights[i];
            if (float.IsPositiveInfinity(w)) return i;
            else if (w >= 0f && !float.IsNaN(w)) t += weights[i];
        }

        float r = Random.value;
        float s = 0f;

        for (i = 0; i < weights.Length; i++)
        {
            w = weights[i];
            if (float.IsNaN(w) || w <= 0f) continue;

            s += w / t;
            if (s >= r) return i;
        }

        return -1;
    }

}
