using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Configuration;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

// Author: Cornelis Kuijpers

namespace Hyperboliq_Assesment
{

    class Program
    {

        //Global Variables
        private static bool hasImageSet = true;
        private static List<KeyValuePair<Image, Color>> originalSplit = new List<KeyValuePair<Image, Color>>();
        private static List<KeyValuePair<Image, Color>> imagesSplit = new List<KeyValuePair<Image, Color>>();
        private static string directory = "";

        private static void printInfo()
        {

            Console.WriteLine("This application was written by Cornelis Kuijpers \n" +
                "Argument 1 is the path to the image you would wish to use path\\*.jpg \n" +
                "Argument 2 is the path to the image set bmp files for example path\\ \n" +
                "if no arguments is supplied it will be asked to supply information \n" +
                "if only 1 argument is supplied (This must be the image) then a image set will be created");

        }

        static void Main(string[] args)
        {
            try
            {
                printInfo();
                // args 1 : FileName
                // args 2 - ... Image set location for rest of images 

                string filename = "";
                string answer = "";

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
                    {
                        hasImageSet = false;
                    }
                    else
                    {
                        imagesSplit = loadSplitImagesFromFolder(answer);
                    }
                        
                    SplitImage(filename, directory);

                    if (!hasImageSet)
                        imagesSplit = loadSplitImagesFromFolder($"{directory}\\imageSet");

                }
                else if (args.Length == 1)
                {
                    // This is when only 1 Argument is supplied and this will be the filename
                    filename = args[0];
                    directory = System.IO.Path.GetDirectoryName(filename);

                    hasImageSet = false;

                    SplitImage(filename, directory);

                    imagesSplit = loadSplitImagesFromFolder($"{directory}\\imageSet");
                }
                else
                {

                    // This is when both Arguments is supplied arg 1 = filename, arg 2 = imageset folder
                    filename = args[0];
                    directory = System.IO.Path.GetDirectoryName(filename);

                    hasImageSet = true;

                    SplitImage(filename, directory);

                    imagesSplit = loadSplitImagesFromFolder(args[1]);

                }

                //Get average RGB for split and original image
                imagesSplit = GetAvgRGB(imagesSplit);
                originalSplit = GetAvgRGB(originalSplit);

                //Get Distance and rebuild image
                GetDistances(imagesSplit, originalSplit);

                System.Console.WriteLine("All done");
                System.Console.ReadLine();

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                System.Console.ReadLine();
            }

        }

        private static void GetDistances(List<KeyValuePair<Image, Color>> myImages, List<KeyValuePair<Image, Color>> originalImages)
        {
            try
            {
                //Create list that contains image in order (Added Color to debug color averages)
                Image[] myFinalImages = new Image[originalImages.Count];
                Color[] myFinalRGBAvg = new Color[originalImages.Count];

                string[] results = new string[400];

                bool hasPair = false;

                int k = 0;

                foreach (KeyValuePair<Image, Color> valuePair in myImages)
                {

                    hasPair = false;

                    for (int i = 0; i < originalImages.Count; i++)
                    {
                        KeyValuePair<Image, Color> originalValuePair = originalImages[i];

                        
                        if (Calculated_distance(valuePair.Value, originalValuePair.Value) == 0)
                        {
                            //This check is for when a tile has the exact same average RGB then a previous tile
                            if (myFinalImages[i] == null)
                            {
                                myFinalImages[i] = valuePair.Key;
                                myFinalRGBAvg[i] = valuePair.Value;
                                hasPair = true;

                                break;
                            }
                            else
                            {
                                //System.Console.WriteLine($"Duplicate {k} : {i} : {valuePair.Value} : {originalValuePair.Value}");
                            }
                        }

                    }

                }



                RebuildImage(myFinalImages);

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

        }

        private static double Calculated_distance(Color split, Color original)
        {
            double returnInt = 0;

            //first the RGB needs to be converted to lab, this process is from RGB -> XYZ -> Lab
            float[] color1 = RGBToLab(split);
            float[] color2 = RGBToLab(original);

            float L1 = color1[0];
            float L2 = color2[0];
            float a1 = color1[1];
            float a2 = color2[1];
            float b1 = color1[2];
            float b2 = color2[2];

            //Calculation to use
            //CIE - L * 1, CIE - a * 1, CIE - b * 1          //Color #1 CIE-L*ab values
            //CIE - L * 2, CIE - a * 2, CIE - b * 2          //Color #2 CIE-L*ab values

            //Delta E* = sqrt(((CIE - L * 1 - CIE - L * 2) ^ 2)
            //               + ((CIE - a * 1 - CIE - a * 2) ^ 2)
            //               + ((CIE - b * 1 - CIE - b * 2) ^ 2))

            returnInt = Math.Sqrt(Math.Pow((L1 - L2), 2) + Math.Pow((a1 - a2), 2) + Math.Pow((b1 - b2), 2));

            return returnInt;
        }

        public static float[] RGBToLab(Color color)
        {
            float[] xyz = new float[3];
            float[] lab = new float[3];
            float[] rgb = new float[] { color.R, color.G, color.B };

            rgb[0] = color.R / 255.0f;
            rgb[1] = color.G / 255.0f;
            rgb[2] = color.B / 255.0f;

            if (rgb[0] > .04045f)
            {
                rgb[0] = (float)Math.Pow((rgb[0] + .055) / 1.055, 2.4);
            }
            else
            {
                rgb[0] = rgb[0] / 12.92f;
            }

            if (rgb[1] > .04045f)
            {
                rgb[1] = (float)Math.Pow((rgb[1] + .055) / 1.055, 2.4);
            }
            else
            {
                rgb[1] = rgb[1] / 12.92f;
            }

            if (rgb[2] > .04045f)
            {
                rgb[2] = (float)Math.Pow((rgb[2] + .055) / 1.055, 2.4);
            }
            else
            {
                rgb[2] = rgb[2] / 12.92f;
            }
            rgb[0] = rgb[0] * 100.0f;
            rgb[1] = rgb[1] * 100.0f;
            rgb[2] = rgb[2] * 100.0f;


            xyz[0] = ((rgb[0] * .412453f) + (rgb[1] * .357580f) + (rgb[2] * .180423f));
            xyz[1] = ((rgb[0] * .212671f) + (rgb[1] * .715160f) + (rgb[2] * .072169f));
            xyz[2] = ((rgb[0] * .019334f) + (rgb[1] * .119193f) + (rgb[2] * .950227f));


            xyz[0] = xyz[0] / 95.047f;
            xyz[1] = xyz[1] / 100.0f;
            xyz[2] = xyz[2] / 108.883f;

            if (xyz[0] > .008856f)
            {
                xyz[0] = (float)Math.Pow(xyz[0], (1.0 / 3.0));
            }
            else
            {
                xyz[0] = (xyz[0] * 7.787f) + (16.0f / 116.0f);
            }

            if (xyz[1] > .008856f)
            {
                xyz[1] = (float)Math.Pow(xyz[1], 1.0 / 3.0);
            }
            else
            {
                xyz[1] = (xyz[1] * 7.787f) + (16.0f / 116.0f);
            }

            if (xyz[2] > .008856f)
            {
                xyz[2] = (float)Math.Pow(xyz[2], 1.0 / 3.0);
            }
            else
            {
                xyz[2] = (xyz[2] * 7.787f) + (16.0f / 116.0f);
            }

            lab[0] = (116.0f * xyz[1]) - 16.0f;
            lab[1] = 500.0f * (xyz[0] - xyz[1]);
            lab[2] = 200.0f * (xyz[1] - xyz[2]);

            return lab;

        }

        private static void RebuildImage(Image[] myImages)
        {
            int column = 0;
            int row = 0;
            int cal_width = 0;
            int cal_height = 0;

            foreach (Image myImagesKV in myImages)
            {
                if (myImagesKV == null)
                    continue;

                if (column < 20 && row == 0)
                    cal_width += myImagesKV.Width;

                if (column == 19)
                {
                    ++row;
                    column = 0;
                    cal_height += myImagesKV.Height;
                }

                ++column;

            }

            int current_width = 0;
            int current_height = 0;

            using (Bitmap bmp = new Bitmap(cal_width, cal_height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {

                    column = 0;
                    row = 0;

                    foreach (Image myImagesKV in myImages)
                    {
                        if (myImagesKV == null)
                            continue;

                        if (column == 20)
                        {
                            int a = 1;
                        }

                        if (column < 20)
                        {
                            g.DrawImage(myImagesKV, current_width, current_height, myImagesKV.Width, myImagesKV.Height);
                            current_width += myImagesKV.Width;
                        }
                        else
                        {
                            current_height += myImagesKV.Height;
                            current_width = 0;
                            column = 0;
                            g.DrawImage(myImagesKV, current_width, current_height, myImagesKV.Width, myImagesKV.Height);
                            current_width += myImagesKV.Width;
                        }

                        ++column;

                    }

                }

                bmp.Save($"{directory}\\rebuilt.jpg");
            }


        }

        private static Color GetAverageColor(Bitmap bm)
        {
            Rectangle bounds = new Rectangle(0, 0, bm.Width, bm.Height);
            BitmapData bmd = bm.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            // The stride is the width of 1 row of pixels in bytes. As 1 pixels requires 3 bytes of color   
            // information, you would think this would always be 3 * bm.Width - But it isn't. Each row of  
            // pixels is aligned so that it starts at a 4 byte boundary, this is done by padding rows with   
            // extra bytes if required. (might be 8 byte boundary on x64)          
            int stride = bmd.Stride;
            // An array to store the color information:  
            byte[] pixels = new byte[bmd.Stride * bm.Height - 1 + 1];
            // Copy it all out of the bitmap:  
            Marshal.Copy(bmd.Scan0, pixels, 0, pixels.Length);
            bm.UnlockBits(bmd);
            int totalR = 0;
            int totalG = 0;
            int totalB = 0;
            for (int y = 0; y <= bm.Height - 1; y++)
            {
                for (int x = 0; x <= bm.Width - 1; x++)
                {
                    // Get the index of a pixel in the array.  
                    // The index will be the number of bytes in all the rows above the pixel,   
                    // which is (y * stride)   
                    // plus the number of bytes in all the pixels to the left of it   
                    // so add x*3:  
                    int index = (y * stride) + (x * 3);
                    totalB += pixels[index];
                    totalG += pixels[index + 1];
                    totalR += pixels[index + 2];
                }
            }
            // Average the components  
            int pixelCount = bm.Width * bm.Height;
            int averageR = System.Convert.ToInt32(totalR / pixelCount);
            int averageG = System.Convert.ToInt32(totalG / pixelCount);
            int averageB = System.Convert.ToInt32(totalB / pixelCount);
            return Color.FromArgb(averageR, averageG, averageB);
        }

        private static List<KeyValuePair<Image, Color>> GetAvgRGB(List<KeyValuePair<Image, Color>> myImages)
        {
            try
            {
                //because KeyValuePair<Image, Color> is immutable it has to be recreated
                List<KeyValuePair<Image, Color>> returnList = new List<KeyValuePair<Image, Color>>();

                foreach (KeyValuePair<Image, Color> myImagesKV in myImages)
                {

                    Bitmap orig = new Bitmap(myImagesKV.Key);

                    returnList.Add(new KeyValuePair<Image, Color>(myImagesKV.Key, GetAverageColor(orig)));

                }
                return returnList;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return myImages;
            }

        }

        private static List<KeyValuePair<Image, Color>> loadSplitImagesFromFolder(string mydirectory)
        {

            List<KeyValuePair<Image, Color>> returnList = new List<KeyValuePair<Image, Color>>();

            try
            {
                foreach (string file in System.IO.Directory.GetFiles(mydirectory, "*.bmp"))
                {

                    Image img = Image.FromFile(file);
                    returnList.Add(new KeyValuePair<Image, Color>(img, Color.Black));

                }

            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            return returnList;

        }

        private static void SplitImage(string filename, string mydirectory)
        {
            try
            {

                int c = 0;

                Image img = Image.FromFile(filename);
                int widthThird = (int)((double)img.Width / 20);
                int heightThird = (int)((double)img.Height / 20);
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
                            if (!System.IO.Directory.Exists($"{ mydirectory}\\imageSet"))
                                System.IO.Directory.CreateDirectory($"{mydirectory}\\imageSet");

                            bmps[i, j].Save($"{mydirectory}\\imageSet\\{Guid.NewGuid().ToString()}.bmp");

                            originalSplit.Add(new KeyValuePair<Image, Color>(bmps[i, j], Color.Black));

                        }
                        else
                        {
                            originalSplit.Add(new KeyValuePair<Image, Color>(bmps[i, j], Color.Black));
                        }

                    }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

    }

}
