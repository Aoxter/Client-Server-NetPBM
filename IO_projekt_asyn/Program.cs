using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace IO_projekt_asyn
{
    class Program
    {
        static async Task serverTask()
        {
            TcpListener server = new TcpListener(IPAddress.Any, 2048);
            server.Start();
            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                byte[] buffer = new byte[1024];
                await client.GetStream().ReadAsync(buffer, 0, buffer.Length).ContinueWith(
                    async (t) =>
                    {
                        int i = t.Result;
                        string[] args = new ASCIIEncoding().GetString(buffer).Substring(0,i).Split(' ');
						string img_path = "";
						string dest_path = "";
						int iterations = 1;
						if (args.Length == 3)
						{
							img_path = args[0];
							iterations = int.Parse(args[1]);
							dest_path = args[2];
						}
						else Console.WriteLine("Arguments error");
                        ImageNetPBM img = new ImageNetPBM(img_path);
                        img.transformation(dest_path, iterations);
						buffer = new ASCIIEncoding().GetBytes("finished");
						client.GetStream().WriteAsync(buffer, 0, buffer.Length);
					});
            }
        }
        static async Task clientTask(string img_path, string dest_path, int iterations = 1)
        {
            TcpClient client = new TcpClient();
            client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2048));
			string message = img_path + " " + iterations.ToString() + " " + dest_path;
            byte[] buffer = new ASCIIEncoding().GetBytes(message);
            client.GetStream().WriteAsync(buffer, 0, buffer.Length);
			int i = await client.GetStream().ReadAsync(buffer, 0, buffer.Length);
		}

        static void Main(string[] args)
        {
			int option;
			String src_path;
			String dst_path;
			ImageNetPBM img;
			int transformation_flag = 1;
			String custom_file_extension;
			ConsoleColor default_foreground = Console.ForegroundColor;
			do
			{
				Console.WriteLine();
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.WriteLine("Project in development! Only P2 (pgm) and P3 (ppm) formats can be transformed. P1 (pbm) only for view.");
				Console.ForegroundColor = default_foreground;			
				Console.WriteLine("Choose file to load:"); ;
				Console.WriteLine("1 - pliczek.pbm");
				Console.WriteLine("2 - pliczek2.pgm");
				Console.WriteLine("3 - pliczek3.ppm");
				Console.WriteLine("4 - your own file (Only P1-P3 Netpbm format!)");
				Console.WriteLine("0 - exit");
				Console.WriteLine();
				option = int.Parse(Console.ReadLine());
				switch (option)
				{
					case 0: img = new ImageNetPBM("files/pliczek.pbm"); break;
					case 1: img = new ImageNetPBM("files/pliczek.pbm"); break;
					case 2: img = new ImageNetPBM("files/pliczek2.pgm"); break;
					case 3: img = new ImageNetPBM("files/pliczek3.ppm"); break;
					case 4:
						{
							Console.WriteLine("Write file path. If file is in bin/Debug folder, you can write only file name");
							src_path = Console.ReadLine();
							String[] spearator = { "." };
							Int32 count = 2;
							String[] words = src_path.Split(spearator, count, StringSplitOptions.RemoveEmptyEntries);
							custom_file_extension = words[words.Length - 1];
							if(custom_file_extension == "pbm")
							{
								img = new ImageNetPBM(src_path);
								transformation_flag = 0; //transformation set to disable
							}
							else if(custom_file_extension == "pgm" || custom_file_extension == "ppm")
							{
								img = new ImageNetPBM(src_path);
								transformation_flag = 1; //transformation set to enable
							}
							else
							{
								Console.WriteLine();
								Console.ForegroundColor = ConsoleColor.DarkRed;
								Console.WriteLine("Wrong file format! Press any key to continue");
								Console.ForegroundColor = default_foreground;
								Console.ReadKey();
								continue;
							}
							break;
						}
					default: img = new ImageNetPBM("pliczek2.pgm"); break;
				}
				if (option == 1 || transformation_flag == 0)
				{
					int option_2;
					Console.WriteLine();
					Console.WriteLine("What you want to do with file?");
					Console.WriteLine("1 - show file parameters");
					Console.ForegroundColor = ConsoleColor.DarkGray;
					Console.WriteLine("[ transformation is disable for pbm ]");
					Console.ForegroundColor = default_foreground;
					Console.WriteLine("0 - go back to loading menu");
					option_2 = int.Parse(Console.ReadLine());
					Console.WriteLine();
					if (option_2 == 1)
					{
						img.showParameters();
						Console.WriteLine("\nPress any key to continue");
						Console.ReadKey();
					}
					else continue;
				}
				else if (option == 0) break;
				else
				{
					int option_2;
					do
					{
						Console.WriteLine();
						Console.WriteLine("What you want to do with file?");
						Console.WriteLine("1 - show file parameters");
						Console.WriteLine("2 - transformation with gauss filter");
						Console.WriteLine("0 - go back to loading menu");
						option_2 = int.Parse(Console.ReadLine());
						Console.WriteLine();
						if (option_2 == 1) img.showParameters();
						else if (option_2 == 2)
						{
							Console.WriteLine("Number of iterations?");
							int iterations = int.Parse(Console.ReadLine());
							Console.WriteLine("Path to save result (with extension)");
							dst_path = Console.ReadLine();
							img.transformation(dst_path, iterations);
						}
						else continue;
					} while (option_2 != 0);
				}
			} while (option != 0);
			
			// TESTS SECTION

			/*
            ImageNetPBM img = new ImageNetPBM("pliczek2.pgm");
            img.showParameters();
			img.format = Format.P5;
			//img.transformation("wyniczek5.pgm");
            Console.WriteLine("Wcisnij dowolny klawisz aby przejsc dalej");
            Console.ReadKey();
			img.saveImage("pliczek5.pgm");
			*/

			/*
			ImageNetPBM img = new ImageNetPBM("pliczek3.ppm");
			img.showParameters();
			//Console.WriteLine("Podaj nazwe pliku docelowego (z rozszerzeniem)");
			//Console.WriteLine("Podaj nazwe pliku docelowego (z rozszerzeniem)");
			//string path = Console.ReadLine();
			img.transformation("wyniczek3.ppm");
			Console.WriteLine("Wcisnij dowolny klawisz aby przejsc dalej");
			Console.ReadKey();
			*/
			/*
            var tasks = new List<Task>();
            serverTask();
            tasks.Add(clientTask("pliczek2.pgm", "wyniczek2a.pgm"));
            //for (int i = 0; i < 5; i++) tasks.Add(clientTask(i));
            Task.WaitAll(tasks.ToArray());
			Console.WriteLine("Naciśnij dowolny klawisz by zakończyć");
			Console.ReadKey();
			*/
		}
    }
}
