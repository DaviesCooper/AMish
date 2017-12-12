using System;
using System.Collections.Generic;
using System.IO;

namespace DataStructures
{
    /// <summary>
    /// A given datastructure for after the conversion of voxels via marching cubes.
    /// contains a list of FloatCoords which are edges for 3DVertexes in Unity, and
    /// a list of ints which says how to build triangles from those edges
    /// </summary>
    public class Cube
    {
        //This is NOT! the corners of the cube. This is the centre points of all the edges.
        public Point[] edges;
        //Index for which edge point is part of the mesh
        public int[] triangles;
        public int bitmask;
        /// <summary>
        /// Pass in the bottom left frontmost locations as x, y, and z. This is because the
        /// centre points of the boolean arrays are considered the corner points of the cubes.
        /// Because of this, the step length must also be passed in so as to calculate the centre
        /// of the cubes themselves
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="stepx"></param>
        /// <param name="stepy"></param>
        /// <param name="stepz"></param>
        public Cube(float x, float y, float z)
        {
            //X, Y, Z are the centre of the cube.
            float hx = 0.5f;
            float hy = 0.5f;
            float hz = 0.5f;
            edges = new Point[12];
            //REFER TO THE PAPER AT http://www.cc.gatech.edu/~bader/COURSES/GATECH/CSE6140-Fall2007/papers/LC87.pdf
            //FOR NOTATION
            edges[0] = new Point(x, (y - hy), (z - hz)); //0--
            edges[1] = new Point((x + hx), y, (z - hz)); //+0-
            edges[2] = new Point(x, (y + hy), (z - hz)); //0+-
            edges[3] = new Point((x - hx), y, (z - hz)); //-0-
            edges[4] = new Point(x, (y - hy), (z + hz)); //0-+
            edges[5] = new Point((x + hx), y, (z + hz)); //+0+
            edges[6] = new Point(x, (y + hy), (z + hz)); //0++
            edges[7] = new Point((x - hx), y, (z + hz)); //-0+
            edges[8] = new Point((x - hx), (y - hy), z); //--0
            edges[9] = new Point((x + hx), (y - hy), z); //+-0
            edges[10] = new Point((x - hx), (y + hy), z);//-+0
            edges[11] = new Point((x + hx), (y + hy), z);//++0
        }

        /// <summary>
        /// Given a list of edges and triangles, stores these within
        /// </summary>
        /// <param name="e"></param>
        /// <param name="t"></param>
        public Cube(Point[] e, int[] t)
        {
            edges = e;
            triangles = t;
        }

        /// <summary>
        /// Writes a single cube to a file:
        /// 36 floats followed by 16 ints
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="cube"></param>
        public static void WriteCube(BinaryWriter writer, float cube)
        {

            writer.Write(cube);
        }

        /// <summary>
        /// </summary>
        /// <returns>Stringified Hashset of the verticies.</returns>
        public static void WriteCubes(string filename, List<float> cubes)
        {
            FileStream stream = File.Create(filename);
            BinaryWriter writer = new BinaryWriter(stream);
            foreach (float c in cubes)
            {
                WriteCube(writer, c);
            }
            writer.Close();
            stream.Close();
        }

        public static List<Point> readMeshes(string filename)
        {
            List<Point> retVal = new List<Point>();
            FileStream stream = File.OpenRead(filename);
            BinaryReader reader = new BinaryReader(stream);
            while(reader.BaseStream.Position < reader.BaseStream.Length)
            {
                retVal.Add(Point.ReadPoint(reader));
            }
            return retVal;
        }

        public bool CompareTo(Cube c)
        {
            for(int i = 0; i < c.edges.Length; i++)
            {
                if(edges[i].CompareTo(c.edges[i]) != 0)
                {
                    return false;
                }
            }
            for(int i = 0; i < triangles.Length; i++)
            {
                if(triangles[i] != c.triangles[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool CompareTo(List<Cube> c1, List<Cube> c2)
        {
            if(c1.Count != c2.Count)
            {
                return false;
            }
            for(int i = 0; i < c1.Count; i++)
            {
                if (!c1[i].CompareTo(c2[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
