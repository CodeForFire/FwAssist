using Avalonia.Media.Imaging;

namespace FwAssist.Acceptance.Tests;

internal static class BitmapSaveExtensions
{
    public static void SavePng(this Bitmap bitmap, string path) =>
        bitmap.Save(path, new PngBitmapEncoderOptions());
}
