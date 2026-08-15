using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DriftLift.Services
{
    public static class GameIconExtractor
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int ExtractIconEx(string lpszFile, int nIconIndex, IntPtr[]? phiconLarge, IntPtr[]? phiconSmall, int nIcons);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private static readonly ConcurrentDictionary<string, ImageSource?> _iconCache = new(StringComparer.OrdinalIgnoreCase);

        public static ImageSource? GetExecutableIcon(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;

            if (_iconCache.TryGetValue(path, out var cached)) return cached;

            try
            {
                IntPtr[] largeIcons = new IntPtr[1];
                int count = ExtractIconEx(path, 0, largeIcons, null, 1);
                if (count > 0 && largeIcons[0] != IntPtr.Zero)
                {
                    try
                    {
                        var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                            largeIcons[0],
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                        bitmapSource.Freeze();
                        _iconCache[path] = bitmapSource;
                        return bitmapSource;
                    }
                    finally
                    {
                        DestroyIcon(largeIcons[0]);
                    }
                }
            }
            catch { }

            _iconCache[path] = null;
            return null;
        }
    }
}
