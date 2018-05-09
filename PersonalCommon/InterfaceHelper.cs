using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PersonalCommon
{
    /// <summary>
    /// 接口调用帮助类
    /// </summary>
    public class InterfaceHelper
    {
        #region 请求接口
        /// <summary>
        /// 获取接口
        /// </summary>
        /// <param name="Url">接口地址</param>
        /// <param name="postDataStr">参数字符串</param>
        /// <param name="method">GET/POST</param>
        /// <param name="timeout">请求时间：毫秒(1000ms=1s)</param>
        /// <param name="cookie">cookie</param>
        /// <returns></returns>
        public static string queryRespose(string Url,string postDataStr, string method,int timeout,CookieContainer cookie)
        {
            string retString = "";
            try
            {
                if (method == "POST")
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = Encoding.UTF8.GetByteCount(postDataStr);
                    request.CookieContainer = cookie;
                    request.AllowAutoRedirect = false; //设置重定向禁用
                    request.Timeout = timeout;
                    Stream myRequestStream = request.GetRequestStream();
                    StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.GetEncoding("gb2312"));
                    myStreamWriter.Write(postDataStr);
                    myStreamWriter.Close();

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    response.Cookies = cookie.GetCookies(response.ResponseUri);
                    Stream myResponseStream = response.GetResponseStream();
                    StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                    retString = myStreamReader.ReadToEnd();
                    myStreamReader.Close();
                    myResponseStream.Close();

                    return retString;
                }
                else
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
                    request.Method = "GET";
                    request.ContentType = "text/html;charset=UTF-8";
                    request.AllowAutoRedirect = false; //设置重定向禁用
                    request.Timeout = timeout;

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Stream myResponseStream = response.GetResponseStream();
                    StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                    retString = myStreamReader.ReadToEnd();
                    myStreamReader.Close();
                    myResponseStream.Close();

                    return retString;
                }

            }
            catch (Exception ex) { retString = ex.Message; }
            return retString;
        }
        /// <summary>
        /// 获取接口
        /// </summary>
        /// <typeparam name="T">对象</typeparam>
        /// <param name="url">接口地址</param>
        /// <param name="postDataStr">参数字符串</param>
        /// <param name="method">GET/POST</param>
        /// <param name="timeout">请求时间：毫秒(1000ms=1s)</param>
        /// <param name="cookie">cookie</param>
        /// <returns></returns>
        public static T queryRespose<T>(string url,string postDataStr, string method, int timeout,CookieContainer cookie) where T : class
        {
            return JsonHelper.DeserializeJsonToObject<T>(queryRespose(url, postDataStr, method, timeout,cookie));
        }
        #endregion

        #region 服务器下载图片
        /// <summary>
        /// 下载图片
        /// </summary>
        /// <param name="picUrl">图片Http地址</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="timeOut">Request最大请求时间，如果为-1则无限制,单位：毫秒</param>
        /// <returns></returns>
        public static bool DownloadPicture(string picUrl, string savePath, int timeOut)
        {
            bool value = false;
            WebResponse response = null;
            Stream stream = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(picUrl);
                if (timeOut != -1)
                    request.Timeout = timeOut;
                response = request.GetResponse();
                stream = response.GetResponseStream();
                if (!response.ContentType.ToLower().StartsWith("text/"))
                    value = SaveBinaryFile(response, savePath);
            }
            finally
            {
                if (stream != null) stream.Close();
                if (response != null) response.Close();
            }
            return value;
        }
        private static bool SaveBinaryFile(WebResponse response, string savePath)
        {
            bool value = false;
            byte[] buffer = new byte[1024];
            Stream outStream = null;
            Stream inStream = null;
            try
            {
                if (System.IO.File.Exists(savePath))
                    System.IO.File.Delete(savePath);
                outStream = System.IO.File.Create(savePath);
                inStream = response.GetResponseStream();
                int l;
                do
                {
                    l = inStream.Read(buffer, 0, buffer.Length);
                    if (l > 0) outStream.Write(buffer, 0, l);
                } while (l > 0);
                value = true;
            }
            finally
            {
                if (outStream != null) outStream.Close();
                if (inStream != null) inStream.Close();
            }
            return value;
        }
        #endregion
        
    }
}
