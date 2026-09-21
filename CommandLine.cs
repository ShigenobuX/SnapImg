using ImageMagick;
using System.IO;
using WpfMessageBox = System.Windows.MessageBox;

namespace ImgConv;

public static class CommandLine
{
    public static bool Run(string[] args)
    {
        if (args.Contains("--register-menu")) { ContextMenuService.Register(); WpfMessageBox.Show("右クリックメニューを登録しました。", "imgconv"); return true; }
        if (args.Contains("--unregister-menu")) { ContextMenuService.Unregister(); WpfMessageBox.Show("右クリックメニューを解除しました。", "imgconv"); return true; }
        var index = Array.IndexOf(args, "--convert-to");
        if (index < 0) return false;
        if (index + 1 >= args.Length) throw new ArgumentException("変換形式が指定されていません。");
        var format = args[index + 1].ToLowerInvariant() switch { "jpg" => MagickFormat.Jpeg, "png" => MagickFormat.Png, "webp" => MagickFormat.WebP, "bmp" => MagickFormat.Bmp, _ => throw new ArgumentException("未対応の出力形式です。") };
        var files = ImageConverterService.Scan(args.Skip(index + 2));
        if (files.Count == 0) throw new FileNotFoundException("対応する画像ファイルが見つかりません。");
        var ok = 0; var fail = 0;
        foreach (var file in files) { if (ImageConverterService.Convert(file, format, 85, true, null, null, true, null).Success) ok++; else fail++; }
        WpfMessageBox.Show($"{ok} 件の変換が完了しました。{(fail > 0 ? $" {fail} 件は失敗しました。" : "")}", "変換完了");
        return true;
    }
}
