using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Geometry;


namespace Bimcommand.ShopDrawing
{
    internal class MoveCircleMtext
    {
        /// <summary>
        /// Move Cirlce to Mtext
        /// </summary>
        [CommandMethod("CT1")]
        public void cmdCT1()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db= doc.Database;

            // Quét chọn qua các đối tượng Mtext, Circle
            PromptSelectionOptions pso = new PromptSelectionOptions(); 
            SelectionFilter slf = new SelectionFilter(new[]
            {
                new TypedValue((int)DxfCode.Start, "MTEXT, CIRCLE")
            } );
            PromptSelectionResult psr = ed.GetSelection(pso, slf);
            if ( psr.Status != PromptStatus.OK ) return;

            // Khái báo biến lưu CAD
            List<ObjectId> mtext = new List<ObjectId>();
            List<ObjectId> circle = new List<ObjectId>();

            //Phân loại Object
            using(Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach( ObjectId id in psr.Value.GetObjectIds())
                {
                    Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                    if(ent is MText)
                    {
                        mtext.Add(id);
                    }
                    else if (ent is Circle)
                    {
                        Circle c = (Circle)ent;
                    }
                }
                if (!mtext.Any() || !circle.Any())
                {
                    ed.WriteMessage("\nCT1: Not enough MTEXT or CIRCLE.");
                    return;
                }
                //Lấy toàn bộ circle để check collision

            }
        }

        // --- Hàm hỗ trợ ---
        // Hàm chuyển đổi radian
        private static double DegRadians(double deg )
        {
            return deg * Math.PI / 180; 
        }
        // Xử lý Circle
        private static List<Circle> GetAllCircle(Transaction tr, Database db)
        {
            List<Circle> result = new List<Circle>();

            BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
            foreach (ObjectId oId in btr)
            {
                if (tr.GetObject(oId, OpenMode.ForRead) is Circle c)
                {
                    result.Add(c);
                }
            }

            return result;
        }
    }
}
