using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using Tekla.Structures.Geometry3d;

namespace ExtensionCAD
{
    public static class ConvertPoints
    {
        #region Convert từ Tekla -> CAD

        public static Point2d Convert_ToCad2d(this Point ptTekla)
        {
            return new Point2d(ptTekla.X, ptTekla.Y);
        }
        public static Point3d Convert_ToCad3d(this Point ptTekla)
        {
            return new Point3d(ptTekla.X, ptTekla.Y, ptTekla.Z);
        }
        public static Point3d Convert_ToCad3dZero(this Point ptTekla)
        {
            return new Point3d(ptTekla.X, ptTekla.Y, 0);
        }

        #endregion

        #region Convert từ CAD -> Tekla

        public static Point Convert_ToTekla(this Point3d ptCAD)
        {
            return new Point(ptCAD.X, ptCAD.Y, ptCAD.Z);
        }

        #endregion

    }
}
