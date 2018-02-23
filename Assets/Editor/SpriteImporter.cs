

using UnityEngine;
using UnityEditor;  // Most of the utilities we are going to use are contained in the UnityEditor namespace

// We inherit from the AssetPostProcessor class which contains all the exposed variables and event triggers for asset importing pipeline
internal sealed class CustomAssetImporter : AssetPostprocessor {
	// Couple of constants used below to be able to change from a single point, you may use direct literals instead of these consts to if you please
	private const int webTextureSize = 2048;
	private const int standaloneTextureSize = 4096;
	private const int iosTextureSize = 1024;
	private const int androidTextureSize = 1024;

	#region Methods

	//-------------Pre Processors

	// This event is raised when a texture asset is imported
	private void OnPreprocessTexture() {
		// Get the reference to the assetImporter (From the AssetPostProcessor class) and unbox it to a TextureImporter (Which is inherited and extends the AssetImporter with texture specific utilities)
		var importer = assetImporter as TextureImporter;
		var path = assetPath.Split('/');
		var fileName = path[path.Length-1];
		if (fileName.Substring(0, 1) == "L") return;

		// Set the texture import type drop-down to advanced so our changes reflect in the import settings inspector
		importer.textureType = TextureImporterType.Sprite;
		importer.filterMode = FilterMode.Point;
		importer.spritePixelsPerUnit = 16;
		importer.textureCompression = TextureImporterCompression.Uncompressed;
		importer.compressionQuality = 100;
	}

	// This event is raised when a new mesh asset is imported
	private void OnPreprocessModel() {
		// I prefix my mesh assets with "msh", this line says "if msh is not in the asset file name, do nothing"
		var fileNameIndex = assetPath.LastIndexOf('/');
		var fileName = assetPath.Substring(fileNameIndex + 1);

		if (!fileName.Contains("msh")) return;

		// Once again I unbox the assetImporter reference, to a ModelImporter this time
		var importer = assetImporter as ModelImporter;

		// I use the Stat prefix to determine if the gameobject produced by this model is going to be static or dynamic
		// So a static tree mesh file name would be "mshStatTree" for my asset importer
		if (assetPath.Contains("Stat")) {
			// If it is static we don't want any kind of animation imported
			importer.animationType = ModelImporterAnimationType.None;
			importer.importAnimation = false;
			// Generates lightmap uvs, comment out if you don't use lightmapping OR you provide your own lightmap/second uvs.
			importer.generateSecondaryUV = true;
		}

		// Sets the import scale to 1 from 0.01. Works well with Blender and other 1:1 scale ratio applications. Comment out if it doesn't work for you
		importer.globalScale = 1;
		// I like creating my own materials using my handy script at "http://www.sarpersoher.com/materials-from-textures-through-a-context-menu-in-unity/"
		// So I don't want Unity generating any materials everytime a model is imported
		importer.importMaterials = false;
		// This lets Unity get rid of any unused mesh data (bones, vertex colors etc.) on build
		importer.optimizeMesh = true;
	}

	// This event is raised every time an audio asset is imported
	// This method does nothing at the moment, just a skeleton to fill in if we ever need to do audio specific importing
	// Imports audio assets in the default way without changing anything
	private void OnPreprocessAudio() {
		var fileNameIndex = assetPath.LastIndexOf('/');
		var fileName = assetPath.Substring(fileNameIndex + 1);

		if (!fileName.Contains("snd")) return;

		var importer = assetImporter as AudioImporter;
		if (importer == null) return;
	}

	//-------------Post Processors

	// This event is called as soon as the texture asset is imported successfully
	// Does nothing currently, just here for future possibilities
	private void OnPostprocessTexture(Texture2D import) { }

	// This event is called as soon as the mesh asset is imported successfully
	private void OnPostprocessModel(GameObject import) {

		// Sometimes the artist who created the model forgets to "freeze" the position and rotation of the mesh
		// I find it dirty and telling an artists to fix and re-export the same mesh pisses them off especially if you are working on a game with a lot of assets
		// So OnPostprocessModel() to the rescue!
		// We simply zero out the position and rotation fields so when we put these models in our scene they don't come with their saved transform data
		import.transform.position = Vector3.zero;
		import.transform.rotation = Quaternion.identity;
	}

	// This event is called as soon as the audio asset is imported successfully
	private void OnPostprocessAudio(AudioClip import) { }

	#endregion
}