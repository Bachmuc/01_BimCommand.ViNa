using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorFuction
{
    // Fixed the issue by changing the invalid enum declaration to a valid constant declaration.
    public static class Constants
    {
        public const double EPSILON = 1E-06;
    }

    public class Vector3
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public Vector3(Vector3 vector)
        {
            this.X = vector.X;
            this.Y = vector.Y;
            this.Z = vector.Z;
        }


        #region Phép toán Operator Overloading

        /// <summary>
        /// Hàm hỗ trợ vector với một số
        /// </summary>
        /// <param name="Vector"></param>
        /// <param name="Multiplier"></param>
        /// <returns></returns>
        public static Vector3 operator *(Vector3 Vector, double Multiplier) //Vector * số
        {
            if (Vector == null)
            {
                return null; //throw new ArgumentNullException("Vector không được null");
            }
            return new Vector3((float)(Vector.X * Multiplier), (float)(Vector.Y * Multiplier), (float)(Vector.Z * Multiplier));
        }
        public static Vector3 operator /(Vector3 Vector, double Divisor) //Vector / số
        {
            if (Vector == null)
            {
                return null; //throw new ArgumentNullException("Vector không được null");
            }
            if (Math.Abs(Divisor) < Constants.EPSILON)
            {
                throw new DivideByZeroException("Không thể chia cho 0");
            }
            return new Vector3((float)(Vector.X / Divisor), (float)(Vector.Y / Divisor), (float)(Vector.Z / Divisor));
        }

        public static Vector3 operator *(double Multiplier, Vector3 Vector) //số * Vector, đổi chỗ tham số
        {
            return Multiplier * Vector; ;
        }
        public static Vector3 operator /(double Divisor, Vector3 Vector) //số / Vector, đổi chỗ tham số
        {
            if (Vector == null)
            {
                return null; //throw new ArgumentNullException("Vector không được null");
            }
            if (Math.Abs(Divisor) < Constants.EPSILON)
            {
                throw new DivideByZeroException("Không thể chia cho 0");
            }
            return new Vector3((float)(Divisor / Vector.X), (float)(Divisor / Vector.Y), (float)(Divisor / Vector.Z));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector3 GetVectorTo(Vector3 v1, Vector3 v2)
        {
            if (v1 == null || v2 == null)
            {
                return null; //throw new ArgumentNullException("Vector không được null");
            }
            return new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }
        #endregion


        /// <summary>
        /// Hàm hỗ trợ nhân số với vector (đổi chỗ tham số)
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static double Dot(Vector3 v1, Vector3 v2)
        {
            if (v1 == null || v2 == null)
            {
                throw new ArgumentNullException("Vector không được null");
            }
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }


        /// <summary>
        /// Hàm chuẩn hóa vector về độ dài 1
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Vector3 Normalize(Vector3 vector)
        {
            if (vector == null)
            {
                return null; //throw new ArgumentNullException("Vector không được null");
            }
            double length = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
            if (length < Constants.EPSILON)
            {
                throw new ArgumentException("Không thể chuẩn hóa vector có độ dài bằng 0");
            }
            return new Vector3((float)(vector.X / length), (float)(vector.Y / length), (float)(vector.Z / length));
        }

        /// <summary>
        /// Hàm hỗ trợ tích có hướng (Cross Product) của 2 vector
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Vector3 Cross(Vector3 v1, Vector3 v2)
        {
            if (v1 == null || v2 == null)
            {
                throw new ArgumentNullException("Vector không được null");
            }
            return new Vector3(
                v1.Y * v2.Z - v1.Z * v2.Y,
                v1.Z * v2.X - v1.X * v2.Z,
                v1.X * v2.Y - v1.Y * v2.X
            );
        }

        public static double AngleBetween(Vector3 v1, Vector3 v2)
        {
            if (v1 == null || v2 == null)
            {
                throw new ArgumentNullException("Vector không được null");
            }
            Vector3 n1 = Normalize(v1);
            Vector3 n2 = Normalize(v2);
            double dotProduct = Dot(n1, n2);
            dotProduct = Math.Max(-1.0, Math.Min(1.0, dotProduct));
            return Math.Acos(dotProduct); // Trả về góc bằng radian
        }

        public double GetLength()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }
    }
}
