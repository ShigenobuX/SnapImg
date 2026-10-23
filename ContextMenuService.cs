using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Principal;

namespace ImgConv;

public enum ContextMenuRegistrationState
{
    Unregistered,
    Registered,
    Incomplete
}

public static class ContextMenuService
{
    private const string MenuBase = @"Software\Classes\*\shell";
    private const string LegacyMenuBase = @"Software\Classes\SystemFileAssociations\image\shell";
    private const string CommandStoreBase = @"Software\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell";
    private const string MenuName = "SnapImg";
    private static readonly (string Key, string Label, string Ext)[] Formats =
        [("SnapImg_Jpg", "JPGに変換 (品質85%)", "jpg"), ("SnapImg_Png", "PNGに変換", "png"), ("SnapImg_Webp", "WEBPに変換", "webp"), ("SnapImg_Avif", "AVIFに変換", "avif"), ("SnapImg_Bmp", "BMPに変換", "bmp")];

    public static bool RegisterWithElevation() => RunElevated("--register-menu");
    public static bool UnregisterWithElevation() => RunElevated("--unregister-menu");

    public static ContextMenuRegistrationState GetRegistrationState()
    {
        using var menu = Registry.LocalMachine.OpenSubKey($"{MenuBase}\\{MenuName}");
        if (menu is null) return ContextMenuRegistrationState.Unregistered;

        var expectedSubCommands = string.Join(';', Formats.Select(x => x.Key));
        if (!string.Equals(menu.GetValue("SubCommands") as string, expectedSubCommands, StringComparison.Ordinal))
            return ContextMenuRegistrationState.Incomplete;

        using var commandStore = Registry.LocalMachine.OpenSubKey(CommandStoreBase);
        if (commandStore is null) return ContextMenuRegistrationState.Incomplete;

        foreach (var item in Formats)
        {
            using var command = commandStore.OpenSubKey($"{item.Key}\\command");
            if (command?.GetValue(null) is not string value || !value.Contains($"--convert-to {item.Ext}", StringComparison.OrdinalIgnoreCase))
                return ContextMenuRegistrationState.Incomplete;
        }

        return ContextMenuRegistrationState.Registered;
    }

    public static void Register()
    {
        var exe = Environment.ProcessPath ?? throw new InvalidOperationException("実行ファイルの場所を取得できません。");
        Unregister();

        using var menuRoot = Registry.LocalMachine.CreateSubKey(MenuBase)!;
        using (var menu = menuRoot.CreateSubKey(MenuName)!)
        {
            menu.SetValue("MUIVerb", "SnapImg");
            menu.SetValue("Icon", $"{exe},0");
            menu.SetValue("SubCommands", string.Join(';', Formats.Select(x => x.Key)));
        }

        using var commandStore = Registry.LocalMachine.CreateSubKey(CommandStoreBase)!;
        foreach (var item in Formats)
        {
            using var commandItem = commandStore.CreateSubKey(item.Key)!;
            commandItem.SetValue("MUIVerb", item.Label);
            commandItem.SetValue("Icon", $"{exe},0");
            using var command = commandItem.CreateSubKey("command")!;
            command.SetValue(null, $"\"{exe}\" --convert-to {item.Ext} \"%1\"");
        }
    }

    public static void Unregister()
    {
        Registry.LocalMachine.DeleteSubKeyTree($"{MenuBase}\\{MenuName}", false);
        Registry.LocalMachine.DeleteSubKeyTree($"{LegacyMenuBase}\\{MenuName}", false);
        foreach (var item in Formats)
            Registry.LocalMachine.DeleteSubKeyTree($"{CommandStoreBase}\\{item.Key}", false);

        RemoveUserKey($"{MenuBase}\\{MenuName}");
        RemoveUserKey($"{LegacyMenuBase}\\{MenuName}");
        foreach (var legacy in new[] { "ImageFormatConvert_jpg", "ImageFormatConvert_png", "ImageFormatConvert_webp", "ImageFormatConvert_avif", "ImageFormatConvert_bmp", "ImageFormatConvert" })
        {
            RemoveUserKey($"{MenuBase}\\{legacy}");
            RemoveUserKey($"{LegacyMenuBase}\\{legacy}");
        }
    }

    private static void RemoveUserKey(string key) => Registry.CurrentUser.DeleteSubKeyTree(key, false);

    private static bool RunElevated(string argument)
    {
        if (IsElevated())
        {
            if (argument == "--register-menu") Register(); else Unregister();
            return true;
        }

        var exe = Environment.ProcessPath ?? throw new InvalidOperationException("実行ファイルの場所を取得できません。");
        try
        {
            using var process = Process.Start(new ProcessStartInfo(exe, argument) { UseShellExecute = true, Verb = "runas" });
            process?.WaitForExit();
            return process?.ExitCode == 0;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return false;
        }
    }

    private static bool IsElevated()
    {
        using var identity = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    }
}
