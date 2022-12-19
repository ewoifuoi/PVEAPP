using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI;
using Windows.Graphics;

namespace PVEAPP.ViewModels;
public class LoginViewModel
{
    public void ClearWindow(IntPtr hWnd)
    {
        WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        var apw = AppWindow.GetFromWindowId(myWndId).Presenter as OverlappedPresenter;
        apw.IsResizable = false;
        apw.IsMinimizable = true;
        AppWindow.GetFromWindowId(myWndId).Resize(new SizeInt32(1000, 1300));
        
    }

    
}
