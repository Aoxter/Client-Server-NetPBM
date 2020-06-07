using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Text.RegularExpressions;

namespace IO_projekt_asyn
{
    enum Format
    {
        P1,
        P2,
        P3,
        P4,
        P5,
        P6
    }
    class ImageNetPBM
    {
        public Format format;
        private int width;
        private int height;
        private int depth;
        public int[,] values; //[h,w]
        public int[,] values_trans;
        public int[,,] valuesRGB; //[h,w,ch]
        public int[,,] valuesRGB_trans;
        private double[,] filtr;
		public ImageNetPBM(string path)
		{
			string[] info = File.ReadAllLines(path);
			int line = 0;
			int bypass = 0; //bypass for binary
			while (isComment(line, info)) { bypass += info[line].Length; line++; }
            switch (info[line])
            {
                case "P1": format = Format.P1; break;
                case "P2": format = Format.P2; break;
                case "P3": format = Format.P3; break;
                case "P4": format = Format.P4; break;
                case "P5": format = Format.P5; break;
                case "P6": format = Format.P6; break;
                //default: break;
            }
			bypass += info[line].Length;
			line++;
			while (isComment(line, info)) { bypass += info[line].Length; line++; }
            string[] numbers = info[line].Split(' ');
            int[] temp = new int[2];
            for (int i = 0; i < 2; i++) temp[i] = int.Parse(numbers[i]);
            width = temp[0];
            height = temp[1];
			bypass += info[line].Length;
			line++;
            if ((format == Format.P1) || (format == Format.P4)) depth = 2;
            else
            {
                while (isComment(line, info)) line++;
                depth = int.Parse(info[line]);
				bypass += info[line].Length;
				line++;
            }
            if((format == Format.P1) || (format == Format.P2) || (format == Format.P4) || (format == Format.P5))
            {
                values = new int[height, width];
                values_trans = new int[height, width];
            }
            else
            {
                valuesRGB = new int[height, width, 3];
                valuesRGB_trans = new int[height, width, 3];
            }
            if ((format == Format.P1) || (format == Format.P2))
            {
                while (isComment(line, info)) line++;
                for (int i = 0; i < height; i++)
                {
					numbers = Regex.Split(info[line], "[ ]+");
                    for (int j = 0; j < width; j++) values[i, j] = int.Parse(numbers[j]);
                    line++;
                }
            }
            else if (format == Format.P3)
            {
				while (isComment(line, info)) line++;
				int id;
				for (int i = 0; i < height; i++)
				{
					numbers = Regex.Split(info[line], "[ \t]+");
					id = 0;
					for (int j = 0; j < width; j++)
					{
						for(int k = 0; k < 3; k++)
						{
							valuesRGB[i, j, k] = int.Parse(numbers[id]);
							id++;
						}
					}
					line++;
				}
			}
            else if (format == Format.P4)
            {
                //TODO
            }
            else if (format == Format.P5)
            {
				while (isComment(line, info)) { bypass += info[line].Length; line++; }
				byte[] pixels = File.ReadAllBytes(path).Skip(bypass).ToArray();
				int id = 0;
				for (int i = 0; i < height; i++)
				{
					for (int j = 0; j < width; j++) values[i, j] = pixels[id]; id++;
				}
				//TODO
			}
            else if(format == Format.P6)
            {
				while (isComment(line, info)) { bypass += info[line].Length; line++; }
				byte[] pixels = File.ReadAllBytes(path).Skip(bypass).ToArray();
				int id = 0;
				for (int i = 0; i < height; i++)
				{
					for (int j = 0; j < width; j++)
					{
						for (int k = 0; k < 3; k++)
						{
							valuesRGB[i, j, k] = pixels[id];
							id++;
						}
					}
				}
				//TODO
			}

        }
        public string parametersTostring()
        {
            string result = "";
			result = result + "mode: " + format.ToString() + "\n";
            result = result + "width: " + width.ToString() + "\n";
            result = result + "height: " + height.ToString() + "\n";
            result = result + "depth: " + depth.ToString() + "\n";
            result = result + "values:\n";
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++) result = result + values[i, j].ToString() + " ";
                result += "\n";
            }
            return result;
        }
        public void showParameters()
        {
			Console.WriteLine("Mode: " + format.ToString());
            Console.WriteLine("Width: " + width);
            Console.WriteLine("Height: " + height);
            Console.WriteLine("Depth: " + depth);
            Console.WriteLine("Values:");
			if(format == Format.P3 || format == Format.P6)
			{
				for (int i = 0; i < height; i++)
				{
					for (int j = 0; j < width; j++)
					{
						for(int k = 0; k < 3; k++) Console.Write(valuesRGB[i, j, k] + " ");
						Console.Write("\t");
					} 
					Console.WriteLine("");
				}
			}
			else
			{
				for (int i = 0; i < height; i++)
				{
					for (int j = 0; j < width; j++) Console.Write(values[i, j] + " ");
					Console.WriteLine("");
				}
			}     
        }
        public void transformation(string dest_path, int iterations = 1)
        {
			for(int o = 1; o <= iterations; o++)
			{
				int[,] neighbourhood;
				int[,,] neighbourhoodRGB;
				filtr = new double[3, 3];
				for (int i = 0; i < 3; i++)
				{
					for (int j = 0; j < 3; j++)
					{
						filtr[i, j] = 0;
						if ((i == 1 && j == 0) || (i == 0 && j == 1) || (i == 1 && j == 2) || (i == 2 && j == 1))
							filtr[i, j] = 0.1;
						if (i == 1 && j == 1)
							filtr[i, j] = 0.6;
					}
				}
				if (format == Format.P3 || format == Format.P6)
				{
					for (int i = 0; i < height; i++)
					{
						for (int j = 0; j < width; j++)
						{
							neighbourhoodRGB = new int[3, 3, 3];
							for (int k = 0; k < 3; k++)
							{
								neighbourhoodRGB[1, 1, k] = valuesRGB[i, j, k];
							}
							if ((i == 0 && j == 0))
							{
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[0, 0, k] = neighbourhoodRGB[0, 1, k] = neighbourhoodRGB[0, 2, k] = neighbourhoodRGB[1, 0, k] = neighbourhoodRGB[2, 0, k] = 0;
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[1, 2, k] = valuesRGB[i, j + 1, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[2, 1, k] = valuesRGB[i + 1, j, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[2, 2, k] = valuesRGB[i + 1, j + 1, k];
								}
							}
							else if (i == 0 && j == width - 1)
							{
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[0, 0, k] = neighbourhoodRGB[0, 1, k] = neighbourhoodRGB[0, 2, k] = neighbourhoodRGB[1, 2, k] = neighbourhoodRGB[2, 2, k] = 0;
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[1, 0, k] = valuesRGB[i, j - 1, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[2, 0, k] = valuesRGB[i + 1, j - 1, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[2, 1, k] = valuesRGB[i + 1, j, k];
								}
							}
							else if (i == height - 1 && j == 0)
							{
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[0, 0, k] = neighbourhoodRGB[1, 0, k] = neighbourhoodRGB[2, 0, k] = neighbourhoodRGB[2, 1, k] = neighbourhoodRGB[2, 2, k] = 0;
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[0, 1, k] = valuesRGB[i - 1, j, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[0, 2, k] = valuesRGB[i - 1, j + 1, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[1, 2, k] = valuesRGB[i, j + 1, k];
								}
							}
							else if (i == height - 1 && j == width - 1)
							{
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[0, 2, k] = neighbourhoodRGB[1, 2, k] = neighbourhoodRGB[2, 2, k] = neighbourhoodRGB[2, 1, k] = neighbourhoodRGB[2, 0, k] = 0;
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[0, 0, k] = valuesRGB[i - 1, j - 1, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[0, 1, k] = valuesRGB[i - 1, j, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[1, 0, k] = valuesRGB[i, j - 1, k];
								}
							}
							else if (i == 0)
							{
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[0, 0, k] = neighbourhoodRGB[0, 1, k] = neighbourhoodRGB[0, 2, k] = 0;
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[1, 0, k] = valuesRGB[i, j - 1, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[1, 1, k] = valuesRGB[i, j, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[1, 2, k] = valuesRGB[i, j + 1, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[2, 0, k] = valuesRGB[i + 1, j - 1, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[2, 1, k] = valuesRGB[i + 1, j, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[2, 2, k] = valuesRGB[i + 1, j + 1, k];
								}
							}
							else if (i == height - 1)
							{
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[2, 0, k] = neighbourhoodRGB[2, 1, k] = neighbourhoodRGB[2, 2, k] = 0;
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[0, 0, k] = valuesRGB[i - 1, j - 1, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[0, 1, k] = valuesRGB[i - 1, j, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[0, 2, k] = valuesRGB[i - 1, j + 1, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[1, 0, k] = valuesRGB[i, j - 1, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[1, 1, k] = valuesRGB[i, j, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[1, 2, k] = valuesRGB[i, j + 1, k];
								}
							}
							else if (j == 0)
							{
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[0, 0, k] = neighbourhoodRGB[1, 0, k] = neighbourhoodRGB[2, 0, k] = 0;
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[0, 1, k] = valuesRGB[i - 1, j, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[0, 2, k] = valuesRGB[i - 1, j + 1, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[1, 1, k] = valuesRGB[i, j, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[1, 2, k] = valuesRGB[i, j + 1, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[2, 1, k] = valuesRGB[i + 1, j, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[2, 2, k] = valuesRGB[i + 1, j + 1, k];
								}
							}
							else if (j == width - 1)
							{
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[0, 2, k] = neighbourhoodRGB[1, 2, k] = neighbourhoodRGB[2, 2, k] = 0;
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[0, 0, k] = valuesRGB[i - 0, j - 1, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[0, 1, k] = valuesRGB[i - 0, j, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[1, 0, k] = valuesRGB[i, j - 1, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[1, 1, k] = valuesRGB[i, j, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[2, 0, k] = valuesRGB[i + 1, j - 1, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[2, 1, k] = valuesRGB[i + 1, j, k];
								}
							}
							else
							{
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[0, 0, k] = valuesRGB[i - 1, j - 1, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[0, 1, k] = valuesRGB[i - 1, j, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[0, 2, k] = valuesRGB[i - 1, j + 1, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[1, 0, k] = valuesRGB[i, j - 1, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[1, 1, k] = valuesRGB[i, j, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[1, 2, k] = valuesRGB[i, j + 1, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[2, 0, k] = valuesRGB[i + 1, j - 1, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[2, 1, k] = valuesRGB[i + 1, j, k];
								}
								for (int k = 0; k < 3; k++)
								{
									neighbourhoodRGB[2, 2, k] = valuesRGB[i + 1, j + 1, k];
								}
							}
							for(int k = 0; k < 3; k++)
							{
								valuesRGB_trans[i, j, k] = pixelTransformationRGB(neighbourhoodRGB, filtr, k);
							}		
						}
					}
				}
				else
				{
					for (int i = 0; i < height; i++)
					{
						for (int j = 0; j < width; j++)
						{
							neighbourhood = new int[3, 3];
							neighbourhood[1, 1] = values[i, j];
							if ((i == 0 && j == 0))
							{
								neighbourhood[0, 0] = neighbourhood[0, 1] = neighbourhood[0, 2] = neighbourhood[1, 0] = neighbourhood[2, 0] = 0;
								neighbourhood[1, 2] = values[i, j + 1];
								neighbourhood[2, 1] = values[i + 1, j];
								neighbourhood[2, 2] = values[i + 1, j + 1];
							}
							else if (i == 0 && j == width - 1)
							{
								neighbourhood[0, 0] = neighbourhood[0, 1] = neighbourhood[0, 2] = neighbourhood[1, 2] = neighbourhood[2, 2] = 0;
								neighbourhood[1, 0] = values[i, j - 1];
								neighbourhood[2, 0] = values[i + 1, j - 1];
								neighbourhood[2, 1] = values[i + 1, j];
							}
							else if (i == height - 1 && j == 0)
							{
								neighbourhood[0, 0] = neighbourhood[1, 0] = neighbourhood[2, 0] = neighbourhood[2, 1] = neighbourhood[2, 2] = 0;
								neighbourhood[0, 1] = values[i - 1, j];
								neighbourhood[0, 2] = values[i - 1, j + 1];
								neighbourhood[1, 2] = values[i, j + 1];
							}
							else if (i == height - 1 && j == width - 1)
							{
								neighbourhood[0, 2] = neighbourhood[1, 2] = neighbourhood[2, 2] = neighbourhood[2, 1] = neighbourhood[2, 0] = 0;
								neighbourhood[0, 0] = values[i - 1, j - 1];
								neighbourhood[0, 1] = values[i - 1, j];
								neighbourhood[1, 0] = values[i, j - 1];
							}
							else if (i == 0)
							{
								neighbourhood[0, 0] = neighbourhood[0, 1] = neighbourhood[0, 2] = 0;
								neighbourhood[1, 0] = values[i, j - 1];
								neighbourhood[1, 1] = values[i, j];
								neighbourhood[1, 2] = values[i, j + 1];
								neighbourhood[2, 0] = values[i + 1, j - 1];
								neighbourhood[2, 1] = values[i + 1, j];
								neighbourhood[2, 2] = values[i + 1, j + 1];
							}
							else if (i == height - 1)
							{
								neighbourhood[2, 0] = neighbourhood[2, 1] = neighbourhood[2, 2] = 0;
								neighbourhood[0, 0] = values[i - 1, j - 1];
								neighbourhood[0, 1] = values[i - 1, j];
								neighbourhood[0, 2] = values[i - 1, j + 1];
								neighbourhood[1, 0] = values[i, j - 1];
								neighbourhood[1, 1] = values[i, j];
								neighbourhood[1, 2] = values[i, j + 1];
							}
							else if (j == 0)
							{
								neighbourhood[0, 0] = neighbourhood[1, 0] = neighbourhood[2, 0] = 0;
								neighbourhood[0, 1] = values[i - 1, j];
								neighbourhood[0, 2] = values[i - 1, j + 1];
								neighbourhood[1, 1] = values[i, j];
								neighbourhood[1, 2] = values[i, j + 1];
								neighbourhood[2, 1] = values[i + 1, j];
								neighbourhood[2, 2] = values[i + 1, j + 1];
							}
							else if (j == width - 1)
							{
								neighbourhood[0, 2] = neighbourhood[1, 2] = neighbourhood[2, 2] = 0;
								neighbourhood[0, 0] = values[i - 1, j - 1];
								neighbourhood[0, 1] = values[i - 1, j];
								neighbourhood[1, 0] = values[i, j - 1];
								neighbourhood[1, 1] = values[i, j];
								neighbourhood[2, 0] = values[i + 1, j - 1];
								neighbourhood[2, 1] = values[i + 1, j];
							}
							else
							{
								neighbourhood[0, 0] = values[i - 1, j - 1];
								neighbourhood[0, 1] = values[i - 1, j];
								neighbourhood[0, 2] = values[i - 1, j + 1];
								neighbourhood[1, 0] = values[i, j - 1];
								neighbourhood[1, 1] = values[i, j];
								neighbourhood[1, 2] = values[i, j + 1];
								neighbourhood[2, 0] = values[i + 1, j - 1];
								neighbourhood[2, 1] = values[i + 1, j];
								neighbourhood[2, 2] = values[i + 1, j + 1];
							}
							values_trans[i, j] = pixelTransformation(neighbourhood, filtr);
						}
					}
				}
			}  
            saveImage(dest_path);
        }
        protected int pixelTransformation(int[,] original_values, double[,] filtr)
        {
            double result = 0;
            for(int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    result += original_values[i, j] * filtr[i, j];
                }
            }
            //result %= depth;
            if (result >= depth) result = depth-1;
            return (int)result;
        }
		protected int pixelTransformationRGB(int[,,] original_values, double[,] filtr, int channel)
		{
			double result = 0;
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					result += original_values[i, j, channel] * filtr[i, j];
				}
			}
			//result %= depth;
			if (result >= depth) result = depth - 1;
			return (int)result;
		}
		public void saveImage(string path)
        {
            int lines;
            if (format == Format.P1 || format == Format.P4) lines = 2;
            else lines = 3;
            lines += height;
            string[] info = new string[lines];
            int line = 0;
            switch (format)
            {
                case Format.P1:
                    {
                        info[line++] = "P1"; 
                        info[line++] = width.ToString() + " " + height.ToString();
                        break;
                    }
                case Format.P2:
                    {
                        info[line++] = "P2";
                        info[line++] = width.ToString() + " " + height.ToString();
                        info[line++] = depth.ToString();
                        break;
                    }
                case Format.P3:
                    {
                        info[line++] = "P3";
                        info[line++] = width.ToString() + " " + height.ToString();
                        info[line++] = depth.ToString();
                        break;
                    }
            }
			if(format == Format.P1 || format == Format.P2)
			{
				for (int i = 0; i < height; i++)
				{
					info[line] = "";
					for (int j = 0; j < width; j++)
					{
						info[line] += values_trans[i, j].ToString() + " ";
					}
					line++;
				}
				File.WriteAllLines(path, info);
			}
			else if(format == Format.P3)
			{
				for (int i = 0; i < height; i++)
				{
					info[line] = "";
					for (int j = 0; j < width; j++)
					{
						for(int k = 0; k < 3; k++)
						{
							info[line] += valuesRGB_trans[i, j, k].ToString() + " ";
						}
						info[line] += "\t";
					}
					line++;
				}
				File.WriteAllLines(path, info);
			}
			else if(format == Format.P4)
			{
				
			}
			else if(format == Format.P5)
			{
				File.WriteAllLines(path, info);
				byte[] pixels = new byte[width*height];
				int id = 0;
				for (int i = 0; i < height; i++)
				{
					for (int j = 0; j < width; j++)
					{
						pixels[id] = (byte)values_trans[i, j];
						id++;
					}
				}
				using (var stream = new FileStream(path, FileMode.Append))
				{
					stream.Write(pixels, 0, pixels.Length);
				}
			}
			else if(format == Format.P6)
			{
				File.WriteAllLines(path, info);
				byte[] pixels = new byte[width * height * 3];
				int id = 0;
				for (int i = 0; i < height; i++)
				{
					for (int j = 0; j < width; j++)
					{
						for (int k = 0; k < 3; k++)
						{
							pixels[id] = (byte)valuesRGB_trans[i, j, k];
							id++;
						}
					}
				}
				using (var stream = new FileStream(path, FileMode.Append))
				{
					stream.Write(pixels, 0, pixels.Length);
				}
			}
        }
        protected bool isComment(int line, string[] info)
        {
            string check = info[line];
            char first = check[0];
            if (first == '#') return true;
            else return false;
        }
        /*
		public bool saveImage(string path)
		{

		}
		*/

    }
}
