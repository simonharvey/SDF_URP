using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class SDFEditor : ScriptableWizard 
{
	private const string MenuPath = "Assets/Bake SDF";

	[Range(0, 64)]
	public int MaxIter = 64;
	private Texture2D _texture;

	[MenuItem(MenuPath)]
	private static void BakeSDF()
	{
		var wiz = DisplayWizard<SDFEditor>("Bake SDF", "Bake", "Cancel");
		wiz._texture = (Texture2D) Selection.activeObject;
	}

	[MenuItem(MenuPath, true)]
	private static bool BakeSDFValidation()
	{
		return Selection.activeObject.GetType() == typeof(Texture2D);
	}

	public static void SaveRTToFile(RenderTexture rt, string path)
	{
		//RenderTexture rt = Selection.activeObject as RenderTexture;

		RenderTexture.active = rt;
		Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
		tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
		RenderTexture.active = null;

		byte[] bytes;
		bytes = tex.EncodeToPNG();

		//string path = AssetDatabase.GetAssetPath(rt) + ".png";
		System.IO.File.WriteAllBytes(path, bytes);
		AssetDatabase.ImportAsset(path);

		var asset = (TextureImporter)AssetImporter.GetAtPath(path);
		asset.textureType = TextureImporterType.Default;
		asset.compressionQuality = 100;

		AssetDatabase.ImportAsset(path);
		Debug.Log("Saved to " + path);
	}

	private void OnWizardCreate()
	{
		SDF.MaxIter = MaxIter;
		var rt = SDF.Bake(_texture);

		var path = AssetDatabase.GetAssetPath(_texture);
		var sdfPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path)) + "-SDF.png";

		SaveRTToFile(rt, sdfPath);

		RenderTexture.ReleaseTemporary(rt);
	}

	private void OnWizardOtherButton()
	{

	}
}
