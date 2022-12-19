using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVEAPP.Models;
public class WordModel
{
    public string word="";
    public string meaning="";
    public string notes = "";
    public string ep = "";
    public int rate = 0;
    public DateTime date;

    public WordModel()
    {
    }

    public WordModel(string word, string meaning, string notes, string ep, int rate)
    {
        this.word = word;
        this.meaning = meaning;
        this.notes = notes;
        this.ep = ep;
        this.rate = rate;
    }

    public void Parse(List<List<string>> list)
    {
        if (list == null || list[0].Count < 6) return;
        word = list[0][1];
        meaning = list[0][2];
        notes = list[0][3];
        rate=Convert.ToInt32(list[0][4]);
    }
}
