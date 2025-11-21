using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.EditorInput;
//using Autodesk.AutoCAD.Geometry;
//using Autodesk.AutoCAD.Runtime;
//using Autodesk.AutoCAD.Colors;


namespace Bimcommand.Test
{
    internal class BaiToanVector
    {
        public const double EPSILON = 1E-06;
        public class Vector2
        {
            //------------- Khai báo -------------//
            private double X { get; set; }
            private double Y { get; set; }

            //Tạo ra vector có tọa độ (x,y)
            public Vector2(double x, double y)
            {
                X = x;
                Y = y;
            }

            //Tạo ra vector sao chép từ vector khác (clone Contructor)
            //
            // <Tạo bản sao độc lập>
            // Vector v1 = new Vector(2,3);
            // Vector v2 = new Vector(v1); //Tạo v2 giống hệt v1
            // v2.X = 5; //Chỉ thay đổi v2, v1 không đổi
            //
            public Vector2(Vector2 vector)
            {
                this.X = vector.X;
                this.Y = vector.Y;
            }


            //------------------- Phép toán-------------------//

            //Nhân vector với một số
            public static Vector2 operator *(Vector2 Vector, double Multiplier) //Vector * số
            {
                if (Vector == null)
                {
                    return null; //throw new ArgumentNullException("Vector không được null");
                }

                return new Vector2(Vector.X * Multiplier, Vector.Y * Multiplier);
            }
            public static Vector2 operator *(double Multiplier, Vector2 Vector) //số * Vector, đổi chỗ tham số
            {
                return Multiplier * Vector; ;
            }

            //Chiều dài vector: v(x,y) -> |v| = sqrt(x^2 + y^2)
            public double GetLength()
            {
                return Math.Sqrt(X * X + Y * Y);
            }

            //Tích vô hướng: v1 . v2 = x1*x2 + y1*y2
            public double Dot(Vector2 Vector)
            {
                return this.X * Vector.X + this.Y * Vector.Y;
            }

            //Normalize về độ dài cho trước
            public double Normalize(double NewLength)
            {
                double CurrentLength = GetLength();
                if (CurrentLength < EPSILON)
                {
                    return 0.0; //Không thể chuẩn hóa vector có độ dài bằng 0
                }
                double ScaleFactor = NewLength / CurrentLength;
                this.X *= ScaleFactor;
                this.Y *= ScaleFactor;
                return CurrentLength;
            }

            //Normalize về độ dài 1 (mặc định)
            public double Normalize()
            {
                return Normalize(1.0);
            }

            //Hàm thành viên
            public Vector2 GetNormal()
            {
                Vector2 vector = new Vector2(this);
                vector.Normalize();
                return vector;
            }

            //Get Angle between two vectors in radians
            public double GetAngle(Vector2 Vector)
            {
                double dotProduct = this.Dot(Vector);
                double lengthsProduct = this.GetLength() * Vector.GetLength();
                if (lengthsProduct < EPSILON)
                {
                    MessageBox.Show("Không thể tính góc giữa các vector có độ dài bằng 0");
                    return 0.0; // throw new InvalidOperationException("Không thể tính góc giữa các vector có độ dài bằng 0");
                }
                double cosTheta = dotProduct / lengthsProduct;
                cosTheta = Math.Max(-1.0, Math.Min(1.0, cosTheta));
                return Math.Acos(cosTheta);

            }

            public void Main()
            {
                double a = 0;
                double b = 0;

                Vector2 v1 = new Vector2(a, b);
            }
        }
    }
}
