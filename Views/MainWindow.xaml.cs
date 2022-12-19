using System;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;
using PVEAPP.ViewModels;
using PVEAPP.Views;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PVEAPP;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private SearchPageViewModel viewModel;
    public IntPtr hWnd;
    private bool IsInit = true;
    private bool IsStart = true;
    private bool IsChanged = false;

    public static SizeInt32 target;


    public MainWindow()
    {
        this.InitializeComponent();
        hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        SizeInt32 t = AppWindow.GetFromWindowId(myWndId).Size;
        target = new SizeInt32(Convert.ToInt32(t.Width / 2.88), Convert.ToInt32(t.Height / 3.034));
        AppWindow.GetFromWindowId(myWndId).MoveInZOrderAtTop();
        viewModel = new SearchPageViewModel();
        
        viewModel.ClearWindow(hWnd, ref lv);
        this.ExtendsContentIntoTitleBar = true;  // 指定自定义标题栏
        this.SetTitleBar(AppTitleBar);
  
    }

    
    /// <summary>
    /// 输入框更改
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void WordBox_Input(object sender, TextChangedEventArgs e)
    {
        IsChanged = true;
        if(WordBox.Text == "")
        {
            viewModel.ClearWindow(hWnd, ref lv); // 还原搜索框
            lv.Height = 0;
        }
    }


    private void WordBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        
        if (e.Key==Windows.System.VirtualKey.Enter && IsChanged==true) // 回车键查词
        {
            IsChanged=false;
            lv.Height = 310;
            viewModel.InitWindow(hWnd,ref lv);

            if(viewModel.SearchHistory(WordBox.Text, ref lv))
            {
                Button btn = new Button { Content = "重新编辑该词条", FontSize = 20 ,Height=70, Width=300, Margin=new Thickness(20)};
                btn.Click += ReEdit;
                StackPanel temp = lv.Items[0] as StackPanel;
                temp.Children.Add(btn);
            } // 查词逻辑

            WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var apw = AppWindow.GetFromWindowId(myWndId).Presenter as OverlappedPresenter;
            apw.IsResizable = false;
            apw.IsMinimizable = true;
            AppWindow.GetFromWindowId(myWndId).Resize(new SizeInt32(target.Width, Convert.ToInt32(target.Height*2.6)));
        }
    }

    private void ReEdit(object sender, RoutedEventArgs e)
    {
        lv.Items.Clear();
        viewModel.Search(WordBox.Text, ref lv);
    }

    private void WordBox_GotFocus(object sender, RoutedEventArgs e)
    {
       if(IsInit)
        {
            IsInit = false;
            WordBox.Text = "";
            WordBox.Foreground = new SolidColorBrush(Colors.White);
        }
    }

    private void ListView_Loaded(object sender, RoutedEventArgs e)
    {
        this.WordBox.IsEnabled = true;
    }

    private void WordBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if(this.WordBox.Text=="")
        {
            IsInit = true;
            WordBox.Text = "输入或粘贴文字";
            WordBox.Foreground = new SolidColorBrush(Colors.DarkGray);
        }
    }

    private void login_Click(object sender, RoutedEventArgs e)
    {
        var t = new LoginView();
        t.Activate();
    }
}
