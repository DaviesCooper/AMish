using DataStructures;
using ServerSide.Parsing;
using ServerSide.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ServerSide.MarchingAlgorithm
{
    /// <summary>
    /// Class for threading the mesh creation. Each thread will create one of these
    /// </summary>
    public class ThreadMesh
    {
        public static int totalMeshes;
        public void createMesh(ConcurrentStack<string> stack, int resolution, string[] axis, float[] minMaxArray, float isoLevel)
        {
            DebugLog.logConsole("Mesh creation thread started");
            totalMeshes = stack.Count;
            while (!stack.IsEmpty)
            {
                //If we fail popping, we may be attempting to access
                //at the same time as someone else. This means the stack
                //might have emptied since we last checked so we check again
                if (!stack.TryPop(out string file))
                {
                    continue;
                }
                //If we got this far, then we have a file
                DebugLog.logConsole(file + " has been selected");
                SingularMesh alg = alg = new SingularMesh();
                VoxelArray array = null;
                try
                {
                    array = alg.createPointCloud(file, resolution, axis[0], axis[1], axis[2], minMaxArray);
                }
                catch (Exception) { DebugLog.logConsole("Threading failed at array creation for " + file); }
                MarchingCubes test = new MarchingCubes();
                test.SetTarget(isoLevel);
                IM intermediate = null;
                try
                {
                    intermediate = test.CreateMesh(array.toFloatArray());
                }
                catch(Exception) { DebugLog.logConsole("Threading failed at intermediate creation for " + file); }
                try
                {
                    if (intermediate.verts.Count > 64999)
                    {
                        DebugLog.logConsole("Dividing " + file);
                        IM[] divs = intermediate.divideIntoSmall();
                        for (int i = 0; i < divs.Length; i++)
                        {
                            divs[i].WriteIntermediateToFile(alg.outputPath + "_" + i + ".IMF");
                        }
                    }
                    else
                    {
                        intermediate.WriteIntermediateToFile(alg.outputPath + ".IMF");
                    }
                    DebugLog.logConsole(alg.outputPath + ".IMF");
                }
                catch(Exception e) { DebugLog.logConsole(e.Message + "\n" + e.StackTrace +"\n"+file+"\n--------------------------------------------"); }

                array = null;
                alg = null;
                intermediate = null;

                try
                {
                    double perc = ((double)(totalMeshes - stack.Count) / (double)totalMeshes) * 100;
                    PercentageClass.UpdatePercentage("Creating Mesh", perc);
                    DebugLog.logConsole("Mesh Creation: " + perc.ToString("#.##") + "% total");
                }
                catch(Exception) { DebugLog.logConsole("Threading failed at percentage writing for " + file);  }

            }
        }
    }
    /// <summary>
    /// Class that creates meshes from all divisions of a file. Is multi-threaded
    /// </summary>
    public static class AllMeshes
    {
        /// <summary>
        /// Pass in the name of the file without extension or path, and the header you want to
        /// have the divions made out of, and will create all the meshes for it for you
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="headerName"></param>
        public static void allFromDivisions(string fileName, string headerName, int resolution, int numberOfThreads, string[] axis, float isoLevel)
        {
            string directory = FileStructure.cacheDirectory + fileName + "\\fields\\" + headerName + "\\";
            string[] files = Directory.GetFiles(directory);
            bool divided = false;
            int numOfMeshes = 0;

            List<float> minMaxArray = new List<float>();

            foreach (string a in axis)
            {
                string filePath = FileStructure.cacheDirectory;
                string path = Path.Combine(filePath, fileName, "fields", a, "mmAray.bin");

                float[] minMax = minMaxOfFile(path);
                minMaxArray.Add(minMax[0]);
                minMaxArray.Add(minMax[1]);
            }

            foreach (string file in files)
            {
                if (Path.GetExtension(file) == ".DIVF" && new FileInfo(file).Length > 1024)
                {
                    divided = true;
                    numOfMeshes++;
                }
            }
            if (!divided)
            {
                DebugLog.logConsole(fileName + " field " + headerName + " has not been formatted properly");
                throw new Exception(fileName + " field " + headerName + " has not been formatted properly");
            }
            //stack creation for multithreading
            ConcurrentStack<string> stack = new ConcurrentStack<string>();
            foreach (string file in files)
            {
                if (Path.GetExtension(file) == ".DIVF" && new FileInfo(file).Length > 1024)
                {
                    stack.Push(file);
                }
            }
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < numberOfThreads; i++)
            {
                //new task that creates a new threadsplit class
                //and calls split on it
                Task t = new Task(() => new ThreadMesh().createMesh(stack, resolution, axis, minMaxArray.ToArray(), isoLevel));
                //add our task to the list
                tasks.Add(t);
                //start the task
                t.Start();
            }
            //wait for all files to be written
            Task.WaitAll(tasks.ToArray());

            DebugLog.logConsole("All tasks completed writing meshes");
        }

        public static float[] minMaxOfFile(string filePath)
        {
            float min = float.MaxValue;
            float max = float.MinValue;
            FileStream stream = File.OpenRead(filePath);
            BinaryReader reader = new BinaryReader(stream);
            min = reader.ReadSingle();
            max = reader.ReadSingle();
            stream.Close();
            reader.Close();
            return new float[] { min, max };
        }


        static string help = "Creates all the meshes for an already divided file based on certain requirements\n" +
                             "AllMeshes <filename> <header> <resolution> <threadNum> <xaxis> <yaxis> <zaxis> <isoLevel>\n\n" +
                             "\t<filename> = the name of the file stored in the cache(not the path)\n" +
                             "\t<header> = the header with which values you want the mesh divded by\n" +
                             "\t<resolution> = the number of \"bins\" you would like to aggregate data by\n" +
                             "\t<threadNum> = the number of threads you would like created\n" +
                             "\t<xaxis> = the column whose values you would like on the x axis\n" +
                             "\t<yaxis> = the column whose values you would like on the y axis\n" +
                             "\t<zaxis> = the column whose values you would like on the z axis\n" +
                             "\t<isoLevel> = the minimum value a bin must have to be considered below the isosurface of the mesh\n";
        static string numArgs = "Invalid number of arguments\nRun with --help to see documentation";


        //arg0 = file name no path
        //arg1 = header for which to create meshes
        //arg2 = resolution
        //arg3 = number of threads
        //arg4 = x axis
        //arg5 = y axis
        //arg6 = z axis
        //arg7 = isoLevel
        public static void Main(string[] args)
        {
            DebugLog.resetLog();
            DebugLog.setDebug(true);
            string xaxis = args[4];
            string yaxis = args[5];
            string zaxis = args[6];
            if (args.Length > 8 || args.Length < 1)
            {
                Console.Out.WriteLine(numArgs);
                Console.In.ReadLine();
                return;
            }
            if (args.Length == 1 && (args[0].ToLower() == "--help" || args[0].ToLower() == "help" || args[0].ToLower() == "?" || args[0].ToLower() == "h"))
            {
                Console.Out.WriteLine(help);
                Console.In.ReadLine();
                return;
            }
            if (!Int32.TryParse(args[2], out int resolution))
            {
                DebugLog.logConsole("resolution given was not a number");
                throw new Exception("resolution given was not a number");
            }
            if (!Int32.TryParse(args[3], out int threadCount))
            {
                DebugLog.logConsole("thread count given was not a number");
                throw new Exception("thread count given was not a number");
            }
            if (!float.TryParse(args[7], out float isoLevel))
            {
                DebugLog.logConsole("isoLevel given was not a number");
                throw new Exception("isoLevel given was not a number");
            }
            if (resolution < 0 || threadCount < 0 || isoLevel < 0f)
            {
                DebugLog.logConsole("Neither thread count, resolution, nor isoLevel can be negative");
                throw new Exception("Neither thread count, resolution, nor isoLevel can be negative");
            }
            string[] axis = new string[] { xaxis, yaxis, zaxis };
            allFromDivisions(args[0], args[1], resolution, threadCount, axis, isoLevel);
        }
    }
}
