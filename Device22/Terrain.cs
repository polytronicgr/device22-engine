using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;


/* Test values:
 * 
 * terrain size: 1024 + 1
 * patch size: 64
 */

namespace Device22
{
    public class Terrain
    {
        private int lodLevels;
        public int LODLEVELS
        { get { return lodLevels; } }
        // terrain size
        private int tSize;
        public int Width
        { get { return tSize; } }
        public int Height
        { get { return tSize; } }
        // patches size
        private int pSize;
        public int PatchSize
        { get { return pSize; } }
        // number of patches per row
        private int ppR;
        public int PatchesPerRow
        { get { return ppR; } }

        public Vector3 position;
        public float radius;

        private int[] indexBuffer;
        public int[] IndexBuffer
        { get { return indexBuffer; } }
        private int[] indicesCount;
        public int[] IndicesCount
        { get { return indicesCount; } }
        private int[,] bridgeIndexBuffer;
        public int[,] BridgeIndexBuffer
        { get { return bridgeIndexBuffer; } }
        private int[,] bridgeIndicesCount;
        public int[,] BridgeIndicesCount
        { get { return bridgeIndicesCount; } }
        private List<Vector3> vertices;
        public List<Vector3> Vertices
        { get { return vertices; } }
        private List<Vector2> uvCoor;
        public List<Vector2> uvCoordinates
        { get { return uvCoor; } }

        private int texID;
        public int TextureID
        { get { return texID; } }
        private int detailtexID;
        public int DetailTextureID
        { get { return detailtexID; } }

        private float heightMult = 0.01f;

        public Terrain(Vector3 centerPos, Vector3 axisX, Vector3 axisZ, int LODLevels, int terrainSize, int patchSize, float planetRadius, int textureID, string heightmapURL)
        {
            if ((!Math2.IsPowerOfTwo(terrainSize - 1)) || (!Math2.IsPowerOfTwo(patchSize - 1)))
            {
                Core.ErrorExit("Error: (Class: Terrain) Patch size must be 2^x+1 and terrain size must be 2^x+1");
            }

            if (!File.Exists(heightmapURL))
            {
                Core.ErrorExit("Error: (Class: Terrain) Heightmap not found!");
            }

            Bitmap bitmap = new Bitmap(heightmapURL);

            texID = textureID;
            lodLevels = LODLevels;
            tSize = terrainSize;
            pSize = patchSize;
            radius = planetRadius;
            // Number of patches per row
            ppR = (terrainSize - 1) / patchSize;

            detailtexID = Texture.loadFromFile("detail.jpg");

            // Temp arrays to ease the vertex and uv calculation
            Vector3[] vertArray = new Vector3[terrainSize * terrainSize];
            Vector2[] uvArray = new Vector2[terrainSize * terrainSize];

            int index = 0;
            float height = 0;
            for (int z = 0; z < terrainSize; z++)
            {
                for (int x = 0; x < terrainSize; x++)
                {
                    index = (z * terrainSize) + x;
                    // Since the heightmap is in greyscales, we can use any color channel
                    //if ((x < bitmap.Height) && (z < bitmap.Width)) height = bitmap.GetPixel(x, z).R * heightMult;
                    height = bitmap.GetPixel(x, z).R * heightMult;
                    //vertArray[index] = new Vector3(x, height, z);
                    vertArray[index] = new Vector3(centerPos + (axisX) * ((x))
                                                             //+ (axisY / terrainScale) * ((terrainSize) / terrainScale)
                                                             + (axisZ) * ((z)));

                    vertArray[index] = GenerateCubeToSphereCoordinates(vertArray[index], height);

                    // calculate uv coordinates for that vertex point
                    uvArray[index] = new Vector2((float)(1.0 / terrainSize * x), (float)(1.0 / terrainSize * z));
                }
            }

            bitmap.Dispose();

            // Vertices array holds all terrain vertices multiplied by the 3 coordinates (x, y and z)
            vertices = new List<Vector3>();
            index = 0;
            foreach (Vector3 vert in vertArray)
            {
                vertices.Add(vert);
            }

            // UV array array holds all uv texture coordinates multiplied by the 2 coordinates (u and v)
            uvCoor = new List<Vector2>();
            index = 0;
            foreach (Vector2 vert in uvArray)
            {
                uvCoor.Add(vert);
            }

            indicesCount = new int[(int)Math.Pow(2, lodLevels - 1) + 1];
            indexBuffer = new int[(int)Math.Pow(2, lodLevels - 1) + 1];

            // this array stores the indice count for the bridges - for the case Resolution-To-Resolution and the case Resolution-To-(Resolution-1)
            bridgeIndicesCount = new int[2, (int)Math.Pow(2, lodLevels - 1) + 1];
            // this array stores all bridges buffers. 4 bridges to the same resolution, 4 bridges to a lower resolution.
            bridgeIndexBuffer = new int[(int)Math.Pow(2, lodLevels - 1) + 1, 8];

            for (int i = 1; i <= lodLevels; i++)
            {
                int resolution = (int)Math.Pow(2, i - 1);
                // calculate indices count for every lod level
                int currLevel = 0, lastLevel = 0;
                if (i == 1)
                {
                    // there are 3 indices per triangle and 2 triangle to make a polygon
                    indicesCount[i] = ((patchSize) * (patchSize) * 3 * 2);
                }
                else
                {
                    currLevel = resolution;
                    lastLevel = resolution - (int)Math.Pow(2, i - 2);
                    indicesCount[currLevel] = indicesCount[lastLevel] / 4;
                }

                // assign the index buffer ids and then calculate and create the index buffers
                BuildIndices(resolution);

                // calculate the 8 bridges
                bridgeIndicesCount[0, resolution] = ((pSize - 1) / resolution) * 3 * 2;
                // top bridge (to same level)
                BuildDefaultHoriBridge(resolution, true);
                // right bridge (to same level)
                BuildDefaultVertBridge(resolution, false);
                // bottom bridge (to same level)
                BuildDefaultHoriBridge(resolution, false);
                // left bridge (to same level)
                BuildDefaultVertBridge(resolution, true);

                bridgeIndicesCount[1, resolution] = ((((pSize - 1) / (resolution * 2)) *3) - 2) * 3;
                // top bridge (to lower level)
                BuildLowerHoriBridge(resolution, true);
                // right bridge (to lower level)
                BuildLowerVertBridge(resolution, false);
                // bottom bridge (to lower level)
                BuildLowerHoriBridge(resolution, false);
                // left bridge (to lower level)
                BuildLowerVertBridge(resolution, true);


            }
            Debug.Trace("(Class: Terrain) Terrain created");
        }

        private void BuildIndices(int resolution)
        {
            int[] indices = new int[indicesCount[resolution]];            // indices dem Crackfix anpassen?
            int index = 0;
            for (int z = resolution; z < pSize - resolution - 1; z++)     // -1 oder -2 (Im alten Code habe ich pSize-2 stehen)
            {
                for (int x = resolution; x < pSize - resolution - 1; x++)
                {
                    indices[index++] = x + z * pSize;
                    indices[index++] = (x + resolution) + z * pSize;
                    indices[index++] = x + (z + resolution) * pSize;
                    indices[index++] = x + (z + resolution) * pSize;
                    indices[index++] = (x + resolution) + z * pSize;
                    indices[index++] = (x + resolution) + (z + resolution) * pSize;
                    x += resolution - 1;        // -1? => da oben x++!
                }
                z += resolution - 1;
            }

            // For debug
            int size;

            // Create Index buffer 
            GL.GenBuffers(1, out indexBuffer[resolution]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer[resolution]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicesCount[resolution] * BlittableValueType.StrideOf(indices[resolution])), indices, BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (indicesCount[resolution] * BlittableValueType.StrideOf(indices[resolution]) != size) { throw new ApplicationException("Error: (Class: Terrain) indices data not uploaded correctly"); }
        }

        // Calculates a horizontal bridge to a patch of the same level
        private void BuildDefaultHoriBridge(int resolution, bool isUpperBridge)
        {
            int[] indices = new int[bridgeIndicesCount[0, resolution]];
            int z = isUpperBridge ? 0 : pSize - resolution - 1;
            int index = 0;

            for (int x = 0; x < pSize - resolution; x++)
            {
                // first triangle
                if (x == 0)
                {
                    int modPoint = isUpperBridge ? z : (z + resolution);
                    indices[index++] = (x + modPoint * pSize);
                    indices[index++] = ((x + resolution) + z * pSize);
                    indices[index++] = ((x + resolution) + (z + resolution) * pSize);
                }
                // last triangle
                else if (x == pSize - resolution - 1)
                {
                    int modPoint = isUpperBridge ? z : (z + resolution);
                    indices[index++] = (x + z * pSize);
                    indices[index++] = (x + (z + resolution) * pSize);
                    indices[index++] = ((x + resolution) + modPoint * pSize);
                }
                else
                {
                    // middle polygons
                    indices[index++] = (x + z * pSize);
                    indices[index++] = ((x + resolution) + z * pSize);
                    indices[index++] = (x + (z + resolution) * pSize);
                    indices[index++] = (x + (z + resolution) * pSize);
                    indices[index++] = ((x + resolution) + z * pSize);
                    indices[index++] = ((x + resolution) + (z + resolution) * pSize);
                }                

                x += resolution - 1;
            }

            // For debug
            int size;

            // Create Index buffer
            int bridgeIndex = isUpperBridge ? 0 : 2;
            GL.GenBuffers(1, out bridgeIndexBuffer[resolution, bridgeIndex]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, bridgeIndexBuffer[resolution, bridgeIndex]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(int)), indices, BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (indices.Length * sizeof(int) != size) { throw new ApplicationException("Error: (Class: Terrain) indices data not uploaded correctly"); }
        }

        // Calculates a vertical bridge to a patch of the same level
        private void BuildDefaultVertBridge(int resolution, bool isLeftBridge)
        {
            int[] indices = new int[bridgeIndicesCount[0, resolution]];
            int x = isLeftBridge ? 0 : pSize - resolution - 1;
            int index = 0;

            for (int z = 0; z < pSize - resolution; z++)
            {
                // first triangle
                if (z == 0)
                {
                    int modPoint = isLeftBridge ? x : (x + resolution);
                    indices[index++] = (modPoint + z * pSize);
                    indices[index++] = (x + (z + resolution) * pSize);
                    indices[index++] = ((x + resolution) + (z + resolution) * pSize);
                }
                // last triangle
                else if (z == pSize - resolution - 1)
                {
                    int modPoint = isLeftBridge ? x : (x + resolution);
                    indices[index++] = (x + z * pSize);
                    indices[index++] = ((x + resolution) + z * pSize);
                    indices[index++] = (modPoint + (z + resolution) * pSize);
                }
                else
                {
                    // middle polygons
                    indices[index++] = (x + z * pSize);
                    indices[index++] = ((x + resolution) + z * pSize);
                    indices[index++] = (x + (z + resolution) * pSize);
                    indices[index++] = (x + (z + resolution) * pSize);
                    indices[index++] = ((x + resolution) + z * pSize);
                    indices[index++] = ((x + resolution) + (z + resolution) * pSize);
                }

                z += resolution - 1;
            }

            // For debug
            int size;

            // Create Index buffer
            int bridgeIndex = isLeftBridge ? 3 : 1;
            GL.GenBuffers(1, out bridgeIndexBuffer[resolution, bridgeIndex]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, bridgeIndexBuffer[resolution, bridgeIndex]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(int)), indices, BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (indices.Length * sizeof(int) != size) { throw new ApplicationException("Error: (Class: Terrain) indices data not uploaded correctly"); }
        }

        // Calculates a horizontal bridge to a patch of a lower level
        private void BuildLowerHoriBridge(int resolution, bool isUpperBridge)
        {
            int[] indices = new int[bridgeIndicesCount[1, resolution]];
            int z = isUpperBridge ? 0 : pSize - resolution - 1;
            int doubleResolution = resolution * 2;
            int index = 0;

            for (int x = 0; x < pSize - doubleResolution; x++)
            {
                // left triangle
                if (x != 0)
                {
                    int modPoint = isUpperBridge ? (z + resolution) : z;
                    indices[index++] = x + z * pSize;
                    indices[index++] = x + (z + resolution) * pSize;
                    indices[index++] = (x + resolution) + modPoint * pSize;
                }
                // right triangle
                if (x != pSize - doubleResolution - 1)
                {
                    int modPoint = isUpperBridge? (z + resolution) : z;
                    indices[index++] = (x + doubleResolution) + z * pSize;
                    indices[index++] = (x + resolution) + modPoint * pSize;
                    indices[index++] = (x + doubleResolution) + (z + resolution) * pSize;
                }
                // middle triangle
                if (isUpperBridge)
                {
                    indices[index++] = x + z * pSize;
                    indices[index++] = (x + resolution) + (z + resolution) * pSize;
                    indices[index++] = (x + doubleResolution) + z * pSize;
                }
                else
                {
                    indices[index++] = x + (z + resolution) * pSize;
                    indices[index++] = (x + resolution) + z * pSize;
                    indices[index++] = (x + doubleResolution) + (z + resolution) * pSize;
                }

                x += doubleResolution - 1;
            }

            // For debug
            int size;

            // Create Index buffer
            int bridgeIndex = isUpperBridge ? 4 : 6;
            GL.GenBuffers(1, out bridgeIndexBuffer[resolution, bridgeIndex]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, bridgeIndexBuffer[resolution, bridgeIndex]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(int)), indices, BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (indices.Length * sizeof(int) != size) { throw new ApplicationException("Error: (Class: Terrain) indices data not uploaded correctly"); }
        }

        // Calculates a vertical bridge to a patch of a lower level
        private void BuildLowerVertBridge(int resolution, bool isLeftBridge)
        {
            int[] indices = new int[bridgeIndicesCount[1, resolution]];
            int x = isLeftBridge ? 0 : pSize - resolution - 1;
            int doubleResolution = resolution * 2;
            int index = 0;

            for (int z = 0; z < pSize - doubleResolution; z++)
            {
                // left triangle
                if (z != 0)
                {
                    int modPoint = isLeftBridge ? (x + resolution) : x;
                    indices[index++] = x + z * pSize;
                    indices[index++] = (x + resolution) + z * pSize;
                    indices[index++] = modPoint + (z + resolution) * pSize;
                }
                // right triangle
                if (z != pSize - doubleResolution - 1)
                {
                    int modPoint = isLeftBridge ? (x + resolution) : x;
                    indices[index++] = modPoint + (z + resolution) * pSize;
                    indices[index++] = x + (z + doubleResolution) * pSize;
                    indices[index++] = (x + resolution) + (z + doubleResolution) * pSize;
                }
                // middle triangle
                if (isLeftBridge)
                {
                    indices[index++] = x + z * pSize;
                    indices[index++] = (x + resolution) + (z + resolution) * pSize;
                    indices[index++] = x + (z + doubleResolution) * pSize;
                }
                else
                {
                    indices[index++] = (x + resolution) + z * pSize;
                    indices[index++] = x + (z + resolution) * pSize;
                    indices[index++] = (x + resolution) + (z + doubleResolution) * pSize;
                }

                z += doubleResolution - 1;
            }

            // For debug
            int size;

            // Create Index buffer
            int bridgeIndex = isLeftBridge ? 7 : 5;
            GL.GenBuffers(1, out bridgeIndexBuffer[resolution, bridgeIndex]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, bridgeIndexBuffer[resolution, bridgeIndex]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(int)), indices, BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (indices.Length * sizeof(int) != size) { throw new ApplicationException("Error: (Class: Terrain) indices data not uploaded correctly"); }

        }

        private Vector3 GenerateCubeToSphereCoordinates(Vector3 cubeV, float height)
        {
            cubeV.Normalize();
            Vector3 result = new Vector3(cubeV);
            // + Parameter überschrieben?
            result.X *= radius + height;
            result.Y *= radius + height;
            result.Z *= radius + height;
            return result;
        }

        public void Destroy()
        {
            /*for (int i = 0; i < indexBuffer.Length; i++)
            {
                if (indexBuffer[i] != 0) { GL.DeleteBuffers(1, ref indexBuffer[i]); }
            }*/
        }

        public struct Vertex3
        {
            public float X;
            public float Y;
            public float Z;
        }
    }
}
