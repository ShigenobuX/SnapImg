using ImageMagick;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using WpfClipboard = System.Windows.Clipboard;
using WpfDataObject = System.Windows.DataObject;

namespace ImgConv;

public static class ClipboardImageService
{
    public static (bool Success, string Message) Copy(string input)
    {
        try
        {
            using var image = ImageConverterService.ReadImage(input);

            // Match the converter's known-stable orientation behavior for HEIC/HEIF.
            var extension = Path.GetExtension(input);
            if (!extension.Equals(".heic", StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(".heif", StringComparison.OrdinalIgnoreCase))
            {
                image.AutoOrient();
            }

            image.Format = MagickFormat.Png;
            var png = image.ToByteArray();
            using var stream = new MemoryStream(png);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();

            // The clipboard can be briefly locked by another process.
            for (var attempt = 0; ; attempt++)
            {
                try
                {
                    var data = new WpfDataObject();
                    data.SetImage(bitmap);
                    // Persist the rendered pixels after this short-lived
                    // Explorer-launched process exits.
                    WpfClipboard.SetDataObject(data, true);
                    break;
                }
                catch (ExternalException) when (attempt < 4)
                {
                    Thread.Sleep(100);
                }
            }

            return (true, "画像をクリップボードにコピーしました。貼り付け先で Ctrl+V を押してください。");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
