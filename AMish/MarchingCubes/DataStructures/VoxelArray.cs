using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DataStructures
{
    /// <summary>
    /// Creates a PointCloud array for a mesh that can be passed to Unity.
    /// </summary>
    public class VoxelArray
    {
        #region vars
        public List<int>[,,] array;
        /// <summary>
        /// This way as opposed to a defined class to save 4 bytes of a pointer
        /// </summary>
        public float stepLength;
        public int desiredPoints;
        public float[] minMaxArray;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes boolean array, sets the difference from zero that each point needs to be shifted, sets step length between each aggreated point.
        /// </summary>
        /// <param name="maxDesiredPoints"></param>
        /// <param name="minMaxArray"></param>
        public VoxelArray(int maxDesiredPoints, float[] minMaxArray)
        {
            desiredPoints = maxDesiredPoints;
            int coord = (int)Math.Ceiling(Math.Pow((double)maxDesiredPoints, (1.0 / 3.0)));

                float xLength = minMaxArray[1] - minMaxArray[0];
                float yLength = (minMaxArray[3] - minMaxArray[2]);
                float zLength = (minMaxArray[5] - minMaxArray[4]);


            float minLength = Math.Min(Math.Min(xLength, yLength), zLength);

            float xLengthFactor = xLength / minLength;
            float yLengthFactor = yLength / minLength;
            float zLengthFactor = zLength / minLength;

            float factorJ = ((coord * xLengthFactor) * (coord * yLengthFactor) * (coord * zLengthFactor)) / maxDesiredPoints;

            float factor = (float)Math.Pow((float)factorJ, (1.0 / 3.0));

            int xstep = (int)Math.Floor(coord / factor * xLengthFactor);
            int ystep = (int)Math.Floor(coord / factor * yLengthFactor);
            int zstep = (int)Math.Floor(coord / factor * zLengthFactor);

            array = new List<int>[xstep + 1, ystep + 1, zstep + 1];

            stepLength = xLength / xstep;
            if(Math.Abs(stepLength) < 0.0001f)
            {
                stepLength = 0.0000001f;
            }
            this.minMaxArray = minMaxArray;
        }
        #endregion

        /// <summary>
        /// Takes a list of points and inserts them into the coord array
        /// </summary>
        /// <param name="inputList"></param>
        public void insertPoints(List<AttCoordCell> inputList)
        {
            foreach (AttCoordCell p in inputList)
            {
                insertPoint(p);
            }
        }

        public long numOfBytes()
        {
            long bytes = 0;
            foreach(List<int> list in array)
            {
                if(list == null)
                {
                    continue;
                }
                bytes += (list.Count * 4);
            }

            bytes += (4 * 8);
            return bytes;
        }

        public List<Voxel> toList()
        {
            List<Voxel> retVal = new List<Voxel>();
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    for (int k = 0; k < array.GetLength(2); k++)
                    {
                        if (array[i, j, k] != null && array[i, j, k].Count > 0)
                        {
                            Point point = new Point(i, j, k);
                            Voxel voxel = new Voxel(array[i, j, k], point);
                            retVal.Add(voxel);
                        }
                    }
                }
            }
            return retVal;
        }

        /// <summary>
        /// Inserts point into the PointCloud array to be mapped.
        /// </summary>
        /// <param name="x">x coordinate of the point in relation to the original model data.</param>
        /// <param name="y">y coordinate of the point in relation to the original model data.</param>
        /// <param name="z">z coordinate of the point in relation to the original model data.</param>
        public void insertPoint(int cellNum, float x, float y, float z)
        {
            //These are the "initial" starting coords
            //essentially normalising to 0
            float tempx = x - minMaxArray[0];
            float tempy = y - minMaxArray[2];
            float tempz = z - minMaxArray[4];
            int xkey = (int)Math.Floor(tempx / stepLength);
            int ykey = (int)Math.Floor(tempy / stepLength);
            int zkey = (int)Math.Floor(tempz / stepLength);

            if (array[xkey, ykey, zkey] == null)
            {
                array[xkey, ykey, zkey] = new List<int>
                {
                    cellNum
                };
            }
            else
            {
                array[xkey, ykey, zkey].Add(cellNum);
            }

        }
        /// <summary>
        /// Inserts point into the PointCloud array to be mapped.
        /// </summary>
        /// <param name="p">VertexPoint that holds the x, y and z coordinate of the original model data.</param>
        public void insertPoint(AttCoordCell p)
        {
            insertPoint(p.fieldCell.cellNum, p.coords.x, p.coords.y, p.coords.z);
        }

        public static void WriteFloatArray(string filepath, VoxelArray array)
        {
            FileStream stream = File.Create(filepath);
            BinaryWriter writer = new BinaryWriter(stream);
            float[,,] fa = array.toFloatArray();
            writer.Write(fa.GetLength(0));
            writer.Write(fa.GetLength(1));
            writer.Write(fa.GetLength(2));
            for (int i = 0; i < fa.GetLength(0); i++)
            {
                for (int j = 0; j < fa.GetLength(1); j++)
                {
                    for (int k = 0; k < fa.GetLength(2); k++)
                    {
                        writer.Write(fa[i, j, k]);
                    }
                }
            }
            writer.Close();
            stream.Close();
        }

        public static float[,,] ReadFloatArray(string filepath)
        {
            if (Path.GetExtension(filepath) != ".FARA")
            {
                throw new InvalidOperationException("Not a valid .VOXA file");
            }
            FileStream stream = File.OpenRead(filepath);
            BinaryReader reader = new BinaryReader(stream);
            float[,,] retVal = new float[reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()];
            for (int i = 0; i < retVal.GetLength(0); i++)
            {
                for (int j = 0; j < retVal.GetLength(1); j++)
                {
                    for (int k = 0; k < retVal.GetLength(2); k++)
                    {
                        retVal[i, j, k] = reader.ReadSingle();
                    }
                }
            }
            return retVal;
        }

        public float[,,] toFloatArray()
        {
            //setting the min and max so that we can normalize all the values to a float value
            int max = int.MinValue;
            foreach (List<int> ls in array)
            {
                if (ls != null)
                {
                    if (ls.Count > max)
                        max = ls.Count;
                }
            }

            //Now we iterate, while normalizing for the values
            float[,,] retVal = new float[array.GetLength(0), array.GetLength(1), array.GetLength(2)];

            for (int i = 0; i < array.GetLength(0) - 1; i++)
            {
                //from 0, to the max of the second dimension of the array. (array[,j,])
                for (int j = 0; j < array.GetLength(1) - 1; j++)
                {
                    //from 0, to the max of the third dimension of the array. (array[,,k])
                    for (int k = 0; k < array.GetLength(2) - 1; k++)
                    {
                        if (array[i, j, k] != null)
                            retVal[i, j, k] = ((float)array[i, j, k].Count) / ((float)max);
                    }
                }
            }

            return retVal;
        }

    }
}

