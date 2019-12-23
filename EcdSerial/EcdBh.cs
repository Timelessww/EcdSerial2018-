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

namespace EcdSerial
{
  public   class EcdBh
    {

        /// <summary> 
        /// 自动编号 
        /// </summary> 
        /// <param name="insertPoint">插入点</param> 
        public static void CreateItemNo(Point3d insertPoint, EntityList list, string prefix, int number,int height)
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

      
        [CommandMethod("EcdBH", CommandFlags.UsePickSet)]
        public void BH()
        {

            // 获取当前文档获
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Transaction tr = db.TransactionManager.StartTransaction();
            EntityList itemEnts = new EntityList();
            Point3d insertPt;//插入文字的位置
            using (tr)
            {
                // 获取PickFirst选择集
                PromptSelectionOptions promptSelOpt1 = new PromptSelectionOptions();
                promptSelOpt1.MessageForAdding = "\n请选择需要编号的车位";


                SelectionFilter frameFilter = new SelectionFilter(
                    new TypedValue[]
                    {
 
                 
                      //new TypedValue((int)DxfCode.Operator, "<or"),
                        new TypedValue(0, "LWPOLYLINE"),

                   // new TypedValue(0, "INSERT"),
                   //new TypedValue((int)DxfCode.Operator, "or>"),
                   
                    });

               // PromptSelectionResult acSSPrompt = ed.SelectImplied();


                PromptSelectionResult acSSPrompt = ed.GetSelection(promptSelOpt1, frameFilter);
                SelectionSet acSSet;
                List<Entity> entityList = new List<Entity>();
                List<string> layerNameList = new List<string>();
                List<Extents3d> extentsList = new List<Extents3d>();
                String layerName;
                
                if (acSSPrompt.Status != PromptStatus.OK)
                {  //提示用户交互
                    ed.WriteMessage("\n请先选择要编号的设备");
                    return;
                }
                else {


                    //编号局部变量
                    int number = ed.GetInteger("\n请输入编号起始数字").Value;
                    number--;
                    int height = ed.GetInteger("\n请输入编号文字的高度").Value;
                    string prefix = ed.GetString("\n请输入编号字母前缀（如A-）").StringResult;
                    prefix = prefix.ToUpper();
                    acSSet = acSSPrompt.Value;
                    foreach (ObjectId id in acSSet.GetObjectIds().Reverse())
                    {
                        Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
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
                   // pt1 = Point("请拾取自动编号的位置");
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
               
                    foreach (var ent in entityList)
                    {
                        Point3dCollection collection = new Point3dCollection();//存放矩形的四个点、
                        Point3dCollection collection1 = new Point3dCollection();//存放矩形的2个点、
                        Point3dCollection collection2 = new Point3dCollection();//存放矩形的2个点、
                        Point3dCollection collection3 = new Point3dCollection();//存放交点
                        Polyline pl1 = new Polyline();
                        Polyline pl2 = new Polyline();
                        number++;
                        var id= ent.ObjectId;
                       
                        Polyline pl = (Polyline)tr.GetObject(id, OpenMode.ForRead);
                        var num= pl.NumberOfVertices;
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
                        CreateItemNo(insertPt, itemEnts, prefix, number,height);

                        collection.Clear();
                        collection1.Clear();
                        collection2.Clear();
                        collection3.Clear();
                        pl1.Dispose();
                        pl2.Dispose();
                    }

                }

                db.AddToCurrentSpace(itemEnts);
                tr.Commit();//提交事务处理
            }
        }

    }


    public static class DBHelper
    {
        /// <summary>
        /// 通过三维点集合创建多段线
        /// </summary>
        /// <param name="pline">多段线对象</param>
        /// <param name="pts">多段线的顶点</param>
        public static void CreatePolyline(this Polyline pline, Point3dCollection pts)
        {
            for (int i = 0; i < pts.Count; i++)
            {
                //添加多段线的顶点
                pline.AddVertexAt(i, new Point2d(pts[i].X, pts[i].Y), 0, 0, 0);
            }
        }
    }
}
