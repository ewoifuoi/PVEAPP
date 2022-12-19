using System;
using System.Collections.Generic;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PVEAPP.DAL;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PVEAPP.Views;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class UserInfo : Page
{
    public UserInfo()
    {
        this.InitializeComponent();
        t1.Text = DataAccess.Query("select count(*) from Review_History where ReviewTime=date();")[0][0];
        t2.Text = DataAccess.Query("select count(*) from Dictionary;")[0][0];
        t3.Text = DataAccess.Query("select count(*) from Meanings;")[0][0];
        t4.Text = DataAccess.Query("select count(*) from (select * from Review_History group by ReviewTime)A;")[0][0];

    }

    private void EditDict(object sender, RoutedEventArgs e)
    {
        LoginView.contentframe.NavigateToType(typeof(DictionaryView), null, null);
    }

    private void History(object sender, RoutedEventArgs e)
    {
       
        WindowId myWndId = Win32Interop.GetWindowIdFromWindow(LoginView.hWnd);
        SizeInt32 t = AppWindow.GetFromWindowId(myWndId).Size;
        AppWindow.GetFromWindowId(myWndId).Resize(new SizeInt32(t.Width*2,t.Height));
        LoginView.contentframe.NavigateToType(typeof(HistoryView), null, null);
    }

    public static string Review_num="";
    private async void Review(object sender, RoutedEventArgs e)
    {
        if (review_num.SelectedItem == null)
        {
            ContentDialog dialog = new ContentDialog();
            TextBox tb = new TextBox { };
            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "请输入今日复习单词个数";
            dialog.PrimaryButtonText = "确定";
            dialog.DefaultButton = ContentDialogButton.Primary;
            var result = await dialog.ShowAsync();
        }
        else
        {
            Review_num = review_num.SelectedValue.ToString();
            
            LoginView.contentframe.NavigateToType(typeof(Review), null, null);
        }
    }
    public static List<List<string>> res;
}
