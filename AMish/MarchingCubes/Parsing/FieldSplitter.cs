using DataStructures;
using ServerSide.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ServerSide.Parsing
{
    /// <summary>
    /// Threads that will pop from the stack until the stack is empty
    /// The stack should contain the header names
    /// </summary>
    public class ThreadSplit
    {
        /// <summary>
        /// The main thread method that will pop from the stack, then split the file
        /// based on the header defined in the passed in stack
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="stack"></param>
        /// <param name="numOfFiles"></param>
        public void split(string filename, ConcurrentStack<String> stack, int numOfFiles)
        {
            while (!stack.IsEmpty)
            {
                //If we fail popping, we may be attempting to access
                //at the same time as someone else. This means the stack
                //might have emptied since we last checked so we check again
                if (!stack.TryPop(out string header))
                {
                    continue;
                }
                //If we got this far, then we have a header
                FieldSplitter splitter = new FieldSplitter();
                DebugLog.logConsole("Splitting "+filename+" file based on " + header);
                splitter.splitFile(filename, header, numOfFiles);
                //remove our reference to splitter for the next iteration
                splitter = null;

                double perc = ((double)(numOfFiles - stack.Count) / (double)numOfFiles) * 100;

                PercentageClass.UpdatePercentage("Splitting " + header, perc);
                DebugLog.logConsole("File splitting: "+perc.ToString("#.##") + "% total");
            }
            //Since the stack is empty, we are done
        }
    }
    /// <summary>
    /// Class that splits files. Can split either an individual header, or all header at once.
    /// If all at once, then threading is used to speed the process up
    /// </summary>
    public class FieldSplitter
    {

        static string numArgs = "Invalid number of arguments\nRun with --help to see documentation";
        static string help =    "Parses a binary file that was created via FileParser and\n" +
                                "Then splits the binary into x number of divisions based on its values.\n\n" +
                                "FieldSplitter <file> <divisionNumber> <columnToDivide>*\n\n" +
                                "\t<file> = the path to the file ou want a mesh created out of\n" +
                                "\t<divisionNumber> = the number of \"bins\" you would like to aggregate data by\n" +
                                "\t<ColumnsToDivide> = the column whose values you would like on the x axis. blank means all\n" +
                                "\t<yaxis> = the column whose values you would like on the y axis\n" +
                                "\t<zaxis> = the column whose values you would like on the z axis\n" +
                                "\t<isoLevel> = the minimum value a bin must have to be considered below the isosurface of the mesh\n";

        /// <summary>
        /// args0 = file name not path
        /// args1 = number of divisions
        /// args2+=
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            DebugLog.resetLog();
            DebugLog.setDebug(true);
            DateTime now = DateTime.Now;
            if(args.Length < 1)
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
            if(!Int32.TryParse(args[1], out int divNum))
            {
                DebugLog.logConsole("divisionNumber was not a number");
                throw new Exception("divisionNumber was not a number");
            }
            if(divNum < 1)
            {
                DebugLog.logConsole("divisionNumber must be a positive number");
                throw new Exception("divisionNumber must be a positive number");
            }
            if (args.Length <2)
            {
                FieldSplitter.splitFileAll(args[0], divNum);
            }
            else
            {
                string[] headers = new string[args.Length - 2];
                for(int i = 2; i < args.Length; i++)
                {
                    headers[i - 2] = args[i];
                }
                FieldSplitter.splitFileGiven(args[0], headers,divNum);
            }
            DateTime fin = DateTime.Now;
            TimeSpan span = (fin - now);
            DebugLog.logConsole("Minutes to divide all = " + span.TotalMinutes);
        }
        /// <summary>
        /// Multithreads splitting all the full files into divisions
        /// </summary>
        /// <param name="name">file name not path</param>
        /// <param name="numOfThreads"></param>
        /// <param name="numOfFiles"></param>
        public static void splitFileAll(string name, int numOfFiles)
        {
            //grab the header names for the attributes.
            ConcurrentStack<string> headers = new ConcurrentStack<string>();
            string dir = FileStructure.cacheDirectory + name + "\\fields\\";
            foreach(string headerPath in Directory.GetDirectories(dir))
            {
                string headerName = Path.GetFileName(headerPath);
                headers.Push(headerName);
            }
            //cleaning every last bit of memory I can
            dir = null;
            //Create each task now
            List<Task> tasks = new List<Task>();
            for(int i =0; i < 2; i++)
            {
                //new task that creates a new threadsplit class
                //and calls split on it
                Task t = new Task(() => new ThreadSplit().split(name, headers, numOfFiles));
                //add our task to the list
                tasks.Add(t);
                //start the task
                t.Start();
            }
            //wait for all files to be written
            Task.WaitAll(tasks.ToArray());
            DebugLog.logConsole("All tasks completed writing divisions");
            //return control to main thread
        }
        /// <summary>
        /// Multithreads splitting a given list of headers into divisions
        /// </summary>
        /// <param name="name">file name not path</param>
        /// <param name="numOfThreads"></param>
        /// <param name="numOfFiles"></param>
        public static void splitFileGiven(string name, string[] headerNames, int numOfFiles)
        {
            //grab the header names for the attributes.
            ConcurrentStack<string> headers = new ConcurrentStack<string>();
            string dir = FileStructure.cacheDirectory + name + "\\fields\\";
            foreach (string header in headerNames)
            {
                headers.Push(header);
            }
            //cleaning every last bit of memory I can
            dir = null;
            //Create each task now
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 2; i++)
            {
                //new task that creates a new threadsplit class
                //and calls split on it
                Task t = new Task(() => new ThreadSplit().split(name, headers, numOfFiles));
                //add our task to the list
                tasks.Add(t);
                //start the task
                t.Start();
            }
            //wait for all files to be written
            Task.WaitAll(tasks.ToArray());
            DebugLog.logConsole("All tasks completed writing divisions");
            //return control to main thread
        }
        /// <summary>
        /// Given a file that is found in the cache, (WITHOUT PATH), will divide the
        /// .BNF of the given header file into numOfFIles files. Outputs the divisions as
        /// their own .DIV file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="header"></param>
        /// <param name="numOfFiles"></param>
        public void splitFile(string file, string header, int numOfFiles)
        {
            DebugLog.logConsole("Deleting old files in directory");
            
            string dirPath = Path.Combine(FileStructure.cacheDirectory, file, "fields", header);
            string filename = Path.Combine(dirPath, "full.BNF");
            string outputPath = Path.GetDirectoryName(filename);     
            string[] files = Directory.GetFiles(outputPath);
            outputPath = null;
            for (int i = 0; i < files.Length; i++)
            {
                if (Path.GetExtension(files[i]) == ".DIVF")
                {
                    File.Delete(files[i]);
                }
            }           
            List<FieldCell> cells = new List<FieldCell>();
            FileStream stream = File.OpenRead(filename);
            BinaryReader reader = new BinaryReader(stream);
            int cell = 0;
            dirPath = null;            
            //If the wrong type of file was passed in
            if((new FileInfo(filename)).Extension != ".BNF")
            {
                DebugLog.logConsole("Disallowed file type for reading");
                throw new Exception("Disallowed file type for reading");
            }
            DebugLog.logConsole(file+": Reading Binary...");
            //Read the full BNF
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                //The size of one deciaml
                float field = reader.ReadSingle();
                int cellNumber = cell;

                FieldCell fromFile = new FieldCell(cellNumber, field);
                cells.Add(fromFile);
                cell++;
                fromFile = null;
            }
            reader.Close();
            stream.Close();

            if (numOfFiles > cells.Count)
            {
                DebugLog.logConsole("Too many divisions. There are less points than that");
                return;
            }

            //Now that we have the full array, we can divide it into its individual files
            //First we sort
            DebugLog.logConsole("Sorting "+ file+"...");
            cells.Sort
                (
                    delegate (FieldCell t1, FieldCell t2)
                    {
                        return (t1.field.CompareTo(t2.field));
                    }
                );

            //How to divide the points
            /***
            NOTE:
            A point belongs to mesh n where
            n = (floor(value of point / divider))
            n cannot be greater than the numOfMeshes
            this also means that 
            n * divider = temp of Mesh n

            This means we can store an array of size n to hold all the values of the meshes
            then convert this to a dictionary for later use
            ***/

            float divider = (cells.Last<FieldCell>().field / numOfFiles);


            //Now we need to separate this array into smaller arrays
            //Initializing all the lists of the array
            List<List<FieldCell>> pointArray = new List<List<FieldCell>>();
            for (int i = 0; i < numOfFiles; i++)
            {
                pointArray.Add(new List<FieldCell>());
            }
            DebugLog.logConsole("Filling "+file+" arrays...");
            //Iterate through all our points and add them to their corresponding array
            //foreach loop impossible due to removal
            for (int i = 0; i < cells.Count; i++)
            {
                int meshGroup = ((int)Math.Ceiling(cells[i].field / divider)) - 1;
                if (meshGroup < 0)
                {
                    meshGroup = 0;
                }
                try
                {
                    pointArray[meshGroup].Add(new FieldCell(cells[i]));
                }
                catch(Exception)
                {
                    DebugLog.logConsole(file +": Error on adding point " + i);
                }
                //This is to semi-clean memory (other-wise we have all the coords appear twice in ram)
                cells[i] = null;
            }

            //For garbage cleaning, make sure I get rid of any remnants of the original array
            cells = null;
            DebugLog.logConsole("Writing Files from " +file+"...");
            //Write each array to its own file
            for (int i = 0; i < pointArray.Count; i++)
            {
                float temp = i * divider;
                float temp2 = (i * divider) + divider;
                outputPath = Path.GetDirectoryName(filename) + "\\";
                string outputName = outputPath + temp +"_"+temp2+".DIVF";
                if (pointArray[i].Count > 3)
                {
                    FieldCell.WriteCells(outputName, pointArray[i]);
                    DebugLog.logConsole("File " + outputName + " written with " + pointArray[i].Count + " entries");
                }
                
                //Clearing ram as we progress
                pointArray[i] = null;
            }
        }
    }
}
