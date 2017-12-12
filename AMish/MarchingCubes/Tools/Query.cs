using DataStructures;
using ServerSide.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ServerSide.Tools
{
    public class Query
    {
        public static void readText(string textFile, List<int> cells)
        {
            cells = cells.Distinct().ToList();
            cells.Sort();

            DateTime start = DateTime.Now;
            StreamReader fileIN = new StreamReader(textFile);
            string line = "";
            line = fileIN.ReadLine();

            string[] parsedPoints = line.Replace(" ", "").Split(',');
            FullCell.headers = new string[parsedPoints.Length];
            for (int i = 0; i < parsedPoints.Length; i++)
            {
                FullCell.headers[i] = parsedPoints[i];
            }

            int numToRead = 1;
            int bIndex = 0;
            for (int i = 0; i < cells.Count; i++)
            {
                //number of lines to read
                numToRead = (cells[i] - bIndex) - 1;
                //incrementing our readers to the correct index
                for (int j = 0; j < numToRead; j++)
                {
                    fileIN.ReadLine();
                }
                //Now on the line we want
                line = fileIN.ReadLine();
                parsedPoints = line.Replace(" ", "").Split(',');

                FullCell cell1 = new FullCell()
                {
                    values = new float[parsedPoints.Length]
                };
                for (int k = 0; k < parsedPoints.Length; k++)
                {
                    cell1.values[k] = float.Parse(parsedPoints[k]);
                }
                bIndex = cells[i];
            }
            DebugLog.logConsole((DateTime.Now - start).Seconds.ToString());
        }

        public static void readBinaries(string filename, List<int> cells)
        {
            cells = cells.Distinct().ToList();
            cells.Sort();

            DateTime start = DateTime.Now;
            string folderPath = Path.Combine(FileStructure.cacheDirectory, filename, "fields");
            string[] directories = Directory.GetDirectories(folderPath);

            //stream setup
            FileStream[] streams = new FileStream[directories.Length];
            BinaryReader[] readers = new BinaryReader[directories.Length];
            for (int i = 0; i < directories.Length; i++)
            {
                string filePath = Path.Combine(directories[i], "full.BNF");
                streams[i] = new FileStream(filePath, FileMode.Open);
                readers[i] = new BinaryReader(streams[i]);
            }

            FullCell.headers = new string[directories.Length + 1];
            for (int j = 0; j < directories.Length; j++)
            {
                FullCell.headers[j + 1] = directories[j];
            }
            FullCell.headers[0] = "CellNumber";

            int numToRead = 1;
            int bIndex = 0;
            for (int i = 0; i < cells.Count; i++)
            {
                //The number of decimals to read from each file is:
                //numToRead = cells[i].cellnumber - bIndex
                //The next bytes will be the values associated with the
                //desired cell
                //Then set bIndex = cells[i].cellnumber so that our bIndex
                //is updated
                numToRead = (cells[i] - bIndex) - 1;
                //incrementing our readers to the correct index

                foreach (BinaryReader r in readers)
                {
                    r.ReadBytes(16 * numToRead);
                }
                FullCell cell1 = new FullCell()
                {
                    values = new float[FullCell.headers.Length]
                };
                cell1.values[0] = cells[i];
                for (int j = 0; j < readers.Length; j++)
                {
                    float val = readers[0].ReadSingle();
                    cell1.values[j + 1] = val;
                }

                bIndex = cells[i];

            }
            DebugLog.logConsole((DateTime.Now - start).Seconds.ToString());
        }

    }
}
