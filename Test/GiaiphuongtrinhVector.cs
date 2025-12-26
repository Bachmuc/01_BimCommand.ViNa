using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Test
{
    internal class GiaiphuongtrinhVector
    {
        static void Main(string[] args)
        {
            // Cho phép HIỂN THỊ tiếng Việt
            Console.OutputEncoding = Encoding.UTF8;

            // Cho phép NHẬP tiếng Việt (nếu cần nhập từ bàn phím)
            Console.InputEncoding = Encoding.UTF8;

            List<PhuongTrinh> pt = new List<PhuongTrinh>();

            PhuongTrinh p1 = new PhuongTrinh();
            Console.WriteLine("Nhập biến cho phương trình: aX + bY + c = 0\n\nPhương trình 1:");
            p1.Giatri();
            pt.Add(p1);

            PhuongTrinh p2 = new PhuongTrinh();
            Console.WriteLine("\nPhương trình 2:");
            p2.Giatri();
            pt.Add(p2);

            List<Vector3> v = new List<Vector3>();

            Console.WriteLine($"\nPhương trình 1: {p1.a}X + {p1.b}Y + {p1.c} = 0");
            Vector3 v1 = new Vector3(p1.a, p1.b , 0);
            v.Add(v1);

            Console.WriteLine($"\nPhương trình 2: {p2.a}X + {p2.b}Y + {p2.c} = 0\n");
            Vector3 v2 = new Vector3(p2.a, p2.b, 0);
            v.Add(v2);

            Check(v1 , v2);

            /// Định thức (Determinant)
            /// Cho 2 vector pháp tuyến n1(a1,b1), n2(a2,b2)
            ///  D = a1.b2 + a2.b1
            ///  D != 0 -> cắt nhau (nghiệm duy nhất
            ///  D == 0 -> song song hoặc trùng
            float D = v1.X * v2.Y + v2.X * v1.Y;
            float EPSILON = 0.0000001f; // sủ dụng sai số để so sánh với 0 cho an toàn

            if (Math.Abs(D) < EPSILON)
            {
                return;
            }
            else
            {
                // Mở rộng: Nếu muốn tìm điểm cắt nhau đó (Giải hệ phương trình Cramer)
                // x = (c1*b2 - c2*b1) / det
                // y = (a1*c2 - a2*c1) / det
                // (Lưu ý dấu của c chuyển vế: ax+by = -c)
                float x = (p1.c * p2.b - p2.c * p1.b) / D;
                float y = (p1.a * p2.c - p2.a * p1.c) / D;
                Console.WriteLine($"\nTọa độ điểm cắt là A({x},{y})");
            }

            Console.ReadLine();
        }

        public class PhuongTrinh
        {
            public float a;
            public float b;
            public float c;

            public void Giatri()
            {
                Console.Write("Giá trị a : ");
                this.a = Convert.ToSingle(Console.ReadLine());
                Console.Write("Giá trị b : ");
                this.b = Convert.ToSingle(Console.ReadLine());
                Console.Write("Giá trị c : ");
                this.c = Convert.ToSingle(Console.ReadLine());
            }

        }
        static void Check(Vector3 p1, Vector3 p2)
        {
            p1 = Vector3.Normalize(p1);
            p2 = Vector3.Normalize(p2);

            float result = Vector3.Dot(p1, p2);
            double EPSILON = 0.0000001;

            if (Math.Abs(result) < EPSILON)
            {
                Console.WriteLine("Hai phương trình vuông góc");
            }
            else if (result >= 1.0f - EPSILON || result <= 1.0f + EPSILON)
            {
                Console.WriteLine("Hai phương trình cùng phương, cùng hướng");
            }
            else if (result <= -1.0f + EPSILON || result >= -1.0f - EPSILON)
            {
                Console.WriteLine("Hai phương trình cùng phương, ngược hướng");
            }
            else
            {
                // Góc lệch
                double radian = Math.Acos(result); // ra radian
                double degrease = radian * (180 / Math.PI); // ra độ
                Console.WriteLine($"Hai phương trình cắt nhau một góc Anpha = {degrease}");
            }
        }
    }
}
