using System;
using System.Collections.Generic;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;
using MicroPPt=Microsoft.Office.Interop.PowerPoint;
using System.Linq;

namespace UrbanX.Application.Office
{
    public class PowerPointHelper
    {
        
        public static string ReplacePowerPoint(string filePath, string savedFilePath, Dictionary<string,string>keyValues)
        {
            string path = "";
            MicroPPt.Application pptApp;
            Presentations presentations = null;
            Presentation presentation = null;
            pptApp = new MicroPPt.Application();
            try
            {
                presentations = pptApp.Presentations;
                //打开ppt
                presentation = presentations.Open(filePath, MsoTriState.msoFalse, MsoTriState.msoFalse, MsoTriState.msoFalse);
                //替换模板ppt中的文本
                foreach (MicroPPt.Slide slide in presentation.Slides)
                {
                    foreach (MicroPPt.Shape shape in slide.Shapes)
                    {
                        ReplaceShapeText(shape,keyValues);
                    }
                }

                //保持文件
                //path=AppDomain.CurrentDomain.BaseDirectory+"PPT/test-"+ DateTime.Now.ToString("yyyyMMddhhmmss") + ".pptx";
                path= savedFilePath + "/"+DateTime.Now.ToString("yyyyMMddhhmmss") + ".pptx";
                presentation.SaveAs(path);
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
            finally
            {
                try
                {
                    presentation.Close();
                    pptApp.Quit();
                }
                catch (Exception) { }
            }
            return path;
        }

        /// <summary>
        /// 替换PowerPoint TextBox的内容
        /// </summary>
        public static void ReplaceShapeText(MicroPPt.Shape shape, Dictionary<string, string> dicKeyWordList)
        {
            if (dicKeyWordList !=null && dicKeyWordList.Count>0)
            {
                if (shape.Type==MsoShapeType.msoGroup)
                {
                    foreach (MicroPPt.Shape sh in shape.GroupItems)
                    {
                        ReplaceShapeText(sh, dicKeyWordList);
                    }
                }
                if (shape.HasTextFrame != Microsoft.Office.Core.MsoTriState.msoTrue)
                {
                    return;
                }
                foreach (string strKeyWord in dicKeyWordList.Keys)
                {
                    TextRange textRange = shape.TextFrame.TextRange.Find(strKeyWord, 0, MsoTriState.msoTriStateMixed, MsoTriState.msoFalse);
                    if (textRange !=null)
                    {
                        shape.TextFrame.TextRange.Text = shape.TextFrame.TextRange.Text.Replace(strKeyWord, dicKeyWordList[strKeyWord]);
                    }
                }
            }
        }
    }
}
