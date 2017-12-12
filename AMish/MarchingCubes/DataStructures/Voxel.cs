using System;
using System.Collections.Generic;
using System.IO;

namespace DataStructures
{
    public class Voxel
    {
        public List<int> cells;
        public Point coord;

        public Voxel(List<int> contains, Point point)
        {
            cells = new List<int>(contains);
            coord = point;
        }

        public bool CompareTo(Voxel v1)
        {
            if (coord.CompareTo(v1.coord) != 0)
            {
                return false;
            }
            if (cells.Count != v1.cells.Count)
            {
                return false;
            }
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i] != v1.cells[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool CompareTo(List<Voxel> v1, List<Voxel> v2)
        {
            if (v1.Count != v2.Count)
            {
                return false;
            }
            for (int i = 0; i < v1.Count; i++)
            {
                if (!v1[i].CompareTo(v2[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static void WriteVoxel(BinaryWriter writer, Voxel voxel)
        {
            Point.WritePoint(writer, voxel.coord);
            //write the number of cells in here
            writer.Write(voxel.cells.Count);
            //followed by each cell
            foreach (int cellnum in voxel.cells)
            {
                writer.Write(cellnum);
            }
        }

        /// <summary>
        /// Writes the current voxels to a given file path.
        /// </summary>
        public static void WriteVoxels(string filename, VoxelArray cloud)
        {
            WriteVoxels(filename, cloud.toList());
        }

        public static void WriteVoxels(string filename, List<Voxel> voxels)
        {
            FileStream stream = File.Create(filename);
            BinaryWriter writer = new BinaryWriter(stream);
            foreach (Voxel voxel in voxels)
            {
                WriteVoxel(writer, voxel);
            }
            writer.Close();
            stream.Close();
        }

        /// <summary>
        /// Reads a voxel from a specified file
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Voxel ReadVoxel(BinaryReader reader)
        {
            ///read the point
            Point point = Point.ReadPoint(reader);
            //The number of cells that were found in this voxel
            int numOfCells = reader.ReadInt32();
            List<int> cells = new List<int>();
            //grab the cell number of each cell in this voxel
            for (int i = 0; i < numOfCells; i++)
            {
                if (reader.BaseStream.Position != reader.BaseStream.Length)
                    cells.Add(reader.ReadInt32());
            }
            return new Voxel(cells, point);
        }

        /// <summary>
        /// Returns a list of voxels from a .VOX file. Throws an error if not a vox file
        /// </summary>
        public static List<Voxel> ReadVoxels(string filename)
        {
            if (Path.GetExtension(filename) != ".VOX")
            {
                throw new InvalidOperationException("Not a valid .VOX file");
            }
            FileStream stream = File.OpenRead(filename);
            BinaryReader reader = new BinaryReader(stream);
            List<Voxel> retVal = new List<Voxel>();
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                retVal.Add(ReadVoxel(reader));
            }
            reader.Close();
            stream.Close();
            return retVal;
        }



    }
}
