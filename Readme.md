# AMish mesh creation tool

AMish is an application backend created by the Agile Surface Engineering lab at the Universiy of Calgary. The backend can take a csv file, and create a series of mesh files based on given criteria for use in Virtual/Mixed reality devices

## Getting Started

After downloading, you can either build the library as a class library or as a console application. If you are attempting to build a user interface, build as a class library. The Datastructures project will also need to be built as a dll if doing it this way.
If you want to use it as a console application, you will need to set your target main class. The correct usage should be available for each class by running with argument --help.

### Contents

There is essentially 3 processes that a file must undergo before becoming a mesh

```
Parsing
Splitting
Marching
```
These basic functions perform vital tasks for mesh creation. Because there is a lot of customization available, there is no singular "streamlined" process from start to finish. As it is, each function is independant of the others and each stage is essentially piped into the next.

### Parsing

Parsing a file is the first step and requires the least parameters. Simply pass in the location to a given csv formatted file, and the parser will create file structure for you which will hold the csv as a binary.

As library
```
parseDataIntoCache("C://testFile.txt")
```
As Console
```
FileParser "C://testFile.txt"
```

### Local Cache

After a file is parsed, a local cache directory is created. The structure is as follows:
```
	              Cache
			|
	                V
                     FileName
		     |      |
		     V      V
		  Meshes  Fields
		    |       |
                    V       V
                 Fields   .BNF
		   |   	  .DIVF
		   V
		 .FARA
		 .IMF
```
.BNF = The binary file of the full column after being parsed. Contains all the values for the given column
.DVF = The divided binary files. Will be named according to their division (ex. cache/testFile/fields/temperature/205-210.DIVF would be a the temperature range of 205-210 of the testFile) 
.FARA = A float array holding the position and scalar value of each "binned" entry
.IMF = An intermediate mesh format which is used to create the actual mesh within unity

### Splitting

Splitting a file is not technically necessarry but for the sake of file-type checking I have made it so. Splitting a file is essentially a way to "break" apart your file into multiple smaller meshes. That way, you can overlay multiple sections of file as different meshes.
Splitting can be performed one of two ways

1)
```
Split all: All the columns found in the csv are split into muliple files. This will make it so that you can batch your splitting if you know you will eventually be creating a mesh using aall of your columns eventually.
```
2)
```
Split by: By giving only the columns you wish to view in 3D, you save processing time, as well as memory costs by definining which columns precisely you would like split
```

So for example, say I wanted to view my file in 3D

As Library
```
1)splitFileAll("testFile", 5) //Note that I used testFile instead of "C://testFile.txt". This is because the file I parsed in is saved into the cache and so only the identify name is needed.
2)splitFileGiven("testFile", ["temperature"], 4) //The string array is simply the array containing all the columns you would like split.
```
As Console
```
1)FieldSplitter "testFile" 5
2)FieldSplitter "testFile" 5 "temperature" "pressure" //This would split the temperature column as well as the pressure column
```
This process reads in the .BNF of a given column, and produces multiple .DIVF files in the same column

### Marching

After splitting a file, marching is what then iterates over the multiple divided files, and creates an intermediate mesh to be passed into unity.
Marching takes a divided file, aggregates all the entry points into bins, then creates an array of those bins to perform marching cubes on.

Much like splitting I can chose to create a mesh for all given .DIVF files in a column, or I can specify a precise Mesh I would like to create

As Library
```
All) AllMeshes.allFromDivisions(filename, header, numberOfBins, numberOfThreads, [axis'])
		ex:	AllMeshes.allFromDivisions("testFile", "temperature", 1000000, 4, ["x-coordinate", "y-coordinate", "z-coordinate"])
		would create mesh files for all .DIVF files in /cache/testFile/fields/temperature/ directory where each mesh was binned with 1000000 bins performed across 4 threads where the x axis is the "x-coordinate" column values etc.

Specific)
	SingularMesh mcAlg = new SingularMesh();
        VoxelArray array = mcAlg.createPointCloud(filename, resolution, xaxis, yaxis, zaxis);
        //VoxelArray.WriteFloatArray(mcAlg.outputPath + ".FARA", array);  //This will write out the voxel file as well
        MarchingCubes test = new MarchingCubes();
        test.SetTarget(0.005f); //Se the target isoLevel
        IM intermediate = test.CreateMesh(array.toFloatArray());
        intermediate.WriteIntermediateToFile(mcAlg.outputPath + ".IMF");
```
As Console
```
All) AllMeshes "testFile" "temperature" 1000000 5 "x-coordinate" "y-coordinate" "z-coordinate"

Specific) SingularMesh "C:/.../104-153.DivF" 1000000 "x-coordinate" "y-coordinate" "z-coordinate"
```

### Wrapper

There exists a main wrapper class which contains a singular main method for doing all three methods in succession.

```
MainWrapper MainWrapper <fileIn> <numOfDivisions> <MeshHeader> <Resolution> <ThreadCount> <xAxis> <yAxis> <zAxis> <isoLevel> <SplitHeaders>*
```

## Authors

* **Cooper Davies** - *2017* - [email](mailto:cooper.davies@agilesoftwareengineering.org)
