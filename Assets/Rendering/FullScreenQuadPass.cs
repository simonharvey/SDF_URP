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

        public override void Execute(ScriptableRenderContext context, ref UnityEngine.Rendering.Universal.RenderingData renderingData)
        {
            Camera camera = renderingData.cameraData.camera;

            var cmd = CommandBufferPool.Get(m_ProfilerTag);
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.DrawMesh(UnityEngine.Rendering.Universal.RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_Settings.material);
            cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
			context.Submit();
        }
    }
}