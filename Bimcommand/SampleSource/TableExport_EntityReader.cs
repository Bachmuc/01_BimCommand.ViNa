using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Bimcommand.AppLisp.TableExport;

namespace Bimcommand.SampleSource
{
    internal class TableExport_EntityReader
    {
        /// <summary>
        ///     TableExport[Gom nhóm và sắp xếp Text]. Chỉ xử lý content không gom nhóm cho text
        /// </summary>
        public class EntityReader
        {
            public void ReadContent(ObjectId[] ids, Transaction tr, TableModel table, CellLocator locator)
            {
                foreach (ObjectId id in ids)
                {
                    if (!id.IsValid || id.IsErased) continue;

                    var ent = tr.GetObject(id, OpenMode.ForRead);
                    int r = -1, c = -1;
                    string content = "";

                    if (ent is DBText txt)
                    {
                        (r, c) = locator.Locate(txt.Position, table);
                        content = txt.TextString;
                    }
                    else if (ent is MText mtxt)
                    {
                        (r, c) = locator.Locate(mtxt.Location, table);
                        content = mtxt.Text;
                    }
                    else if (ent is BlockReference blk)
                    {
                        (r, c) = locator.Locate(blk.Position, table);
                        content = blk.Name;
                    }

                    // Chỉ thêm nếu index hợp lệ (>=0)
                    if (r >= 0 && c >= 0)
                    {
                        table.Cells.Add(new TableCell(r, c) { Content = content });
                    }
                }
            }
        }

    }
}
