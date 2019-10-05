using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.LWRP;
using UnityEngine.Rendering.Universal;

//https://pastebin.com/u69vkZjU
//https://pastebin.com/iZZUZLMa
public class OutlineRenderPass : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
		OutlineRenderPass _host;
		string m_ProfilerTag = "OutlineRenderPass CustomRenderPass";
		private FilteringSettings m_FilteringSettings;
		//ShaderTagId m_ShaderTagId = new ShaderTagId("DepthOnly");
		ShaderTagId m_ShaderTagId = new ShaderTagId("UniversalForward");
		//ShaderTagId m_ShaderTagId = new ShaderTagId("SRPDefaultUnlit");

		public CustomRenderPass(OutlineRenderPass host)
		{
			_host = host;
		}

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in an performance manner.
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
			//Debug.Log($"Configure {this}");
			m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque, _host.LayerMask);
			cmd.GetTemporaryRT(123, cameraTextureDescriptor.width, cameraTextureDescriptor.height, 0);
			ConfigureClear(ClearFlag.Color, Color.clear);
			ConfigureTarget(123);
		}

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
			CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

			using (new ProfilingSample(cmd, m_ProfilerTag))
			{
				//context.ExecuteCommandBuffer(cmd);
				//cmd.Clear();
				var cam = renderingData.cameraData;
				var w = cam.camera.pixelWidth;
				var h = cam.camera.pixelHeight;
				var rtd = renderingData.cameraData.cameraTargetDescriptor;
				rtd.depthBufferBits = 0;
				//cmd.GetTemporaryRT(123, rtd);

				var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
				var drawSettings = CreateDrawingSettings(m_ShaderTagId, ref renderingData, sortFlags);
				drawSettings.perObjectData = PerObjectData.None;

				ref CameraData cameraData = ref renderingData.cameraData;
				Camera camera = cameraData.camera;
				if (cameraData.isStereoEnabled)
					context.StartMultiEye(camera);

				var mat = CoreUtils.CreateEngineMaterial("SDF");
				drawSettings.overrideMaterialPassIndex = mat.FindPass("CamSeed");
				drawSettings.overrideMaterial = mat;

				context.DrawRenderers(renderingData.cullResults, ref drawSettings,
					ref m_FilteringSettings);

				cmd.SetGlobalTexture("_MainTex", 123);
				//cmd.SetGlobalTexture("_CameraDepthNormalsTexture", depthAttachmentHandle.id);
			}

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);

			//cmd.Blit(123, null);

			/*//https://github.com/Unity-Technologies/ScriptableRenderPipeline/wiki/SRP-Drawing
			context.SetupCameraProperties(renderingData.cameraData.camera);
			var rds = CreateDrawingSettings(new ShaderTagId("Seed"), ref renderingData, SortingCriteria.None);

			renderingData.cameraData.camera.TryGetCullingParameters(out var cullingParams);
			//cullingParams.cullingMask = (uint)(int)_host.LayerMask;
			var targets = context.Cull(ref cullingParams);
			var filter = FilteringSettings.defaultValue;
			context.DrawRenderers(targets, ref rds, ref filter);
			context.Submit();*/
		}

        /// Cleanup any allocated resources that were created during the execution of this render pass.
        public override void FrameCleanup(CommandBuffer cmd)
        {
			//cmd.Blit(123, 0);
			cmd.ReleaseTemporaryRT(123);
			//Debug.Log($"Cleanup {this}");
        }
    }

	public LayerMask LayerMask;
	public RenderTexture RT;
	public Material OutlineMat;

    CustomRenderPass _drawTargetsPass;
	FullScreenQuadPass _fsqPass;

	public override void Create()
    {
		_drawTargetsPass = new CustomRenderPass(this)
		{
			renderPassEvent = RenderPassEvent.AfterRenderingOpaques
		};

		_fsqPass = new FullScreenQuadPass(new FullScreenQuad.FullScreenQuadSettings
		{
			material = OutlineMat,
			renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
		});
	}

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_drawTargetsPass);
		renderer.EnqueuePass(_fsqPass);
    }
}


