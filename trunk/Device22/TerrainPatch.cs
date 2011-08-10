using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Device22
{
    public class TerrainPatch
    {
        private const float distanceFactor = 20.0f;

        private int id;
        public int ID
        { get { return id; } }

        private int vertexBuffer;
        public int VertexBuffer
        { get { return vertexBuffer; } }
        public List<Vector3> Vertices;
        public int verticesCount;
        private List<Vector2> uvCoor;
        private int textureBuffer;
        public int TextureBuffer
        { get { return textureBuffer; } }
        private Vector3 highestV;
        private Vector3 lowestV;
        private Vector3 centerV;
        public Vector3 CenterVertex
        { get { return centerV; } }
        private int patchSize;
        public int PatchSize
        { get { return patchSize; } }
        // Vertices describing the bounding box (if there is any)
        private Vector3[] bbV;
        public Vector3[] BoundingBoxV
        { get { return bbV; } }

        // just for testing purpose
        public TerrainPatch(int startX, int startZ, int patchSize)
        {
            Debug.Warning.Obsolete();

            Vertices = new List<Vector3>();
            uvCoor = new List<Vector2>();

            this.patchSize = patchSize;

            for (int x = 0; x < patchSize; x++)
            {
                for (int z = 0; z < patchSize; z++)
                {
                    Vertices.Add(new Vector3(x, 0, z));
                    uvCoor.Add(new Vector2(1.0f, 1.0f));
                }
            }

            int size;

            int uvCount = uvCoor.ToArray().Length;

            float[] uvCoorf = new float[uvCoor.Count * 2];
            int index = 0;
            foreach (Vector2 uv in uvCoor)
            {
                uvCoorf[index++] = uv.X;
                uvCoorf[index++] = uv.Y;
            }

            // Create Texture buffer
            GL.GenBuffers(1, out textureBuffer);
            GL.BindBuffer(BufferTarget.ArrayBuffer, textureBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(uvCoorf.Length * BlittableValueType.StrideOf(uvCoorf)), uvCoorf, BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (uvCoorf.Length * BlittableValueType.StrideOf(uvCoorf) != size) { throw new ApplicationException("Error: (Class: Terrain) texture data not uploaded correctly"); }

            // Create array with vertices coordinates for buffer data
            float[] verticesf = new float[Vertices.Count * 3];
            index = 0;
            foreach (Vector3 vec in Vertices)
            {
                verticesf[index++] = vec.X;
                verticesf[index++] = vec.Y;
                verticesf[index++] = vec.Z;
            }

            verticesCount = verticesf.Length;

            // Create Vertex buffer
            GL.GenBuffers(1, out vertexBuffer);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verticesCount * BlittableValueType.StrideOf(verticesf)), verticesf, BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (verticesCount * BlittableValueType.StrideOf(verticesf) != size) { throw new ApplicationException("Error: (Class: Terrain) vertex data not uploaded correctly"); }
        }
        // ************************

        public TerrainPatch(int id, Vector3[] boundingBox, List<Vector3> vertices, List<Vector2> uvCoordinates, int patchSize)
        {
            this.id = id;
            Vertices = vertices;
            uvCoor = uvCoordinates;

            this.patchSize = patchSize;

            centerV = vertices[vertices.Count / 2];

            highestV = new Vector3(0, 0, 0);
            // Calculate the highest point. Used to calculate the boundingbox later on
            for (int i = 0; i < vertices.Count; i++)
            {
                if (vertices[i].Y > highestV.Y) { highestV = vertices[i]; }
            }
            if (highestV.Y == 0.0f) { highestV.Y = 1.0f; }

            lowestV = new Vector3(highestV);
            // Calculate the lowest point. Used to calculate the boundingbox later on
            for (int i = 0; i < vertices.Count; i++)
            {
                if (vertices[i].Y < lowestV.Y) { lowestV = vertices[i]; }
            }

            // Calculate the 3d bounding box
            bbV = new Vector3[8];
            // corner: bottom half, back, left
            bbV[0] = new Vector3(boundingBox[0].X, lowestV.Y, boundingBox[0].Z);
            // corner: bottom half, back, right
            bbV[1] = new Vector3(boundingBox[1].X, lowestV.Y, boundingBox[1].Z);
            // corner: bottom half, front, left
            bbV[2] = new Vector3(boundingBox[2].X, lowestV.Y, boundingBox[2].Z);
            // corner: bottom half, front, right
            bbV[3] = new Vector3(boundingBox[3].X, lowestV.Y, boundingBox[3].Z);
            // corner: top half, back, left
            bbV[4] = new Vector3(boundingBox[0].X, highestV.Y, boundingBox[0].Z);
            // corner: top half, back, right
            bbV[5] = new Vector3(boundingBox[1].X, highestV.Y, boundingBox[1].Z);
            // corner: top half, front, left
            bbV[6] = new Vector3(boundingBox[2].X, highestV.Y, boundingBox[2].Z);
            // corner: top half, front, right
            bbV[7] = new Vector3(boundingBox[3].X, highestV.Y, boundingBox[3].Z);

            // For debug
            int size;

            int uvCount = uvCoor.ToArray().Length;

            // Create array with uv coordinates for buffer data
            float[] uvCoorf = new float[uvCoor.Count * 2];
            int index = 0;
            foreach (Vector2 uv in uvCoor)
            {
                uvCoorf[index++] = uv.X;
                uvCoorf[index++] = uv.Y;
            }

            // Create Texture buffer
            GL.GenBuffers(1, out textureBuffer);
            GL.BindBuffer(BufferTarget.ArrayBuffer, textureBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(uvCoorf.Length * BlittableValueType.StrideOf(uvCoorf)), uvCoorf, BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (uvCoorf.Length * BlittableValueType.StrideOf(uvCoorf) != size) { throw new ApplicationException("Error: (Class: Terrain) texture data not uploaded correctly"); }

            // Create array with vertices coordinates for buffer data
            float[] verticesf = new float[vertices.Count * 3];
            index = 0;
            foreach (Vector3 vec in vertices)
            {
                verticesf[index++] = vec.X;
                verticesf[index++] = vec.Y;
                verticesf[index++] = vec.Z;
            }

            verticesCount = verticesf.Length;

            // Create Vertex buffer
            GL.GenBuffers(1, out vertexBuffer);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verticesCount * BlittableValueType.StrideOf(verticesf)), verticesf, BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (verticesCount * BlittableValueType.StrideOf(verticesf) != size) { throw new ApplicationException("Error: (Class: Terrain) vertex data not uploaded correctly"); }
        }

        public int GetResolution(int maxLodLevel)
        {
            int patchResolution = (int)Math.Pow(2, maxLodLevel - 1); ;
            float distance = Game.Instance.Camera.calcDistanceToVertexPoint(centerV);
            //Debug.Trace(distance);
            for (int i = maxLodLevel; i > 0; i--)
            {
                if (distance < distanceFactor * i) { patchResolution = (int)Math.Pow(2, i - 1); }
            }
            // for debug only!
            //Debug.Trace(patchResolution);
            //patchResolution = 1;
            return patchResolution;
        }

        public void Render(int mainPatchIndexBuffer, int[] defaultBridgeIndexBuffer, int[] lowerBridgeIndexBuffer, int[] indicesCount)
        {
            // for debug
            //Debug.BoundingBox.Render(bbV);

            GL.BindBuffer(BufferTarget.ArrayBuffer, this.textureBuffer);
            GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, new IntPtr(0));

            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBuffer);
            GL.VertexPointer(3, VertexPointerType.Float, 0, new IntPtr(0));

            // render the main block
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mainPatchIndexBuffer);
            GL.DrawElements(BeginMode.Triangles, indicesCount[0], DrawElementsType.UnsignedInt, IntPtr.Zero);
            // Only working if the main class = Game class
            Game.Instance.polyCount += indicesCount[0];

            GL.Disable(EnableCap.CullFace);

            // if there is any default bridges, render them
            if (defaultBridgeIndexBuffer.Length > 0)
            {
                for (int i = 0; i < defaultBridgeIndexBuffer.Length; i++)
                {
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, defaultBridgeIndexBuffer[i]);
                    GL.DrawElements(BeginMode.Triangles, indicesCount[1], DrawElementsType.UnsignedInt, IntPtr.Zero);
                    Game.Instance.polyCount += indicesCount[1];
                }
            }

            // if there is any lower bridges, render them
            if (lowerBridgeIndexBuffer.Length > 0)
            {
                for (int i = 0; i < lowerBridgeIndexBuffer.Length; i++)
                {
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, lowerBridgeIndexBuffer[i]);
                    GL.DrawElements(BeginMode.Triangles, indicesCount[2], DrawElementsType.UnsignedInt, IntPtr.Zero);
                    Game.Instance.polyCount += indicesCount[2];
                }
            }

            GL.Enable(EnableCap.CullFace);
        }

        public void Destroy()
        {
            GL.DeleteBuffers(1, ref vertexBuffer);
            GL.DeleteBuffers(1, ref textureBuffer);
        }
    }
}
