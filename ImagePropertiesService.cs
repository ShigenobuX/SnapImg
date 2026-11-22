using ImageMagick;
using System.Globalization;
using System.IO;
using System.Windows.Media.Imaging;

namespace ImgConv;

public sealed record PropertyRow(string Label, string Value);

public sealed class ImageFileProperties
{
    public required string FilePath { get; init; }
    public required BitmapSource Thumbnail { get; init; }
    public required IReadOnlyList<PropertyRow> FileDetails { get; init; }
    public required IReadOnlyList<PropertyRow> ImageDetails { get; init; }
}

public static class ImagePropertiesService
{
    public static ImageFileProperties Read(string filePath)
    {
        var file = new FileInfo(filePath);
        var imageInfos = MagickImageInfo.ReadCollection(file.FullName).ToList();
        if (imageInfos.Count == 0)
            throw new InvalidDataException("画像情報を読み取れませんでした。");

        // Depth は簡易情報に含まれないため、先頭フレームだけを読み込みます。
        using var image = new MagickImage(file.FullName);
        var inputExtension = Path.GetExtension(file.FullName);
        if (!inputExtension.Equals(".heic", StringComparison.OrdinalIgnoreCase) &&
            !inputExtension.Equals(".heif", StringComparison.OrdinalIgnoreCase))
        {
            image.AutoOrient();
        }

        var bitDepth = image.Depth;
        var info = imageInfos[0];
        var dpi = "未設定";
        if (info.Density is { } density)
        {
            dpi = density.Units switch
            {
                DensityUnit.PixelsPerInch => $"{density.X.ToString("0.#", CultureInfo.CurrentCulture)} × {density.Y.ToString("0.#", CultureInfo.CurrentCulture)} dpi",
                DensityUnit.PixelsPerCentimeter => $"{density.X.ToString("0.#", CultureInfo.CurrentCulture)} × {density.Y.ToString("0.#", CultureInfo.CurrentCulture)} px/cm",
                _ => dpi
            };
        }

        var thumbnail = CreateThumbnail(image);

        return new ImageFileProperties
        {
            FilePath = file.FullName,
            Thumbnail = thumbnail,
            FileDetails =
            [
                new("ファイル名", file.Name),
                new("保存場所", file.DirectoryName ?? string.Empty),
                new("ファイル形式", $"{Path.GetExtension(file.Name).TrimStart('.').ToUpperInvariant()} ({info.Format})"),
                new("ファイルサイズ", FormatFileSize(file.Length)),
                new("更新日時", file.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.CurrentCulture))
            ],
            ImageDetails =
            [
                new("画像サイズ", $"{info.Width:N0} × {info.Height:N0} ピクセル"),
                new("縦横比", FormatAspectRatio(info.Width, info.Height)),
                new("解像度", dpi),
                new("色深度", $"{bitDepth} bit"),
                new("フレーム数", $"{imageInfos.Count:N0}")
            ]
        };
    }

    private static string FormatAspectRatio(uint width, uint height)
    {
        if (width == 0 || height == 0)
            return "不明";

        var divisor = GreatestCommonDivisor(width, height);
        var exactWidth = width / divisor;
        var exactHeight = height / divisor;
        var exact = $"{exactWidth}:{exactHeight}";

        const uint approximateShortSide = 9;
        var approximateWidth = width <= height
            ? approximateShortSide
            : Math.Max(1u, (uint)Math.Round(width * (double)approximateShortSide / height));
        var approximateHeight = height < width
            ? approximateShortSide
            : Math.Max(1u, (uint)Math.Round(height * (double)approximateShortSide / width));
        var approximateDivisor = GreatestCommonDivisor(approximateWidth, approximateHeight);
        var approximate = $"{approximateWidth / approximateDivisor}:{approximateHeight / approximateDivisor}";

        return exact == approximate ? exact : $"{exact} ({approximate})";
    }

    private static uint GreatestCommonDivisor(uint a, uint b)
    {
        while (b != 0)
            (a, b) = (b, a % b);

        return a;
    }

    private static BitmapSource CreateThumbnail(MagickImage image)
    {
        const uint maxSide = 256;
        var scale = Math.Min(1d, maxSide / (double)Math.Max(image.Width, image.Height));
        var width = Math.Max(1u, (uint)Math.Round(image.Width * scale));
        var height = Math.Max(1u, (uint)Math.Round(image.Height * scale));
        image.Resize(width, height);
        image.Format = MagickFormat.Png;

        using var stream = new MemoryStream(image.ToByteArray());
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = stream;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    private static string FormatFileSize(long bytes)
    {
        const double kb = 1024;
        const double mb = kb * 1024;
        const double gb = mb * 1024;
        return bytes switch
        {
            >= (long)gb => $"{bytes / gb:N2} GB",
            >= (long)mb => $"{bytes / mb:N2} MB",
            >= (long)kb => $"{bytes / kb:N2} KB",
            _ => $"{bytes:N0} バイト"
        };
    }
}
