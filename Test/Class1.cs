using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using DotNetARX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
   public  class Class1
    {
        [CommandMethod("GetBlockName")]
        public void GetBlockName()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            PromptEntityOptions options = new PromptEntityOptions("\nSelect block reference");
            options.SetRejectMessage("\nSelect only block reference");
            options.AddAllowedClass(typeof(BlockReference), false);
            PromptEntityResult ret = doc.Editor.GetEntity(options);
            if (ret.Status != PromptStatus.OK)
                return;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockReference blkRef = tr.GetObject(ret.ObjectId,
                    OpenMode.ForRead) as BlockReference;
                ObjectId blkRecdId = ObjectId.Null;
                if (blkRef.IsDynamicBlock)
                {
                    blkRecdId = blkRef.DynamicBlockTableRecord;
                }
                else
                {
                    blkRecdId = blkRef.BlockTableRecord;
                }

                BlockTableRecord block = tr.GetObject(blkRecdId,
                    OpenMode.ForRead) as BlockTableRecord;

                doc.Editor.WriteMessage(block.Name + "\n");
            }
        }

        /// <summary> 
        /// 自动编号 
        /// </summary> 
        /// <param name="insertPoint">插入点</param> 
        public static void CreateItemNo(Point3d insertPoint, EntityList list, string prefix, int number)
        {
            // Put your command code here
            Plane plane = new Plane();
            Ellipse ellipse = new Ellipse(insertPoint, plane.Normal, new Vector3d(200, 0, 0), 0.54, 0, GeTools.DegreeToRadian(360));
            DBText text = new DBText();
            text.Height = 120;
            //单行文字的插入点有两个，不同的对齐方式，需要设置的点不同 
            text.HorizontalMode = TextHorizontalMode.TextCenter;
            text.VerticalMode = TextVerticalMode.TextVerticalMid;
            text.Position = insertPoint;
            text.AlignmentPoint = insertPoint;

            if (number < 10)
            {
                text.TextString = prefix + "0" + number.ToString();
            }
            else
            {
                text.TextString = prefix + number.ToString();
            }
            list.Add(ellipse);
            list.Add(text);
        }

        /// <summary> 
        /// 泛型结构体5
        /// </summary> 
        public struct Struct5<T1, T2, T3, T4, T5>
        {
            public T1 Value1;
            public T2 Value2;
            public T3 Value3;
            public T4 Value4;
            public T5 Value5;
        }




        [CommandMethod("demo", CommandFlags.UsePickSet)]
        public static void AutoNumbering()
        {
            // 获取当前文档获
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Transaction trans = db.TransactionManager.StartTransaction();
            using (trans)
            {
                // 获取PickFirst选择集
                PromptSelectionResult acSSPrompt;
                acSSPrompt = ed.SelectImplied();
                SelectionSet acSSet;
                List<Entity> entityList = new List<Entity>();
                List<string> layerNameList = new List<string>();
                List<Extents3d> extentsList = new List<Extents3d>();
                String layerName;
                Point3d pt1;
                Point3d lineStart;
                int i4 = 0;
                //四点
                double minX, minY, maxX, maxY;
                EntityList itemEnts = new EntityList();
                // 如果提示状态OK，说明启动命令前选择了对象;
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    //编号局部变量
                    int number = ed.GetInteger("\n请输入编号起始数字").Value;
                    number--;
                    string prefix = ed.GetString("\n请输入编号字母前缀（如AA）").StringResult;
                    prefix = prefix.ToUpper();
                    acSSet = acSSPrompt.Value;
                    foreach (ObjectId id in acSSet.GetObjectIds())
                    {
                        Entity ent = trans.GetObject(id, OpenMode.ForRead) as Entity;
                        entityList.Add(ent);
                        layerNameList.Add(ent.Layer);
                    }
                    //重复次数最多的元素
                    var query = (from string n in layerNameList
                                 group n by n into g
                                 where g.Count() > 0
                                 orderby g.Count() descending
                                 select new
                                 {
                                     LayerName = g.Key,
                                     Count = g.Count()
                                 }).First();
                    layerName = query.LayerName;
                    pt1 = Point("请拾取自动编号的位置");
                    //倒序删除List中不需要的实体
                    for (int i = entityList.Count - 1; i >= 0; i--)
                    {
                        if (entityList[i].Layer != layerName)
                            entityList.Remove(entityList[i]);
                    }
                    //现在只需要Extent3D数据
                    foreach (Entity ent in entityList)
                    {
                        extentsList.Add(ent.GeometricExtents);
                    }
                    //判断拾取点在哪个位置方向
                    minX = extentsList[0].MinPoint.X;
                    minY = extentsList[0].MinPoint.Y;
                    maxX = extentsList[0].MaxPoint.X;
                    maxY = extentsList[0].MaxPoint.Y;
                    foreach (Extents3d ent in extentsList)
                    {
                        if (ent.MinPoint.X < minX)
                        {
                            minX = ent.MinPoint.X;
                        }
                        else { }
                        if (ent.MinPoint.Y < minY)
                        {
                            minY = ent.MinPoint.Y;
                        }
                        else { }
                        if (ent.MaxPoint.X > maxX)
                        {
                            maxX = ent.MaxPoint.X;
                        }
                        else { }
                        if (ent.MaxPoint.Y > maxY)
                        {
                            maxY = ent.MaxPoint.Y;
                        }
                        else { }
                    }
                    if (pt1.X > minX && pt1.X < maxX)
                    {
                        //根据插入点的相对位置来决定i4正负值
                        i4 = pt1.Y > maxY ? -1 : 1;
                        #region
                        //以拾取点的Y坐标不变，以编号X方向尺寸来自动排列坐标。
                        //只需要在X方向上指定坐标
                        //泛型结构体
                        Struct5<double, double, double, Extents3d, double> struct5;
                        List<Struct5<double, double, double, Extents3d, double>> struct5List = new List<Struct5<double, double, double, Extents3d, double>>();
                        foreach (Extents3d extents in extentsList)
                        {
                            struct5.Value1 = extents.MinPoint.X;
                            struct5.Value2 = extents.MaxPoint.X;
                            struct5.Value3 = extents.MaxPoint.X - extents.MinPoint.X;
                            struct5.Value4 = extents;
                            struct5.Value5 = (extents.MinPoint.X + extents.MaxPoint.X) / 2;
                            struct5List.Add(struct5);
                        }
                        struct5List = struct5List.OrderBy(p => p.Value5).ToList<Struct5<double, double, double, Extents3d, double>>();
                        for (int i = 0; i < struct5List.Count - 1; i++)
                        {
                            int b = i + 1;
                            if (struct5List[b].Value5 - struct5List[i].Value5 < 400.0)
                            {
                                struct5 = struct5List[b];
                                struct5.Value5 = struct5List[i].Value5 + 420.0;
                                if (struct5.Value5 > struct5List[b].Value2 - 200.0)
                                {
                                    struct5.Value5 = struct5List[b].Value2 - 200.0;
                                }
                                struct5List[b] = struct5;
                            }
                        }
                        struct5List = struct5List.OrderBy(p => p.Value5).ToList<Struct5<double, double, double, Extents3d, double>>();
                        foreach (Struct5<double, double, double, Extents3d, double> s5 in struct5List)
                        {
                            number++;
                            Point3d insertPt = new Point3d(s5.Value5, pt1.Y, 0.0);
                            CreateItemNo(insertPt, itemEnts, prefix, number);
                            lineStart = new Point3d(insertPt.X, insertPt.Y + 108 * i4, 0);
                            Line line = new Line(lineStart, new Point3d(lineStart.X, (s5.Value4.MaxPoint.Y + s5.Value4.MinPoint.Y) / 2 - 60.0 * i4, 0));
                            itemEnts.Add(line);
                        }
                        #endregion
                    }

                    else
                    {
                        //根据插入点的相对位置来决定i4正负值
                        i4 = pt1.X > maxX ? -1 : 1;
                        #region
                        //以拾取点的X坐标不变，以编号Y方向尺寸来自动排列坐标。
                        //只需要在Y方向上指定坐标
                        //泛型结构体
                        Struct5<double, double, double, Extents3d, double> struct5;
                        List<Struct5<double, double, double, Extents3d, double>> struct5List = new List<Struct5<double, double, double, Extents3d, double>>();
                        foreach (Extents3d extents in extentsList)
                        {
                            struct5.Value1 = extents.MinPoint.Y;
                            struct5.Value2 = extents.MaxPoint.Y;
                            struct5.Value3 = extents.MaxPoint.Y - extents.MinPoint.Y;
                            struct5.Value4 = extents;
                            struct5.Value5 = (extents.MinPoint.Y + extents.MaxPoint.Y) / 2;
                            struct5List.Add(struct5);
                        }
                        struct5List = struct5List.OrderBy(p => p.Value5).ToList<Struct5<double, double, double, Extents3d, double>>();
                        for (int i = 0; i < struct5List.Count - 1; i++)
                        {
                            int b = i + 1;
                            if (struct5List[b].Value5 - struct5List[i].Value5 < 200.0)
                            {
                                struct5 = struct5List[b];
                                struct5.Value5 = struct5List[i].Value5 + 220.0;
                                if (struct5.Value5 > struct5List[b].Value2 - 200.0)
                                {
                                    struct5.Value5 = struct5List[b].Value2 - 200.0;
                                }
                                struct5List[b] = struct5;
                            }
                        }
                        struct5List = struct5List.OrderBy(p => p.Value5).ToList<Struct5<double, double, double, Extents3d, double>>();
                        foreach (Struct5<double, double, double, Extents3d, double> s5 in struct5List)
                        {
                            number++;
                            Point3d insertPt = new Point3d(pt1.X, s5.Value5, 0.0);
                            CreateItemNo(insertPt, itemEnts, prefix, number);
                            lineStart = new Point3d(insertPt.X + 200 * i4, insertPt.Y, 0);
                            Line line = new Line(lineStart, new Point3d((s5.Value4.MaxPoint.X + s5.Value4.MinPoint.X) / 2 - 60.0 * i4, lineStart.Y, 0));
                            itemEnts.Add(line);
                        }
                        #endregion
                    }
                }
                // 请求采用先选择后执行命令的方式
                else
                {
                    //提示用户交互
                    ed.WriteMessage("\n请先选择要编号的设备");
                    return;
                }
                db.AddToCurrentSpace(itemEnts);
                trans.Commit();//提交事务处理
            }
        }

        private static Point3d Point(string v)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            while (true)
            {
                Transaction trans = db.TransactionManager.StartTransaction();
                using (trans)
                {
                    BlockTable blockTbl = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord modelSpace = trans.GetObject(blockTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    MText mt = new MText();

                    PromptPointResult ppr = ed.GetPoint(v);
                    Point3d location;
                    if (ppr.Status != PromptStatus.OK)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        location = ppr.Value;
                    }
                    return location;

                }
            }
        }
    }
}
