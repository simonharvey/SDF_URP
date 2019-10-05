using UnityEngine.Rendering.Universal;

namespace UnityEngine.Rendering.LWRP
{
	// The render pass contains logic to configure render target and perform drawing.
	// It contains a renderPassEvent that tells the pipeline where to inject the custom render pass. 
	// The execute method contains the rendering logic.
	public class FullScreenQuadPass : UnityEngine.Rendering.Universal.ScriptableRenderPass
	{
		string m_ProfilerTag = "DrawFullScreenPass";

		FullScreenQuad.FullScreenQuadSettings m_Settings;

		public FullScreenQuadPass(FullScreenQuad.FullScreenQuadSettings settings)
		{
			renderPassEvent = settings.renderPassEvent;
			m_Settings = settings;
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			Camera camera = renderingData.cameraData.camera;

			//context.

			var cmd = CommandBufferPool.Get(m_ProfilerTag);
			//cmd.Clear();
			cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
			//cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_Settings.material);
			cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_Settings.material, 0, 0);

			cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
			//context.Submit();
		}
	}
}