using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PVEAPP.Views;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class Review : Page
{
    public Review()
    {
        this.InitializeComponent();
        num.Text = UserInfo.Review_num;
    }

    private void back(object sender, RoutedEventArgs e)
    {
        LoginView.contentframe.NavigateToType(typeof(UserInfo), null, null);
    }

    private void ywxy(object sender, RoutedEventArgs e)
    {
        LoginView.contentframe.NavigateToType(typeof(ReviewTestView), null, null);
    }
}
