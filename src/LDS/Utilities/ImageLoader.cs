using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;
using System.Reflection;
using Windows.Storage.Streams;

namespace LDS.UI.Utilities;

class ImageLoader : BitmapSource
{
    static readonly Assembly _assembly = typeof(ImageLoader).Assembly;
    public string Name {
        set {
            using Stream? stream = _assembly.GetManifestResourceStream(value);

            if (stream is null) {
                return;
            }

            IRandomAccessStream ras = stream.AsRandomAccessStream();

            SetSource(ras);
        }
    }
}
