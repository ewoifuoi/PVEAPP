using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PVEAPP.BLL;
using PVEAPP.DAL;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PVEAPP.Views;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class DictionaryView : Page
{
    SearchingOP op;
    public DictionaryView()
    {
        this.InitializeComponent();
        op = new SearchingOP();
        op.GetDict(ref dict_list);
    }

    private void back(object sender, RoutedEventArgs e)
    {
        LoginView.contentframe.NavigateToType(typeof(UserInfo), null, null);
    }

    private async void AddDict(object sender, RoutedEventArgs e)
    {
        ContentDialog dialog = new ContentDialog();
        TextBox tb = new TextBox { };
        // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
        dialog.XamlRoot = this.XamlRoot;
        dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.Title = "请输入新表名";
        dialog.PrimaryButtonText = "确定";
        dialog.CloseButtonText = "取消";
        dialog.DefaultButton = ContentDialogButton.Primary;
        dialog.Content = tb;

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            try
            {
                DataAccess.AddData("insert into Dictionary (Name, LastReviewedTime, CreatedTime) VALUES ('" + tb.Text + "', datetime(), datetime());");
            }
            catch
            {

            }
            dict_list.Items.Clear();
            op.GetDict(ref dict_list);
        }

    }

    private async void DeleteDict(object sender, RoutedEventArgs e)
    {
        ContentDialog dialog = new ContentDialog();
        if (dict_list.SelectedItem.ToString() == "默认词表")
        {
            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "无法删除默认词表";
            dialog.CloseButtonText = "确定";
            
            dialog.DefaultButton = ContentDialogButton.Primary;
        }
        else
        {
            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "您真的要删除该词表吗?";
            dialog.PrimaryButtonText = "确定";
            dialog.CloseButtonText = "取消";
            dialog.DefaultButton = ContentDialogButton.Primary;
        }

        

        var result = await dialog.ShowAsync();

        if(result == ContentDialogResult.Primary)
        {
            op.DeleteDict(dict_list.SelectedItem.ToString());
            dict_list.Items.Clear();
            op.GetDict(ref dict_list);
        }
    }
    public static string dict = "";
    private void ExportDict(object sender, RoutedEventArgs e)
    {
        dict = (string)dict_list.SelectedItem;
        if(dict_list.SelectedItem != null)
        {
            LoginView.contentframe.NavigateToType(typeof(ExportView), null, null);
        }
        
    }
}
