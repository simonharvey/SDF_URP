using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class Outline : MonoBehaviour
{
	public LayerMask LayerMask;

	[SerializeField] private RenderTexture _output = default;
	[SerializeField] private Shader _solidShader = default;

	public float Distance = 10.0f;

	private Camera _outlineCam, _cam;

	private void Awake()
	{
		_cam = GetComponent<Camera>();

		var go = new GameObject("OutlineCam");
		go.transform.SetParent(transform);
		_outlineCam = go.AddComponent<Camera>();
		//_outlineCam.depth = _cam.depth
		_outlineCam.enabled = false;
		//_outlineCam.AddCommandBuffer(CameraEvent.AfterEverything, SDF.BakeCommandBuffer(_output, _output, 0));
		

		var cmd = new CommandBuffer
		{
			name = "Outline"
		};

		cmd.Blit(_output, _cam.targetTexture);

		_cam.AddCommandBuffer(CameraEvent.AfterEverything, cmd);
	}

	private void OnPreCull()
	{
		//Debug.Log("OnPreCull");
		_outlineCam.CopyFrom(_cam);
		_outlineCam.clearFlags = CameraClearFlags.SolidColor;
		_outlineCam.backgroundColor = new Color32(0, 0, 0, 0);
		_outlineCam.cullingMask = LayerMask;

		//_outlineCam.targetTexture = RenderTexture.GetTemporary(_cam.pixelWidth, _cam.pixelHeight);

		_outlineCam.targetTexture = _output;
	}

	private void OnPreRender()
	{
		//Debug.Log("OnPreRender");
		_outlineCam.RenderWithShader(_solidShader, null);
	}

	private void OnPostRender()
	{
		//RenderTexture.ReleaseTemporary(_outlineCam.targetTexture);
	}
}
