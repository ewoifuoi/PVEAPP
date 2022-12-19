using System;
using System.Collections.Generic;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using PVEAPP.BLL;
using PVEAPP.Models;
using Windows.Data.Json;
using Windows.Graphics;

namespace PVEAPP.ViewModels;

public class SearchPageViewModel
{
    private SearchingOP op = null;
    private List<ToggleButton> toggles = new List<ToggleButton>();
    private string MeaningTemp = "";
    private TextBox tb_meaning;
    private string word="";
    private TextBox notes;
    private TextBox ep;
    private RatingControl ratingControl;
    

    public SearchPageViewModel()
    {
        op = new SearchingOP();
    }

    public ListView Lv;

    public bool SearchHistory(string text, ref ListView lv)
    {
        StackPanel search_result = new StackPanel();
        StackPanel search_tools = new StackPanel();
        Lv = lv;
        if (op.WordExistQuery(text))
        {
            lv.Items.Add(search_result);
            search_result.Children.Add(ShowResult(op.GetWord(text))); // 将数据库中单词数据找出
            return true;
        }
        else
        {
            lv.Items.Add(search_tools);
            word = text;
            search_tools.Children.Add(ShowYouDaoFanyi(text));// 显示数据库查询结果
            search_tools.Children.Add(ShowBaiduFanyi(text)); // 将爬虫数据显示
            search_tools.Children.Add( ShowOp());// 添加单词
            return false;
        }
    }

    public void Search(string text, ref ListView lv)
    {
        StackPanel search_tools = new StackPanel();
        lv.Items.Add(search_tools);
        word = text;
        search_tools.Children.Add(ShowYouDaoFanyi(text));// 显示数据库查询结果
        search_tools.Children.Add(ShowBaiduFanyi(text)); // 将爬虫数据显示
        search_tools.Children.Add(ShowOp());// 添加单词
    }

    private StackPanel ShowOp()
    {

        StackPanel p = new StackPanel { Orientation=Orientation.Vertical, HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Top};

        StackPanel block = new StackPanel { Height = 30 };
        p.Children.Add(block);
        StackPanel input = new StackPanel {Orientation=Orientation.Horizontal};
        TextBlock tb = new TextBlock {Text = "选用含义：" ,Margin=new Thickness(5,12,0,5)};
        input.Children.Add(tb);
        tb_meaning = new TextBox { Height = 35, Width = 220, Margin = new Thickness(5) };
        tb_meaning.TextChanged += Tb_meaning_TextChanged;
        input.Children.Add(tb_meaning);

        p.Children.Add(input);

        StackPanel input4 = new StackPanel { Orientation = Orientation.Horizontal };
        TextBlock tb4 = new TextBlock { Text = "难度评级：", Margin = new Thickness(5, 24, 5, 20) };
        input4.Children.Add(tb4);
        ratingControl = new RatingControl { Margin = new Thickness(5, 20, 5, 20) };
        input4.Children.Add(ratingControl);
        p.Children.Add(input4);

        StackPanel input5 = new StackPanel { Orientation = Orientation.Vertical };
        TextBlock tee = new TextBlock { Text = "请选择词表:", Margin = new Thickness(5, 0,0,10) };
        dict = new ComboBox {Height=40,Width=300,Margin=new Thickness(10,10,10,20) };
        if(dict.Items.Count > 0) dict.SelectedItem = dict.Items[0];

        input5.Children.Add(tee);
        input5.Children.Add(dict);
        p.Children.Add(input5);

        op.GetDict(ref dict);
   

        StackPanel input2 = new StackPanel { Orientation = Orientation.Vertical };
        TextBlock tb2 = new TextBlock { Text = "语境：", Margin = new Thickness(5, 5, 20, 5) };
        input2.Children.Add(tb2);
        ep = new TextBox { Height = 85, Width = 300, Margin = new Thickness(5), AcceptsReturn=true, TextWrapping =TextWrapping.Wrap };
        input2.Children.Add(ep);
        p.Children.Add(input2);


        StackPanel input3 = new StackPanel { Orientation = Orientation.Vertical };
        TextBlock tb3 = new TextBlock { Text = "笔记：", Margin = new Thickness(5, 5, 20, 5) };
        input3.Children.Add(tb3);
        notes = new TextBox { Height = 85, Width = 300, Margin = new Thickness(5), AcceptsReturn = true, TextWrapping = TextWrapping.Wrap };
        input3.Children.Add(notes);
        p.Children.Add(input3);

        


        Button btn = new Button { Content = "添加单词", Height = 70, Width = 300,Margin=new Thickness(10,10,10,10) };
        btn.Click += AddWord;
        
        p.Children.Add(btn);
   

        return p;
    }

    public ComboBox dict;

    private void Tb_meaning_TextChanged(object sender, TextChangedEventArgs e)
    {
        MeaningTemp = tb_meaning.Text;
    }

    private void AddWord(object sender, RoutedEventArgs e)
    {
        WordModel model = new WordModel(word,MeaningTemp,notes.Text, ep.Text,Convert.ToInt32( ratingControl.Value));
        if(dict.SelectedItem==null)
        {
            op.SaveWord(model, "默认词表");
        }
        else op.SaveWord(model, dict.SelectedItem?.ToString());
        
        Button b = sender as Button;
        b.Content = "已添加";
        Lv.Items.Clear();
        SearchHistory(model.word, ref Lv);
        b.IsEnabled = false;
    }

    private StackPanel ShowBaiduFanyi(string word)
    {
        StackPanel p = new StackPanel();
        ToggleButton textBlock = new ToggleButton {Background=new SolidColorBrush(Colors.Transparent), FontSize = 14, Content = op.GetMeaning(word, "baidu"), Margin = new Thickness(5) }; // 词义
        textBlock.Click += TogClick;
        toggles.Add(textBlock);
        TextBlock t2 = new TextBlock { Text = "百度翻译结果:", Foreground = new SolidColorBrush(Colors.Gray), Margin = new Thickness(5) };
        p.Children.Add(t2);
        p.Children.Add(textBlock);
        return p;
    }

    private void TogClick(object sender, RoutedEventArgs e)
    {
        MeaningTemp = "";
        for(int i = 0;i < toggles.Count; i++)
        {
            if(toggles[i].IsChecked == true)
            {
                MeaningTemp += " ";
                MeaningTemp += toggles[i].Content;
                
            }
        }
        tb_meaning.Text = MeaningTemp;
        
    }

    private StackPanel ShowYouDaoFanyi(string word)
    {
        
        StackPanel p = new StackPanel();
        string res = op.GetMeaning(word, "youdao");
        if (res == "") 
        {
            p.Children.Add(new TextBlock { Text="查询失败"}); return p; }
        JsonObject jsonObject = JsonObject.Parse(res);
        List<List<string>> res1 = new List<List<string>>();
        bool failed = false; //是否有道查询单词失败
        try
        {
            JsonArray ja = jsonObject["basic"].GetObject()["explains"].GetArray();
            foreach(var item in ja)
            {
                res1.Add(new List<string>());
                res1[res1.Count -1].Add(item.GetString());
            }
        }
        catch
        {
            res1.Add(new List<string>());
            res1[0].Add(jsonObject["translation"].ToString());
            failed = true;
        }
        
        for(int i = 0; i < res1.Count; i++)
        {
            bool be = true;string temp = "";
            string head = ""; List<string> meanings = new List<string>();
            for (int j = 0; j < res1[i][0].Length; j++)
            {
                if(be)
                {
                    if (res1[i][0][j] != '.')
                    {
                        head += res1[i][0][j];
                    }
                    else
                    {
                        be = false;
                    }
                }
                else
                {
                    if(res1[i][0][j] != '；')
                    {
                        temp += res1[i][0][j];
                    }
                    else
                    {
                        res1[i].Add(temp);
                        temp = "";
                    }
                }
            }
            res1[i].Add(temp);
            res1[i][0] = head;
        }
        
        
        TextBlock t2 = new TextBlock { Text = "有道词典:", Foreground = new SolidColorBrush(Colors.Gray), Margin = new Thickness(5) };
        p.Children.Add(t2);
        if(!failed)
        {
            for (int i = 0; i < res1.Count; i++)
            {
                TextBlock type = new TextBlock { Text = res1[i][0].ToString() + ".", FontSize = 14, Foreground = new SolidColorBrush(Colors.AliceBlue) };
                p.Children.Add(type);
                for (int j = 1; j < res1[i].Count; j++)
                {
                    ToggleButton textBlock = new ToggleButton { Background = new SolidColorBrush(Colors.Transparent), FontSize = 14, Content = res1[i][j].ToString(), Margin = new Thickness(5) }; // 词义
                    textBlock.Click += TogClick;
                    p.Children.Add(textBlock);
                    toggles.Add(textBlock);
                }
            }
        }
        else
        {
            ToggleButton textBlock = new ToggleButton { Background = new SolidColorBrush(Colors.Transparent), FontSize = 14, Content = res1[0][0].ToString(), Margin = new Thickness(5) }; // 词义
            p.Children.Add(textBlock);
        }

        return p;
    }

    private StackPanel epsp;
    private ListView eplb;
    private StackPanel middle;

    public StackPanel ShowResult(WordModel word)
    {
        StackPanel p = new StackPanel { Margin=new Thickness(10,20,10,10)};
        if(word.word != "")
        {
            this.word = word.word;
            TextBlock tb = new TextBlock { Text = "选用的含义", Foreground =new SolidColorBrush(Colors.Gray),Margin=new Thickness(5) };
            TextBlock textBlock = new TextBlock {TextWrapping=TextWrapping.Wrap, Margin=new Thickness(0,10,10,10), FontSize = 20, Text = word.meaning};

            
            TextBlock dict_tb = new TextBlock {Text=op.GetDict(word.word) ,Foreground=new SolidColorBrush(Colors.Gray), Opacity=0.7};
            

            StackPanel last = new StackPanel {Orientation=Orientation.Horizontal,Margin=new Thickness(0,20,10,10) };
            TextBlock tb2 = new TextBlock {FontSize=13, Text = "距离您上次查询已过",Foreground=new SolidColorBrush(Colors.LightGray), };
            TextBlock tb3 = new TextBlock { FontSize = 13, Text = (word.date - DateTime.Now).Days.ToString(), Foreground = new SolidColorBrush(Colors.Gold) ,Margin=new Thickness(10,0,10,0)};
            TextBlock tb4 = new TextBlock { FontSize = 13, Text = "天。" , Foreground=new SolidColorBrush(Colors.LightGray)};
            last.Children.Add(tb2); last.Children.Add(tb3); last.Children.Add(tb4);
            StackPanel total = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 10, 10) };
            TextBlock tb5 = new TextBlock {Margin=new Thickness(0,5,5,5), FontSize = 13, Text = "该单词本月被重复查询", Foreground = new SolidColorBrush(Colors.LightGray) };
            total.Children.Add(tb5);
            TextBlock tb7 = new TextBlock { Margin = new Thickness(0, 5, 5, 5), FontSize = 13, Text = op.GetSearchTimes(word.word), Foreground = new SolidColorBrush(Colors.Gold) };
            TextBlock tb6 = new TextBlock { Margin = new Thickness(0,5,5,5), FontSize = 13, Text = "次。", Foreground = new SolidColorBrush(Colors.LightGray) };
            total.Children.Add(tb7);
            total.Children.Add(tb6);
            ToggleButton btn = new ToggleButton {FontSize=12, Content = "加强复习" };total.Children.Add(btn);

            Expander note = new Expander {Header="笔记" ,Margin=new Thickness(0,5,5,5), Width=330, IsExpanded=true};
            Expander ep = new Expander { Header = "语境记录",Margin=new Thickness(0,5,5,5),Width=330,IsExpanded=true ,HorizontalAlignment=HorizontalAlignment.Left};
            StackPanel notesp = new StackPanel();
            TextBlock notetb = new TextBlock { Text = word.notes, TextWrapping=TextWrapping.Wrap};
            notesp.Children.Add(notetb);
            note.Content = notesp;
            
            epsp = new StackPanel();
            Button btn2 = new Button { Content = "添加语境例句", Width = 280, Height = 50, Margin = new Thickness(5,20,5,5) };
            btn2.Click += AddEp;
            eplb = new ListView();
            epsp.Children.Add(eplb);
            middle = new StackPanel();
            middle.Children.Add(btn2);
            epsp.Children.Add(middle);
            
            ep.Content = epsp;
            GetEps(word.word, ref eplb);

            p.Children.Add(tb);

            
            p.Children.Add(textBlock);
            p.Children.Add(dict_tb);
            p.Children.Add(last);
            p.Children.Add(total);
            p.Children.Add(note);
            p.Children.Add(ep);
            
        }
        return p;
        
    }

    private void AddEp(object sender, RoutedEventArgs e) // 添加例句按钮回调函数,添加编辑框
    {
        TextBox editbox = new TextBox {TextWrapping=TextWrapping.Wrap, Width = 290, Height = 80, Margin=new Thickness(0,20,0,0) };

        editbox.KeyDown += Editbox_KeyDown;
        middle.Children.Clear();
        middle.Children.Add(editbox);
        
    }
    private void Editbox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)//回车键执行添加
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            Editbox_LostFocus(sender, e);
        }
    }
    private void Editbox_LostFocus(object sender, RoutedEventArgs e)// 失去焦点执行添加,并还原添加按钮
    {
        TextBox editbox = sender as TextBox;
        if(editbox.Text != "")
        {
            op.AddEp(word, editbox.Text);// 数据库添加例句
        }
        middle.Children.Clear();
        Button btn2 = new Button { Content = "添加语境例句", Width = 280, Height = 50, Margin = new Thickness(5, 20, 5, 5) };
        btn2.Click += AddEp;
        middle.Children.Add(btn2);
        List<List<string>> res = op.GetEp(word);eplb.Items.Clear();
        for(int i = 0; i < res.Count; i++)
        {
            StackPanel sp = new StackPanel { Margin = new Thickness(5) };
            TextBlock tb = new TextBlock {TextWrapping=TextWrapping.Wrap, Text = res[i][0], FontSize = 15 }; sp.Children.Add(tb);
            TextBlock tbb = new TextBlock { Text = "\t\t--" + Convert.ToDateTime(res[i][1]).ToString("d"), FontSize = 12, Foreground = new SolidColorBrush(Colors.Gray) }; sp.Children.Add(tbb);
            eplb.Items.Add(sp);
        }
    }

    public void GetEps(string word, ref ListView eplb)
    {
        List<List<string>> res = op.GetEp(word); eplb.Items.Clear();
        for (int i = 0; i < res.Count; i++)
        {
            StackPanel sp = new StackPanel { Margin=new Thickness(5)};
            TextBlock tb = new TextBlock { TextWrapping = TextWrapping.Wrap, Text = res[i][0], FontSize=15 }; sp.Children.Add(tb);
            TextBlock tbb = new TextBlock { Text = "\t\t--" + Convert.ToDateTime(res[i][1]).ToString("d"), FontSize=12, Foreground=new SolidColorBrush(Colors.Gray) }; sp.Children.Add(tbb);
            eplb.Items.Add(sp);
        }
    }

    public void ClearWindow(IntPtr hWnd, ref ListView lv)
    {
        WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        var apw = AppWindow.GetFromWindowId(myWndId).Presenter as OverlappedPresenter;
        apw.IsResizable = false;
        apw.IsMinimizable = true;
        SizeInt32 t = AppWindow.GetFromWindowId(myWndId).Size;
        AppWindow.GetFromWindowId(myWndId).Resize(MainWindow.target);
        lv.Items.Clear();
        toggles.Clear();
        MeaningTemp = "";
    }
    public void InitWindow(IntPtr hWnd, ref ListView lv)
    {
        lv.Items.Clear();
        
    }
}
