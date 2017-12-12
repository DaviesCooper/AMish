using System.Collections.Generic;
using System.IO;

namespace DataStructures
{
    /// <summary>
    /// A Cell containing an IDCell, and a Point
    /// </summary>
    public class AttCoordCell
    {
        public FieldCell fieldCell;
        public Point coords;

        public AttCoordCell()
        {
            fieldCell = null;
            coords = null;
        }

        public AttCoordCell(FieldCell cell, Point point)
        {
            fieldCell = cell;
            coords = point;
        }

        public AttCoordCell(int c, float xc, float yc, float zc, float field)
        {
            this.fieldCell = new FieldCell(c, field);
            this.coords = new Point((float)xc, (float)yc, (float)zc);
        }

        /// <summary>
        /// Comparable for two cells
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool CompareTo(AttCoordCell other)
        {
            //check x
            if (coords.x == other.coords.x)
            {
                //if x is equal check y
                if (coords.y == other.coords.y)
                {
                    //if y is equal, return the compare of z
                    return coords.z == other.coords.z;
                }
                //otherwise fall through
            }
            //otherwise return this
            return false;
        }

        /// <summary>
        /// Iterates over two lists of AttCoordCells to compare if they are equal
        /// </summary>
        /// <param name="one"></param>
        /// <param name="two"></param>
        /// <returns></returns>
        public static bool CompareTo(List<AttCoordCell> one, List<AttCoordCell> two)
        {
            if (one.Count != two.Count) return false;
            for (int i = 0; i < one.Count; i++)
            {
                if(!one[i].CompareTo(two[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static void WriteCell(BinaryWriter writer, AttCoordCell cell)
        {
            FieldCell.WriteCell(writer, cell.fieldCell);
            Point.WritePoint(writer, cell.coords);
        }

        /// <summary>
        /// Writes a list of points to a binary file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="points"></param>
        public static void WriteCells(string fileName, List<AttCoordCell> points)
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

        public static AttCoordCell ReadCell(BinaryReader reader)
        {
            FieldCell cell = FieldCell.readCell(reader);
            Point point = Point.ReadPoint(reader);
            return new AttCoordCell(cell, point);
        }

        /// <summary>
        /// Reads a binary file into a list of points
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static List<AttCoordCell> ReadCells(string fileName)
        {
            FileStream stream = File.OpenRead(fileName);
            BinaryReader reader = new BinaryReader(stream);
            List<AttCoordCell> retVal = new List<AttCoordCell>();
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                retVal.Add(ReadCell(reader));
            }
            reader.Close();
            stream.Close();
            return retVal;
        }
    }

    /// <summary>
    /// A Cell containing the value for the x,y,z coords, and the cell number
    /// </summary>
    public class NumCoordCell
    {
        public int cellNumber;
        public Point coords;

        /// <summary>
        /// sets cell number to -1 to signify that the cell is empty
        /// </summary>
        public NumCoordCell()
        {
            this.cellNumber = -1;
            this.coords = null;
        }
        public NumCoordCell(int cellNum, Point point)
        {
            this.cellNumber = cellNum;
            this.coords = point;
        }
        public NumCoordCell(int c, double xc, double yc, double zc)
        {
            this.cellNumber = c;
            coords = new Point((float)xc, (float)yc, (float)zc);
        }
        /// <summary>
        /// Comparable for two cells
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool CompareTo(NumCoordCell other)
        {
            //check x
            if (coords.x == other.coords.x)
            {
                //if x is equal check y
                if (coords.y == other.coords.z)
                {
                    //if y is equal, return the compare of z
                    return coords.z == other.coords.z;
                }
                //otherwise fall through
            }
            //otherwise return this
            return false;
        }
        /// <summary>
        /// Iterates over two lists of AttCoordCells to compare if they are equal
        /// </summary>
        /// <param name="one"></param>
        /// <param name="two"></param>
        /// <returns></returns>
        public static bool CompareTo(List<NumCoordCell> one, List<NumCoordCell> two)
        {
            if (one.Count != two.Count) return false;
            for (int i = 0; i < one.Count; i++)
            {
                if (!one[i].CompareTo(two[i]))
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Given a binary writer and a cell, writes the cell to a
        /// file using the writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="cell"></param>
        public static void WriteCell(BinaryWriter writer, NumCoordCell cell)
        {
            writer.Write(cell.cellNumber);
            Point.WritePoint(writer, cell.coords);
        }
        /// <summary>
        /// Writes a list of points to a binary file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="points"></param>
        public static void WriteCells(string fileName, List<NumCoordCell> points)
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

        public static NumCoordCell ReadCell(BinaryReader reader)
        {
            int cellNum = reader.ReadInt32();
            Point point = Point.ReadPoint(reader);
            return new NumCoordCell(cellNum, point);
        }
        /// <summary>
        /// Reads a binary file into a list of points
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static List<NumCoordCell> ReadCells(string fileName)
        {
            FileStream stream = File.OpenRead(fileName);
            BinaryReader reader = new BinaryReader(stream);
            List<NumCoordCell> retVal = new List<NumCoordCell>();
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                NumCoordCell cell = ReadCell(reader);
                retVal.Add(cell);
            }
            reader.Close();
            stream.Close();
            return retVal;
        }

        public static float[] findMinMaxArray(List<NumCoordCell> cells)
        {
            float minx = float.MaxValue;
            float miny = float.MaxValue;
            float minz = float.MaxValue;
            float maxx = float.MinValue;
            float maxy = float.MinValue;
            float maxz = float.MinValue;
            //finding the min/max for each coord
            foreach (NumCoordCell p in cells)
            {
                if (p.coords.x > maxx) maxx = p.coords.x;
                if (p.coords.x < minx) minx = p.coords.x;
                if (p.coords.y > maxy) maxy = p.coords.y;
                if (p.coords.y < miny) miny = p.coords.y;
                if (p.coords.z > maxz) maxz = p.coords.z;
                if (p.coords.z < minz) minz = p.coords.z;
            }
            return new float[]{ minx, maxx, miny, maxy, minz, maxz };
        }
    }
}
