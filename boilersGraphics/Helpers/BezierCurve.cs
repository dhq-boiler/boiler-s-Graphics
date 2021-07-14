using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace boilersGraphics.Helpers
{
    /// <summary>
    /// [数学] 3D空間にベジェ曲線を描く
    /// Author: @edo_m18
    /// https://qiita.com/edo_m18/items/643512f27c2b083b47ac
    /// で掲載されているソースコードを改変して実装
    /// </summary>
    public static class BezierCurve
    {

        /// <summary>
        /// ベジェ曲線関数
        /// </summary>
        public static Point Evaluate(double t, IEnumerable<Point> points)
        {
            Point result = new Point();
            int n = points.Count();
            for (int i = 0; i < n; i++)
            {
                result.X += points.ElementAt(i).X * Bernstein(n - 1, i, t);
                result.Y += points.ElementAt(i).Y * Bernstein(n - 1, i, t);
            }

            return result;
        }

        /// <summary>
        /// バーンスタイン基底関数
        /// </summary>
        static double Bernstein(int n, int i, double t)
        {
            return Binomial(n, i) * Math.Pow(t, i) * Math.Pow(1 - t, n - i);
        }

        /// <summary>
        /// 二項係数を計算する
        /// </summary>
        static double Binomial(int n, int k)
        {
            return Factorial(n) / (Factorial(k) * Factorial(n - k));
        }

        /// <summary>
        /// 階乗を計算する
        /// </summary>
        static double Factorial(int a)
        {
            double result = 1d;
            for (int i = 2; i <= a; i++)
            {
                result *= i;
            }

            return result;
        }
    }
}
