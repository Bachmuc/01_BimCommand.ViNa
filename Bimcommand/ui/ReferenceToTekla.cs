using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bimcommand.UI
{
    internal class ReferenceToTekla
    {
        #region Shared.Contracts
        // SlabContour.cs
        public class SlabContour
        {
            public List<Point3d> Points { get; set; }
            public double Thickness { get; set; }
        }

        // ISlabService.cs
        public interface ISlabService
        {
            void CreateSlab(SlabContour contour);
        }
        #endregion

        #region Cad.Core (logic AutoCAD)
        public class SlabExporter
        {
            private readonly ISlabService _slabService;

            public SlabExporter(ISlabService slabService)
            {
                _slabService = slabService;
            }

            public void Export(SlabContour contour)
            {
                _slabService.CreateSlab(contour);
            }
        }
        #endregion

        #region Tekla.Adapter (ChỈ build khi có Tekla)
        public static ISlabService LoadTeklaService()
        {
            string dllPath = @"Tekla.Adapter.dll";
            Assembly asm = Assembly.LoadFrom(dllPath);

            Type type = asm.GetTypes()
                           .First(t => typeof(ISlabService).IsAssignableFrom(t));

            return (ISlabService)Activator.CreateInstance(type);
        }
        #endregion
    }
}
