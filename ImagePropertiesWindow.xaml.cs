using System.Diagnostics;
using System.IO;
using System.Windows;

namespace ImgConv;

public partial class ImagePropertiesWindow : Window
{
    private readonly ImageFileProperties properties;

    public ImagePropertiesWindow(ImageFileProperties properties)
    {
        this.properties = properties;
        InitializeComponent();
        HeaderFileName.Text = Path.GetFileName(properties.FilePath);
        ThumbnailImage.Source = properties.Thumbnail;
        FileDetails.ItemsSource = properties.FileDetails;
        ImageDetails.ItemsSource = properties.ImageDetails;
    }

    private void CopyPath_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Windows.Clipboard.SetText(properties.FilePath);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(this, ex.Message, "パスをコピーできません", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var folder = Path.GetDirectoryName(properties.FilePath);
            if (!string.IsNullOrEmpty(folder))
                Process.Start(new ProcessStartInfo("explorer.exe", $"\"{folder}\"") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(this, ex.Message, "保存場所を開けません", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
