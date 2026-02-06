using System.Diagnostics;
using TextEditorProject.Core;
using TextEditorProject.Core.Engine;
using TextEditorProject.Core.Snapshots;
using TextEditorProject.Rendering.backend.Avalonia;
using TextEditorProject.Rendering.Backend.Skia;

namespace TextEditorProject.Host
{
    public class Program
    {
        private AppHost AppHost;
        public static void Main(string[] args)
        {
            new Program().Run(args);
        }

        public void Run(string[] args)
        {
            IHostServices hostServices = null;

            var engine = new EditorEngine(hostServices);          // Core
            var skiaRenderer = new SkiaRenderBackend(); // Rendering adapter

            AppHost = new AppHost(engine, skiaRenderer);
            AppHost.Run();
        }
    }
}