using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

// using AutoCad 
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows.Data;

//using tekla
using Tekla.Structures;
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Catalogs;
using Tekla.Structures.Model.UI;
using Tekla.Structures.Solid;

namespace Bimcommand.CADtoTEKLA
{
    internal class SupportFuctions
    {
        public static Tekla.Structures.Geometry3d.Point ConvertPointFromCADtoTekla(Autodesk.AutoCAD.Geometry.Point3d cadPoint)
        {
            // Chuyển đổi tọa độ từ hệ tọa độ AutoCAD sang Tekla
            return new Tekla.Structures.Geometry3d.Point(cadPoint.X, cadPoint.Y, cadPoint.Z);
        }

    }
}
