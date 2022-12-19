using System;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PVEAPP.ViewModels;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PVEAPP.Views;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class LoginView : Window
{
    public LoginViewModel viewModel;
    public static IntPtr hWnd;
    public static Frame contentframe;
    public static SizeInt32 target;
    
    public LoginView()
    {

        this.InitializeComponent();
        contentframe = contentFrame;
        viewModel = new LoginViewModel();
        hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        SizeInt32 t = AppWindow.GetFromWindowId(myWndId).Size;
        target = new SizeInt32(Convert.ToInt32(t.Width / 2.88), Convert.ToInt32(t.Height * 2.6 / 3.034));
        AppWindow.GetFromWindowId(myWndId).Resize(target);
        this.ExtendsContentIntoTitleBar = true;  // 指定自定义标题栏
        this.SetTitleBar(AppTitleBar);
        contentFrame.NavigateToType(typeof(UserInfo), null, null);


    }

    private void contentFrame_Loaded(object sender, RoutedEventArgs e)
    {
       
    }
}
