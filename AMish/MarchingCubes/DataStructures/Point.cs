using System;
using System.Collections.Generic;
using System.IO;


namespace DataStructures
{

    /// <summary>
    /// Class containing an x, y, and z axis type
    /// As long as the type is comparable, this will work
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Point
    {
        public float x;
        public float y;
        public float z;

        public Point()
        {
            x = 0f;
            y = 0f;
            z = 0f;
        }

        public Point(float i1, float i2, float i3)
        {
            x = i1;
            y = i2;
            z = i3;
        }

        #region maths
        public static float distance(Point p1, Point p2)
        {
            float a = p1.x - p2.x;
            float b = p1.y - p2.y;
            float c = p1.z - p2.z;
            return (float)Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2) + Math.Pow(c, 2));
        }

        public static Point add(Point p1, Point p2)
        {
            float newX = p1.x + p2.x;
            float newY = p1.y + p2.y;
            float newZ = p1.z + p2.z;
            return new Point(newX, newY, newZ);
        }


        public static Point subtract(Point p1, Point p2)
        {
            float newX = p1.x - p2.x;
            float newY = p1.y - p2.y;
            float newZ = p1.z - p2.z;
            return new Point(newX, newY, newZ);
        }

        public static Point subtract(Point p1, float val)
        {
            float newX = p1.x - val;
            float newY = p1.y - val;
            float newZ = p1.z - val;
            return new Point(newX, newY, newZ);
        }

        public static Point add(Point p1, float val)
        {
            float newX = p1.x + val;
            float newY = p1.y + val;
            float newZ = p1.z + val;
            return new Point(newX, newY, newZ);
        }

        public static Point multiply(Point p1, float val)
        {
            float newX = p1.x * val;
            float newY = p1.y * val;
            float newZ = p1.z * val;
            return new Point(newX, newY, newZ);
        }

        public static Point multiply(Point p1, Point p2)
        {
            float newX = p1.x * p2.x;
            float newY = p1.y * p2.y;
            float newZ = p1.z * p2.z;
            return new Point(newX, newY, newZ);
        }

        public static Point divide(Point p1, Point p2)
        {
            float newX = p1.x / p2.x;
            float newY = p1.y / p2.y;
            float newZ = p1.z / p2.z;
            return new Point(newX, newY, newZ);
        }

        public static Point divide(Point p1, float val)
        {
            float newX = p1.x / val;
            float newY = p1.y / val;
            float newZ = p1.z / val;
            return new Point(newX, newY, newZ);
        }
        #endregion


        public static bool CompareTo(List<double> p1, List<double> p2)
        {
            if(p1.Count != p2.Count)
            {
                return false;
            }
            for(int i = 0; i < p1.Count; i++)
            {
                if(p1[i].CompareTo(p2[i]) != 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Compares a point based on x, y, and z coords
        /// in that order
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Point other)
        {
            int compareResult;
            //check x
            if ((compareResult = this.x.CompareTo(other.x)) == 0)
            {
                //if x is equal check y
                if ((compareResult = this.y.CompareTo(other.y)) == 0)
                {
                    //if y is equal, return the compare of z
                    return this.z.CompareTo(other.z);
                }
                //otherwise fall through
            }
            //otherwise return this
            return compareResult;
        }

        public override bool Equals(object obj)
        {
            Point p = obj as Point;
            if(this.CompareTo(p) == 0)
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2;
        }

        /// <summary>
        /// given a binary writer, and a type, will write
        /// the given point to the writer as a the given type
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="point"></param>
        /// <param name="t"></param>
        public static void WritePoint(BinaryWriter writer, Point point)
        {
                writer.Write(point.x);
                writer.Write(point.y);
                writer.Write(point.z);
        }

        /// <summary>
        /// Given a list of points, and a filename, will write
        /// the points with type of t to the file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="t"></param>
        /// <param name="points"></param>
        public static void WritePoints(string fileName, List<Point> points)
        {
            FileStream stream = File.Create(fileName);
            BinaryWriter writer = new BinaryWriter(stream);
            for (int i = 0; i < points.Count; i++)
            {
                WritePoint(writer, points[i]);
            }
            writer.Close();
            stream.Close();
        }

        /// <summary>
        /// Given a reader, and a type, will read from the stream the
        /// correct number of bytes
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Point ReadPoint(BinaryReader reader)
        {
                float xv = reader.ReadSingle();
                float yv = reader.ReadSingle();
                float zv = reader.ReadSingle();
                return new Point(xv, yv, zv);
        }

        /// <summary>
        /// Given a file name, and the type of binary reading you would like,
        /// will binary read a file into a list of IPoints
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static List<Point> ReadPointsFromFile(string fileName)
        {
            FileStream stream = File.OpenRead(fileName);
            BinaryReader reader = new BinaryReader(stream);
            List<Point> retVal = new List<Point>();
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                Point returned = ReadPoint(reader);
                retVal.Add(returned);
            }
            reader.Close();
            stream.Close();
            return retVal;
        }
    }
}
   
   