using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace PrintImage
{
    [Cmdlet(VerbsCommon.Show, "Image")]
    public class ShowImage: Cmdlet
    {
        [Parameter(Mandatory = true)]
        public string ImageFile { get; set; }

        [Parameter(Mandatory = false)]
        public int Width { get; set; } = 80;

        [Parameter(Mandatory = false)]
        public int LoopSeconds { get; set; } = 5;

        [Parameter(Mandatory = false)]
        public double FrameRate { get; set; } = 10;

        private const int TransparentAlphaThreshold = 100;

        private TimeSpan FrameTime {
            get => TimeSpan.FromMilliseconds(1000/FrameRate);
        }

        protected override void ProcessRecord()
        {
            using var image = Image.Load<Rgba32>(ImageFile);
            var width = Width;
            var heightDouble = image.Height * width * 7.0 / (image.Width * 8.0);
            var height = (int)Math.Round(heightDouble / 2) * 2;
            image.Mutate(x => x.Resize(width, height));

            if (image.Frames.Count > 1)
            {
                var frames = new List<string>();
                for (var i = 0; i < image.Frames.Count; i++)
                {
                    var frame = image.Frames.CloneFrame(i);
                    var frameString = ImageString(frame);
                    frames.Add(frameString);
                }

                var start = DateTime.Now;
                do
                {
                    foreach (var frame in frames)
                    {
                        var frameStarted = DateTime.Now;
                        WriteObject($"{frame}\u001B[s\u001B[{height + 1}A");
                        var timeLeft = (int)(FrameTime - (DateTime.Now - frameStarted)).TotalMilliseconds;
                        if (timeLeft > 0)
                        {
                            Thread.Sleep(timeLeft);
                        }
                    }
                } while (DateTime.Now - start < TimeSpan.FromSeconds(LoopSeconds));
                WriteObject("\u001B[u");
            }
            else
            {
                WriteObject(ImageString(image));
            }
        }

        private string ImageString(Image<Rgba32> img)
        {
            var imageString = string.Empty;
            for (var y = 0; y < img.Height; y += 2)
            {
                var wasTrans = false;
                for (var x = 0; x < img.Width; x++)
                {
                    imageString += PixelString(img[x, y], img[x, y + 1], ref wasTrans);
                }
                imageString += "\n";
            }

            return imageString;
        }

        private string PixelString(Rgba32 bg, Rgba32 fg, ref bool wasTrans)
        {
            if (bg.A < TransparentAlphaThreshold && fg.A < TransparentAlphaThreshold) {
                if (wasTrans) {
                    return "·";
                }
                wasTrans = true;
                return "\u001B[0m·";
            }
            else if (bg.A < TransparentAlphaThreshold) {
                wasTrans = false;
                return $"\u001B[0m\u001B[38;2;{fg.R};{fg.G};{fg.B}m▄";
            }
            else if (fg.A < TransparentAlphaThreshold) {
                wasTrans = false;
                return $"\u001B[48;2;{bg.R};{bg.G};{bg.B}m\u001B[30m▄";
            }
            wasTrans = false;
            return $"\u001B[48;2;{bg.R};{bg.G};{bg.B}m\u001B[38;2;{fg.R};{fg.G};{fg.B}m▄";
        }
    }
}
