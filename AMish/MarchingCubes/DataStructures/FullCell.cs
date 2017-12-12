using System.Collections.Generic;
using System.IO;

namespace DataStructures
{
    /// <summary>
    /// A Cell containing all the information for the cell given in the inputted text
    /// headers are held statically in the order given in the text
    /// values are held in the same order as headers
    /// </summary>
    public class FullCell
    {
        //The headers of the file
        public static string[] headers;
        //The cell number of this cell
        public int cellNumber;
        //the decimal values associated with the cell
        public float[] values;

        public FullCell()
        {
            cellNumber = -1;
            headers = null;
            values = null;
        }
        public FullCell(int num, float[] vals)
        {
            cellNumber = num;
            values = vals;
        }

        public bool CompareTo(FullCell f1)
        {
            if(cellNumber != f1.cellNumber)
            {
                return false;
            }
            for(int i = 0; i < headers.Length; i++)
            {
                if(values[i] != f1.values[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool CompareTo(List<FullCell> f1, List<FullCell> f2)
        {
            if(f1.Count != f2.Count)
            {
                return false;
            }
            for(int i = 0; i < f1.Count; i++)
            {
                if (!f1[i].CompareTo(f2[i]))
                {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// Writes a singular cell to a binary writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="cell"></param>
        public static void WriteCell(BinaryWriter writer, FullCell cell)
        {
            writer.Write(cell.cellNumber);
            foreach (double val in cell.values)
            {
                writer.Write(val);
            }
        }

        /// <summary>
        /// Writes a list of Full Cells to a specified file with the following format
        /// <para />
        /// int - number of header <para/>
        /// list(string) - header names separated by \n <para/>
        /// ^^^^This is for all cells^^^^^<para/>
        /// int - the cell number<para/>
        /// list(decimal) - the values for each header<para/>
        /// ^^^^repeated for individual cells^^^^
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="cells"></param>
        public static void WriteCells(string filename, List<FullCell> cells)
        {
            FileStream stream = File.Create(filename);
            BinaryWriter writer = new BinaryWriter(stream);
            //write the number of headers
            writer.Write(FullCell.headers.Length);
            foreach (string header in FullCell.headers)
            {
                writer.Write(header);
            }
            //write the cells
            for (int i = 0; i < cells.Count; i++)
            {
                WriteCell(writer, cells[i]);
            }
            writer.Close();
            stream.Close();
        }

        /// <summary>
        /// Reads a single cell from a given BinaryReader
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="headerCount"></param>
        /// <returns></returns>
        public static FullCell ReadCell(BinaryReader reader, int headerCount)
        {
            int cellNumber = reader.ReadInt32();
            float[] vals = new float[headerCount];
            for (int i = 0; i < vals.Length; i++)
            {
                vals[i] = reader.ReadSingle();
            }
            return new FullCell(cellNumber, vals);
        }

        public static List<FullCell> ReadCells(string filename)
        {
            FileStream stream = File.OpenRead(filename);
            BinaryReader reader = new BinaryReader(stream);
            //read the number of headers
            int numOfHeaders = reader.ReadInt32();
            //for each header, read a string and add it to the static headers
            FullCell.headers = new string[numOfHeaders];
            for(int i = 0; i < numOfHeaders; i++)
            {
                if (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    headers[i] = reader.ReadString();
                }
            }
            List<FullCell> cells = new List<FullCell>();
            //read the cells
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                FullCell cell = ReadCell(reader, numOfHeaders);
                cells.Add(cell);
            }
            reader.Close();
            stream.Close();
            return cells;
        }
    }

}
