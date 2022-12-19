using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using Windows.Storage;
using System.IO;
using Windows.Media.Protection.PlayReady;

namespace PVEAPP.DAL;
public class DataAccess
{
    public async static void InitializeDatabase()
    {
        await ApplicationData.Current.LocalFolder.CreateFileAsync("pvedb.db", CreationCollisionOption.OpenIfExists);
        string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "pvedb.db");
        using (SqliteConnection db =
           new SqliteConnection($"Filename={dbpath}"))
        {
            db.Open();

            // 建立用户查询历史表
            String tableCommand =
                "CREATE TABLE IF NOT EXISTS Meanings\r\n(\r\n    wid          integer primary key autoincrement not null,\r\n    Words        VARCHAR(255) unique               NOT NULL,\r\n    Meaning      NVARCHAR(100)                     NOT NULL,\r\n    Notes        NVARCHAR(2048) default '尚未添加笔记'   NULL,\r\n    Rate         integer        default 0,\r\n    Picture      BLOB                              NULL,\r\n    CreatedDate  DATETIME                          NOT NULL,\r\n    LastEdited   Datetime                          not null,\r\n    LastReviewed Datetime\r\n);" +
                "create table if not exists Search_Cache\r\n(\r\n    hid   integer primary key autoincrement not null,\r\n    Words VARCHAR(255)                      NOT NULL,\r\n    youdao VARCHAR(2048) not null ,\r\n    baidu VARChar(2048) not null,\r\n    Time  datetime                          not null\r\n);" +
                "create table if not exists Search_History\r\n(\r\n  search_id integer primary key autoincrement not null ,\r\n  hid integer not null ,\r\n  Search_time datetime not null\r\n);" +
                "create table if not exists Dictionary\r\n(\r\n    Dict             integer primary key autoincrement not null,\r\n    Name             VARCHAR(255) unique               NOT NULL,\r\n    LastReviewedTime datetime,\r\n    CreatedTime      datetime                          not null\r\n);" +
                "CREATE TABLE If not exists Eps\r\n(\r\n    sid         integer primary key autoincrement not null,\r\n    wid         integer                           not null,\r\n    Sentence    NVARCHAR(1024)                    not null,\r\n    CreatedTime Datetime                          not null,\r\n    foreign key (wid) references Meanings (wid)\r\n);" +
                "create trigger if not exists history_trigger2 after update on Search_Cache for each row\r\nbegin\r\n    insert into Search_History (hid, Search_time) VALUES (new.hid, new.Time);\r\nend;" +
                "create trigger if not exists Del_dict_trigger before delete on Dictionary for each row\r\nbegin\r\n    delete from word_dict\r\n    where dict in(\r\n        select dict from Dictionary where dict=old.Dict\r\n        );\r\nend;" +
                "create table if not exists Review_History\r\n(\r\n    rid        integer primary key autoincrement not null,\r\n    wid        integer                           not null,\r\n    wrong_time integer                           not null,\r\n    ReviewTime datetime                          not null,\r\n    LastReviewTime datetime not null,\r\n    foreign key (wid) references Meanings (wid)\r\n);" +
                "create table if not exists word_dict\r\n(\r\n    word_dict_id integer primary key autoincrement not null,\r\n    wid          integer             not null,\r\n    dict         integer             not null,\r\n    foreign key (wid) references Meanings (wid),\r\n    foreign key (dict) references Dictionary (dict),\r\n    unique (wid, dict)\r\n);";
            SqliteCommand createTable = new SqliteCommand(tableCommand, db);

            createTable.ExecuteReader();
            db.Close();
        }

        try
        {
            AddData("insert into Dictionary (Name, LastReviewedTime, CreatedTime) VALUES ('默认词表',datetime(), datetime());");
        }
        catch
        {

        }

    }

    public static DateTime GetDate(string text)
    {
        DateTime dt = DateTime.Now;
        string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "pvedb.db");
        using (SqliteConnection db =
           new SqliteConnection($"Filename={dbpath}"))
        {
            db.Open();

            SqliteCommand selectCommand = new SqliteCommand
                ("select lastedited from Meanings where Words='"+text+"';", db);

            SqliteDataReader query = selectCommand.ExecuteReader();


            
            while (query.Read())
            {
                dt = query.GetDateTime(0);
            }
            SqliteCommand Command = new SqliteCommand("update Meanings set lastedited=datetime() where Words='"+text+"';",db);
            Command.ExecuteNonQuery();
            db.Close();
        }
        return dt;
    }

    public static List<List<string>> Query(string sql)
    {
        List<List<string>> entries = new List<List<string>>();

        string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "pvedb.db");
        using (SqliteConnection db =
           new SqliteConnection($"Filename={dbpath}"))
        {
            db.Open();

            SqliteCommand selectCommand = new SqliteCommand
                (sql, db);
            SqliteDataReader query;
            try
            {
                query = selectCommand.ExecuteReader();
            }
            catch
            {
                return entries;
            }
            

            while (query.Read())
            {
                entries.Add(new List<string>());
                for(int i = 0; i < query.FieldCount; i++)
                {
                    if(query.IsDBNull(i))
                    {
                        entries[entries.Count - 1].Add("null");
                    }
                    else entries[entries.Count - 1].Add(query.GetString(i));
                }
                
            }
        }

        return entries;
    }

    public static void AddData(string sql)
    {
        string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "pvedb.db");
        using (SqliteConnection db =
          new SqliteConnection($"Filename={dbpath}"))
        {
            db.Open();

            SqliteCommand insertCommand = new SqliteCommand();
            insertCommand.Connection = db;

            // Use parameterized query to prevent SQL injection attacks


            insertCommand.CommandText = sql;

            insertCommand.ExecuteReader();
            db.Close();
        }
    }

    internal static bool GetWordHistory(string text)
    {
        List<List<string>> res = Query("select * from Search_Cache where Words='"+text+"';");
        if(res.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    internal static string GetWordHistory_baidu(string text)
    {
        List<List<string>> res = new List<List<string>>();
        res = Query("select baidu from Search_Cache where Words='" + text + "';");
        if (res != null && res.Count > 0)
        {
            return res[0][0];
        }
        else return "";
    }
    internal static string GetWordHistory_youdao(string text)
    {
        List<List<string>> res = new List<List<string>>();
            res = Query("select youdao from Search_Cache where Words='" + text + "';");
        if (res != null && res.Count > 0)
        {
            return res[0][0];
        }
        else return "";
        
    }
}
