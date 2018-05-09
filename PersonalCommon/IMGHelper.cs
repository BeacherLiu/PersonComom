using System;
using System.IO;
using System.Drawing;

namespace PersonalCommon
{
    /// <summary>
    /// 图片处理类
    /// </summary>
    public class IMGHelper
    {
        /// <summary>
        /// //图片转为base64编码的字符串
        /// </summary>
        /// <param name="Imagefilename">图片路径及文件名</param>
        /// <returns></returns>
        private string ImgToBase64String(string Imagefilename)
        {
            try
            {
                if (!System.IO.File.Exists(Imagefilename))
                {
                    return "";
                }

                var bmp = new System.Drawing.Bitmap(Imagefilename);
                var ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                var arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                return Convert.ToBase64String(arr);
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        /// <summary>
        /// Base64String转换图片
        /// </summary>
        /// <param name="filePath">文件保存路径,到图片文件夹</param>
        /// <param name="base64string">图片BASE64字符串</param>
        /// <param name="filename">图片名称</param>
        /// <param name="extension">图片后缀名 jpg</param>
        /// <param name="filepath"></param>
        /// <returns></returns>
        private string Base64StringToImage(string filePath,string base64string, string filename, string extension, out string filepath)
        {
            try
            {
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                var saveName = filename + "." + extension;
                filepath = filePath + saveName;
                
                var dummyData = base64string.Trim();
                if (dummyData.Length % 4 > 0)
                {
                    dummyData = dummyData.PadRight(dummyData.Length + 4 - dummyData.Length % 4, '=');
                }
                var arr = Convert.FromBase64String(dummyData);
                System.IO.File.WriteAllBytes(filepath, arr);
                return saveName;
            }
            catch (Exception ex)
            {
                throw new Exception("Base64StringToImage 转换失败Exception：");
            }
        }
    }
}
