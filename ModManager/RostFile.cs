using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace ModManager
{
    class RostFile
    {
        public static void SavePlugins(string filePath)
        {
            if (filePath == "*") return;
            byte[] assets = CreateZipFromModded(filePath + ".modded" + Constants.TEMPORARY_FILE_FORMAT);
            byte[] plugins = CreateZipFromPlugins(filePath + ".plugins.tmp" + Constants.TEMPORARY_FILE_FORMAT);
            using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(filePath)))
            {
                writer.Write(assets);
                writer.Write("This is seperator");
                writer.Write(plugins);
            }
        }

        public static void LoadPlugins(string filePath)
        {
            if (!File.Exists(filePath)) return;

            using (BinaryReader reader = new BinaryReader(File.OpenRead(filePath)))
            {
                byte[] assets = reader.ReadBytes((int)reader.BaseStream.Length / 2); // Initial assumption: assets and plugins are of similar size
                reader.BaseStream.Seek(assets.Length + 18, SeekOrigin.Begin); // Seek to the position after the separator "This is seperator"

                byte[] plugins = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));

                // Optionally, you can handle the assets and plugins here (e.g., extracting them from the byte arrays).
                ExtractZipFromBytes(assets, "modded.zip" + Constants.TEMPORARY_FILE_FORMAT, Constants.MODDED_FOLDER_PATH);
                ExtractZipFromBytes(plugins, "plugins.zip" + Constants.TEMPORARY_FILE_FORMAT, Constants.PLUGINS_PATH);
            }
        }

        private static void ExtractZipFromBytes(byte[] bytes, string outputFile, string folder)
        {
            File.WriteAllBytes(outputFile, bytes); // Save the zip data to a file
            ZipFile.ExtractToDirectory(outputFile, folder); // Extract to directory
            File.Delete(outputFile); // Optionally delete the temporary zip file
        }

        private static byte[] CreateZipFromModded(string file)
        {
            ZipFile.CreateFromDirectory(Constants.MODDED_FOLDER_PATH, file, CompressionLevel.Optimal, true);
            byte[] bytes = File.ReadAllBytes(file);
            File.Delete(file);
            return bytes;
        }

        private static byte[] CreateZipFromPlugins(string file)
        {
            ZipFile.CreateFromDirectory(Constants.PLUGINS_PATH, file, CompressionLevel.Optimal, true);
            byte[] bytes = File.ReadAllBytes(file);
            File.Delete(file);
            return bytes;
        }
    }
}
