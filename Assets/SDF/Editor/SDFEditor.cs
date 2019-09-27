using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class SDFEditor : ScriptableWizard 
{
	private const string MenuPath = "Assets/Bake SDF";

	public int Size = 0;
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

	private void OnWizardCreate()
	{
		var rt = SDF.Bake(_texture, Size);//RenderTexture.GetTemporary(_texture.width, _texture.height);
		//Graphics.Blit(_texture, rt);
		var tex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
		RenderTexture.active = rt;
		tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
		RenderTexture.ReleaseTemporary(rt);

		var bytes = tex.EncodeToPNG();
		var path = AssetDatabase.GetAssetPath(_texture);
		var sdfPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path)) + "-SDF.png";
		File.WriteAllBytes(sdfPath, bytes);
		AssetDatabase.Refresh();
		var asset = ((TextureImporter)AssetImporter.GetAtPath(sdfPath));
		asset.textureType = TextureImporterType.Default;
		asset.compressionQuality = 100;
	}

	private void OnWizardOtherButton()
	{

	}
}
