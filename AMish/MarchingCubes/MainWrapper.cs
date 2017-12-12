using ServerSide.MarchingAlgorithm;
using ServerSide.Parsing;
using ServerSide.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSide
{
    public class MainWrapper
    {

        static string help = "Takes a CSV, and performs all the necessary functions to create a full, final mesh\n" +
                             "MainWrapper <fileIn> <numOfDivisions> <MeshHeader> <Resolution> <ThreadCount> <xAxis> <yAxis> <zAxis> <isoLevel> <SplitHeaders>*\n\n" +
                             "\t<fileIn> = the path to the file you want a mesh created out of\n" +
                             "\t<numOfDivisions> = the number of divisions you want the files to be divided into\n" +
                             "\t<MeshHeader> = the header you would like the mesh to be split by\n" +
                             "\t<Resolution> = the number of \"bins\" you would like to aggregate data by\n" +
                             "\t<ThreadCount> = the number of threads used during mesh creation\n" +
                             "\t<xaxis> = the column whose values you would like on the x axis\n" +
                             "\t<yaxis> = the column whose values you would like on the y axis\n" +
                             "\t<zaxis> = the column whose values you would like on the z axis\n" +
                             "\t<isoLevel> = the minimum value a bin must have to be considered below the isosurface of the mesh\n" +
                             "\t<splitHeaders>* = the columns you would like divided. Leave blank to split all\n";

        //arg0 = fileIn
        //arg1 = NumOfDivisions
        //arg2 = MeshHeader
        //arg3 = Resolution
        //arg4 = ThreadCount
        //arg5 = x axis
        //arg6 = y axis
        //arg7 = z axis
        //arg8 = isoLevel
        //args9+ = headers to split
        public static void Main(string[] args)
        {
            DebugLog.setDebug(true);
            DebugLog.resetLog();
            DebugLog.logConsole("RAM : " + SystemRecommendations.getRAM().ToString() + " b");
            DebugLog.logConsole("Physical Cores : " + SystemRecommendations.getCores().ToString());
            DebugLog.logConsole("CPU : " + SystemRecommendations.getHz().ToString() + " Hz");
            DebugLog.logConsole("Recommended resolution : " + SystemRecommendations.getRecommendedResolution().ToString());

            string toBeParsed = null;
            string meshHeader = args[2];
            string[] axis = new string[] { args[5], args[6], args[7] };
            if (args.Length < 10)
            {
                Console.Out.WriteLine("Incorrect number of Args. run with --help to see doc");
                Console.In.ReadLine();
                return;
            }
            if (args.Length == 1 && (args[0].ToLower() == "--help" || args[0].ToLower() == "help" || args[0].ToLower() == "?" || args[0].ToLower() == "h"))
            {
                Console.Out.WriteLine(help);
                Console.In.ReadLine();
                return;
            }
            if (!File.Exists(args[0]))
            {
                DebugLog.logConsole("File not found. Are you sure you typed the right file name?");
                throw new Exception("File not found. Are you sure you typed the right file name?");
            }
            else
            {
                toBeParsed = args[0];
            }

            DebugLog.logConsole("Recommended threads : " + SystemRecommendations.getRecommendedThreads(FileParser.estimatedNumberOfEntries(toBeParsed)).ToString());
            if (!Int32.TryParse(args[1], out int numOfDivisions))
            {
                DebugLog.logConsole("divisionNumber was not a number");
                throw new Exception("divisionNumber was not a number");
            }
            if (!Int32.TryParse(args[3], out int resolution))
            {
                DebugLog.logConsole("resolution given was not a number");
                throw new Exception("resolution given was not a number");
            }
            if (!Int32.TryParse(args[4], out int threadCount))
            {
                DebugLog.logConsole("thread count given was not a number");
                throw new Exception("thread count given was not a number");
            }
            if (!float.TryParse(args[8], out float isoLevel))
            {
                DebugLog.logConsole("isoLevel given was not a number");
                throw new Exception("isoLevel given was not a number");
            }
            if (resolution < 0 || threadCount < 0 || isoLevel < 0f || numOfDivisions < 0)
            {
                DebugLog.logConsole("Neither thread count, resolution, isoLevel, nor divNum can be negative");
                throw new Exception("Neither thread count, resolution, isoLevel, nor divNum can be negative");
            }

            #region ALL
            DateTime now = DateTime.Now;
            //first we parse

            FileParser.parseDataIntoCache(toBeParsed);
            DebugLog.logConsole("Time to finish Parsing : " + (DateTime.Now - now).TotalMinutes.ToString());

            now = DateTime.Now;
            //This is what it will be in the cache
            string nameNotPath = Path.GetFileNameWithoutExtension(toBeParsed);
            //now we split the file


            if (args.Length < 8)
            {
                FieldSplitter.splitFileAll(nameNotPath, numOfDivisions);
            }
            else
            {
                string[] headers = new string[args.Length - 9];
                for (int i = 9; i < args.Length; i++)
                {
                    headers[i - 9] = args[i];
                }
                FieldSplitter.splitFileGiven(nameNotPath, headers, numOfDivisions);
            }


            DebugLog.logConsole("Time to finish file splitting : " + (DateTime.Now - now).TotalMinutes.ToString());

            DebugLog.logConsole("Beginnging mesh creation");
            //now we create the meshes for a header
            //for (int i = 200000; i < resolution; i += 200000)
            //{
                now = DateTime.Now;
                AllMeshes.allFromDivisions(nameNotPath, meshHeader, resolution, threadCount, axis, isoLevel);
                DebugLog.logConsole("Time to finish mesh "+resolution+" : " + (DateTime.Now - now).TotalMinutes.ToString());
            //}     
            #endregion
        }
    }
}
