using boilersGraphics.Adorners;
using boilersGraphics.Exceptions;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace boilersGraphics.Controls
{
    public class AuxiliaryArcBetweenCeilingAndTarget
    {
        private UIElement adornedElement;
        private Vector _startVector;
        private double _beginDegree;
        private AuxiliaryLine _CenterToP1;
        private Point _centerPoint;
        private Vector _endVector;
        private AuxiliaryLine _CenterToP2;
        private FrameworkElementAdorner _Arc;
        private AuxiliaryText _DegreeText;

        public AuxiliaryArcBetweenCeilingAndTarget(UIElement adornedElement)
        {
            this.adornedElement = adornedElement;
        }

        public void OnMouseUp()
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
            RemoveFromAdornerLayer(adornerLayer, nameof(_CenterToP1));
            RemoveFromAdornerLayer(adornerLayer, nameof(_CenterToP2));
            RemoveFromAdornerLayer(adornerLayer, nameof(_Arc));
            RemoveFromAdornerLayer(adornerLayer, nameof(_DegreeText));
        }

        public void Render1st(Point centerPoint, Point endPoint)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
            RemoveFromAdornerLayer(adornerLayer, nameof(_CenterToP1));
            _centerPoint = centerPoint;
            Point startPoint = Mouse.GetPosition(adornedElement);
            var vector = new Vector(endPoint.X - centerPoint.X, endPoint.Y - centerPoint.Y);
            _startVector = Point.Subtract(startPoint, centerPoint);
            _beginDegree = Vector.AngleBetween(_startVector, new Vector(0, 1));
            _CenterToP1 = new AuxiliaryLine(adornedElement, new Point(centerPoint.X, centerPoint.Y - vector.Length), centerPoint);
            adornerLayer.Add(_CenterToP1);
        }

        public (double, double) Render2nd(Point endPoint, double rotationAngle)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);

            RemoveFromAdornerLayer(adornerLayer, nameof(_CenterToP2));

            Point currentPoint = Mouse.GetPosition(adornedElement);
            Vector deltaVector = Point.Subtract(currentPoint, _centerPoint);
            _endVector = Vector.Subtract(new Vector(endPoint.X, endPoint.Y), new Vector(_centerPoint.X, _centerPoint.Y));
            _CenterToP2 = new AuxiliaryLine(adornedElement, _centerPoint, endPoint);
            adornerLayer.Add(_CenterToP2);
            var deltaAngle = Vector.AngleBetween(new Vector(0, 1), deltaVector);
            var beginDegree = MakeDegree(-180);
            var endDegree = MakeDegree(deltaAngle);

            var angleType = (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.AngleType.Value;
            rotationAngle = Math.Round(endDegree + 90);
            RemoveFromAdornerLayer(adornerLayer, nameof(_Arc));
            _Arc = new FrameworkElementAdorner(adornedElement);
            var roundDegree = Math.Round(rotationAngle);
            roundDegree = roundDegree % 360;
            if (angleType == Helpers.AngleType.Minus180To180 && roundDegree > 180)
            {
                roundDegree = roundDegree - 360;
            }
            rotationAngle = roundDegree;
            var pie = GeometryCreator.CreatePie(_centerPoint, 20, beginDegree, endDegree, DecideSweepDirection(angleType, rotationAngle));
            _Arc.Child = new Path()
            {
                Data = pie,
                Stroke = Brushes.Blue,
                StrokeThickness = 1,
            };
            adornerLayer.Add(_Arc);
            if (_DegreeText != null)
            {
                adornerLayer.Remove(_DegreeText);
            }
            _DegreeText = new AuxiliaryText(adornedElement, $"{roundDegree}°", new Point(_centerPoint.X + 40 * Math.Cos(θ(rotationAngle, angleType, roundDegree)) - 20, _centerPoint.Y + 40 * Math.Sin(θ(rotationAngle, angleType, roundDegree)) - 20));
            adornerLayer.Add(_DegreeText);
            return (rotationAngle, roundDegree);
        }

        public (double, double) Render2nd(Point endPoint, double rotationAngle, SweepDirection sweepDirection)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);

            RemoveFromAdornerLayer(adornerLayer, nameof(_CenterToP2));

            Point currentPoint = Mouse.GetPosition(adornedElement);
            Vector deltaVector = Point.Subtract(currentPoint, _centerPoint);
            _endVector = Vector.Subtract(new Vector(endPoint.X, endPoint.Y), new Vector(_centerPoint.X, _centerPoint.Y));
            _CenterToP2 = new AuxiliaryLine(adornedElement, _centerPoint, endPoint);
            adornerLayer.Add(_CenterToP2);
            var deltaAngle = Vector.AngleBetween(new Vector(0, 1), deltaVector);
            var beginDegree = MakeDegree(-180);
            var endDegree = MakeDegree(deltaAngle);

            var angleType = (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.AngleType.Value;
            rotationAngle = Math.Round(endDegree + 90);
            RemoveFromAdornerLayer(adornerLayer, nameof(_Arc));
            _Arc = new FrameworkElementAdorner(adornedElement);
            var roundDegree = Math.Round(rotationAngle);
            roundDegree = roundDegree % 360;
            if (angleType == Helpers.AngleType.Minus180To180 && roundDegree > 180)
            {
                roundDegree = roundDegree - 360;
            }
            rotationAngle = roundDegree;
            var pie = GeometryCreator.CreatePie(_centerPoint, 20, beginDegree, endDegree, sweepDirection);
            _Arc.Child = new Path()
            {
                Data = pie,
                Stroke = Brushes.Blue,
                StrokeThickness = 1,
            };
            adornerLayer.Add(_Arc);
            if (_DegreeText != null)
            {
                adornerLayer.Remove(_DegreeText);
            }
            _DegreeText = new AuxiliaryText(adornedElement, $"{roundDegree}°", new Point(_centerPoint.X + 40 * Math.Cos(θ(rotationAngle, angleType, roundDegree)) - 20, _centerPoint.Y + 40 * Math.Sin(θ(rotationAngle, angleType, roundDegree)) - 20));
            adornerLayer.Add(_DegreeText);
            return (rotationAngle, roundDegree);
        }

        private double θ(double rotationAngle, Helpers.AngleType angleType, double roundDegree)
        {
            if (DecideSweepDirection(angleType, rotationAngle) == SweepDirection.Clockwise)
            {
                return Radian((roundDegree - 90) / 2);
            }
            else
            {
                return -Radian((-roundDegree - 90) / 2 - 180);
            }
        }

        private double Radian(double degree)
        {
            return Math.PI / 180.0 * degree;
        }

        private SweepDirection DecideSweepDirection(Helpers.AngleType angleType, double endDegree)
        {
            if (angleType == Helpers.AngleType.ZeroTo360)
            {
                return SweepDirection.Clockwise;
            }
            else //angleType == Helpers.AngleType.Minus180To180
            {
                if (endDegree > 180 || endDegree < 0)
                {
                    return SweepDirection.Counterclockwise;
                }
                else
                {
                    return SweepDirection.Clockwise;
                }
            }
        }

        private void RemoveFromAdornerLayer(AdornerLayer adornerLayer, string fieldName)
        {
            var field = typeof(AuxiliaryArcBetweenCeilingAndTarget).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field.FieldType.BaseType != typeof(Adorner))
                throw new UnexpectedException("field.FieldType.BaseType != typeof(Adorner)");
            var target = (Adorner)field.GetValue(this);
            if (target != null)
            {
                adornerLayer.Remove(target);
                field.SetValue(this, null);
            }
        }

        private double MakeDegree(double deltaAngle)
        {
            var degree = 270 + (deltaAngle + 180);
            if (degree >= 360)
                degree -= 360;
            return degree;
        }
    }
}
