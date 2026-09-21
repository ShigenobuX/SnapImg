namespace ImgConv;

public sealed class AppSettings
{
    public string Theme { get; set; } = "System";
    public double Width { get; set; } = 950;
    public double Height { get; set; } = 700;
}

public static class AppInfo
{
    public const string Version = "0.9.0";
    public static readonly string[] InputExtensions = [".heic", ".heif", ".png", ".jpg", ".jpeg", ".webp", ".bmp", ".tif", ".tiff", ".gif", ".ico"];
}
