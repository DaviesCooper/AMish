using ServerSide.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataStructures
{
    public class IM
    {
        public List<Point> verts;
        public HashSet<Point> set;
        public List<int> tris;

        public IM()
        {
            verts = new List<Point>();
            set = new HashSet<Point>();
            tris = new List<int>();
        }
        public IM(List<Point> ps, List<int> ts)
        {
            verts = ps;
            tris = ts;
        }

        /// <summary>
        /// Requires filepath WITHOUT the extension. Weird, and not
        /// constant across the program I know, but thats only because 
        /// </summary>
        /// <param name="filePath"></param>
        public void WriteIntermediateToFile(string filePath)
        {
            if (verts.Count <= 0)
            {
                DebugLog.logConsole(filePath + " vertices do not exists");
                return;
            }
            //weld the verts before writing
            makeUnique();
            FileStream stream = File.Create(filePath);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(verts.Count);
            for (int i = 0; i < verts.Count; i++)
            {
                Point.WritePoint(writer, verts[i]);
            }
            writer.Write(tris.Count);
            for (int i = 0; i < tris.Count; i++)
            {
                writer.Write(tris[i]);
            }
        }

        public static IM ReadIntermediateFromFile(string filePath)
        {
            FileStream stream = File.OpenRead(filePath);
            BinaryReader reader = new BinaryReader(stream);
            List<Point> vertices = new List<Point>();
            List<int> triangles = new List<int>();

            int vertCount = reader.ReadInt32();
            for (int i = 0; i < vertCount; i++)
            {
                vertices.Add(Point.ReadPoint(reader));
            }
            int triCount = reader.ReadInt32();
            for (int i = 0; i < triCount; i++)
            {
                triangles.Add(reader.ReadInt32());
            }

            return new IM(vertices, triangles);
        }

        /// <summary>
        /// Adds a point to the list IF it does not already exist within it.
        /// Also adds the triangle of the newly added point
        /// </summary>
        /// <param name="p"></param>
        public void addPoint(Point p)
        {
            if (set.Contains(p))
            {
                tris.Add(verts.IndexOf(p));
            }
            set.Add(p);
            verts.Add(p);
            tris.Add(verts.Count - 1);
        }

        ///// <summary>
        ///// To be used in conjunction with make unique to reduce the vertex count
        ///// of all meshes greater than 65000 for use in Unity
        ///// </summary>
        ///// <param name="meshToDivide"></param>
        ///// <returns></returns>
        public IM[] divideIntoSmall()
        {
            int divisions = (verts.Count + 64999) / 65000;
            IM[] retVal = new IM[divisions + 1];
            for (int i = 0; i < retVal.Length; i++)
            {
                retVal[i] = new IM();
            }
            int currentIndex = 0;
            for (int p = 0; p < verts.Count - 2; p += 3)
            {
                if (retVal[currentIndex].verts.Count > 64996)
                {
                    currentIndex++;
                }
                for (int i = 0; i < 3; i++)
                {
                    Point a = verts[p + i];
                    retVal[currentIndex].verts.Add(a);
                    retVal[currentIndex].tris.Add(retVal[currentIndex].verts.Count - 1);
                    retVal[currentIndex].set.Add(a);
                }
            }
            return retVal;
        }

        /// <summary>
        /// Vertex welding
        /// </summary>
        public void makeUnique()
        {
            Point[] asArray = set.ToArray<Point>();
            int[] triArray = new int[tris.Count];
            //For every triangle find the vertices "unique" index
            for (int i = 0; i < tris.Count; i++)
            {
                //▼▼▼▼▼ READABLE VERSION ▼▼▼▼▼▼▼
                //int pointIndex = tris[i];
                //Point p = verts[pointIndex];
                //int uniqueIndex = Array.IndexOf(asArray, p);
                //triArray[i] = uniqueIndex;
                //▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
                triArray[i] = Array.IndexOf(asArray, verts[tris[i]]);
            }
            //Now that we have "welded" we replace our old versions
            verts = asArray.ToList<Point>();
            tris = triArray.ToList<int>();
        }
    }
}
