using System.IO;
using System.Reflection;

namespace ServerSide.Parsing
{
    public static class FileStructure
    {
        //Path of the cache directory
        public static readonly string cacheDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\cache\";
        public static string meshDirectory;
        public static string fieldDirectory;

        /// <summary>
        /// Checks if the cache directory exists, and creates it if not
        /// </summary>
        private static void createCache()
        {
            Directory.CreateDirectory(cacheDirectory);
        }

        /// <summary>
        /// Pass in the filename !WITHOUT THE PATH, OR EXTENSION! to create a directory
        /// within the cache. Returns the path to the created directory
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>The path of the created directory</returns>
        private static string instantiateFileDirectory(string filename)
        {
            DirectoryInfo info = Directory.CreateDirectory(cacheDirectory + filename + "\\");
            return info.FullName;
        }

        /// <summary>
        /// Clears the cache
        /// </summary>
        public static void clearCache()
        {
            Directory.Delete(cacheDirectory, true);
            createCache();
        }

        /// <summary>
        /// Pass in the filename WITHOUT THE PATH OR EXTENSION
        /// </summary>
        /// <param name="filename"></param>
        public static void removeFileFromCache(string filename)
        {
            if (Directory.Exists(cacheDirectory + filename + "\\"))
                Directory.Delete(cacheDirectory + filename + "\\", true);
        }

        /// <summary>
        /// Give a full path, and a mesh directory will be created within it
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static void instantiateMeshDirectory(string path)
        {
            DirectoryInfo info = Directory.CreateDirectory(path + "meshes\\");
            meshDirectory = info.FullName;
        }

        /// <summary>
        /// Give a full path, and a field directory will be created within it
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static void instantiatefieldsDirectory(string path)
        {
            DirectoryInfo info = Directory.CreateDirectory(path + "fields\\");
            fieldDirectory = info.FullName;
        }

        /// <summary>
        /// Pass in a file, and the sub-directory for that file will be created
        /// </summary>
        /// <param name="filename"></param>
        public static void addFileToDirectory(string filename)
        {
            createCache();
            string name = Path.GetFileNameWithoutExtension(filename);
            string dir = instantiateFileDirectory(name);
            instantiateMeshDirectory(dir);
            instantiatefieldsDirectory(dir);
        }

        public static string createHeaderDirectory(string headerName)
        {
            return Directory.CreateDirectory(fieldDirectory + headerName + "\\").FullName;
        }

        public static string[] getAllCurrentNames()
        {
            if (Directory.Exists(cacheDirectory))
            {
                string[] directories = Directory.GetDirectories(cacheDirectory);
                for (int i = 0; i < directories.Length; i++)
                    directories[i] = directories[i].Replace(cacheDirectory, "");
                return directories;
            }
                
            else
                return null;
        }
    }
}

