﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;


namespace PVEAPP.Services;
public class youdao
{
    public static string GetMeaning(string word)
    {
        try
        {
            Dictionary<String, String> dic = new Dictionary<String, String>();
            string url = "https://openapi.youdao.com/api";
            string q = word;
            string appKey = "03cf644e0f11a544";
            string appSecret = "tQ65lWlIKfYQpcXVSIE2HakjLDgRyT51";
            string salt = DateTime.Now.Millisecond.ToString();
            dic.Add("from", "en");
            dic.Add("to", "zh");
            dic.Add("signType", "v3");
            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            long millis = (long)ts.TotalMilliseconds;
            string curtime = Convert.ToString(millis / 1000);
            dic.Add("curtime", curtime);
            string signStr = appKey + Truncate(q) + salt + curtime + appSecret; ;
            string sign = ComputeHash(signStr, new SHA256CryptoServiceProvider());
            dic.Add("q", System.Web.HttpUtility.UrlEncode(q));
            dic.Add("appKey", appKey);
            dic.Add("salt", salt);
            dic.Add("sign", sign);
            dic.Add("vocabId", "您的用户词表ID");
            return Post(url, dic);
            
        }
        catch
        {
            return "查询失败";
        }
    }

    protected static string ComputeHash(string input, HashAlgorithm algorithm)
    {
        Byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        Byte[] hashedBytes = algorithm.ComputeHash(inputBytes);
        return BitConverter.ToString(hashedBytes).Replace("-", "");
    }

    protected static string Post(string url, Dictionary<String, String> dic)
    {
        string result = "";
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
        req.Method = "POST";
        req.ContentType = "application/x-www-form-urlencoded";
        StringBuilder builder = new StringBuilder();
        int i = 0;
        foreach (var item in dic)
        {
            if (i > 0)
                builder.Append("&");
            builder.AppendFormat("{0}={1}", item.Key, item.Value);
            i++;
        }
        byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
        req.ContentLength = data.Length;
        using (Stream reqStream = req.GetRequestStream())
        {
            reqStream.Write(data, 0, data.Length);
            reqStream.Close();
        }
        HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
        if (resp.ContentType.ToLower().Equals("audio/mp3"))
        {
            SaveBinaryFile(resp, "合成的音频存储路径");
            return "合成的音频存储路径";
        }
        else
        {
            Stream stream = resp.GetResponseStream();
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }
    }

    protected static string Truncate(string q)
    {
        if (q == null)
        {
            return null;
        }
        int len = q.Length;
        return len <= 20 ? q : (q.Substring(0, 10) + len + q.Substring(len - 10, 10));
    }


    private static bool SaveBinaryFile(WebResponse response, string FileName)
    {
        string FilePath = FileName + DateTime.Now.Millisecond.ToString() + ".mp3";
        bool Value = true;
        byte[] buffer = new byte[1024];

        try
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
            Stream outStream = System.IO.File.Create(FilePath);
            Stream inStream = response.GetResponseStream();

            int l;
            do
            {
                l = inStream.Read(buffer, 0, buffer.Length);
                if (l > 0)
                    outStream.Write(buffer, 0, l);
            }
            while (l > 0);

            outStream.Close();
            inStream.Close();
        }
        catch
        {
            Value = false;
        }
        return Value;
    }
}
