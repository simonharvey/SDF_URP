using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DebugView : MonoBehaviour
{
	public Texture2D tex;
	[Range(1, 32)]
	public int maxIter;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		/*Debug.Log("here");
		Graphics.Blit(source, destination);
		Graphics.Blit(tex, destination);
		var i = SDF.MaxIter;
		SDF.MaxIter = maxIter;
		var sdf = SDF.Bake(tex);
		SDF.MaxIter = i;
		Graphics.Blit(sdf, destination);
		RenderTexture.ReleaseTemporary(sdf);*/
	}
}
