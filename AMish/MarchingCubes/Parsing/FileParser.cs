using ServerSide.Tools;
using System;
using System.IO;
using System.Text;


namespace ServerSide.Parsing
{
    /// <summary>
    /// Class used for parsing a raw, comma-delimited file into fields.
    /// Each file MUST contain a "CellNumber", "x-coordinate", "y-coordinate",
    /// and "z-coordinate" header.
    /// </summary>
    public class FileParser
    {
        //Console Input
        #region console ver
        static string help = "Parses a large file and splits it into its respective attributes binaries\n" +
                             "file which can be entirely held in ram for fast searching\n\n" +
                             "FileParser <filename>\n\n" +
                             "<filename> = the file with the original data\n" +
                             "Attributes must be in the first line. See Documentation for file directory structure.\n";

        static string numArgs = "Invalid number of arguments\nRun with --help to see documentation";

        static void Main(string[] args)
        {
            DebugLog.resetLog();
            DebugLog.setDebug(true);
            string fileIn = "";


            if (args.Length > 1 || args.Length < 1)
            {
                Console.Out.WriteLine(numArgs);
                Console.In.ReadLine();
                return;
            }
            if (args.Length == 1 && (args[0].ToLower() == "--help" || args[0].ToLower() == "help" || args[0].ToLower() == "?" || args[0].ToLower() == "h"))
            {
                Console.WriteLine(help);
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
                fileIn = args[0];
            }
            //Parse
            parseDataIntoCache(fileIn);
        }
        #endregion
        

        public static long estimatedNumberOfEntries(string filein)
        {
            long fileSize = new FileInfo(filein).Length;
            StreamReader fileIN = new StreamReader(filein);
            string line = "";
            line = fileIN.ReadLine();
            fileSize -= Encoding.ASCII.GetByteCount(line);
            line = fileIN.ReadLine();
            long entry = Encoding.ASCII.GetByteCount(line);
            return (fileSize / entry);
        }

        public static int estimatedBytesInOneEntry(string filein)
        {
            StreamReader fileIN = new StreamReader(filein);
            string line = "";
            line = fileIN.ReadLine();
            //These are the string headers
            string[] parsedPoints = line.Replace(" ", "").Split(',');
            fileIN.Close();
            return 16 * (parsedPoints.Length);
        }
        
        /// <summary>
        /// Takes a file path. If the file already exists in the cache,
        /// the file is deleted from the cache. The cache directory
        /// is then filled with the corresponding files
        /// </summary>
        /// <param name="filein"></param>
        public static void parseDataIntoCache(string filein)
        {
            long fileSize = new FileInfo(filein).Length;
            StreamReader fileIN = new StreamReader(filein);
            string line = "";
            line = fileIN.ReadLine();
            //These are the string headers
            string[] parsedPoints = line.Replace(" ", "").Split(',');
            FileStructure.removeFileFromCache(Path.GetFileNameWithoutExtension(filein));
            //Creates cache directories for file
            FileStructure.addFileToDirectory(filein);
            //Writers for their own subjective 
            FileStream[] streams = new FileStream[parsedPoints.Length - 1];
            BinaryWriter[] writers = new BinaryWriter[parsedPoints.Length - 1];
            //We're doing something a bit weird here, but it saves time in the
            //post-parsing stage if we do it now.
            //we're going to save the min and max of each header into its own file.
            //The min max array goes [header1Min, header1Max, header2min, ...]
            float[] minMaxArray = new float[2 * (parsedPoints.Length - 1)];
            //here we set the min max to be -+inf
            for(int i = 0; i < minMaxArray.Length; i++)
            {
                if(i%2 == 0)
                {
                    minMaxArray[i] = float.MaxValue;
                }
                else
                {
                    minMaxArray[i] = float.MinValue;
                }
            }
            //creating a string array so that we know where to write our min max arrays to at each end
            string[] minMaxDestinations = new string[parsedPoints.Length - 1];
            //for each header, we create a directory within it, then 
            //a file named full.NBF in each which we will write to over
            //the course of the parsing
            //We don't care about the cell number as that is held natively
            for (int i = 1; i < parsedPoints.Length-2; i++)
            {
                string headerPath = FileStructure.createHeaderDirectory(parsedPoints[i]);
                streams[i-1] = File.Open(headerPath+"full.bnf", FileMode.Create);
                writers[i-1] = new BinaryWriter(streams[i-1]);
                //here we do the add the headerpath to our minMacDestinations
                minMaxDestinations[i - 1] = headerPath;
            }
            long bytes = 0;
            int count = 0;
            DebugLog.logConsole("Reading "+filein+"...");
            //As long as the string is not null or empty
            while (!String.IsNullOrEmpty(line = fileIN.ReadLine()))
            {
                bytes += Encoding.ASCII.GetByteCount(line);
                parsedPoints = line.Replace(" ", "").Split(',');
                //For each point, save it to it's respective file
                //again we don't care about cell number
                for(int i = 1; i < parsedPoints.Length - 2 ; i++)
                {
                    float point = float.Parse(parsedPoints[i]);
                    writers[i-1].Write(point);
                    //checking our min max of each point
                    if (point < minMaxArray[2 * (i-1)])
                        minMaxArray[2 * (i-1)] = point;
                    if (point > minMaxArray[(2 * (i-1)) + 1])
                        minMaxArray[(2 * (i-1)) + 1] = point;
                }
                //For measuring progress
                if (count == 500000)
                {
                    count = 0;
                    double perc = ((double)bytes / (double)fileSize) * 100;
                    PercentageClass.UpdatePercentage("Parsing", perc);
                    DebugLog.logConsole("Parsing File :"+perc.ToString("#.##") + "%");
                }
                count++;
            }
            //Closing the outputs
            foreach (BinaryWriter w in writers)
            {
                if (w != null)
                {
                    w.Close();
                }
            }
            foreach (FileStream s in streams)
            {
                if (s != null)
                {
                    s.Close();
                }
            } 
            //We are done with the input
            fileIN.Close();
            //Now we write the min max of each header to file
            for(int i = 0; i < minMaxDestinations.Length; i++)
            {
                if(minMaxDestinations[i] == null)
                {
                    continue;
                }
                Stream stream = File.Create(minMaxDestinations[i] + "mmAray.bin");
                BinaryWriter writer = new BinaryWriter(stream);
                DebugLog.logConsole(minMaxDestinations[i] +
                    "mmAray.bin had minimum :\n\t" + minMaxArray[i * 2].ToString() +
                    "\nand maximum :\n\t" + minMaxArray[(i * 2) + 1].ToString());
                writer.Write(minMaxArray[i * 2]);
                writer.Write(minMaxArray[(i * 2) + 1]);
                writer.Close();
                stream.Close();
            }
        }
    }
}

