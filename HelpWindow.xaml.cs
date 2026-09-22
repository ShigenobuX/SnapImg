using System.Windows;

namespace ImgConv;

public partial class HelpWindow : Window
{
    public HelpWindow() => InitializeComponent();
    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
