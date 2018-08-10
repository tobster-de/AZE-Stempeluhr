using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace AZE.Resources
{
    public static class FlagIcons
    {
        /// <summary>
        /// Gets the resources dictionary.
        /// </summary>
        private static ResourceDictionary Resources { get; set; }

        static FlagIcons()
        {
            FlagIcons.Resources = new ResourceDictionary();
            FlagIcons.Resources.Source = new Uri("/AZE Stempeluhr;component/Resources/FlagIcons.xaml", UriKind.RelativeOrAbsolute);
        }

        /// <summary>
        /// Gets the <see cref="Image"/> for the specified key.
        /// </summary>
        /// <param name="item">The name of the resource key.</param>
        /// <returns>The image for the key, if found, otherwise null.</returns>
        public static Image GetImage(string item)
        {
            Debug.Assert(!string.IsNullOrEmpty(item));
            return (FlagIcons.Resources.Contains(item) ? (Image)FlagIcons.Resources[item] : null);
        }

        /// <summary>
        /// Gets the contains result for the specified key.
        /// </summary>
        /// <param name="item">The name of the resource key.</param>
        /// <returns>True, if specific image found, otherwise false.</returns>
        public static bool HasSpecificImage(string item)
        {
            Debug.Assert(!string.IsNullOrEmpty(item));
            return FlagIcons.Resources.Contains(item);
        }

        public static Image German => (Image)FlagIcons.Resources["German"];

        public static Image English => (Image)FlagIcons.Resources["English"];

        /*
        /// <summary>
        /// Adds an overlay image to an image, located at the lower left.
        /// </summary>
        /// <param name="image">The base image.</param>
        /// <param name="overlay">The overlay image.</param>
        /// <returns>The given image with overlay image, located at the lower left.</returns>
        public static Image AddOverlay(Image image, Image overlay)
        {
            return TreeIcons.AddOverlay(image, overlay, HorizontalAlignment.Left, VerticalAlignment.Bottom);
        }

        /// <summary>
        /// Adds an overlay image to an image with the given alignment.
        /// </summary>
        /// <param name="image">The base image.</param>
        /// <param name="overlay">The overlay image.</param>
        /// <param name="horizontalAlignment">The horizontal alignment of the overlay image.</param>
        /// <param name="verticalAlignment">The vertical alignment of the overlay image.</param>
        /// <returns>The given image with overlay image.</returns>
        public static Image AddOverlay(Image image, Image overlay, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
        {
            // validate params
            //DoAssert.IfArgumentNull(image, "image");
            //DoAssert.IfArgumentNull(overlay, "overlay");

            ImageSource source = TreeIcons.AddOverlay(image.Source, overlay.Source, horizontalAlignment, verticalAlignment);
            return new Image { Source = source };
        }

        /// <summary>
        /// Adds an overlay image to an image, located at the lower left.
        /// </summary>
        /// <param name="image">The base image.</param>
        /// <param name="overlay">The overlay image.</param>
        /// <returns>The given image with overlay image, located at the lower left.</returns>
        public static ImageSource AddOverlay(ImageSource image, ImageSource overlay)
        {
            return TreeIcons.AddOverlay(image, overlay, HorizontalAlignment.Left, VerticalAlignment.Bottom);
        }

        /// <summary>
        /// Adds an overlay image to an image with the given alignment.
        /// </summary>
        /// <param name="image">The base image.</param>
        /// <param name="overlay">The overlay image.</param>
        /// <param name="horizontalAlignment">The horizontal alignment of the overlay image.</param>
        /// <param name="verticalAlignment">The vertical alignment of the overlay image.</param>
        /// <returns>The given image with overlay image.</returns>
        public static ImageSource AddOverlay(ImageSource image, ImageSource overlay, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
        {
            // validate params
            DoAssert.IfArgumentNull(image, "image");
            DoAssert.IfArgumentNull(overlay, "overlay");

            // already cached?
            int hash = image.GetHashCode() ^ overlay.GetHashCode();
            if (TreeIcons.Overlays.ContainsKey(hash))
            {
                return TreeIcons.Overlays[hash];
            }

            // assemble image
            DrawingGroup group = new DrawingGroup();
            group.Children.Add(new ImageDrawing(image, new Rect(0, 0, image.Width, image.Height)));

            // position of overlay
            double x = 0, y = 0;
            double width = overlay.Width, height = overlay.Height;
            switch (horizontalAlignment)
            {
                case HorizontalAlignment.Center:
                    x = (image.Width - overlay.Width) / 2;
                    break;
                case HorizontalAlignment.Right:
                    x = image.Width - overlay.Width;
                    break;
                case HorizontalAlignment.Stretch:
                    width = image.Width;
                    break;
                case HorizontalAlignment.Left:
                default:
                    break;
            }

            switch (verticalAlignment)
            {
                case VerticalAlignment.Center:
                    y = (image.Height - overlay.Height) / 2;
                    break;
                case VerticalAlignment.Bottom:
                    y = image.Height - overlay.Height;
                    break;
                case VerticalAlignment.Stretch:
                    height = image.Height;
                    break;
                case VerticalAlignment.Top:
                default:
                    break;
            }
            group.Children.Add(new ImageDrawing(overlay, new Rect(x, y, width, height)));

            // cache final image
            DrawingImage finalImage = new DrawingImage(group);
            TreeIcons.Overlays[hash] = finalImage;
            return finalImage;
        }
        */
    }
}
