using Ursa.Controls;

namespace Ks.Ava.Mvvm.Views;

public partial class MainWindow : UrsaWindow
{
    public WindowNotificationManager? NotificationManager { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        NotificationManager = new WindowNotificationManager(this) { MaxItems = 3 };
    }
}