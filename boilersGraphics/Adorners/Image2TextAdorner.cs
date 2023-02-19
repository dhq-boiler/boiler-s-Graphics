using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using boilersGraphics.Controls;
using boilersGraphics.Dao;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.UserControls;
using boilersGraphics.ViewModels;
using boilersGraphics.Views;
using Tesseract;

namespace boilersGraphics.Adorners;

internal class Image2TextAdorner : Adorner
{
    private DesignerCanvas _designerCanvas;
    private Point? _endPoint;
    private readonly Pen _rectanglePen;
    private readonly SnapAction _snapAction;
    private Point? _startPoint;
    private readonly string language;

    public Image2TextAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint, string language)
        : base(designerCanvas)
    {
        _designerCanvas = designerCanvas;
        _startPoint = dragStartPoint;
        this.language = language;
        var parent = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
        var brush = parent.EdgeBrush.Value.Clone();
        brush.Opacity = 0.5;
        _rectanglePen = new Pen(brush, parent.EdgeThickness.Value.Value);
        _snapAction = new SnapAction();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            if (!IsMouseCaptured)
                CaptureMouse();

            //ドラッグ終了座標を更新
            _endPoint = e.GetPosition(this);
            var currentPosition = _endPoint.Value;
            _snapAction.OnMouseMove(ref currentPosition);
            _endPoint = currentPosition;

            (Application.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.CurrentPoint =
                currentPosition;
            (Application.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value =
                $"({_startPoint.Value.X}, {_startPoint.Value.Y}) - ({_endPoint.Value.X}, {_endPoint.Value.Y}) (w, h) = ({_endPoint.Value.X - _startPoint.Value.X}, {_endPoint.Value.Y - _startPoint.Value.Y})";

            InvalidateVisual();
        }
        else
        {
            if (IsMouseCaptured) ReleaseMouseCapture();
        }

        e.Handled = true;
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        // release mouse capture
        if (IsMouseCaptured) ReleaseMouseCapture();

        _snapAction.OnMouseUp(this);

        if (_startPoint.HasValue && _endPoint.HasValue)
        {
            var rect = new System.Windows.Rect(_startPoint.Value, _endPoint.Value);
            var renderer = new Renderer(new WpfVisualTreeHelper());
            var mainWindow = Application.Current.MainWindow;
            var diagramViewModel = mainWindow.GetChildOfType<DiagramControl>().DataContext as DiagramViewModel;
            var rtb = renderer.Render(rect, DesignerCanvas.GetInstance(), diagramViewModel, diagramViewModel.BackgroundItem.Value, null);

            var filename = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());

            try
            {
                OpenCvSharpHelper.ImShow("OCR Target", rtb);
                OpenCvSharpHelper.SaveAsPng(rtb, filename);

                // 画像からテキストを抽出する
                using (var engine = new TesseractEngine(@"Assets\tessdata", language, EngineMode.Default))
                {
                    using (var img = Pix.LoadFromFile(filename))
                    {
                        using (var page = engine.Process(img))
                        {
                            string text = page.GetText();

                            // テキストをクリップボードに保存する
                            Clipboard.SetText(text);

                            (Application.Current.MainWindow.DataContext as MainWindowViewModel).CurrentOperation.Value = "Image2Text";
                            MainWindowViewModel.Instance.Details.Value = $"OCRの結果をクリップボードにコピーしました：{OneLine(text)}";
                        }
                    }
                }
            }
            finally
            {
                File.Delete(filename);
            }

            //UpdateStatisticsCount();

            _startPoint = null;
            _endPoint = null;
        }


        e.Handled = true;
    }

    private string OneLine(string text)
    {
        if (text.IndexOf("\n") != -1)
        {
            return text.Substring(0, text.IndexOf("\n"));
        }
        else
        {
            return text;
        }
    }

    private static void UpdateStatisticsCount()
    {
        var statistics = (Application.Current.MainWindow.DataContext as MainWindowViewModel).Statistics.Value;
        statistics.NumberOfDrawsOfTheGaussianFilterTool++;
        var dao = new StatisticsDao();
        dao.Update(statistics);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        dc.DrawRectangle(Brushes.Transparent, null, new System.Windows.Rect(RenderSize));

        if (_startPoint.HasValue && _endPoint.HasValue)
            dc.DrawRectangle(new SolidColorBrush(new Color
            {
                A = byte.MaxValue / 2,
                B = byte.MaxValue,
                G = 0,
                R = 0
            }), _rectanglePen, ShiftEdgeThickness());
    }

    private System.Windows.Rect ShiftEdgeThickness()
    {
        var parent = (AdornedElement as DesignerCanvas).DataContext as IDiagramViewModel;
        var point1 = _startPoint.Value;
        point1.X += parent.EdgeThickness.Value.Value / 2;
        point1.Y += parent.EdgeThickness.Value.Value / 2;
        var point2 = _endPoint.Value;
        point2.X -= parent.EdgeThickness.Value.Value / 2;
        point2.Y -= parent.EdgeThickness.Value.Value / 2;
        return new System.Windows.Rect(point1, point2);
    }
}