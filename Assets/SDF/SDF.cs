using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

public static class SDF
{
	public static int MaxIter = 1024;

	public static RenderTexture Bake(Texture tex)
	{
		//var rt = RenderTexture.GetTemporary(Mathf.NextPowerOfTwo(tex.width + size), Mathf.NextPowerOfTwo(tex.height + size), 0);
		var rt = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32);
		rt.wrapMode = TextureWrapMode.Clamp;
		rt.filterMode = FilterMode.Point;
		rt.autoGenerateMips = false;
		BakeRT(tex, rt);
		return rt;
	}

	static Material s_Mat = null;

	private static Material SDFMaterial {
		get
		{
			if (s_Mat == null)
			{
				s_Mat = new Material(Shader.Find("SDF"));
			}

			return s_Mat;
		}
	}

	public static CommandBuffer BakeCommandBuffer(Texture tex, RenderTexture output, int size)
	{
		CommandBuffer buf = new CommandBuffer();
		/*buf.name = "SDF (" + output + ")";
		buf.BeginSample(buf.name);
		var mat = SDFMaterial;

		var scale = new Vector2(tex.width/(float)(tex.width+size), tex.height/(float)(tex.height+size));
		buf.SetGlobalMatrix("_Mat", Matrix4x4.Scale(scale));
		var src = 123;
		var dst = 234;
		buf.GetTemporaryRT(src, output.descriptor);
		buf.GetTemporaryRT(dst, output.descriptor);
		buf.SetRenderTarget(src);
		buf.ClearRenderTarget(true, true, Color.clear);
		buf.Blit(tex, src, mat, 0);

		var off = (uint)Mathf.NextPowerOfTwo(Mathf.Max(output.width, output.height));
		while (off > 0)
		{
			off >>= 1;
			var texOff = new Vector2((float)off / output.width, off / (float)output.height);
			buf.SetGlobalVector("_Offset", texOff);
			buf.Blit(src, dst, mat, 2);
			Swap(ref src, ref dst);
		}

		buf.Blit(dst, output);
		buf.ReleaseTemporaryRT(src);
		buf.ReleaseTemporaryRT(dst);
		buf.EndSample(buf.name);*/
		return buf;
	}

	public static void BakeRT(Texture tex, RenderTexture output)
	{
		var seed = RenderTexture.GetTemporary(output.width, output.height, 0, RenderTextureFormat.ARGB32);
		var src = RenderTexture.GetTemporary(output.width, output.height, 0, RenderTextureFormat.ARGB32);
		seed.filterMode = src.filterMode = FilterMode.Point;
		seed.wrapMode = src.wrapMode = TextureWrapMode.Clamp;
		/*RenderTexture.active = seed;
		GL.Clear(true, true, Color.clear);
		RenderTexture.active = null;*/

		//if (s_Mat == null)
		{
			s_Mat = new Material(Shader.Find("SDF"));
			Debug.Log(s_Mat);
		}

		// seeding
		//var scale = new Vector2((float)tex.width / output.width, (float)tex.height / output.height);
		//var mat = Matrix4x4.Scale(scale);
		var material = s_Mat;
		//material.SetMatrix("_Mat", mat);
		//material.SetVector("_Dst_TexelSize", new Vector2(1.0f / output.width, 1.0f / tex.width));

		/*Graphics.Blit(tex, output, material, 0);
		return;*/

		Graphics.Blit(tex, seed, material, 0);
		Graphics.Blit(seed, src);//, material, 1);
		RenderTexture.ReleaseTemporary(seed);

		var off = (uint)Mathf.NextPowerOfTwo(Mathf.Max(output.width, output.height));
		//off >>= 2;

		int iter = 0;
		while (off > 0)
		{
			if (iter++ > MaxIter) break;
			var dst = RenderTexture.GetTemporary(output.width, output.height, 0, RenderTextureFormat.ARGBFloat);
			dst.filterMode = FilterMode.Point;
			dst.wrapMode = TextureWrapMode.Clamp;

			//var texOff = new Vector2((float)off / output.width, off / (float)output.height);
			float size = output.width;
			var texOff = new Vector2(off / size, off / size);

			material.SetVector("_Offset", texOff);
			Graphics.Blit(src, dst, material, 1);
			Swap(ref src, ref dst);
			RenderTexture.ReleaseTemporary(dst);
			off >>= 1;
		}

		//Graphics.Blit(b, dst);
		//material.SetFloat("_SDFSize", size);
		Graphics.Blit(src, output);

		RenderTexture.ReleaseTemporary(src);
	}

	public static void Swap<T>(ref T a, ref T b)
	{
		T tmp = b;
		b = a;
		a = tmp;
	}
}
