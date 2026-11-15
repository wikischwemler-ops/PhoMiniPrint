using System.Windows;
using PhoMiniPrint.Core;

namespace PhoMiniPrint.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = $"PhoMiniPrint {Programmversion.Von(typeof(MainWindow).Assembly)}";
    }
}
