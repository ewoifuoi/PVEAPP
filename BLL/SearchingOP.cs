using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml.Controls;
using PVEAPP.DAL;
using PVEAPP.Models;
using PVEAPP.Services;
using TransAPICSharpDemo;
using Windows.Storage;

namespace PVEAPP.BLL;
public class SearchingOP
{

    public bool WordExistQuery(string word)
    {
        var res = "";

        var dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "pvedb.db");
        using (var db =
           new SqliteConnection($"Filename={dbpath}"))
        {
            db.Open();

            var selectCommand = new SqliteCommand
                ("select Words from Meanings where Words='" + word + "';", db);

            var query = selectCommand.ExecuteReader();
            while (query.Read()) res = query.GetString(0);

        }
        if (res == "")
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public WordModel GetWord(string text)
    {
        WordModel model = new WordModel();
        model.Parse(DataAccess.Query("select * from Meanings where Words='"+ text +"';"));

        model.date = TimeZoneInfo.ConvertTimeFromUtc(DataAccess.GetDate(text), TimeZoneInfo.Local);
        try
        {
            DataAccess.AddData("update Search_Cache set Time=datetime() where Words='" + text + "';");
        }
        catch
        {

        }
        return model;
    }

    public string GetMeaning(string text, string op)
    {
        
        if (!DataAccess.GetWordHistory(text))
        {
            string t1 = youdao.GetMeaning(text);
            string t2 = fanyi.GetMeaning(text);
            DataAccess.Query("insert into Search_Cache (Words, youdao, baidu, Time) values ('"+text+"','"+t1+"','"+t2+"',datetime());");
        }
        else if(op == "baidu")
        {
            DataAccess.AddData("update Search_Cache set Time=datetime() where Words='"+text+"';");
        }
        
        if (op == "baidu") return DataAccess.GetWordHistory_baidu(text);
        else if (op == "youdao") return DataAccess.GetWordHistory_youdao(text);
        else return "error";
        
        

    }

    public void SaveWord(WordModel model, string dict)
    {
        if (WordExistQuery(model.word))
        {
            string temp = "update Meanings set (Meaning,Notes,Rate,LastEdited)=(";
            temp += "'" + model.meaning + "',";
            temp += "'" + model.notes + "',";
            temp += "'" + model.rate + "',";
            temp += "datetime()) ";
            temp += " where Words='"+ model.word +"';";
            DataAccess.AddData(temp);
            AddEp(model.word, model.ep);
        }
        else
        {
            string temp = "insert into Meanings (Words, Meaning, Notes,Rate, createddate,lastedited) values (";
            temp += "'" + model.word + "',";
            temp += "'" + model.meaning + "',";
            temp += "'" + model.notes + "',";
            temp += "'" + model.rate + "',";
            temp += "datetime(),";
            temp += "datetime());";
            DataAccess.AddData(temp);
        }
        try
        {
            DataAccess.AddData("insert into word_dict (wid, dict) VALUES (\r\n(select wid from Meanings where Words='"+model.word+"'),(select dict from Dictionary where Name='"+dict+"'));");
        }
        catch
        {

        }
        
    }

    internal string GetSearchTimes(string word)
    {
        List<List<string>> res = DataAccess.Query("select count(*)\r\nfrom Search_History\r\nwhere julianday(datetime()) - julianday(Search_time) <= 30 and hid in (\r\n    select hid from Search_Cache\r\n    where Words='"+word+"');");
        return res[0][0];
    }

    internal void AddEp(string word, string text)
    {
        string sql = "insert into Eps (wid, Sentence, CreatedTime) VALUES ((\r\n    SELECT wid from Meanings where Words='" + word + "'\r\n    ),'" + text + "', datetime());";
        DataAccess.AddData(sql);
    }

    internal List<List<string>> GetEp(string word)
    {
        return DataAccess.Query("select Sentence, CreatedTime from Eps\r\nwhere wid in(\r\n    select wid from Meanings\r\n    where Words='" + word+"'\r\n    )\r\norder by CreatedTime\r\ndesc ;");
    }

    internal void GetDict(ref ComboBox dict)
    {
        List<List<string>> res = DataAccess.Query("select Name from Dictionary;");
        for(int i = 0; i < res.Count; i++)
        {
            dict.Items.Add(res[i][0]);
        }
    }

    internal string GetDict(string word)
    {
        string s="";
        List<List<string>> res = DataAccess.Query("select Name from Dictionary where Dict in (\r\n    select dict from word_dict where wid in (\r\n        select wid from Meanings where Words='"+word+"'\r\n        )\r\n    );");
        for (int i = 0; i < res.Count; i++)
        {
            s += "(" +res[i][0] + ")";
            s += " ";
        }
        return s;
    }

    internal void GetDict(ref ListBox dict_list)
    {
        List<List<string>> res = DataAccess.Query("select Name from Dictionary;");
        for (int i = 0; i < res.Count; i++)
        {
            dict_list.Items.Add(res[i][0]);
        }
    }

    internal void DeleteDict(string v)
    {
        DataAccess.AddData("delete from Dictionary where Name='"+v+"';");
    }
}
