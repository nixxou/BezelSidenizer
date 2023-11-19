using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

internal class Program
{
	private static void Main(string[] args)
	{
		if (args.Length < 2)
		{
			Console.WriteLine("Usage: BezelSidenizer.exe bezelPath opacityPercent [outPath]");
			return;
		}

		string bezelPath = args[0];
		int opacityPercent;

		if (!int.TryParse(args[1], out opacityPercent) || opacityPercent < 0 || opacityPercent > 100)
		{
			Console.WriteLine("Invalid opacity percent. It should be an integer between 0 and 100.");
			return;
		}

		string outPath = args.Length > 2 ? args[2] : bezelPath;

		if (!CheckImage(bezelPath, 1920, 1080))
		{
			Console.WriteLine("The bezel image does not have the expected size (1920x1080) or is not a PNG file.");
			return;
		}

		string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
		
		string sidenPath = Path.Combine(programDirectory, "Siden.png");


		if (!CheckImageSize(bezelPath, 1920, 1080))
		{
			Console.WriteLine("Image size is not 1920 x 1080");
			return;
		}
		OverlayImages(bezelPath, sidenPath, outPath, opacityPercent);

	}


	static bool CheckImage(string imagePath, int expectedWidth, int expectedHeight)
	{
		if (!File.Exists(imagePath))
		{
			Console.WriteLine("The specified bezel image does not exist.");
			return false;
		}

		string extension = Path.GetExtension(imagePath);
		if (!string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase))
		{
			Console.WriteLine("The bezel image must be a PNG file.");
			return false;
		}

		using (var image = Image.FromFile(imagePath))
		{
			return image.Width == expectedWidth && image.Height == expectedHeight;
		}
	}

	static void OverlayImages(string baseImagePath, string overlayImagePath, string outputPath, int opacityPercent)
	{

		var rect1 = new Rectangle(0, 0, 217, 1080);
		var rect2 = new Rectangle(1703, 0, 217, 1080);
		var transparentRegion = new Rectangle(239, 11, 1443, 1057);

		using (var baseImage = Image.FromFile(baseImagePath))
		using (var overlayImage = Image.FromFile(overlayImagePath))
		using (var graphics = Graphics.FromImage(baseImage))
		{
			// Assurez-vous que le format de l'image prend en charge la transparence
			if (baseImage.PixelFormat != PixelFormat.Format32bppArgb)
			{
				throw new ArgumentException("Le format de l'image de base doit être 32 bits par pixel avec canal alpha.", nameof(baseImagePath));
			}

			graphics.CompositingMode = CompositingMode.SourceOver;
			graphics.DrawImage(overlayImage, new Rectangle(0, 0, baseImage.Width, baseImage.Height), new Rectangle(0, 0, overlayImage.Width, overlayImage.Height), GraphicsUnit.Pixel);

			// Rendez la région spécifiée transparente
			for (int x = transparentRegion.X; x < transparentRegion.X + transparentRegion.Width; x++)
			{
				for (int y = transparentRegion.Y; y < transparentRegion.Y + transparentRegion.Height; y++)
				{
					((Bitmap)baseImage).SetPixel(x, y, Color.Transparent);
				}
			}



			int alpha = (int)(opacityPercent / 100.0 * 255);
			using (var brush = new SolidBrush(Color.FromArgb(alpha, Color.Black)))
			{
				graphics.FillRectangle(brush, rect1);
				graphics.FillRectangle(brush, rect2);
			}

			// Enregistrez le résultat dans le fichier de sortie
			baseImage.Save(outputPath, ImageFormat.Png);
		}
	}

	static bool CheckImageSize(string imagePath, int expectedWidth, int expectedHeight)
	{
		using (var image = Image.FromFile(imagePath))
		{
			return image.Width == expectedWidth && image.Height == expectedHeight;
		}
	}

}