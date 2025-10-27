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

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Utils;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using System.Collections.ObjectModel;

namespace AvaloniaEdit.Folding
{
    /// <summary>
    /// A margin that shows markers for foldings and allows to expand/collapse the foldings.
    /// </summary>
    public class FoldingMargin : AbstractMargin
    {
        /// <summary>
        /// Gets/Sets the folding manager from which the foldings should be shown.
        /// </summary>
        public FoldingManager FoldingManager { get; set; }

        internal const double SizeFactor = Constants.PixelPerPoint;

        static FoldingMargin()
        {
            FoldingMarkerBrushProperty.Changed.bes_Subscribe(OnUpdateBrushes);
            FoldingMarkerBackgroundBrushProperty.Changed.bes_Subscribe(OnUpdateBrushes);
            SelectedFoldingMarkerBrushProperty.Changed.bes_Subscribe(OnUpdateBrushes);
            SelectedFoldingMarkerBackgroundBrushProperty.Changed.bes_Subscribe(OnUpdateBrushes);
        }

        #region Brushes
        /// <summary>
        /// FoldingMarkerBrush dependency property.
        /// </summary>
        public static readonly AttachedProperty<IBrush> FoldingMarkerBrushProperty =
            AvaloniaProperty.RegisterAttached<FoldingMargin, Control, IBrush>("FoldingMarkerBrush",
                                                Brushes.Gray, inherits: true);

        /// <summary>
        /// Gets/sets the Brush used for displaying the lines of folding markers.
        /// </summary>
        public IBrush FoldingMarkerBrush
        {
            get => GetValue(FoldingMarkerBrushProperty);
            set => SetValue(FoldingMarkerBrushProperty, value);
        }

        /// <summary>
        /// FoldingMarkerBackgroundBrush dependency property.
        /// </summary>
        public static readonly AttachedProperty<IBrush> FoldingMarkerBackgroundBrushProperty =
            AvaloniaProperty.RegisterAttached<FoldingMargin, Control, IBrush>("FoldingMarkerBackgroundBrush",
                                                Brushes.White, inherits: true);

        /// <summary>
        /// Gets/sets the Brush used for displaying the background of folding markers.
        /// </summary>
        public IBrush FoldingMarkerBackgroundBrush
        {
            get => GetValue(FoldingMarkerBackgroundBrushProperty);
            set => SetValue(FoldingMarkerBackgroundBrushProperty, value);
        }

        /// <summary>
        /// SelectedFoldingMarkerBrush dependency property.
        /// </summary>
        public static readonly AttachedProperty<IBrush> SelectedFoldingMarkerBrushProperty =
            AvaloniaProperty.RegisterAttached<FoldingMargin, Control, IBrush>("SelectedFoldingMarkerBrush",
                                                Brushes.Black, inherits: true);

        /// <summary>
        /// Gets/sets the Brush used for displaying the lines of selected folding markers.
        /// </summary>
        public IBrush SelectedFoldingMarkerBrush
        {
            get => GetValue(SelectedFoldingMarkerBrushProperty);
            set => SetValue(SelectedFoldingMarkerBrushProperty, value);
        }

        /// <summary>
        /// SelectedFoldingMarkerBackgroundBrush dependency property.
        /// </summary>
        public static readonly AttachedProperty<IBrush> SelectedFoldingMarkerBackgroundBrushProperty =
            AvaloniaProperty.RegisterAttached<FoldingMargin, Control, IBrush>("SelectedFoldingMarkerBackgroundBrush",
                                                Brushes.White, inherits: true);

        /// <summary>
        /// Gets/sets the Brush used for displaying the background of selected folding markers.
        /// </summary>
        public IBrush SelectedFoldingMarkerBackgroundBrush
        {
            get => GetValue(SelectedFoldingMarkerBackgroundBrushProperty);
            set => SetValue(SelectedFoldingMarkerBackgroundBrushProperty, value);
        }

        private static void OnUpdateBrushes(AvaloniaPropertyChangedEventArgs e)
        {
            FoldingMargin m = null;
            if (e.Sender is FoldingMargin margin)
                m = margin;
            else if (e.Sender is TextEditor editor)
                m = editor.TextArea.LeftMargins.FirstOrDefault(c => c is FoldingMargin) as FoldingMargin;
            if (m == null) return;
            if (e.Property.Name == FoldingMarkerBrushProperty.Name)
            {
                m._foldingControlPen = new Pen((IBrush)e.NewValue);
            }
            if (e.Property.Name == SelectedFoldingMarkerBrushProperty.Name)
            {
                m._selectedFoldingControlPen = new Pen((IBrush)e.NewValue);
            }
        }
        #endregion

        /// <inheritdoc/>
        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (FoldingMarginMarker m in _markers)
            {
                m.Measure(availableSize);
            }
            double width = SizeFactor * GetValue(TextBlock.FontSizeProperty);
            return new Size(PixelSnapHelpers.RoundToOdd(width, PixelSnapHelpers.GetPixelSize(this).Width), 0);
        }

        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size finalSize)
        {
            Size pixelSize = PixelSnapHelpers.GetPixelSize(this);
            foreach (FoldingMarginMarker m in _markers)
            {
                int visualColumn = m.VisualLine.GetVisualColumn(m.FoldingSection.StartOffset - m.VisualLine.FirstDocumentLine.Offset);
                TextLine textLine = m.VisualLine.GetTextLine(visualColumn);
                double yPos = m.VisualLine.GetTextLineVisualYPosition(textLine, VisualYPosition.TextMiddle) - TextView.VerticalOffset;
                yPos -= m.DesiredSize.Height / 2;
                double xPos = (finalSize.Width - m.DesiredSize.Width) / 2;
                m.Arrange(new Rect(PixelSnapHelpers.Round(new Point(xPos, yPos), pixelSize), m.DesiredSize));
            }
            return finalSize;
        }

        private readonly List<FoldingMarginMarker> _markers = new List<FoldingMarginMarker>();

        protected override void OnTextViewVisualLinesChanged()
        {
            foreach (FoldingMarginMarker m in _markers)
            {
                VisualChildren.Remove(m);
            }

            _markers.Clear();
            InvalidateVisual();
            if (TextView != null && FoldingManager != null && TextView.VisualLinesValid)
            {
                foreach (VisualLine line in TextView.VisualLines)
                {
                    FoldingSection fs = FoldingManager.GetNextFolding(line.FirstDocumentLine.Offset);
                    if (fs?.StartOffset <= line.LastDocumentLine.Offset + line.LastDocumentLine.Length)
                    {
                        FoldingMarginMarker m = new FoldingMarginMarker
                        {
                            IsExpanded = !fs.IsFolded,
                            VisualLine = line,
                            FoldingSection = fs
                        };
                        ((ISetLogicalParent)m).SetParent(this);

                        _markers.Add(m);
                        VisualChildren.Add(m);

                        m.PropertyChanged += (o, args) =>
                        {
                            if (args.Property == IsPointerOverProperty)
                            {
                                InvalidateVisual();
                            }
                        };

                        InvalidateMeasure();
                    }
                }
            }
        }

        private Pen _foldingControlPen = new Pen(FoldingMarkerBrushProperty.GetDefaultValue(typeof(FoldingMargin)));
        private Pen _selectedFoldingControlPen = new Pen(SelectedFoldingMarkerBrushProperty.GetDefaultValue(typeof(FoldingMargin)));

        /// <inheritdoc/>
        public override void Render(DrawingContext drawingContext)
        {
            if (TextView == null || !TextView.VisualLinesValid)
                return;
            if (TextView.VisualLines.Count == 0 || FoldingManager == null)
                return;

            List<TextLine> allTextLines = TextView.VisualLines.SelectMany(vl => vl.TextLines).ToList();
            Pen[] colors = new Pen[allTextLines.Count + 1];
            Pen[] endMarker = new Pen[allTextLines.Count];

            CalculateFoldLinesForFoldingsActiveAtStart(allTextLines, colors, endMarker);
            CalculateFoldLinesForMarkers(allTextLines, colors, endMarker);
            DrawFoldLines(drawingContext, colors, endMarker);
        }

        /// <summary>
        /// Calculates fold lines for all folding sections that start in front of the current view
        /// and run into the current view.
        /// </summary>
        private void CalculateFoldLinesForFoldingsActiveAtStart(List<TextLine> allTextLines, Pen[] colors, Pen[] endMarker)
        {
            int viewStartOffset = TextView.VisualLines[0].FirstDocumentLine.Offset;
            int viewEndOffset = TextView.VisualLines.Last().LastDocumentLine.EndOffset;
            ReadOnlyCollection<FoldingSection> foldings = FoldingManager.GetFoldingsContaining(viewStartOffset);
            int maxEndOffset = 0;
            foreach (FoldingSection fs in foldings)
            {
                int end = fs.EndOffset;
                if (end <= viewEndOffset && !fs.IsFolded)
                {
                    int textLineNr = GetTextLineIndexFromOffset(allTextLines, end);
                    if (textLineNr >= 0)
                    {
                        endMarker[textLineNr] = _foldingControlPen;
                    }
                }
                if (end > maxEndOffset && fs.StartOffset < viewStartOffset)
                {
                    maxEndOffset = end;
                }
            }
            if (maxEndOffset > 0)
            {
                if (maxEndOffset > viewEndOffset)
                {
                    for (int i = 0; i < colors.Length; i++)
                    {
                        colors[i] = _foldingControlPen;
                    }
                }
                else
                {
                    int maxTextLine = GetTextLineIndexFromOffset(allTextLines, maxEndOffset);
                    for (int i = 0; i <= maxTextLine; i++)
                    {
                        colors[i] = _foldingControlPen;
                    }
                }
            }
        }

        /// <summary>
        /// Calculates fold lines for all folding sections that start inside the current view
        /// </summary>
        private void CalculateFoldLinesForMarkers(List<TextLine> allTextLines, Pen[] colors, Pen[] endMarker)
        {
            foreach (FoldingMarginMarker marker in _markers)
            {
                int end = marker.FoldingSection.EndOffset;
                int endTextLineNr = GetTextLineIndexFromOffset(allTextLines, end);
                if (!marker.FoldingSection.IsFolded && endTextLineNr >= 0)
                {
                    if (marker.IsPointerOver)
                        endMarker[endTextLineNr] = _selectedFoldingControlPen;
                    else if (endMarker[endTextLineNr] == null)
                        endMarker[endTextLineNr] = _foldingControlPen;
                }
                int startTextLineNr = GetTextLineIndexFromOffset(allTextLines, marker.FoldingSection.StartOffset);
                if (startTextLineNr >= 0)
                {
                    for (int i = startTextLineNr + 1; i < colors.Length && i - 1 != endTextLineNr; i++)
                    {
                        if (marker.IsPointerOver)
                            colors[i] = _selectedFoldingControlPen;
                        else if (colors[i] == null)
                            colors[i] = _foldingControlPen;
                    }
                }
            }
        }

        /// <summary>
        /// Draws the lines for the folding sections (vertical line with 'color', horizontal lines with 'endMarker')
        /// Each entry in the input arrays corresponds to one TextLine.
        /// </summary>
        private void DrawFoldLines(DrawingContext drawingContext, Pen[] colors, Pen[] endMarker)
        {
            // Because we are using PenLineCap.Flat (the default), for vertical lines,
            // Y coordinates must be on pixel boundaries, whereas the X coordinate must be in the
            // middle of a pixel. (and the other way round for horizontal lines)
            Size pixelSize = PixelSnapHelpers.GetPixelSize(this);
            double markerXPos = PixelSnapHelpers.PixelAlign(Bounds.Width / 2, pixelSize.Width);
            double startY = 0;
            Pen currentPen = colors[0];
            int tlNumber = 0;
            foreach (VisualLine vl in TextView.VisualLines)
            {
                foreach (TextLine tl in vl.TextLines)
                {
                    if (endMarker[tlNumber] != null)
                    {
                        double visualPos = GetVisualPos(vl, tl, pixelSize.Height);
                        drawingContext.DrawLine(endMarker[tlNumber], new Point(markerXPos - pixelSize.Width / 2, visualPos), new Point(Bounds.Width, visualPos));
                    }
                    if (colors[tlNumber + 1] != currentPen)
                    {
                        double visualPos = GetVisualPos(vl, tl, pixelSize.Height);
                        if (currentPen != null)
                        {
                            drawingContext.DrawLine(currentPen, new Point(markerXPos, startY + pixelSize.Height / 2), new Point(markerXPos, visualPos - pixelSize.Height / 2));
                        }
                        currentPen = colors[tlNumber + 1];
                        startY = visualPos;
                    }
                    tlNumber++;
                }
            }
            if (currentPen != null)
            {
                drawingContext.DrawLine(currentPen, new Point(markerXPos, startY + pixelSize.Height / 2), new Point(markerXPos, Bounds.Height));
            }
        }

        private double GetVisualPos(VisualLine vl, TextLine tl, double pixelHeight)
        {
            double pos = vl.GetTextLineVisualYPosition(tl, VisualYPosition.TextMiddle) - TextView.VerticalOffset;
            return PixelSnapHelpers.PixelAlign(pos, pixelHeight);
        }

        private int GetTextLineIndexFromOffset(List<TextLine> textLines, int offset)
        {
            int lineNumber = TextView.Document.GetLineByOffset(offset).LineNumber;
            VisualLine vl = TextView.GetVisualLine(lineNumber);
            if (vl != null)
            {
                int relOffset = offset - vl.FirstDocumentLine.Offset;
                TextLine line = vl.GetTextLine(vl.GetVisualColumn(relOffset));
                return textLines.IndexOf(line);
            }
            return -1;
        }
    }
}
