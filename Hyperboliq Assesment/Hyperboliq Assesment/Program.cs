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

        //Global Variables
        private static bool hasImageSet = true;
        private static Bitmap[,] originalSplit;
        private static Bitmap[,] imagesSplit;

        static void Main(string[] args)
        {
            try
            {
                // args 1 : FileName
                // args 2 - ... Image set location for rest of images 

                string filename = "";
                string answer = "";
                string directory = "";


                if (args.Length == 0)
                {
                    // Get Main File if not provided through arg
                    System.Console.WriteLine("Please enter path to main image file");
                    filename = System.Console.ReadLine();

                    // Get Directory to save split images to and reconstruct original image 
                    directory = System.IO.Path.GetDirectoryName(filename);

                    // Get image set if not provided else create image set of main image
                    System.Console.WriteLine($"Please enter path to image folder for the image dataset alternatively 'C' to continue");
                    answer = System.Console.ReadLine();

                    if (answer.ToLower() == "c")
                        hasImageSet = false;

                    SplitImmage(filename,directory);

                    //TODO : Add Image Set to Array

                }else if (args.Length == 1)
                {
                    // This is when only 1 Argument is supplied and this will be the filename
                    filename = args[0];
                    directory = System.IO.Path.GetDirectoryName(filename);

                    hasImageSet = false;

                    SplitImmage(filename, directory);
                }
                else
                {

                    // This is when both Arguments is supplied arg 1 = filename, arg 2 = imageset folder
                    filename = args[0];
                    directory = System.IO.Path.GetDirectoryName(filename);

                    hasImageSet = true;

                    SplitImmage(filename, directory);

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

        private static void SplitImmage(string filename,string directory)
        {
            originalSplit = new Bitmap[20, 20];
            imagesSplit = new Bitmap[20, 20];

            Image img = Image.FromFile(filename);
            int widthThird = (int)((double)img.Width / 20.0);
            int heightThird = (int)((double)img.Height / 20.0);
            Bitmap[,] bmps = new Bitmap[20, 20];
            for (int i = 0; i < 20; i++)
                for (int j = 0; j < 20; j++)
                {
                    bmps[i, j] = new Bitmap(widthThird, heightThird);

                    Graphics g = Graphics.FromImage(bmps[i, j]);
                    g.DrawImage(img, new Rectangle(0, 0, widthThird, heightThird), new Rectangle(j * widthThird, i * heightThird, widthThird, heightThird), GraphicsUnit.Pixel);

                    g.Dispose();


                    if (!hasImageSet)
                    {
                        if (!System.IO.Directory.Exists($"{ directory}\\imageSet"))
                            System.IO.Directory.CreateDirectory($"{directory}\\imageSet");

                        imagesSplit[i, j] = bmps[i, j];

                        bmps[i, j].Save($"{directory}\\imageSet\\{i}{j}.jpg", ImageFormat.Jpeg);
                    }

                    originalSplit[i, j] = bmps[i, j];
                }

        }

    }

}
