using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PVEAPP.DAL;
using Spire.Pdf;
using Spire.Pdf.Exporting.XPS.Schema;
using Spire.Pdf.Graphics;
using Windows.Graphics;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PVEAPP.Views;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class HistoryView : Microsoft.UI.Xaml.Controls.Page
{
    private static List<List<string>> GetDisruptedItems(List<List<string>> source)
    {
        List<List<string>> temp = new List<List<string>>();
        for (int i = 0; i < source.Count; i++)
        {
            temp.Add(source[i]);
        }
        Random rand = new Random(DateTime.Now.Millisecond);
        for(int i = 0; i < temp.Count; i++)
        {
            for(int j = i + 1; j < temp.Count; j++)
            {

                if (rand.Next(2) == 1)
                {
                    List<string> t = temp[i];
                    temp[i] = temp[j];
                    temp[j] = t;
                }
            }
        }
        return temp;
    }

    public string fileName;
    public List<List<string>> res;
    public HistoryView()
    {
        this.InitializeComponent();
        res = DataAccess.Query("select Words,Meaning\r\nfrom Meanings\r\nwhere wid in (\r\n    select wid\r\n    from Review_History\r\n    where ReviewTime=date()\r\n    )\r\n;");
        for(int i = 0; i < res.Count; i++)
        {
            string temp = "" + res[i][0] + "\t" + res[i][1];
            lv.Items.Add(temp);
        }
        Selected_num.Text = res.Count.ToString();
        output = res;
    }

    private void back(object sender, RoutedEventArgs e)
    {
        LoginView.contentframe.NavigateToType(typeof(UserInfo), null, null);
        WindowId myWndId = Win32Interop.GetWindowIdFromWindow(LoginView.hWnd);
        SizeInt32 t = AppWindow.GetFromWindowId(myWndId).Size;
        AppWindow.GetFromWindowId(myWndId).Resize(new SizeInt32(t.Width / 2, t.Height));
        
        
    }
    public string filePath="";
    public async void SaveTXTFile(string a, string b)
    {
        var savePicker = new FileSavePicker();
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, LoginView.hWnd);
        savePicker.SuggestedStartLocation =
            Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
        savePicker.FileTypeChoices.Add(a, new List<string>() { b });
        Random rnd = new Random();
        savePicker.SuggestedFileName = "乱序单词检测卷"+ DateTime.Now.ToString("MM-dd-ffffff");

        // 打开文件选择对话框
        var file = await savePicker.PickSaveFileAsync();
        if (file != null)
        {
            filePath = file.Path; // 暂存绝对路径
            fileName = file.Name;
            isok = true;
            canceled = false;
        }
        else canceled = true;
        
    }
    public bool isok = false;
    private bool canceled = false;

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        ContentDialog dialog = new ContentDialog();

        // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
        dialog.XamlRoot = this.XamlRoot;
        dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.Title = "提示";
        dialog.Content = "该操作将生成两个pdf文件";
        
        dialog.PrimaryButtonText = "确定";
        dialog.CloseButtonText = "取消";

        dialog.DefaultButton = ContentDialogButton.Primary;
        var result = await dialog.ShowAsync();
        BackgroundWorker worker = new BackgroundWorker();
        PdfDocument doc = new PdfDocument();
        PdfDocument doc2 = new PdfDocument();
        if (result == ContentDialogResult.Primary)
        {
            isok = false;
            worker.Dispose();
            
            worker.DoWork += (s, e) => {
                //Some work...
                SaveTXTFile("PDF文件", ".pdf");

                while (isok == false) { if (canceled) {  break; } Thread.Sleep(100); };
            };
            worker.RunWorkerCompleted += (s, e) => {
                if (!canceled)
                {
                    PdfPageBase page = doc.Pages.Add();
                   

                    PdfTrueTypeFont pdfTrueTypeFont = new PdfTrueTypeFont(new Font("Microsoft Yahei", 10), true);
                    PdfTrueTypeFont pdfTrueTypeFont0 = new PdfTrueTypeFont(new Font("Microsoft Yahei", 25), true);
                    PdfTrueTypeFont pdfTrueTypeFont1 = new PdfTrueTypeFont(new Font("Microsoft Yahei", 8), true);
                    PdfSolidBrush pdfSolidBrush = new PdfSolidBrush(Color.Black);

                    output = GetDisruptedItems(output);
                    string s1 = "";string s2 = "";string s3 = "";string s4 = "";
                    for(int i = 0; i < 48 && i < output.Count; i++)
                    {
                        if(i <= 24)
                        {
                            s1 += (i+1).ToString() + ". " + output[i][1] + "\n\n";
                            s3 += output[i][0] + "\n\n";
                        }
                        else
                        {
                            s2 += (i+1).ToString() + ". " + output[i][1] + "\n\n";
                            s4 += output[i][0] + "\n\n";
                        }
                    }
                    page.Canvas.DrawString("PVE乱序单词拼写检测卷", pdfTrueTypeFont0, PdfBrushes.Black, new RectangleF(110, 30, page.GetClientSize().Width, page.GetClientSize().Height));
                    page.Canvas.DrawString("生成日期: "+DateTime.Now.ToString("yyyy-MM-dd dddd"), pdfTrueTypeFont, PdfBrushes.Black, new RectangleF(300, 70, page.GetClientSize().Width, page.GetClientSize().Height));
                    page.Canvas.DrawString(s1, pdfTrueTypeFont, PdfBrushes.Black, new RectangleF(0, 130, page.GetClientSize().Width / 2 - 2f, page.GetClientSize().Height));
                    page.Canvas.DrawString(s2, pdfTrueTypeFont, PdfBrushes.Black, new RectangleF(page.GetClientSize().Width / 2 + 2f, 130, page.GetClientSize().Width / 2, page.GetClientSize().Height));
                    doc.SaveToFile(filePath);
                    doc.Close();
                    
                    PdfPageBase page2 = doc2.Pages.Add();

                    page2.Canvas.DrawString("PVE乱序单词拼写检测卷(答案)", pdfTrueTypeFont0, PdfBrushes.Black, new RectangleF(110, 30, page2.GetClientSize().Width, page2.GetClientSize().Height));
                    page2.Canvas.DrawString("生成日期: " + DateTime.Now.ToString("yyyy-MM-dd dddd"), pdfTrueTypeFont, PdfBrushes.Black, new RectangleF(300, 70, page.GetClientSize().Width, page2.GetClientSize().Height));
                    page2.Canvas.DrawString(s1, pdfTrueTypeFont, PdfBrushes.Black, new RectangleF(0, 130, page2.GetClientSize().Width / 2 - 2f, page2.GetClientSize().Height));
                    page2.Canvas.DrawString(s2, pdfTrueTypeFont, PdfBrushes.Black, new RectangleF(page2.GetClientSize().Width / 2 + 2f, 130, page2.GetClientSize().Width / 2, page2.GetClientSize().Height));
                    page2.Canvas.DrawString(s3, pdfTrueTypeFont, PdfBrushes.Black, new RectangleF(150, 130, page2.GetClientSize().Width / 2 - 2f, page2.GetClientSize().Height));
                    page2.Canvas.DrawString(s4, pdfTrueTypeFont, PdfBrushes.Black, new RectangleF(page2.GetClientSize().Width / 2 + 2f + 150, 130, page2.GetClientSize().Width / 2, page2.GetClientSize().Height));
                    doc2.SaveToFile(filePath +"(答案).pdf");
                    doc2.Close();

                }
                else
                {
                    canceled = false;
                }
            };
            worker.RunWorkerAsync();
            
        }
        //创建PdfDocument类的对象，并加载PDF文档
        

    }
    public List<List<string>> output;
    private void cv_SelectedDatesChanged(Microsoft.UI.Xaml.Controls.CalendarView sender, Microsoft.UI.Xaml.Controls.CalendarViewSelectedDatesChangedEventArgs args)
    {
        lv.Items.Clear();
        string sql = "";
        for(int i = 0; i < cv.SelectedDates.Count; i++)
        {
            DateTime dt = cv.SelectedDates[i].Date;
            string s = dt.ToString("yyyy-MM-dd");
            if(i == 0)
            {
                sql += "select Words,Meaning\r\nfrom Meanings\r\nwhere wid in(\r\n    select wid from Review_History where ReviewTime=date('"+s+"')";
            }
            else
            {
                sql += "or ReviewTime=date('" + s + "')";
            }
        }
        sql += ");";
        List<List<string>> list = DataAccess.Query(sql);
        Selected_num.Text = list.Count.ToString();
        output = list;
        for(int i = 0; i < list.Count; i++)
        {
            string temp = "";
            temp += list[i][0] + "\t" + list[i][1];
            lv.Items.Add(temp);
        }
    }
}
