using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using PVEAPP.DAL;
using PVEAPP.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PVEAPP.Views;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ReviewTestView : Page
{
    public List<List<string>> res;
    public int count = 0;
    public ReviewTestView()
    {
        this.InitializeComponent();
        res = DataAccess.Query("select Words,Meaning\r\nfrom Meanings,(select wid, date('now', '-10 years') , Rate, 0 as wrong_time,datetime('now', '-10 years') as Last\r\n      from Meanings\r\n      where wid not in (\r\n          select wid\r\n          from Review_History\r\n      )\r\n      union\r\n      select Review_History.wid, ReviewTime, Rate, wrong_time,max(LastReviewTime) as Last\r\n      from Review_History,\r\n           Meanings\r\n      where Review_History.wid = Meanings.wid\r\n      group by Review_History.wid\r\n      order by Last asc,  Rate desc, wrong_time desc limit " + UserInfo.Review_num + ") A\r\nwhere A.wid=Meanings.wid;");

        DataAccess.Query("insert into Review_History (wid, wrong_time, ReviewTime, LastReviewTime)\r\nselect A.wid, 0, date(), datetime()\r\nfrom (select wid, date('now', '-10 years') , Rate, 0 as wrong_time,datetime('now', '-10 years') as Last\r\n      from Meanings\r\n      where wid not in (\r\n          select wid\r\n          from Review_History\r\n      )\r\n      union\r\n      select Review_History.wid, ReviewTime, Rate, wrong_time,max(LastReviewTime) as Last\r\n      from Review_History,\r\n           Meanings\r\n      where Review_History.wid = Meanings.wid\r\n      group by Review_History.wid\r\n      order by Last asc,  Rate desc, wrong_time desc limit " +UserInfo.Review_num + ") A ;");
        ShowCard(count);
    }

    private static List<int> GetDisruptedItems(List<int> colors)
    {
        //生成一个新数组：用于在之上计算和返回
        List<int> temp = new List<int>();
        
        for (int i = 0; i < colors.Count; i++)
        {
            temp.Add(colors[i]);
        }

        //打乱数组中元素顺序
        Random rand = new Random(DateTime.Now.Millisecond);
        for (int i = 0; i < temp.Count; i++)
        {
            int x, y; int t;
            x = rand.Next(0, temp.Count);
            do
            {
                y = rand.Next(0, temp.Count);
            } while (y == x);

            t = temp[x];
            temp[x] = temp[y];
            temp[y] = t;
        }

        return temp;

    }

    public void ShowCard(int id)
    {
        testArea.Children.Clear();

        if(count == res.Count)
        {
            TextBlock t = new TextBlock { FontSize = 30, Text = "今日复习已完成！" };
            testArea.Children.Add(t);
            return;
        }

        StackPanel p = new StackPanel();
        TextBlock tb = new TextBlock {TextAlignment=TextAlignment.Center,Width=300, FontSize=30, Text = res[id][0],Margin=new Thickness(0,50,0,60) };

        List<Button> btnList = new List<Button>();
        Random r = new Random();
        int tc = r.Next(3);
        StackPanel p1 = new StackPanel { Orientation=Orientation.Horizontal}; StackPanel p2 = new StackPanel { Orientation = Orientation.Horizontal };
        int ccount =Convert.ToInt32(DataAccess.Query("select count(*) from Meanings;")[0][0]); List<int> cards = new List<int>();
        for (int i = 1; i <= ccount; i++) 
        { 
            if (i != Convert.ToInt32(DataAccess.Query("select wid from Meanings where Words='"+ res[id][0] + "';")[0][0])) cards.Add(i); 
        }
        cards = GetDisruptedItems(cards);
        

        for (int i = 0; i < 4; i++)
        {
            Button btn;
            if (i == tc)
            {
                btn = new Button {FontSize=12, Margin=new Thickness(5), Content = res[id][1],Width=150, Height=80 };
                btn.Click += Correct;
            }
            else
            {

                btn = new Button { FontSize = 12, Margin = new Thickness(5), Width =150, Height = 80, Content = DataAccess.Query("select Meaning\r\nfrom Meanings\r\nwhere wid=" + cards.First().ToString() + ";")[0][0] };
                
                temp_word = res[id][0];
                btn.Click += Wrong;
                cards.RemoveAt(0);
            }
            if (i == 0) p1.Children.Add(btn);
            if (i == 1) p1.Children.Add(btn);
            if (i == 2) p2.Children.Add(btn);
            if (i == 3) p2.Children.Add(btn);
        }

        p.Children.Add(tb);
        p.Children.Add(p1);p.Children.Add(p2);
        testArea.Children.Add(p);
    }
    public string temp_word = "";

    private void Wrong(object sender, RoutedEventArgs e)
    {
        Button btn = sender as Button;
        Button btn1;
        Rectangle abtn = new Rectangle { Margin = new Thickness(5), Width = 150, Height = 80, Fill = new SolidColorBrush(Colors.Crimson) };
        StackPanel p = btn.Parent as StackPanel;
        if (p.Children[0] == btn)
        {
            btn1 = p.Children[1] as Button;
            p.Children.Clear();
            p.Children.Add(abtn);
            p.Children.Add(btn1);
        }
        else
        {
            btn1 = p.Children[0] as Button;
            p.Children.Clear();
            p.Children.Add(btn1);
            p.Children.Add(abtn);
        }
        count++;
        res.Add(new List<string> { temp_word, DataAccess.Query("select Meaning\r\nfrom Meanings\r\nwhere Words='" + temp_word + "';")[0][0] });
        BackgroundWorker worker = new BackgroundWorker();
        worker.DoWork += (s, e) => {
            Thread.Sleep(200);
        };
        worker.RunWorkerCompleted += (s, e) => {
            ShowMeaning(temp_word);
        };
        worker.RunWorkerAsync();
    }

    private void ShowMeaning(string word)
    {
        testArea.Children.Clear();

        ListView lv = new ListView{Width = 450, Height = 250};
        testArea.Children.Add(lv);
        Button btn = new Button {Content = "继续复习", Width = 200, Height = 50, Margin = new Thickness(60,20,40,40) };
        testArea.Children.Add(btn);
        btn.Click += Next;
        SearchPageViewModel vm = new SearchPageViewModel();
        
        vm.SearchHistory(word, ref lv);
        StackPanel llv = lv.Items.First() as StackPanel;
        llv.IsHitTestVisible = false;
        
    }

    private void Next(object sender, RoutedEventArgs e)
    {
        
        ShowCard(count);
    }

    private void Correct(object sender, RoutedEventArgs e)
    {
        Button btn = sender as Button;
        Button btn1;
        Rectangle abtn = new Rectangle { Margin = new Thickness(5), Width = 150, Height = 80, Fill=new SolidColorBrush(Colors.Green) };
        StackPanel p =  btn.Parent as StackPanel;
        if (p.Children[0] == btn)
        {
            btn1 = p.Children[1] as Button;
            p.Children.Clear();
            p.Children.Add(abtn);
            p.Children.Add(btn1);
        }
        else
        {
            btn1 = p.Children[0] as Button;
            p.Children.Clear();
            p.Children.Add(btn1);
            p.Children.Add(abtn);
        }
        count++;
        BackgroundWorker worker = new BackgroundWorker();
        worker.DoWork += (s, e) => {
            Thread.Sleep(200);
        };
        worker.RunWorkerCompleted += (s, e) => {
            ShowCard(count);
        };
        worker.RunWorkerAsync();
        //
    }

    private void back(object sender, RoutedEventArgs e)
    {
        LoginView.contentframe.NavigateToType(typeof(Review), null, null);
    }
}
