using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Helpers
{
    class GeometryCombiner
    {
        public static PathGeometry Connect(PathGeometry geometry1, PathGeometry geometry2)
        {
            var str1 = geometry1.ToString();
            var str2 = geometry2.ToString();
            var figures1 = Interpret(str1);
            var figures2 = Interpret(str2);
            if (figures1.Last().Point2 == figures2.First().Point1)
            {
                var str = $"M {figures1.First().Point1} ";
                foreach (var figure in figures1)
                {
                    str += figure.ToString();
                    str += " ";
                }
                foreach (var figure in figures2)
                {
                    str += figure.ToString();
                    if (figures2.Last() != figure)
                        str += " ";
                }
                return PathGeometry.CreateFromGeometry(Geometry.Parse(str));
            }
            else if (figures2.Last().Point2 == figures1.First().Point1)
            {
                var str = $"M {figures2.First().Point1} ";
                foreach (var figure in figures2)
                {
                    str += figure.ToString();
                    str += " ";
                }
                foreach (var figure in figures1)
                {
                    str += figure.ToString();
                    if (figures2.Last() != figure)
                        str += " ";
                }
                return PathGeometry.CreateFromGeometry(Geometry.Parse(str));
            }
            throw new Exception("geometries do not connect");
        }

        private static IEnumerable<Figure> Interpret(string str)
        {
            var split = str.Split(' ', ',', '-');
            List<Figure> ret = new List<Figure>();
            int i = 0;
            Point prevPoint = new Point();
            while (i < split.Count())
            {
                var str1 = split.ElementAt(i);
                if (str1 == "M")
                {
                    i++;
                    var str2 = split.ElementAt(i); //X coordinate
                    i++;
                    var str3 = split.ElementAt(i); //,
                    i++;
                    var str4 = split.ElementAt(i); //Y coordinate
                    i++;
                    prevPoint = Point.Parse(str2 + str3 + str4);
                }
                else if (str1 == Line.Keyword())
                {
                    i++;
                    var obj = new Line();
                    obj.Point1 = prevPoint;
                    var str2 = split.ElementAt(i); //X coordinate
                    i++;
                    var str3 = split.ElementAt(i); //,
                    i++;
                    var str4 = split.ElementAt(i); //Y coordinate
                    i++;
                    obj.Point2 = Point.Parse(str2 + str3 + str4);
                    ret.Add(obj);
                    prevPoint = obj.Point2;
                }
                else if (str1 == BezierLine.Keyword())
                {
                    i++;
                    var obj = new BezierLine();
                    obj.Point1 = prevPoint;
                    var str2 = split.ElementAt(i); //X1 coordinate
                    i++;
                    var str3 = split.ElementAt(i); //,
                    i++;
                    var str4 = split.ElementAt(i); //Y1 coordinate
                    i++;
                    var str5 = split.ElementAt(i); //space
                    i++;
                    var str6 = split.ElementAt(i); //X2 coordinate
                    i++;
                    var str7 = split.ElementAt(i); //,
                    i++;
                    var str8 = split.ElementAt(i); //Y2 coordinate
                    i++;
                    var str9 = split.ElementAt(i); //space
                    i++;
                    var str10 = split.ElementAt(i); //X coordinate
                    i++;
                    var str11 = split.ElementAt(i); //,
                    i++;
                    var str12 = split.ElementAt(i); //Y coordinate
                    i++;
                    obj.ControlPoint1 = Point.Parse(str2 + str3 + str4);
                    obj.ControlPoint2 = Point.Parse(str6 + str7 + str8);
                    obj.Point2 = Point.Parse(str10 + str11 + str12);
                    ret.Add(obj);
                    prevPoint = obj.Point2;
                }
                i++;
            }
            return ret;
        }
    }

    abstract class Figure
    {
        public Point Point1 { get; set; }
        public Point Point2 { get; set; }
    }

    class Line : Figure
    {
        public static string Keyword()
        {
            return "L";
        }

        public override string ToString()
        {
            return $"L {Point2}";
        }
    }

    class BezierLine : Figure
    {
        public Point ControlPoint1 { get; set; }

        public Point ControlPoint2 { get; set; }

        public static string Keyword()
        {
            return "C";
        }

        public override string ToString()
        {
            return $"C {ControlPoint1} {ControlPoint2} {Point2}";
        }
    }
}
