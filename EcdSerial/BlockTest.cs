using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using DotNetARX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
namespace EcdSerial
{
   public  class BlockTest
    {


        /// <summary> 
        /// 自动编号 
        /// </summary> 
        /// <param name="insertPoint">插入点</param> 
        public static void CreateItemNo(Point3d insertPoint, EntityList list, string prefix, int number, int height)
        {
            // Put your command code here
            Plane plane = new Plane();
            //  Ellipse ellipse = new Ellipse(insertPoint, plane.Normal, new Vector3d(200, 0, 0), 0.54, 0, GeTools.DegreeToRadian(360));
            DBText text = new DBText();

            //设置字体自适应窗口
            text.Height = height;
            //单行文字的插入点有两个，不同的对齐方式，需要设置的点不同 
            text.HorizontalMode = TextHorizontalMode.TextCenter;
            text.VerticalMode = TextVerticalMode.TextVerticalMid;
            text.Position = insertPoint;
            text.AlignmentPoint = insertPoint;

            if (number < 10)
            {
                text.TextString = prefix + "00" + number.ToString();
            }
            else if (number < 100)
            {
                text.TextString = prefix + "0" + number.ToString();
            }
            else
            {
                text.TextString = prefix + number.ToString();
            }
            //list.Add(ellipse);
            list.Add(text);
        }


        [CommandMethod("tests")]
        public void selAll()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            //开启事务
            using (Transaction transaction = db.TransactionManager.StartTransaction())
            {
                //打开块表
                BlockTable bt = transaction.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                //打开模型空间块表记录
                BlockTableRecord modelspace = transaction.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                ObjectIdCollection objidcoll = modelspace.GetBlockReferenceIds(true, true);
                ed.WriteMessage("\n共有{0}个", objidcoll.Count);
                foreach (ObjectId item in objidcoll)
                {
                    Entity ent = (Entity)transaction.GetObject(item, OpenMode.ForRead);
                    ed.WriteMessage("\n名字有：{0}", ent.BlockName);
                }
            }
        }
        [CommandMethod("BlT", CommandFlags.UsePickSet)]
        public void BLt()
        {
            // 获取当前文档获
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Transaction tr = db.TransactionManager.StartTransaction();
            BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

            using (tr)
            {

                PromptSelectionResult acSSPrompt = ed.GetSelection();
                SelectionSet acSSet = acSSPrompt.Value;
                if (acSSPrompt.Status == PromptStatus.OK)
                  {
                    var ids = acSSet.GetObjectIds();
                foreach (var item in ids)
                {
                    Entity ent = (Entity)tr.GetObject(item, OpenMode.ForRead);
                    ed.WriteMessage("\n名字有：{0}", ent.GetType().Name);
                }
            }
        }

        }


        [CommandMethod("tt5")]
        public static void test25()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PromptEntityOptions opts = new PromptEntityOptions("\n请选择一个块:");
            opts.SetRejectMessage("只能选择块参照");
            opts.AddAllowedClass(typeof(BlockReference), false);
            PromptEntityResult res = ed.GetEntity(opts);
            if (res.Status != PromptStatus.OK)
                return;
            Database db = doc.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockReference bref = tr.GetObject(res.ObjectId, OpenMode.ForRead) as BlockReference;
                BlockTableRecord btr = tr.GetObject(bref.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                foreach (ObjectId id in btr)
                {

                    Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                    ed.WriteMessage($"\n{ent.GetType().Name}");
                    //Circle cir = tr.GetObject(id, OpenMode.ForRead) as Circle;
                    //if (cir != null)
                    //{
                    //    Point3d pt = cir.Center.TransformBy(bref.BlockTransform);
                    //    ed.DrawPoint(pt.Convert2d(new Plane()), 1, 1, 8);
                    //}
                }
            }
        }
        [CommandMethod("tt6")]
        public static void test26()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            EntityList itemEnts = new EntityList();
            Point3d insertPt;//插入文字的位置
            Transaction tr = db.TransactionManager.StartTransaction();
            using (tr)
            {
                // 获取PickFirst选择集
                PromptSelectionOptions promptSelOpt1 = new PromptSelectionOptions();
                promptSelOpt1.MessageForAdding = "\n请选择需要编号的车位";


                SelectionFilter frameFilter = new SelectionFilter(
                    new TypedValue[]
                    {
 
                        new TypedValue(0, "INSERT"),        
                   
                    });

                // PromptSelectionResult acSSPrompt = ed.SelectImplied();


                PromptSelectionResult acSSPrompt = ed.GetSelection(promptSelOpt1, frameFilter);
                SelectionSet acSSet;
                List<Entity> entityList = new List<Entity>();
                List<string> layerNameList = new List<string>();
                List<Extents3d> extentsList = new List<Extents3d>();
                if (acSSPrompt.Status != PromptStatus.OK)
                {  //提示用户交互
                    ed.WriteMessage("\n请先选择要编号的设备");
                    return;
                }
                else
                {

                    //编号局部变量
                    int number = ed.GetInteger("\n请输入编号起始数字").Value;
                    number--;
                    int height = ed.GetInteger("\n请输入编号文字的高度").Value;
                    string prefix = ed.GetString("\n请输入编号字母前缀（如A-）").StringResult;
                    prefix = prefix.ToUpper();
                    acSSet = acSSPrompt.Value;
                    foreach (ObjectId id in acSSet.GetObjectIds().Reverse())
                    {

                        BlockReference bref = tr.GetObject(id, OpenMode.ForRead) as BlockReference;
                        BlockTableRecord btr = tr.GetObject(bref.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                        
                        foreach (ObjectId objectId in btr)
                        {

                            Entity ent = tr.GetObject(objectId, OpenMode.ForRead) as Entity;
                            string name = ent.GetType().Name;
                            ed.WriteMessage(ent.GetType().Name);
                            //var name = ent.GetType().Name;
                            if (name== "Polyline")
                            {
                                entityList.Add(ent);
                            }
                          
                        }
                    }

                    //现在只需要Extent3D数据
                    foreach (Entity ent in entityList)
                    {
                        extentsList.Add(ent.GeometricExtents);
                    }

                    foreach (var ent in entityList)
                    {
                        Point3dCollection collection = new Point3dCollection();//存放矩形的四个点、
                        Point3dCollection collection1 = new Point3dCollection();//存放矩形的2个点、
                        Point3dCollection collection2 = new Point3dCollection();//存放矩形的2个点、
                        Point3dCollection collection3 = new Point3dCollection();//存放交点
                        Polyline pl1 = new Polyline();
                        Polyline pl2 = new Polyline();
                        number++;
                        var id = ent.ObjectId;

                        Polyline pl = (Polyline)tr.GetObject(id, OpenMode.ForRead);
                        var num = pl.NumberOfVertices;
                        int[] vertexIndex = new int[num];
                        for (int i = 0; i < num; i++)
                        {

                            vertexIndex[i] = i;
                        }
                        foreach (var item in vertexIndex)
                        {
                            collection.Add(pl.GetPoint3dAt(item));
                        }
                        collection1.Add(collection[0]);
                        collection1.Add(collection[2]);
                        collection2.Add(collection[1]);
                        collection2.Add(collection[3]);
                        DBHelper.CreatePolyline(pl1, collection1);
                        DBHelper.CreatePolyline(pl2, collection2);
                        pl1.IntersectWith(pl2, Intersect.OnBothOperands, new Plane(), collection3, IntPtr.Zero, IntPtr.Zero);
                        insertPt = collection3[0];
                        CreateItemNo(insertPt, itemEnts, prefix, number, height);

                        collection.Clear();
                        collection1.Clear();
                        collection2.Clear();
                        collection3.Clear();
                        pl1.Dispose();
                        pl2.Dispose();
                        //tr.AddNewlyCreatedDBObject(itemEnts,true);
                        
                    }
                }
                
                db.AddToCurrentSpace(itemEnts);
                ed.WriteMessage(itemEnts.GetType().Name);
               // db.AddToModelSpace(itemEnts);
                tr.Commit();//提交事务处理
            }
         
        
        }


        }
}
