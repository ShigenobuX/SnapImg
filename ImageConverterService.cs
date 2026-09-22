using ImageMagick;
using System.IO;

namespace ImgConv;

public static class ImageConverterService
{
    public static List<string> Scan(IEnumerable<string> paths)
    {
        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in paths)
        {
            var path = raw.Trim().Trim('"');
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)) Add(file);
            }
            else if (File.Exists(path)) Add(path);
        }
        return result;
        void Add(string file)
        {
            if (AppInfo.InputExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase) && seen.Add(file)) result.Add(file);
        }
    }

    public static (bool Success, string Message) Convert(string input, MagickFormat format, int quality, bool preserveExif, int? width, int? height, bool keepAspect, string? outputFolder)
    {
        try
        {
            using var image = new MagickImage(input);
            // Magick.NET の HEIC では AutoOrient が不安定なケースがあるため、
            // HEIC/HEIF では明示的な AutoOrient を行わない。
            var inputExtension = Path.GetExtension(input);
            if (!inputExtension.Equals(".heic", StringComparison.OrdinalIgnoreCase) &&
                !inputExtension.Equals(".heif", StringComparison.OrdinalIgnoreCase))
            {
                image.AutoOrient();
            }
            if (width is > 0 || height is > 0)
            {
                var targetW = width ?? checked((int)image.Width);
                var targetH = height ?? checked((int)image.Height);
                if (keepAspect)
                {
                    var ratio = Math.Min((double)targetW / image.Width, (double)targetH / image.Height);
                    targetW = Math.Max(1, (int)(image.Width * ratio));
                    targetH = Math.Max(1, (int)(image.Height * ratio));
                }
                image.Resize((uint)targetW, (uint)targetH);
            }
            if (!preserveExif) image.Strip();
            if (format == MagickFormat.Jpeg)
            {
                if (image.HasAlpha) { image.BackgroundColor = MagickColors.White; image.Alpha(AlphaOption.Remove); }
                image.Quality = (uint)Math.Clamp(quality, 10, 100);
            }
            else if (format == MagickFormat.WebP) image.Quality = (uint)Math.Clamp(quality, 10, 100);
            image.Format = format;
            var folder = outputFolder ?? Path.GetDirectoryName(input)!;
            Directory.CreateDirectory(folder);
            var ext = format switch { MagickFormat.Jpeg => ".jpg", MagickFormat.Png => ".png", MagickFormat.WebP => ".webp", MagickFormat.Avif => ".avif", _ => ".bmp" };
            var baseName = Path.GetFileNameWithoutExtension(input);
            var output = Path.Combine(folder, baseName + ext);
            for (var i = 1; File.Exists(output); i++) output = Path.Combine(folder, $"{baseName}_{i}{ext}");
            image.Write(output);
            return (true, output);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }
}
