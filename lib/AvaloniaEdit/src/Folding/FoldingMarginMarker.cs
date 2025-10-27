// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using Avalonia;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Utils;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace AvaloniaEdit.Folding
{
    internal sealed class FoldingMarginMarker : Control
    {
        internal VisualLine VisualLine;
        internal FoldingSection FoldingSection;

        private bool _isExpanded;

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    InvalidateVisual();
                }
                if (FoldingSection != null)
                    FoldingSection.IsFolded = !value;
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            if (!e.Handled)
            {
                IsExpanded = !IsExpanded;
                e.Handled = true;
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            Cursor = Cursor.Default;
        }

        private const double MarginSizeFactor = 0.7;

        protected override Size MeasureCore(Size availableSize)
        {
            double size = MarginSizeFactor * FoldingMargin.SizeFactor * GetValue(TextBlock.FontSizeProperty);
            size = PixelSnapHelpers.RoundToOdd(size, PixelSnapHelpers.GetPixelSize(this).Width);
            return new Size(size, size);
        }

        public override void Render(DrawingContext drawingContext)
        {
            FoldingMargin margin = (FoldingMargin)Parent;
            Pen activePen = new Pen(margin.SelectedFoldingMarkerBrush,
                lineCap: PenLineCap.Square);
            Pen inactivePen = new Pen(margin.FoldingMarkerBrush,
                lineCap: PenLineCap.Square);
            Size pixelSize = PixelSnapHelpers.GetPixelSize(this);
            Rect rect = new Rect(pixelSize.Width / 2,
                                 pixelSize.Height / 2,
                                 Bounds.Width - pixelSize.Width,
                                 Bounds.Height - pixelSize.Height);

            drawingContext.FillRectangle(
                IsPointerOver ? margin.SelectedFoldingMarkerBackgroundBrush : margin.FoldingMarkerBackgroundBrush,
                rect);

            drawingContext.DrawRectangle(
                IsPointerOver ? activePen : inactivePen,
                rect);

            double middleX = rect.X + rect.Width / 2;
            double middleY = rect.Y + rect.Height / 2;
            double space = PixelSnapHelpers.Round(rect.Width / 8, pixelSize.Width) + pixelSize.Width;
            drawingContext.DrawLine(activePen,
                                    new Point(rect.X + space, middleY),
                                    new Point(rect.Right - space, middleY));
            if (!_isExpanded)
            {
                drawingContext.DrawLine(activePen,
                                        new Point(middleX, rect.Y + space),
                                        new Point(middleX, rect.Bottom - space));
            }
        }


        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == IsPointerOverProperty)
            {
                InvalidateVisual();
            }
        }
    }
}
