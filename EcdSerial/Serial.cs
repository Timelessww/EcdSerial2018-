//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.EditorInput;
//using Autodesk.AutoCAD.Geometry;
//using Autodesk.AutoCAD.Runtime;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace EcdSerial
//{
//    public class Serial
//    {
//        public int orderIndex = 1;
//        [CommandMethod("zdbh")]
//        public void demo()
//        {
//            Document doc = Application.DocumentManager.MdiActiveDocument;
//            Database db = doc.Database;
//            Editor ed = doc.Editor;
//            while (true)
//            {
//                Transaction trans = db.TransactionManager.StartTransaction();
//                using (trans)
//                {
//                    BlockTable blockTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
//                    BlockTableRecord modelSpace = trans.GetObject(blockTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
//                    MText mt = new MText();
//                    mt.Contents = Convert.ToString(orderIndex);

//                    PromptPointResult ppr = ed.GetPoint("\n指点编号的插入点: ");
//                    Point3d location;
//                    if (ppr.Status != PromptStatus.OK)
//                    {
//                        return;
//                    }
//                    else
//                    {
//                        location = ppr.Value;
//                    }

//                    DBText acText = new DBText();
//                    acText.Position = location;
//                    acText.Height = 50;
//                    acText.TextString = Convert.ToString(orderIndex);
//                    modelSpace.AppendEntity(acText);
//                    trans.AddNewlyCreatedDBObject(acText, true);

//                    /*
//                    mt.TextStyleId = AddTextStyle("宋体", "1", "3", 20, 20);
//                    mt.Width = 50;
//                    mt.Height = 50;
//                    mt.Location = location;
//                    modelSpace.AppendEntity(mt);
//                    trans.AddNewlyCreatedDBObject(mt, true);*/
//                    trans.Commit();

//                    orderIndex++;

//                }
//            }
//        }

//        //public static ObjectId AddTextStyle(string name, string smallfont, string bigfont, double height, double xscale)
//        //{
//        //    Database dbH = HostApplicationServices.WorkingDatabase;

//        //    using (Transaction trans = dbH.TransactionManager.StartTransaction())
//        //    {
//        //        TextStyleTable TST = (TextStyleTable)trans.GetObject(dbH.TextStyleTableId, OpenMode.ForWrite);
//        //        ObjectId id = GetIdFromSymbolTable(TST, name);
//        //        if (id == ObjectId.Null)
//        //        {
//        //            TextStyleTableRecord TSTR = new TextStyleTableRecord();
//        //            TSTR.Name = name;
//        //            TSTR.FileName = smallfont;
//        //            TSTR.BigFontFileName = bigfont;
//        //            TSTR.TextSize = height;
//        //            TSTR.XScale = xscale;
//        //            TST.UpgradeOpen();
//        //            id = TST.Add(TSTR);
//        //            trans.AddNewlyCreatedDBObject(TSTR, true);
//        //        }
//        //        return id;
//        //    }
//        //}

//        ////取得符号表的Id
//        //public static ObjectId GetIdFromSymbolTable(SymbolTable st, string key)
//        //{
//        //    Database dbH = HostApplicationServices.WorkingDatabase;
//        //    using (Transaction trans = dbH.TransactionManager.StartTransaction())
//        //    {
//        //        if (st.Has(key))
//        //        {
//        //            ObjectId idres = st[key];
//        //            if (!idres.IsErased)
//        //                return idres;
//        //            foreach (ObjectId id in st)
//        //            {
//        //                if (!id.IsErased)
//        //                {
//        //                    SymbolTableRecord str = (SymbolTableRecord)trans.GetObject(id, OpenMode.ForRead);
//        //                    if (str.Name == key)
//        //                        return id;
//        //                }
//        //            }
//        //        }
//        //    }
//        //    return ObjectId.Null;
//        //}
//    }
//}