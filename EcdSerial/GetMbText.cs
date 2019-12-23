using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EcdSerial
{
   public  class GetMbText
    {
        [CommandMethod("EcdBH1")]
        public void GetText()
        {

            Document doc = Application.DocumentManager.MdiActiveDocument;//
            Database db = doc.Database;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            int index = 0;//编号起始位置
            string indexText="";

            List<String> lists = new List<string>();//存放DBText文字
            List<String> listText = new List<string>();//存放DBText文字
            List<String> newlistText = new List<string>();//存放DBText文字
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = tr.GetObject(blockTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                #region 获取参考文字
                PromptSelectionOptions promptSelOpt1 = new PromptSelectionOptions();
                promptSelOpt1.MessageForAdding = "\n请选择编号起始参考量(如A-001)";
                SelectionFilter frameFilter1 = new SelectionFilter(
                         new TypedValue[]
                         { new TypedValue(0, "TEXT"),
                        });
                PromptSelectionResult selectedFrameResult1 = ed.GetSelection(promptSelOpt1, frameFilter1);
                if (selectedFrameResult1.Status != PromptStatus.OK) return;

                List<ObjectId> resultObjectIds1 = new List<ObjectId>(selectedFrameResult1.Value.GetObjectIds());//获取所有选中实体的ID
                try
                {
                    foreach (var item in resultObjectIds1)
                    {
                        DBText entity = (DBText)tr.GetObject(item, OpenMode.ForRead);
                        index = Convert.ToInt32(entity.TextString.Substring(entity.TextString.IndexOf("-") + 1));
                        lists.Add(entity.TextString);
                        indexText = entity.TextString.Substring(0, entity.TextString.IndexOf("-"));//截取'-'前面的字符
                    }

                }
                catch (System.Exception)
                {

                    ed.WriteMessage("请选择有效的参考量！");
                }    
              
                #endregion
                if (indexText == "")
                {
                   // ed.WriteMessage("请选择有效的参考量！");
                    return;
                }
                else
                {

                    PromptSelectionOptions promptSelOpt = new PromptSelectionOptions();
                    promptSelOpt.MessageForAdding = "\n请选择车位";
                    SelectionFilter frameFilter = new SelectionFilter(
                             new TypedValue[]
                             { new TypedValue(0, "TEXT"),
                            });
                    PromptSelectionResult selectedFrameResult = ed.GetSelection(promptSelOpt, frameFilter);
                    if (selectedFrameResult.Status != PromptStatus.OK) return;

                    List<ObjectId> resultObjectIds = new List<ObjectId>(selectedFrameResult.Value.GetObjectIds());//获取所有选中实体的ID


                    foreach (var item in resultObjectIds)
                    {
                        DBText entity = (DBText)tr.GetObject(item, OpenMode.ForRead);
                        listText.Add(entity.TextString);
                    }
                    // ed.WriteMessage("-------");



                    //排序listText[0]后面的位置
                    //ed.WriteMessage(indexText);
                  
                    var list = from m in listText
                               orderby m
                               select m;
                               
                    var Listnew = list.ToArray().Reverse();
                    listText.Reverse();
                    //foreach (var item in Listnew)
                    //{
                    //    lists.Add(item);

                    //}

                    for (int i = 0; i < listText.Count(); i++)
                    {                      
                        if (index < 10)
                        {

                            newlistText.Add(indexText + "-" + 0 + 0 + index);
                        }
                        else if (index < 100)
                        {

                            newlistText.Add(indexText + "-" + 0 + index);
                        }
                        else
                        {
                            newlistText.Add(indexText + "-" + index);
                        }
                        index = index + 1;
                    }
                    //ed.WriteMessage("---------------\n");

                    foreach (var item in resultObjectIds)
                    {
                        DBText entity = (DBText)tr.GetObject(item, OpenMode.ForWrite);
                        for (int i = 0; i < newlistText.Count; i++)
                        {
                            if (entity.TextString.Equals(listText[i]))
                            {
                                entity.TextString = newlistText[i];
                                break;
                            }
                        }
                        //modelSpace.AppendEntity(entity);
                        //tr.AddNewlyCreatedDBObject(entity, true);

                    }
                    listText.Clear();
                    newlistText.Clear();
                    ed.Regen();
                    tr.Commit();
                }
            }
        }
    }
}
