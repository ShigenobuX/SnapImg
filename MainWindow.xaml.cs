using ImageMagick;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Forms = System.Windows.Forms;
using WpfDragEventArgs = System.Windows.DragEventArgs;
using WpfDataFormats = System.Windows.DataFormats;
using WpfMessageBox = System.Windows.MessageBox;
using WpfMessageBoxButton = System.Windows.MessageBoxButton;
using WpfMessageBoxResult = System.Windows.MessageBoxResult;
using WpfOpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace ImgConv;

public partial class MainWindow : Window
{
    private readonly string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "imgconv", "settings.json");
    private readonly List<string> selectedFiles = [];
    private string? customFolder;
    private CancellationTokenSource? conversionCts;

    public MainWindow()
    {
        InitializeComponent();
        var settings = LoadSettings();
        Width = settings.Width; Height = settings.Height;
        Title = $"画像フォーマットコンバーター v {AppInfo.Version}";
        UpdateGuide();
    }

    private AppSettings LoadSettings()
    {
        try { return File.Exists(settingsPath) ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(settingsPath)) ?? new() : new(); }
        catch { return new(); }
    }
    private void SaveSettings()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!);
        File.WriteAllText(settingsPath, JsonSerializer.Serialize(new AppSettings { Width = Width, Height = Height }, new JsonSerializerOptions { WriteIndented = true }));
    }
    private void Add(IEnumerable<string> paths)
    {
        var files = ImageConverterService.Scan(paths).Where(x => !selectedFiles.Contains(x, StringComparer.OrdinalIgnoreCase)).ToList();
        selectedFiles.AddRange(files); foreach (var f in files) Files.Items.Add(f);
        LogMessage(files.Count > 0 ? $"{files.Count} 件のファイルを追加しました。" : "追加可能な新しいファイルはありませんでした。"); UpdateGuide();
    }
    private void UpdateGuide() => EmptyGuide.Visibility = selectedFiles.Count == 0 ? Visibility.Visible : Visibility.Hidden;
    private void LogMessage(string text) { Log.AppendText(text + Environment.NewLine); Log.ScrollToEnd(); }
    private void AddFiles_Click(object sender, RoutedEventArgs e)
    { var dialog = new WpfOpenFileDialog { Multiselect = true, Filter = "画像ファイル|*.heic;*.heif;*.png;*.jpg;*.jpeg;*.webp;*.bmp;*.tif;*.tiff;*.gif;*.ico|すべてのファイル|*.*" }; if (dialog.ShowDialog() == true) Add(dialog.FileNames); }
    private void AddFolder_Click(object sender, RoutedEventArgs e) { using var dialog = new Forms.FolderBrowserDialog(); if (dialog.ShowDialog() == Forms.DialogResult.OK) Add([dialog.SelectedPath]); }
    private void SelectFolder_Click(object sender, RoutedEventArgs e) { using var dialog = new Forms.FolderBrowserDialog(); if (dialog.ShowDialog() == Forms.DialogResult.OK) { customFolder = dialog.SelectedPath; FolderLabel.Text = customFolder; CustomFolder.IsChecked = true; } }
    private void Remove_Click(object sender, RoutedEventArgs e) { var items = Files.SelectedItems.Cast<string>().ToList(); foreach (var item in items) { selectedFiles.Remove(item); Files.Items.Remove(item); } if (items.Count > 0) LogMessage($"{items.Count} 件をリストから削除しました。"); UpdateGuide(); }
    private void Clear_Click(object sender, RoutedEventArgs e) { selectedFiles.Clear(); Files.Items.Clear(); LogMessage("ファイルリストをクリアしました。"); UpdateGuide(); }
    private void OnDrop(object sender, WpfDragEventArgs e) { if (e.Data.GetDataPresent(WpfDataFormats.FileDrop)) Add((string[])e.Data.GetData(WpfDataFormats.FileDrop)); }
    private void Quality_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (QualityLabel != null && Quality != null) QualityLabel.Text = $"画質: {(int)Quality.Value}";
    }
    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        var action = WpfMessageBox.Show("右クリックメニューを登録しますか？\n\n「いいえ」を選ぶと登録を解除します。", "設定", WpfMessageBoxButton.YesNoCancel);
        try
        {
            if (action == WpfMessageBoxResult.No) { ContextMenuService.Unregister(); WpfMessageBox.Show("右クリックメニューを解除しました。"); return; }
            if (action != WpfMessageBoxResult.Yes) return;
            var formats = new[] { "jpg", "png", "webp", "bmp" };
            var selected = formats.Where(x => WpfMessageBox.Show($"右クリックメニューに {x.ToUpperInvariant()} を登録しますか？", "設定", WpfMessageBoxButton.YesNo) == WpfMessageBoxResult.Yes);
            ContextMenuService.Register(selected);
            WpfMessageBox.Show("右クリックメニューを登録しました。");
        }
        catch (Exception ex) { WpfMessageBox.Show(ex.Message, "設定エラー"); }
    }
    private async void Convert_Click(object sender, RoutedEventArgs e)
    {
        if (selectedFiles.Count == 0) { WpfMessageBox.Show("変換するファイルが選択されていません。"); return; }
        if (CustomFolder.IsChecked == true && string.IsNullOrWhiteSpace(customFolder)) { WpfMessageBox.Show("保存先フォルダを選択してください。"); return; }
        ConvertButton.IsEnabled = false; conversionCts = new(); Progress.Value = 0;
        var output = (Format.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "jpg";
        var magickFormat = output switch { "jpg" => MagickFormat.Jpeg, "png" => MagickFormat.Png, "webp" => MagickFormat.WebP, _ => MagickFormat.Bmp };
        // UI コントロールは UI スレッドで読み取り、バックグラウンド処理には値だけ渡す。
        var quality = (int)Quality.Value;
        var preserveExif = Exif.IsChecked == true;
        var keepAspect = KeepAspect.IsChecked == true;
        var destinationFolder = CustomFolder.IsChecked == true ? customFolder : null;
        var width = ParsePositive(WidthBox.Text); var height = ParsePositive(HeightBox.Text); var files = selectedFiles.ToList(); var ok = 0; var fail = 0;
        foreach (var (file, index) in files.Select((f, i) => (f, i)))
        {
            Status.Text = $"変換中: {Path.GetFileName(file)} ({index + 1}/{files.Count})";
            (bool Success, string Message) result;
            try
            {
                result = await Task.Run(() => ImageConverterService.Convert(file, magickFormat, quality, preserveExif, width, height, keepAspect, destinationFolder));
            }
            catch (Exception ex)
            {
                result = (false, ex.Message);
            }
            if (result.Success) { ok++; LogMessage($"成功: {result.Message}"); } else { fail++; LogMessage($"失敗: {file} - {result.Message}"); }
            Progress.Value = (index + 1.0) / files.Count;
        }
        Status.Text = "処理が完了しました。"; LogMessage($"{ok} 件の変換が完了しました。{(fail > 0 ? $" {fail} 件は失敗しました。" : "")}"); ConvertButton.IsEnabled = true;
        WpfMessageBox.Show($"{ok} 件の変換が完了しました。{(fail > 0 ? $" {fail} 件は失敗しました。" : "")}", "変換完了");
    }
    private static int? ParsePositive(string text) => int.TryParse(text, out var value) && value > 0 ? value : null;
    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e) { conversionCts?.Cancel(); SaveSettings(); }
}
