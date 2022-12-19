using System;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Web;
using Windows.Data.Json;

namespace TransAPICSharpDemo
{
    public class fanyi
    {
        public static string GetMeaning(string word)
        {
            try
            {
                // 原文
                string q = word;
                // 源语言
                string from = "en";
                // 目标语言
                string to = "zh";
                // 改成您的APP ID
                string appId = "20221213001496171";
                Random rd = new Random();
                string salt = rd.Next(100000).ToString();
                // 改成您的密钥
                string secretKey = "lQoWT41gnmzFzOqhnW2T";
                string sign = EncryptString(appId + q + salt + secretKey);
                string url = "http://api.fanyi.baidu.com/api/trans/vip/translate?";
                url += "q=" + HttpUtility.UrlEncode(q);
                url += "&from=" + from;
                url += "&to=" + to;
                url += "&appid=" + appId;
                url += "&salt=" + salt;
                url += "&sign=" + sign;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.ContentType = "text/html;charset=UTF-8";
                request.UserAgent = null;
                request.Timeout = 3000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                JsonObject jsonObject = JsonObject.Parse(retString);
                JsonArray ja = jsonObject.GetObject().GetNamedArray("trans_result");
                foreach(var item in ja)
                {
                    retString = item.GetObject().GetNamedString("dst").ToString();
                }
                return retString;

            }
            catch
            {
                return "查询失败";
            }
            
        }
        // 计算MD5值
        public static string EncryptString(string str)
        {
            MD5 md5 = MD5.Create();
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }
            // 返回加密的字符串
            return sb.ToString();
        }
    }
}
