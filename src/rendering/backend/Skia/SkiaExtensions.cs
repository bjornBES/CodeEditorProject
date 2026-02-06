namespace TextEditorProject.Rendering.backend.Skia
{
    public static class SkiaExtensions
    {
        public static SkiaSharp.SKRect ToSkia(this shared.Rect rect)
        {
            SkiaSharp.SKRect sKRect = new SkiaSharp.SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
            sKRect.Location = new SkiaSharp.SKPoint(rect.X, rect.Y);
            return sKRect;
        }
    }
}