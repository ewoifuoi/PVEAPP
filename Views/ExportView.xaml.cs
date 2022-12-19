using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PVEAPP.DAL;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PVEAPP.Views;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ExportView : Page
{
    private string filePath;
    private bool isok;
    private string output = "";

    public ExportView()
    {
        this.InitializeComponent();
        output = "";
        List<List<string>> res = DataAccess.Query("select words,Meaning from Meanings where wid in(\r\n    select wid from word_dict where dict in (\r\n        select dict from Dictionary where Name='"+DictionaryView.dict+"'\r\n        )\r\n    );");
        for(int i = 0; i < res.Count; i++)
        {
            string temp = res[i][0] + "\t" + res[i][1];
            output += res[i][0] + "\n";
            word_list.Items.Add(temp);
        }
    }

    private void back(object sender, RoutedEventArgs e)
    {
        LoginView.contentframe.NavigateToType(typeof(DictionaryView), null, null);
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        BackgroundWorker worker = new BackgroundWorker();
        worker.DoWork += (s, e) => {
            //Some work...
            SaveTXTFile("纯文本", ".txt");
            while (isok == false) { Thread.Sleep(100); };
        };
        worker.RunWorkerCompleted += (s, e) => {
            //e.Result"returned" from thread

            FileStream fs = new FileStream(filePath, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs, Encoding.Default);
            sw.Write(output);
            //【3】释放资源
            sw.Close();
            fs.Close();

        };
        worker.RunWorkerAsync();

        ContentDialog dialog = new ContentDialog();
        
        // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
        dialog.XamlRoot = this.XamlRoot;
        dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.Title = "导出成功 !";
        dialog.CloseButtonText = "确定";

        dialog.DefaultButton = ContentDialogButton.Primary;
        var result = await dialog.ShowAsync();
    }

    public async void SaveTXTFile(string a, string b)
    {
        var savePicker = new FileSavePicker();
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, LoginView.hWnd);
        savePicker.SuggestedStartLocation =
            Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
        savePicker.FileTypeChoices.Add(a, new List<string>() { b });
        savePicker.SuggestedFileName = DictionaryView.dict + DateTime.Now.ToString("t");

        // 打开文件选择对话框
        var file = await savePicker.PickSaveFileAsync();
        if (file != null)
        {
            filePath = file.Path; // 暂存绝对路径
            isok = true;
        }
    }
}
