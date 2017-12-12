using System;
using System.Collections.Generic;
using System.IO;

namespace DataStructures
{
    /// <summary>
    /// A Cell containing the value for a specific field, and the cell number
    /// </summary>
    public class FieldCell
    {
        /// <summary>
        /// UNIT TESTS
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            List<FieldCell> ls = new List<FieldCell>();
            for(int i = 0; i < 100; i++)
            {
                Random var = new Random();
                int cell = var.Next(100, 10000);
                ls.Add(new FieldCell(cell, (float)var.NextDouble()));
            }
        }


        public float field;
        public int cellNum;

        /// <summary>
        /// Sets cellNum to -1 to indicate an empty cell
        /// </summary>
        public FieldCell()
        {
            cellNum = -1;
            field = 0;
        }

        public FieldCell(int cell, float f)
        {
            cellNum = cell;
            field = f;
        }

        public FieldCell(FieldCell i)
        {
            field = i.field;
            cellNum = i.cellNum;
        }

        /// <summary>
        /// Writes a single point to an already given stream
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="point"></param>
        public static void WriteCell(BinaryWriter writer, FieldCell point)
        {
            writer.Write(point.cellNum);
            writer.Write(point.field);
        }

        /// <summary>
        /// Writes a list of points to a binary file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="points"></param>
        public static void WriteCells(string fileName, List<FieldCell> points)
        {
            FileStream stream = File.Create(fileName);
            BinaryWriter writer = new BinaryWriter(stream);
            for (int i = 0; i < points.Count; i++)
            {
                WriteCell(writer, points[i]);
            }
            writer.Close();
            stream.Close();
        }

        /// <summary>
        /// Given a read, will read a Point of type decimal
        /// from a file
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static FieldCell readCell(BinaryReader reader)
        {
            int cellNum = reader.ReadInt32();
            float field = reader.ReadSingle();
            return new FieldCell(cellNum, field);
        }

        /// <summary>
        /// Reads a binary file
        /// </summary>
        /// <param name="fileName">the filepath to the given file</param>
        /// <returns></returns>
        public static List<FieldCell> ReadCells(string fileName)
        {
            FileStream stream = File.OpenRead(fileName);
            BinaryReader reader = new BinaryReader(stream);
            List<FieldCell> retVal = new List<FieldCell>();
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                FieldCell fromFile = readCell(reader);
                retVal.Add(fromFile);
            }
            reader.Close();
            stream.Close();
            return retVal;
        }

        public bool CompareTo(FieldCell f1)
        {
            if(field != f1.field)
            {
                return false;
            }
            if(cellNum != f1.cellNum)
            {
                return false;
            }
            return true;
        }

        public static bool CompareTo(List<FieldCell> f1, List<FieldCell> f2)
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
    }

}
