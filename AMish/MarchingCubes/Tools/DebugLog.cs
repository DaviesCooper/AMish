using ServerSide.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSide.Tools
{
    /// <summary>
    /// Class for creating a debug log in the cache folder
    /// </summary>
    public static class DebugLog
    {
        //Flag for debugging
        public static bool debug = false;
        // .../cache/debug_log.txt
        private static string logPath = Path.Combine(FileStructure.cacheDirectory, "debug_log.txt");
        //lock for threading
        private static readonly Object @lock = new Object();

        //Set Debug
        public static void setDebug(bool onOff)
        {
            debug = onOff;
        }
        //Toggles Debug
        public static void toggleDebug()
        {
            debug = !debug;
        }
        //Deletes the log
        public static void resetLog()
        {
            try
            {
                File.Create(logPath);
            }
            catch(Exception)
            {
                return;
            }
        }
        //Logs a message
        public static void logConsole(string message)
        {
            if (debug)
            {
                lock (@lock)
                {
                    string outMessage = DateTime.Now.ToString() + ": " + message + "\n";
                    Console.Out.WriteLine(outMessage);
                    try
                    {
                        File.AppendAllText(logPath, outMessage);
                    }
                    catch(Exception)
                    {

                    }
                }
            }
        }
    }
}
