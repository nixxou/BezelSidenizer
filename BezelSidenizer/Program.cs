using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

internal class Program
{
	private static void Main(string[] args)
	{
		/*
		List<string> fakeArgs = new List<string>();
		fakeArgs.Add(@"E:\TODO\BezelRetroarch\BezelProjectSidenized\Originals\GameBezels\Saturn\Policenauts (Japan) (Disc 1) (Translated).png");
		fakeArgs.Add("70");
		fakeArgs.Add(@"E:\TODO\out2.png");
		args = fakeArgs.ToArray();
		*/
		

		if (args.Length < 3)
		{
			Console.WriteLine("Usage: BezelSidenizer.exe bezelPath opacityPercent/wide outPath");
			return;
		}

		bool wide = false;


		string bezelPath = args[0];
		int opacityPercent = 100;
		if (args[1] == "wide")
		{
			wide = true;
		}
		else
		{
			if (!int.TryParse(args[1], out opacityPercent) || opacityPercent < 0 || opacityPercent > 100)
			{
				Console.WriteLine("Invalid opacity percent. It should be an integer between 0 and 100.");
				return;
			}
		}

		string outPath = args.Length > 2 ? args[2] : bezelPath;

		if (!CheckImage(bezelPath, 1920, 1080))
		{
			Console.WriteLine("The bezel image does not have the expected size (1920x1080) or is not a PNG file.");
			return;
		}

		string programDirectory = AppDomain.CurrentDomain.BaseDirectory;

		string sidenPath = Path.Combine(programDirectory, "Siden.png");
		if(wide) sidenPath = Path.Combine(programDirectory, "SidenWide.png");

		if (!CheckImageSize(bezelPath, 1920, 1080))
		{
			Console.WriteLine("Image size is not 1920 x 1080");
			return;
		}

		Console.WriteLine($"o={opacityPercent} {outPath}");
		if (wide)
		{
			OverlayImagesWide(bezelPath, sidenPath, outPath, opacityPercent);
		}
		else
		{
			OverlayImages(bezelPath, sidenPath, outPath, opacityPercent);
		}
		

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

	static void OverlayImagesWide(string baseImagePath, string overlayImagePath, string outputPath, int opacityPercent)
	{

		var rect1 = new Rectangle(22, 11, 217, 1080 - 22);
		var rect2 = new Rectangle(1703 - 22, 11, 217, 1080 - 22);

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


			int width = 217;
			int height = 1080;


			var sourceRect = new Rectangle(0, 0, width, height);
			var sourceRect2 = new Rectangle(1920 - 217, 0, width, height);
			graphics.DrawImage(baseImage, 22, 0, sourceRect, GraphicsUnit.Pixel);
			graphics.DrawImage(baseImage, (1920 - 217) - 22, 0, sourceRect2, GraphicsUnit.Pixel);


			graphics.CompositingMode = CompositingMode.SourceOver;

			RemoveTransparency((Bitmap)baseImage, 22, 0,width, height);
			RemoveTransparency((Bitmap)baseImage, (1920 - 217) - 22, 0, width, height);


			// Rendez la région spécifiée transparente
			for (int x = transparentRegion.X; x < transparentRegion.X + transparentRegion.Width; x++)
			{
				for (int y = transparentRegion.Y; y < transparentRegion.Y + transparentRegion.Height; y++)
				{
					((Bitmap)baseImage).SetPixel(x, y, Color.Transparent);
				}
			}

			graphics.DrawImage(overlayImage, new Rectangle(0, 0, baseImage.Width, baseImage.Height), new Rectangle(0, 0, overlayImage.Width, overlayImage.Height), GraphicsUnit.Pixel);
			
			// Enregistrez le résultat dans le fichier de sortie
			baseImage.Save(outputPath, ImageFormat.Png);
		}
	}

	static void RemoveTransparency(Bitmap image, int x, int y, int width, int height)
	{
		// Vérifier si l'image a une transparence
		if (image.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
		{
			// Itérer à travers les pixels de la zone spécifiée
			for (int i = x; i < x + width; i++)
			{
				for (int j = y; j < y + height; j++)
				{
					// Obtenir la couleur du pixel
					Color pixelColor = image.GetPixel(i, j);

					// Enlever la transparence en définissant l'alpha à 255 (complètement opaque)
					pixelColor = Color.FromArgb(255, pixelColor.R, pixelColor.G, pixelColor.B);

					// Définir la nouvelle couleur du pixel
					image.SetPixel(i, j, pixelColor);
				}
			}
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