using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Device22
{
    public enum NodeDirection
    {
        NORTH_WEST = 1,
        NORTH_EAST = 2,
        SOUTH_WEST = 3,
        SOUTH_EAST = 4
    }

    public enum NodeNeighborDirection
    {
        NORTH = 1,
        EAST = 2,
        SOUTH = 3,
        WEST = 4
    }

    public class QuadTree
    {
        private QuadTreeNode mainNode;
        public QuadTreeNode MainNode
        { get { return mainNode; } }
        private int totalLeaves;
        private int maxLevels;

        private Terrain terrainRef;

        public QuadTree(ref Terrain terrain)
        {
            terrainRef = terrain;

            // the number of leaves to create
            //totalLeaves = terrainRef.PatchesPerRow;

            // calculate the last level where the nodes are being referenced to the patches (each node to one patch)
            //maxLevels = CalcMaxLevels(totalLeaves);

            // generate the main node with the 4 bounding box vertices
            int[] BoundingBox = new int[4];
            BoundingBox[0] = 0;
            BoundingBox[1] = terrain.Width - 1;
            BoundingBox[2] = (terrain.Height - 1) * terrain.Width;
            BoundingBox[3] = ((terrain.Height - 1) * terrain.Width) + (terrain.Width - 1);

            mainNode = new QuadTreeNode(0,
                terrain.Vertices[BoundingBox[0]],
                terrain.Vertices[BoundingBox[1]],
                terrain.Vertices[BoundingBox[2]],
                terrain.Vertices[BoundingBox[3]]);
            AssignBoundingBox3D(ref mainNode, BoundingBox[0], BoundingBox[1], BoundingBox[2], BoundingBox[3]);
            Debug.Trace("BB: Level " + 1 + " -> " + BoundingBox[0] + " " + BoundingBox[1] + " " + BoundingBox[2] + " " + BoundingBox[3]);
            // since its the first node, it gets the first id.
            mainNode.ID = 0;

            QuadTreeNode parent = null;
            GenerateQuadtreeBranch(ref parent, ref mainNode, BoundingBox);
            Debug.Trace("(Class: Quadtree) Quadtree generated");
        }

        private int CalcMaxLevels(int val)      // obsolete?
        {
            // calulate the 4th root of 'val'
            int result = (int)Math.Pow(val, 0.25);
            Debug.Trace("(Class: QuadTree) Calculated " + result + " as the maximum level of depth (With " + val + " patches)");
            return result;
        }

        private void GenerateQuadtreeBranch(ref QuadTreeNode parent, ref QuadTreeNode mainNode, int[] BoundingBox)
        {
            int[] bb = new int[4];
            int currentLevel = mainNode.Level + 1;

            if (parent != null) mainNode.Parent = parent;

            // Top Left child
            bb[0] = BoundingBox[0];                                                                                     // top left vertex (b[0])
            bb[1] = BoundingBox[0] + ((BoundingBox[1] - BoundingBox[0]) / 2);                                           // between b[0] and b[1]   
            bb[2] = BoundingBox[0] + ((BoundingBox[2] - BoundingBox[0]) / 2);                                           // between b[0] and b[2]
            bb[3] = BoundingBox[0] + ((BoundingBox[2] - BoundingBox[0]) / 2) + ((BoundingBox[1] - BoundingBox[0]) / 2); // middle of mainNode

            mainNode.NW = new QuadTreeNode(currentLevel,
                terrainRef.Vertices[bb[0]],
                terrainRef.Vertices[bb[1]],
                terrainRef.Vertices[bb[2]],
                terrainRef.Vertices[bb[3]]);
            mainNode.NW.Parent = mainNode;
            mainNode.NW.ID = (int)NodeDirection.NORTH_WEST;
            if ((bb[1] - bb[0]) > terrainRef.PatchSize)
            {
                // calculate 3d boundingbox for frustum check
                AssignBoundingBox3D(ref mainNode.NW, bb[0], bb[1], bb[2], bb[3]);
                // generate childs (if current node isn't a leaf)
                GenerateQuadtreeBranch(ref mainNode, ref mainNode.NW, bb);
                Debug.Trace("BB: Level " + currentLevel + " -> " + bb[0] + " " + bb[1] + " " + bb[2] + " " + bb[3]);
            }
            else
            {
                // if node is a leaf (because the patchwidth = the desired patchsize for a leaf), assign the patches to this node
                AssignPatch(ref mainNode.NW, NodeDirection.NORTH_WEST, bb[0], bb[1], bb[2], bb[3]);
                Debug.Trace("BB Leaf: Level " + currentLevel + " -> " + bb[0] + " " + bb[1] + " " + bb[2] + " " + bb[3]);
            }
            
            // Top Right child
            bb[0] = BoundingBox[0] + ((BoundingBox[1] - BoundingBox[0]) / 2);                                           // between b[0] and b[1]
            bb[1] = BoundingBox[1];                                                                                     // top right vertex (b[1])   
            bb[2] = BoundingBox[0] + ((BoundingBox[2] - BoundingBox[0]) / 2) + ((BoundingBox[1] - BoundingBox[0]) / 2); // middle of mainNode
            bb[3] = BoundingBox[0] + ((BoundingBox[2] - BoundingBox[0]) / 2) + ((BoundingBox[1] - BoundingBox[0]));     // between b[1] and b[3]

            mainNode.NE = new QuadTreeNode(currentLevel,
                terrainRef.Vertices[bb[0]],
                terrainRef.Vertices[bb[1]],
                terrainRef.Vertices[bb[2]],
                terrainRef.Vertices[bb[3]]);
            mainNode.NE.Parent = mainNode;
            mainNode.NE.ID = (int)NodeDirection.NORTH_EAST;
            if ((bb[1] - bb[0]) > terrainRef.PatchSize)
            {
                // calculate 3d boundingbox for frustum check
                AssignBoundingBox3D(ref mainNode.NE, bb[0], bb[1], bb[2], bb[3]);
                // generate childs (if current node isn't a leaf)
                GenerateQuadtreeBranch(ref mainNode, ref mainNode.NE, bb);
                Debug.Trace("BB: Level " + currentLevel + " -> " + bb[0] + " " + bb[1] + " " + bb[2] + " " + bb[3]);
            }
            else
            {
                // if node is a leaf (because the patchwidth = the desired patchsize for a leaf), assign the patches to this node
                AssignPatch(ref mainNode.NE, NodeDirection.NORTH_EAST, bb[0], bb[1], bb[2], bb[3]);
                Debug.Trace("BB Leaf: Level " + currentLevel + " -> " + bb[0] + " " + bb[1] + " " + bb[2] + " " + bb[3]);
            }

            // Bottom Left child
            bb[0] = BoundingBox[0] + ((BoundingBox[2] - BoundingBox[0]) / 2);                                           // between b[0] and b[2]
            bb[1] = BoundingBox[0] + ((BoundingBox[2] - BoundingBox[0]) / 2) + ((BoundingBox[1] - BoundingBox[0]) / 2); // middle of mainNode  
            bb[2] = BoundingBox[2];                                                                                     // bottom Left vertex (b[2])
            bb[3] = BoundingBox[2] + ((BoundingBox[3] - BoundingBox[2]) / 2);                                           // between b[2] and b[3]

            mainNode.SW = new QuadTreeNode(currentLevel,
                terrainRef.Vertices[bb[0]],
                terrainRef.Vertices[bb[1]],
                terrainRef.Vertices[bb[2]],
                terrainRef.Vertices[bb[3]]);
            mainNode.SW.Parent = mainNode;
            mainNode.SW.ID = (int)NodeDirection.SOUTH_WEST;
            if ((bb[1] - bb[0]) > terrainRef.PatchSize)
            {
                // calculate 3d boundingbox for frustum check
                AssignBoundingBox3D(ref mainNode.SW, bb[0], bb[1], bb[2], bb[3]);
                // generate childs (if current node isn't a leaf)
                GenerateQuadtreeBranch(ref mainNode, ref mainNode.SW, bb);
                Debug.Trace("BB: Level " + currentLevel + " -> " + bb[0] + " " + bb[1] + " " + bb[2] + " " + bb[3]);
            }
            else
            {
                // if node is a leaf (because the patchwidth = the desired patchsize for a leaf), assign the patches to this node
                AssignPatch(ref mainNode.SW, NodeDirection.SOUTH_WEST, bb[0], bb[1], bb[2], bb[3]);
                Debug.Trace("BB Leaf: Level " + currentLevel + " -> " + bb[0] + " " + bb[1] + " " + bb[2] + " " + bb[3]);
            }
            
            // Bottom Right child
            bb[0] = BoundingBox[0] + ((BoundingBox[2] - BoundingBox[0]) / 2) + ((BoundingBox[1] - BoundingBox[0]) / 2); // middle of mainNode
            bb[1] = BoundingBox[0] + ((BoundingBox[2] - BoundingBox[0]) / 2) + ((BoundingBox[1] - BoundingBox[0]));     // between b[1] and b[3]
            bb[2] = BoundingBox[2] + ((BoundingBox[3] - BoundingBox[2]) / 2);                                           // between b[2] and b[3]
            bb[3] = BoundingBox[3];                                                                                     // bottom Right vertex (b[3])

            mainNode.SE = new QuadTreeNode(currentLevel,
                terrainRef.Vertices[bb[0]],
                terrainRef.Vertices[bb[1]],
                terrainRef.Vertices[bb[2]],
                terrainRef.Vertices[bb[3]]);
            mainNode.SE.Parent = mainNode;
            mainNode.SE.ID = (int)NodeDirection.SOUTH_EAST;
            if ((bb[1] - bb[0]) > terrainRef.PatchSize)
            {
                // calculate 3d boundingbox for frustum check
                AssignBoundingBox3D(ref mainNode.SE, bb[0], bb[1], bb[2], bb[3]);
                // generate childs (if current node isn't a leaf)
                GenerateQuadtreeBranch(ref mainNode, ref mainNode.SE, bb);
                Debug.Trace("BB: Level " + currentLevel + " -> " + bb[0] + " " + bb[1] + " " + bb[2] + " " + bb[3]);
            }
            else
            {
                // if node is a leaf (because the patchwidth = the desired patchsize for a leaf), assign the patches to this node
                AssignPatch(ref mainNode.SE, NodeDirection.SOUTH_EAST, bb[0], bb[1], bb[2], bb[3]);
                Debug.Trace("BB Leaf: Level " + currentLevel + " -> " + bb[0] + " " + bb[1] + " " + bb[2] + " " + bb[3]);
            }

            return;
        }

        private void AssignPatch(ref QuadTreeNode node, NodeDirection nodeDirection, int topLeftVertex, int topRightVertex, int bottomLeftVertex, int bottomRightVertex)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvCoor = new List<Vector2>();

            for (int z = topLeftVertex; z <= bottomLeftVertex; z += terrainRef.Width)
            {
                for (int x = z; x < (z + (topRightVertex - topLeftVertex) + 1); x++)
                {
                    vertices.Add(terrainRef.Vertices[x]);
                    uvCoor.Add(terrainRef.uvCoordinates[x]);
                }
            }
            node.Patch = new TerrainPatch((node.Level * 10) + (int)nodeDirection, node.BoundingBox2D, vertices, uvCoor, topRightVertex - topLeftVertex);
        }

        private void AssignBoundingBox3D(ref QuadTreeNode node, int topLeftVertex, int topRightVertex, int bottomLeftVertex, int bottomRightVertex)
        {
            // Calculate the highest point of the terrain patch
            // First use a dummy so we make sure that we get the highest point.
            Vector3 highestV = new Vector3(terrainRef.Vertices[0].X - 999, terrainRef.Vertices[0].Y - 999, terrainRef.Vertices[0].Z - 999);
            for (int z = topLeftVertex; z <= bottomLeftVertex; z += terrainRef.Width)
            {
                for (int x = z; x < (z + (topRightVertex - topLeftVertex) + 1); x++)
                {
                    if (terrainRef.Vertices[x].X > highestV.X)
                    {
                        highestV.X = terrainRef.Vertices[x].X;
                    }
                    if (terrainRef.Vertices[x].Y > highestV.Y)
                    {
                        highestV.Y = terrainRef.Vertices[x].Y;
                    }
                    if (terrainRef.Vertices[x].Z > highestV.Z)
                    {
                        highestV.Z = terrainRef.Vertices[x].Z;
                    }
                }
            }
            // This prevents a bounding box with a height, width and length of 0
            if (highestV.X == 0.0f) { highestV.X = 1.0f; }
            if (highestV.Y == 0.0f) { highestV.Y = 1.0f; }
            if (highestV.Z == 0.0f) { highestV.Z = 1.0f; }

            Vector3 lowestV = new Vector3(highestV);
            // Calculate the lowest point. Used to calculate the boundingbox later on
            for (int z = topLeftVertex; z <= bottomLeftVertex; z += terrainRef.Width)
            {
                for (int x = z; x < (z + (topRightVertex - topLeftVertex) + 1); x++)
                {
                    if (terrainRef.Vertices[x].X < lowestV.X)
                    {
                        lowestV.X = terrainRef.Vertices[x].X;
                    }
                    if (terrainRef.Vertices[x].Y < lowestV.Y)
                    {
                        lowestV.Y = terrainRef.Vertices[x].Y;
                    }
                    if (terrainRef.Vertices[x].Z < lowestV.Z)
                    {
                        lowestV.Z = terrainRef.Vertices[x].Z;
                    }
                }
            }

            // X und Z müssen durch die spherische Verschiebungen AUCH neuberechnet werden!!!
            // !!!!!!!!!!!!!!!!!
            // Nicht vergessen, dies dann auch in der BA zu ändern.


            //highestV.Y = lowestV.Y = 0;

            // Calculate the 3d bounding box
            Vector3[] bbV = new Vector3[8];
            // corner: bottom half(Y), back(X), left(Z)
            bbV[0] = new Vector3(lowestV.X, lowestV.Y, lowestV.Z);
            // corner: bottom half, back, right
            bbV[1] = new Vector3(lowestV.X, lowestV.Y, highestV.Z);
            // corner: bottom half, front, left
            bbV[2] = new Vector3(highestV.X, lowestV.Y, lowestV.Z);
            // corner: bottom half, front, right
            bbV[3] = new Vector3(highestV.X, lowestV.Y, highestV.Z);
            // corner: top half, back, left
            bbV[4] = new Vector3(lowestV.X, highestV.Y, lowestV.Z);
            // corner: top half, back, right
            bbV[5] = new Vector3(lowestV.X, highestV.Y, highestV.Z);
            // corner: top half, front, left
            bbV[6] = new Vector3(highestV.X, highestV.Y, lowestV.Z);
            // corner: top half, front, right
            bbV[7] = new Vector3(highestV.X, highestV.Y, highestV.Z);

            node.BoundingBox3D = bbV;
        }

        public static void CalculateQuadtreeNeighbors(ref QuadTreeNode mainNode)
        {
            // assign all the adjacent nodes
            if (mainNode.NW != null)
            {
                mainNode.NW.Neighbor_N = FindNeighborNorth(ref mainNode.NW);
                mainNode.NW.Neighbor_E = FindNeighborEast(ref mainNode.NW); //mainNode.NE;
                mainNode.NW.Neighbor_S = FindNeighborSouth(ref mainNode.NW); //mainNode.SW;
                mainNode.NW.Neighbor_W = FindNeighborWest(ref mainNode.NW);
            }
            if (mainNode.NE != null)
            {
                mainNode.NE.Neighbor_N = FindNeighborNorth(ref mainNode.NE);
                mainNode.NE.Neighbor_E = FindNeighborEast(ref mainNode.NE);
                mainNode.NE.Neighbor_S = FindNeighborSouth(ref mainNode.NE);  //mainNode.SE;
                mainNode.NE.Neighbor_W = FindNeighborWest(ref mainNode.NE); //mainNode.NW;
            }
            if (mainNode.SW != null)
            {
                mainNode.SW.Neighbor_N = FindNeighborNorth(ref mainNode.SW);
                mainNode.SW.Neighbor_E = FindNeighborEast(ref mainNode.SW); //mainNode.SE;
                mainNode.SW.Neighbor_S = FindNeighborSouth(ref mainNode.SW);
                mainNode.SW.Neighbor_W = FindNeighborWest(ref mainNode.SW);
            }
            if (mainNode.SE != null)
            {
                mainNode.SE.Neighbor_N = FindNeighborNorth(ref mainNode.SE);
                mainNode.SE.Neighbor_E = FindNeighborEast(ref mainNode.SE);
                mainNode.SE.Neighbor_S = FindNeighborSouth(ref mainNode.SE);
                mainNode.SE.Neighbor_W = FindNeighborWest(ref mainNode.SE);
            }

            if ((mainNode.NW != null) && (mainNode.NW.HasChilds()))
            {
                CalculateQuadtreeNeighbors(ref mainNode.NW);
            }
            if ((mainNode.NE != null) && (mainNode.NE.HasChilds()))
            {
                CalculateQuadtreeNeighbors(ref mainNode.NE);
            }
            if ((mainNode.SW != null) && (mainNode.SW.HasChilds()))
            {
                CalculateQuadtreeNeighbors(ref mainNode.SW);
            }
            if ((mainNode.SE != null) && (mainNode.SE.HasChilds()))
            {
                CalculateQuadtreeNeighbors(ref mainNode.SE);
            }

            return;
        }

        private static QuadTreeNode FindNeighborNorth(ref QuadTreeNode node)
        {
            if (node.ID == -1) return null;
            if ((node.ID >= 10) && (node.ID < 20)) return Planet.LookupNeighbors[node.ID - 10, (int)CubeEdgeDirection.NORTH];
            if (node.ID == (int)NodeDirection.SOUTH_WEST)
                return node.Parent.NW;
            if (node.ID == (int)NodeDirection.SOUTH_EAST)
                return node.Parent.NE;
            QuadTreeNode tempNode = FindNeighborNorth(ref node.Parent);
            if ((tempNode == null) || (!tempNode.HasChilds()))
                return tempNode;
            else
                if (node.ID == (int)NodeDirection.NORTH_WEST)
                    return tempNode.SW;
            return tempNode.SE;
        }

        private static QuadTreeNode FindNeighborEast(ref QuadTreeNode node)
        {
            if (node.ID == -1) return null;
            if ((node.ID >= 10) && (node.ID < 20)) return Planet.LookupNeighbors[node.ID - 10, (int)CubeEdgeDirection.EAST];
            if (node.ID == (int)NodeDirection.NORTH_WEST)
                return node.Parent.NE;
            if (node.ID == (int)NodeDirection.SOUTH_WEST)
                return node.Parent.SE;
            QuadTreeNode tempNode = FindNeighborEast(ref node.Parent);
            if ((tempNode == null) || (!tempNode.HasChilds()))
                return tempNode;
            else
                if (node.ID == (int)NodeDirection.NORTH_EAST)
                    return tempNode.NW;
            return tempNode.SW;
        }

        private static QuadTreeNode FindNeighborSouth(ref QuadTreeNode node)
        {
            if (node.ID == -1) return null;
            if ((node.ID >= 10) && (node.ID < 20)) return Planet.LookupNeighbors[node.ID - 10, (int)CubeEdgeDirection.SOUTH];
            if (node.ID == (int)NodeDirection.NORTH_WEST)
                return node.Parent.SW;
            if (node.ID == (int)NodeDirection.NORTH_EAST)
                return node.Parent.SE;
            QuadTreeNode tempNode = FindNeighborSouth(ref node.Parent);
            if ((tempNode == null) || (!tempNode.HasChilds()))
                return tempNode;
            else
                if (node.ID == (int)NodeDirection.SOUTH_WEST)
                    return tempNode.NW;
            return tempNode.NE;
        }

        private static QuadTreeNode FindNeighborWest(ref QuadTreeNode node)
        {
            if (node.ID == -1) return null;
            if ((node.ID >= 10) && (node.ID < 20)) return Planet.LookupNeighbors[node.ID - 10, (int)CubeEdgeDirection.WEST];
            if (node.ID == (int)NodeDirection.NORTH_EAST)
                return node.Parent.NW;
            if (node.ID == (int)NodeDirection.SOUTH_EAST)
                return node.Parent.SW;
            QuadTreeNode tempNode = FindNeighborWest(ref node.Parent);
            if ((tempNode == null) || (!tempNode.HasChilds()))
                return tempNode;
            else
                if (node.ID == (int)NodeDirection.NORTH_WEST)
                    return tempNode.NE;
            return tempNode.SE;
        }
        
        public void Destroy()
        {
            if (mainNode == null) return;
            RecuDestroy(mainNode);
            terrainRef.Destroy();
        }

        private void RecuDestroy(QuadTreeNode mainNode)
        {
            if (mainNode.HasChilds())
            {
                for (int i = 1; i <= 4; i++)
                {
                    QuadTreeNode node = mainNode.GetChild(i);
                    RecuDestroy(node);
                }
            }
            mainNode.Destroy();
        }

        public void Render(ref Frustum frustum)
        {
            Frustum.InFrustumCheck nodeInFrustum = frustum.CubeInFrustum(mainNode.BoundingBox3D);
            if (((nodeInFrustum != Frustum.InFrustumCheck.OUT) && (mainNode.HasChilds())))
            {
                bool checkFrustum = true;
                // if all boundingbox corner where inside the frustum, there is no need to check the childs too
                if (nodeInFrustum == Frustum.InFrustumCheck.IN) { checkFrustum = false; }

                Core.pushState();
                //GL.Scale(terrainRef.scale);
                //GL.Translate(terrainRef.position);

                GL.Enable(EnableCap.Lighting);
                GL.Color4(1.0, 1.0, 1.0, 1.0);

                GL.EnableClientState(ArrayCap.VertexArray);
                GL.EnableClientState(ArrayCap.TextureCoordArray);

                if (terrainRef.TextureID != -1)
                {
                    GL.Enable(EnableCap.Texture2D);
                    Texture.Bind(terrainRef.TextureID);
                }

                CheckNodeInsideFrustum(ref frustum, ref mainNode, checkFrustum);

                if (terrainRef.TextureID != -1)
                {
                    GL.Disable(EnableCap.Texture2D);
                }

                GL.DisableClientState(ArrayCap.TextureCoordArray);
                GL.DisableClientState(ArrayCap.VertexArray);
                Core.popState();
            }
            return;
        }

        private void CheckNodeInsideFrustum(ref Frustum frustum, ref QuadTreeNode mainNode, bool checkFrustum = true)
        {
            Frustum.InFrustumCheck nodeInFrustum;
            bool checkChildFrustum = true;
            // if the node has childs, check if nodes childs are in or outside the frustum
            if (mainNode.HasChilds())
            {
                for (int i = 1; i <= 4; i++)
                {
                    QuadTreeNode node = mainNode.GetChild(i);
                    if (checkFrustum)
                    {
                        if (node.HasChilds())
                        {
                            nodeInFrustum = frustum.CubeInFrustum(node.BoundingBox3D);
                        }
                        else
                        {
                            nodeInFrustum = frustum.CubeInFrustum(node.Patch);
                        }
                        if (nodeInFrustum == Frustum.InFrustumCheck.OUT) { continue; }
                        if (nodeInFrustum == Frustum.InFrustumCheck.IN) { checkChildFrustum = false; }
                    }
                    CheckNodeInsideFrustum(ref frustum, ref node, checkChildFrustum);
                }
            }
            // if it don't, it's a leaf and the terrain patch can be rendered
            else
            {
                int maxLODLevels = terrainRef.LODLEVELS;
                int patchResolution = mainNode.Patch.GetResolution(maxLODLevels);
                if ((mainNode.Patch.UpdateToResolution > -1) && (mainNode.Patch.UpdateToResolution != patchResolution)) patchResolution = mainNode.Patch.UpdateToResolution;
                int mainPatchIndexBuffer = terrainRef.IndexBuffer[patchResolution];

                List<int> defaultBridgeIndexBuffer = new List<int>();
                List<int> lowerBridgeIndexBuffer = new List<int>();

                if (mainNode.Neighbor_N != null)
                {
                    // check north neighbor patch resolution
                    int nRes = mainNode.Neighbor_N.Patch.GetResolution(maxLODLevels);
                    if (nRes > patchResolution)
                    {
                        if (nRes > patchResolution * 2) mainNode.Neighbor_N.Patch.UpdateToResolution = patchResolution * 2;
                        lowerBridgeIndexBuffer.Add(terrainRef.BridgeIndexBuffer[patchResolution, 4]);
                    }
                    else
                    {
                        mainNode.Neighbor_N.Patch.UpdateToResolution = -1;
                        defaultBridgeIndexBuffer.Add(terrainRef.BridgeIndexBuffer[patchResolution, 0]);
                    }
                }
                else
                    defaultBridgeIndexBuffer.Add(terrainRef.BridgeIndexBuffer[patchResolution, 0]);
                if (mainNode.Neighbor_E != null)
                {
                    // check east neighbor patch resolution
                    int eRes = mainNode.Neighbor_E.Patch.GetResolution(maxLODLevels);
                    if (eRes > patchResolution)
                    {
                        if (eRes > patchResolution * 2) mainNode.Neighbor_E.Patch.UpdateToResolution = patchResolution * 2;
                        lowerBridgeIndexBuffer.Add(terrainRef.BridgeIndexBuffer[patchResolution, 5]);
                    }
                    else
                    {
                        mainNode.Neighbor_E.Patch.UpdateToResolution = -1;
                        defaultBridgeIndexBuffer.Add(terrainRef.BridgeIndexBuffer[patchResolution, 1]);
                    }
                }
                else
                    defaultBridgeIndexBuffer.Add(terrainRef.BridgeIndexBuffer[patchResolution, 1]);
                if (mainNode.Neighbor_S != null)
                {
                    // check south neighbor patch resolution
                    int sRes = mainNode.Neighbor_S.Patch.GetResolution(maxLODLevels);
                    if (sRes > patchResolution)
                    {
                        if (sRes > patchResolution * 2) mainNode.Neighbor_S.Patch.UpdateToResolution = patchResolution * 2;
                        lowerBridgeIndexBuffer.Add(terrainRef.BridgeIndexBuffer[patchResolution, 6]);
                    }
                    else
                    {
                        mainNode.Neighbor_S.Patch.UpdateToResolution = -1;
                        defaultBridgeIndexBuffer.Add(terrainRef.BridgeIndexBuffer[patchResolution, 2]);
                    }
                }
                else
                    defaultBridgeIndexBuffer.Add(terrainRef.BridgeIndexBuffer[patchResolution, 2]);
                if (mainNode.Neighbor_W != null)
                {
                    // check west neighbor patch resolution
                    int wRes = mainNode.Neighbor_W.Patch.GetResolution(maxLODLevels);
                    if (wRes > patchResolution)
                    {
                        if (wRes > patchResolution * 2) mainNode.Neighbor_W.Patch.UpdateToResolution = patchResolution * 2;
                        lowerBridgeIndexBuffer.Add(terrainRef.BridgeIndexBuffer[patchResolution, 7]);
                    }
                    else
                    {
                        mainNode.Neighbor_W.Patch.UpdateToResolution = -1;
                        defaultBridgeIndexBuffer.Add(terrainRef.BridgeIndexBuffer[patchResolution, 3]);
                    }
                }
                else
                    defaultBridgeIndexBuffer.Add(terrainRef.BridgeIndexBuffer[patchResolution, 3]);

                int[] indicesCount = new int[3];
                indicesCount[0] = terrainRef.IndicesCount[patchResolution];                 // main patch indices count
                indicesCount[1] = terrainRef.BridgeIndicesCount[0, patchResolution];        // default bridge indices count
                indicesCount[2] = terrainRef.BridgeIndicesCount[1, patchResolution];        // lower bridge indices count

                mainNode.Patch.Render(mainPatchIndexBuffer, defaultBridgeIndexBuffer.ToArray(), lowerBridgeIndexBuffer.ToArray(), indicesCount);
            }
            return;
        }
    }
}
