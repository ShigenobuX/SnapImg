using Microsoft.Win32;

namespace ImgConv;

public static class ContextMenuService
{
    private const string Base = @"Software\Classes\*\shell";
    private static readonly (string Key, string Label, string Ext)[] Formats =
        [("ImageFormatConvert_jpg", "JPGに変換 (品質85%)", "jpg"), ("ImageFormatConvert_png", "PNGに変換", "png"), ("ImageFormatConvert_webp", "WEBPに変換", "webp"), ("ImageFormatConvert_bmp", "BMPに変換", "bmp")];

    public static void Register(IEnumerable<string>? selected = null)
    {
        var choices = selected?.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? Formats.Select(x => x.Ext).ToHashSet();
        using var root = Registry.CurrentUser.CreateSubKey(Base)!;
        var exe = Environment.ProcessPath ?? throw new InvalidOperationException("実行ファイルの場所を取得できません。");
        foreach (var item in Formats.Where(x => choices.Contains(x.Ext)))
        {
            using var key = root.CreateSubKey(item.Key)!;
            key.SetValue("MUIVerb", item.Label);
            key.SetValue("Icon", $"{exe},0");
            using var command = key.CreateSubKey("command")!;
            command.SetValue(null, $"\"{exe}\" --convert-to {item.Ext} \"%1\"");
        }
    }

    public static void Unregister()
    {
        foreach (var item in Formats) Registry.CurrentUser.DeleteSubKeyTree($"{Base}\\{item.Key}", false);
        Registry.CurrentUser.DeleteSubKeyTree($"{Base}\\ImageFormatConvert", false);
    }
}
