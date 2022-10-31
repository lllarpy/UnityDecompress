using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using SevenZipNET;

namespace UnityDecompress
{
    internal class Program
    {
        private string myDir = "";

        [STAThread]
        public static void Main()
        {
            Console.WriteLine("откройте файл");

            var d = new OpenFileDialog();
            d.Filter = "Unity Package|*.unitypackage";
            d.Title = "Откройте файл .unitypackage";

            if (d.ShowDialog() == DialogResult.OK)
                new Program().Main(d.FileName);

            Console.Read();
        }

        private void Main(string input)
        {
            Console.WriteLine("файл: " + input);
            myDir = Path.GetFileNameWithoutExtension(input) + "\\";
            
            if (Directory.Exists(myDir)) Directory.Delete(myDir, true);
            if (Directory.Exists($".\\unpacked-{myDir}"))
            {
                Console.WriteLine($"этот пакет уже распакован в .\\unpacked-{myDir}");
                return;
            }
            Directory.CreateDirectory(myDir + "Temp");

            Console.WriteLine("распаковка пакета");
            new SevenZipExtractor(input).ExtractAll(myDir);
            new SevenZipExtractor(myDir + Path.GetFileNameWithoutExtension(input)).ExtractAll(myDir + "Temp");

            Console.WriteLine("распаковка");
            foreach (var dir in Directory.GetDirectories(myDir + "Temp"))
                ProcessFile($"{dir}\\asset", $"{dir}\\asset.meta", $"{dir}\\pathname");

            Console.WriteLine("перемещение");
            Directory.Move(myDir + "Assets", $".\\unpacked-{myDir}");

            Console.WriteLine("удаление временных файлов");
            Directory.Delete(myDir, true);

            Console.WriteLine("\nзавершено");
        }

        private void ProcessFile(string assetFile, string metaFile, string pathFile)
        {
            var path = ReadStr(File.OpenRead(pathFile)).Replace("\n00", "").Replace("/", "\\");
            var dirPath = myDir + path;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dirPath) ?? string.Empty);
                File.Copy(metaFile, dirPath + ".meta");
                File.Copy(assetFile, dirPath);
                Console.WriteLine("распакован файл: " + path);
            }
            catch (FileNotFoundException)
            {
                Directory.CreateDirectory(dirPath);
            }
        }

        private string ReadStr(Stream fstream)
        {
            var buffer = new byte[fstream.Length];
            fstream.Read(buffer, 0, buffer.Length);
            fstream.Close();
            return Encoding.Default.GetString(buffer);
        }
    }
}