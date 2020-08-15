using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Configuration;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Author: Cornelis Kuijpers

namespace Hyperboliq_Assesment
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // args 1 : FileName
                // args 2 - ... Image set for rest of images 

                string filename = "";
                string answer = "";

                if (args.Length == 0)
                {
                    // Get Main File if not provided through arg
                    System.Console.WriteLine("Please enter path to main image file");
                    filename = System.Console.ReadLine();

                    // Get image set if not provided else create image set of main image
                    System.Console.WriteLine($"Please enter path to image folder for the image dataset alternatively 'C' to continue");
                    answer = System.Console.ReadLine();

                    Image img = Image.FromFile(filename);
                    int widthThird = (int)((double)img.Width / 20.0 + 0.5);
                    int heightThird = (int)((double)img.Height / 20.0 + 0.5);
                    Bitmap[,] bmps = new Bitmap[20, 20];
                    for (int i = 0; i < 20; i++)
                        for (int j = 0; j < 20; j++)
                        {
                            bmps[i, j] = new Bitmap(widthThird, heightThird);

                            Graphics g = Graphics.FromImage(bmps[i, j]);
                            g.DrawImage(img, new Rectangle(0, 0, widthThird, heightThird), new Rectangle(j * widthThird, i * heightThird, widthThird, heightThird), GraphicsUnit.Pixel);

                            g.Dispose();

                            if (answer == "c")
                                bmps[i, j].Save($"C:\\Users\\Christelle.Swanepoel\\Desktop\\Cor\\imageSet\\{i}{j}.jpg", ImageFormat.Jpeg);

                        }

                }

                System.Console.WriteLine("All done");
                System.Console.ReadLine();

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                System.Console.ReadLine();
            }

        }
    }
}
