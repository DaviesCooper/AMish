using DataStructures;
using ServerSide.Parsing;
using ServerSide.Tools;
using System;
using System.Collections.Generic;
using System.IO;

namespace ServerSide.MarchingAlgorithm
{
    public class SingularMesh
    {
        public string outputPath;
        /// <summary>
        /// Given a filename and a resolution, creates a point cloud array from
        /// the locations found in the file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public VoxelArray createPointCloud(string filename, int resolution, string xaxis, string yaxis, string zaxis, float[] minMaxArray)
        {
            //how to output the file based on input
            string filePath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(filename)));
            string attribute = Path.GetFileName(Path.GetDirectoryName(filename));
            string divider = Path.GetFileNameWithoutExtension(filename);
            Directory.CreateDirectory(filePath + "\\meshes\\" + attribute + "\\" + resolution + "\\");
            outputPath = filePath + "\\meshes\\" + attribute + "\\" + resolution + "\\"+ divider;

            List<FieldCell> cells = FieldCell.ReadCells(filename);
            //sort based on cell number
            cells.Sort(delegate (FieldCell c1, FieldCell c2) { return c1.cellNum.CompareTo(c2.cellNum); });
            //Now we need to create a list of ATTRPoint by pulling necessarry parts from
            //other files

            //TODO make this so that it checks if the axis exist in the cache
            string xFPath = Path.Combine(filePath, "fields", xaxis, "full.BNF");
            string yFPath = Path.Combine(filePath, "fields", yaxis, "full.BNF");
            string zFPath = Path.Combine(filePath, "fields", zaxis, "full.BNF");

            //Set up xcoord, ycoord, zcoord readers
            FileStream[] streams = new FileStream[3];
            streams[0] = File.OpenRead(xFPath);
            streams[1] = File.OpenRead(yFPath);
            streams[2] = File.OpenRead(zFPath);
            BinaryReader[] readers = new BinaryReader[3];
            readers[0] = new BinaryReader(streams[0]);
            readers[1] = new BinaryReader(streams[1]);
            readers[2] = new BinaryReader(streams[2]);

            int numToRead = 1;
            int bIndex = 0;
            List<AttCoordCell> points = new List<AttCoordCell>();
            int count = 0;
            for (int i = 0; i < cells.Count; i++)
            {
                //The number of floats to read from each file is:
                //numToRead = cells[i].cellnumber - bIndex
                //The next bytes will be the values associated with the
                //desired cell
                //Then set bIndex = cells[i].cellnumber so that our bIndex
                //is updated
                numToRead = (cells[i].cellNum - bIndex) - 1;
                //incrementing our readers to the correct index

                if (numToRead > 0)
                {
                    int bytes = numToRead * 4;
                    readers[0].ReadBytes(bytes);
                    readers[1].ReadBytes(bytes);
                    readers[2].ReadBytes(bytes);
                }

                float xc = readers[0].ReadSingle();
                float yc = readers[1].ReadSingle();
                float zc = readers[2].ReadSingle();
                bIndex = cells[i].cellNum;
                AttCoordCell toAdd = new AttCoordCell(cells[i].cellNum, xc, yc, zc, cells[i].field);
                points.Add(toAdd);
                count++;
                //if (count % 50000 == 0)
                //{
                //    double perc = ((double)count / (double)cells.Count) * 100;
                //    PercentageClass.UpdatePercentage("Creating Point Cloud Array", perc);
                //    DebugLog.logConsole("Attribute Coordcell Reading: "+perc.ToString("#.##") + "%");
                //}
            }

            foreach (BinaryReader r in readers)
            {
                r.Close();
            }
            foreach (Stream s in streams)
            {
                s.Close();
            }
            if (points.Count < 3)
            {
                throw new InvalidOperationException();
            }
            if (minMaxArray == null)
            {
                List<float> newMinMax = new List<float>();
                string[] axises = new string[] { xaxis, yaxis, zaxis };
                foreach (string a in axises)
                {
                    string axisPath = Path.Combine(filePath, "fields", xaxis, "mmAray.bin");

                    float[] minMax = AllMeshes.minMaxOfFile(axisPath);
                    newMinMax.Add(minMax[0]);
                    newMinMax.Add(minMax[1]);
                }

                VoxelArray array = new VoxelArray(resolution, newMinMax.ToArray());
                //inserting all the points into the array
                array.insertPoints(points);

                return array;
            }
            else
            {
                VoxelArray array = new VoxelArray(resolution, minMaxArray);
                //inserting all the points into the array
                array.insertPoints(points);

                return array;
            }

        }


        private static List<float> generateMinMaxArray(string[] axis, string fileName)
        {
            List<float> minMaxArray = new List<float>();

            foreach (string a in axis)
            {
                string filePath = FileStructure.cacheDirectory;
                string path = Path.Combine(filePath, fileName, "fields", a, "mmAray.bin");

                float[] minMax = AllMeshes.minMaxOfFile(path);
                minMaxArray.Add(minMax[0]);
                minMaxArray.Add(minMax[1]);
            }
            return minMaxArray;
        }

        #region console ver
        static string numArgs = "Invalid number of arguments\nRun with --help to see documentation";
        static string help = "Parses a binary file that was created via FieldSplitter and\n" +
                             "Localizes data points to vertices defined by a given resolution where the vertice coordinates are the values for given axis.\n" +
                             "Then creates an intermediate mesh file out of these points\n" +
                             "SingularMesh <file> <resolution> <xaxis> <yaxis> <zaxis> <isoLevel>\n\n" +
                             "\t<file> = the path to the file ou want a mesh created out of\n" +
                             "\t<resolution> = the number of \"bins\" you would like to aggregate data by\n" +
                             "\t<xaxis> = the column whose values you would like on the x axis\n" +
                             "\t<yaxis> = the column whose values you would like on the y axis\n" +
                             "\t<zaxis> = the column whose values you would like on the z axis\n" +
                             "\t<isoLevel> = the minimum value a bin must have to be considered below the isosurface of the mesh\n";

        //args0 - filepath
        //args1 - resolution
        //args2 - xaxis
        //args3 - yaxis
        //args4 - zaxis
        //args5 - isoLevel
        public static void Main(string[] args)
        {
            DebugLog.resetLog();
            DebugLog.setDebug(true);
            int resolution;
            float isoLevel;

            #region cmd args handling
            if (args.Length > 6 || args.Length < 1)
            {
                Console.Out.WriteLine(numArgs);
                Console.In.ReadLine();
                return;
            }
            if (args.Length == 1 && (args[0].ToLower() == "--help" || args[0].ToLower() == "help" || args[0].ToLower() == "?" || args[0].ToLower() == "h"))
            {
                Console.Out.WriteLine(help);
                Console.In.ReadLine();
                return;
            }

            if (!File.Exists(args[0]))
            {
                DebugLog.logConsole("Could not find file. Are you sure you are passing in the right string?");
                throw new Exception("Could not find file. Are you sure you are passing in the right string?");
            }
            try
            {
                resolution = Int32.Parse(args[1]);
            }
            catch
            {
                DebugLog.logConsole("Resolution not a number");
                throw new Exception("Resolution not a number");
            }
            try
            {
                isoLevel = float.Parse(args[5]);
            }
            catch
            {
                DebugLog.logConsole("IsoLevel is not a float");
                throw new Exception("IsoLevel is not a float");
            }
            if (resolution < 1)
            {
                DebugLog.logConsole("Resolution cannot be less than 1");
                throw new Exception("Resolution cannot be less than 1");
            }
            #endregion
            SingularMesh mcAlg = new SingularMesh();
            string dir = Directory.GetParent(Directory.GetParent(Directory.GetParent(args[0]).FullName).FullName).Name;
            List<float> minMaxArray = generateMinMaxArray(new string[]{ args[2], args[3], args[4]}, dir);


            VoxelArray array = mcAlg.createPointCloud(args[0], resolution, args[2], args[3], args[4], minMaxArray.ToArray());
            //VoxelArray.WriteFloatArray(mcAlg.outputPath + ".FARA", array);
            MarchingCubes test = new MarchingCubes();
            test.SetTarget(isoLevel);
            IM intermediate = test.CreateMesh(array.toFloatArray());
            if (intermediate.verts.Count > 64999)
            {
                IM[] divs = intermediate.divideIntoSmall();
                for (int i = 0; i < divs.Length; i++)
                {
                    divs[i].WriteIntermediateToFile(mcAlg.outputPath + "_" + i + ".IMF");
                }
            }
            else
            {
                intermediate.WriteIntermediateToFile(mcAlg.outputPath + ".IMF");
            }
        }
        #endregion
    }
}