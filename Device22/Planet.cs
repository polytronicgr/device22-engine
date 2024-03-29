﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Device22
{
    public enum CubeFaceDirection
    {
        LEFT = 0,
        TOP = 1,
        RIGHT = 2,
        BOTTOM = 3,
        FRONT = 4,
        BACK = 5
    }

    public enum CubeEdgeDirection
    {
        NORTH = 0,
        EAST = 1,
        SOUTH = 2,
        WEST = 3
    }

    public class Planet
    {
        public static QuadTreeNode[,] LookupNeighbors;

        Terrain[] planetFace;
        QuadTree[] planetFaceQT;
        QuadTreeNode mainNode;

        // always 4 patches per terrain, for now. Or 4^N.
        private int terrainSize = 1025;
        private int patchSize = 257;
        private float planetRadius = 100.0f;
        private int counter = 6;
        private int maxLOD = 7;

        public Planet()
        {
            int tex0 = Texture.loadFromFile("earthtexture_high_c04.jpg"); //earthcubemap_tex_c04.bmp");
            int tex1 = Texture.loadFromFile("earthtexture_high_c02.jpg"); //earthcubemap_tex_c02.bmp");
            int tex2 = Texture.loadFromFile("earthtexture_high_c05.jpg"); //earthcubemap_tex_c05.bmp");
            int tex3 = Texture.loadFromFile("earthtexture_high_c03.jpg"); //earthcubemap_tex_c03.bmp");
            int tex4 = Texture.loadFromFile("earthtexture_high_c00.jpg"); //earthcubemap_tex_c00.bmp");
            int tex5 = Texture.loadFromFile("earthtexture_high_c01.jpg"); //earthcubemap_tex_c01.bmp");

            planetFace = new Terrain[counter];
            planetFaceQT = new QuadTree[counter];

            // Left face:
            planetFace[0] = new Terrain(new Vector3(-(terrainSize / 2), -(terrainSize / 2), -(terrainSize / 2)),
                                        new Vector3(1.0f, 0.0f, 0.0f),
                                        new Vector3(0.0f, 1.0f, 0.0f),
                                        maxLOD,
                                        terrainSize,
                                        patchSize,
                                        planetRadius,
                                        tex0,
                                        "earthcubemap_c04.jpg");

            // Top face:
            planetFace[1] = new Terrain(new Vector3(-(terrainSize / 2), (terrainSize / 2), -(terrainSize / 2)),
                                        new Vector3(1.0f, 0.0f, 0.0f),
                                        new Vector3(0.0f, 0.0f, 1.0f),
                                        maxLOD,
                                        terrainSize,
                                        patchSize,
                                        planetRadius,
                                        tex1,
                                        "earthcubemap_c02.jpg");

            // Right face:
            planetFace[2] = new Terrain(new Vector3((terrainSize / 2), -(terrainSize / 2), (terrainSize / 2)),
                                        new Vector3(-1.0f, 0.0f, 0.0f),
                                        new Vector3(0.0f, 1.0f, 0.0f),
                                        maxLOD,
                                        terrainSize,
                                        patchSize,
                                        planetRadius,
                                        tex2,
                                        "earthcubemap_c05.jpg");

            // Bottom face:
            planetFace[3] = new Terrain(new Vector3((terrainSize / 2), -(terrainSize / 2), -(terrainSize / 2)),
                                        new Vector3(-1.0f, 0.0f, 0.0f),
                                        new Vector3(0.0f, 0.0f, 1.0f),
                                        maxLOD,
                                        terrainSize,
                                        patchSize,
                                        planetRadius,
                                        tex3,
                                        "earthcubemap_c03.jpg");
            planetFaceQT[3] = new QuadTree(ref planetFace[3]);

            // Front face:
            planetFace[4] = new Terrain(new Vector3(-(terrainSize / 2), (terrainSize / 2), -(terrainSize / 2)),
                                        new Vector3(0.0f, 0.0f, 1.0f),
                                        new Vector3(0.0f, -1.0f, 0.0f),
                                        maxLOD,
                                        terrainSize,
                                        patchSize,
                                        planetRadius,
                                        tex4,
                                        "earthcubemap_c00.jpg");

            // Back face:
            planetFace[5] = new Terrain(new Vector3((terrainSize / 2), -(terrainSize / 2), -(terrainSize / 2)),
                                        new Vector3(0.0f, 0.0f, 1.0f),
                                        new Vector3(0.0f, 1.0f, 0.0f),
                                        maxLOD,
                                        terrainSize,
                                        patchSize,
                                        planetRadius,
                                        tex5,
                                        "earthcubemap_c01.jpg");

            // Since there are six terrain faces to create the sphere, there is need to assign the missing border neighbors manually.
            // So we create a new Quadtree.
            // Generate the first node and assign manually the boundingbox.
            mainNode = new QuadTreeNode(0, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero);
            /*mainNode.BoundingBox3D[0] = new Vector3(-(terrainSize / 2), -(terrainSize / 2), (terrainSize / 2));
            mainNode.BoundingBox3D[1] = new Vector3((terrainSize / 2), -(terrainSize / 2), (terrainSize / 2));
            mainNode.BoundingBox3D[2] = new Vector3(-(terrainSize / 2), -(terrainSize / 2), -(terrainSize / 2));
            mainNode.BoundingBox3D[3] = new Vector3((terrainSize / 2), -(terrainSize / 2), -(terrainSize / 2));
            mainNode.BoundingBox3D[4] = new Vector3(-(terrainSize / 2), (terrainSize / 2), (terrainSize / 2));
            mainNode.BoundingBox3D[5] = new Vector3((terrainSize / 2), (terrainSize / 2), (terrainSize / 2));
            mainNode.BoundingBox3D[6] = new Vector3(-(terrainSize / 2), (terrainSize / 2), -(terrainSize / 2));
            mainNode.BoundingBox3D[7] = new Vector3((terrainSize / 2), (terrainSize / 2), -(terrainSize / 2));*/
            // Since its the first node, it gets the first id.
            mainNode.ID = -1;
            

            // Calculate quadtree.
            for (int i = 0; i < counter; i++)
            {
                planetFaceQT[i] = new QuadTree(ref planetFace[i]);
                planetFaceQT[i].MainNode.Parent = mainNode;
            }

            // Assign manually the specific node ids
            planetFaceQT[0].MainNode.ID = 10 + (int)CubeFaceDirection.LEFT;
            planetFaceQT[1].MainNode.ID = 10 + (int)CubeFaceDirection.TOP;
            planetFaceQT[2].MainNode.ID = 10 + (int)CubeFaceDirection.RIGHT;
            planetFaceQT[3].MainNode.ID = 10 + (int)CubeFaceDirection.BOTTOM;
            planetFaceQT[4].MainNode.ID = 10 + (int)CubeFaceDirection.FRONT;
            planetFaceQT[5].MainNode.ID = 10 + (int)CubeFaceDirection.BACK;

            // Generate lookup table (ugle hack!)
            if (LookupNeighbors == null) CreateNeighborLookupTable();

            // Calculate and assign neighbors
            for (int i = 0; i < counter; i++)
            {
                QuadTreeNode mNode = planetFaceQT[i].MainNode;
                QuadTree.CalculateQuadtreeNeighbors(ref mNode);
            }

            // try 1
            /*
            // Each node has 4 childs, so to cover all six cube terrain-sides we need four more nodes.
            // Neighbors for the left face:
            mainNode.NW = new QuadTreeNode(0, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero);
            mainNode.NW.Parent = mainNode;
            mainNode.NW.Neighbor_N = planetFaceQT[1].MainNode;
            mainNode.NW.Neighbor_E = planetFaceQT[5].MainNode;
            mainNode.NW.Neighbor_S = planetFaceQT[3].MainNode;
            mainNode.NW.Neighbor_W = planetFaceQT[4].MainNode;
            mainNode.NW.ID = 0;
            // Neighbors for the back face:
            mainNode.NE = new QuadTreeNode(0, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero);
            mainNode.NE.Parent = mainNode;
            mainNode.NE.Neighbor_N = planetFaceQT[2].MainNode;
            mainNode.NE.Neighbor_E = planetFaceQT[3].MainNode;
            mainNode.NE.Neighbor_S = planetFaceQT[0].MainNode;
            mainNode.NE.Neighbor_W = planetFaceQT[1].MainNode;
            mainNode.NE.ID = 0;
            // Neighbors for the right face:
            mainNode.SW = new QuadTreeNode(0, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero);
            mainNode.SW.Parent = mainNode;
            mainNode.SW.Neighbor_N = planetFaceQT[3].MainNode;
            mainNode.SW.Neighbor_E = planetFaceQT[4].MainNode;
            mainNode.SW.Neighbor_S = planetFaceQT[1].MainNode;
            mainNode.SW.Neighbor_W = planetFaceQT[5].MainNode;
            mainNode.SW.ID = 0;
            // Neighbors for the front face:
            mainNode.SE = new QuadTreeNode(0, Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero);
            mainNode.SE.Parent = mainNode;
            mainNode.SE.Neighbor_N = planetFaceQT[2].MainNode;
            mainNode.SE.Neighbor_E = planetFaceQT[1].MainNode;
            mainNode.SE.Neighbor_S = planetFaceQT[0].MainNode;
            mainNode.SE.Neighbor_W = planetFaceQT[3].MainNode;
            mainNode.SE.ID = 0;
            */
            Debug.Trace("BB: Level " + -1 + " -> Generated");
             
            // Calculate and assign the missing nodes neighbors.
            for (int i = 0; i < counter; i++)
            {
                //QuadTreeNode mNode = planetFaceQT[i].MainNode;
                //QuadTree.CalculateQuadtreeNeighbors(ref mNode);
            }


            // try 2
            /*
            for (int i = 0; i < counter; i++)
            {
                QuadTreeNode mNode = planetFaceQT[i].MainNode;
                QuadTree.CalculateQuadtreeNeighbors(ref mNode);
            }
            
            // Neighbors for the left face:
            planetFaceQT[0].MainNode.Neighbor_N = planetFaceQT[1].MainNode;
            planetFaceQT[0].MainNode.Neighbor_E = planetFaceQT[5].MainNode;
            planetFaceQT[0].MainNode.Neighbor_S = planetFaceQT[3].MainNode;
            planetFaceQT[0].MainNode.Neighbor_W = planetFaceQT[4].MainNode;

            // Neighbors for the top face:
            planetFaceQT[1].MainNode.Neighbor_N = planetFaceQT[2].MainNode;
            planetFaceQT[1].MainNode.Neighbor_E = planetFaceQT[5].MainNode;
            planetFaceQT[1].MainNode.Neighbor_S = planetFaceQT[0].MainNode;
            planetFaceQT[1].MainNode.Neighbor_W = planetFaceQT[4].MainNode;

            // Neighbors for the right face:
            planetFaceQT[2].MainNode.Neighbor_N = planetFaceQT[3].MainNode;
            planetFaceQT[2].MainNode.Neighbor_E = planetFaceQT[4].MainNode;
            planetFaceQT[2].MainNode.Neighbor_S = planetFaceQT[1].MainNode;
            planetFaceQT[2].MainNode.Neighbor_W = planetFaceQT[5].MainNode;

            // Neighbors for the bottom face:
            planetFaceQT[3].MainNode.Neighbor_N = planetFaceQT[0].MainNode;
            planetFaceQT[3].MainNode.Neighbor_E = planetFaceQT[4].MainNode;
            planetFaceQT[3].MainNode.Neighbor_S = planetFaceQT[2].MainNode;
            planetFaceQT[3].MainNode.Neighbor_W = planetFaceQT[5].MainNode;

            // Neighbors for the front face:
            planetFaceQT[4].MainNode.Neighbor_N = planetFaceQT[2].MainNode;
            planetFaceQT[4].MainNode.Neighbor_E = planetFaceQT[1].MainNode;
            planetFaceQT[4].MainNode.Neighbor_S = planetFaceQT[0].MainNode;
            planetFaceQT[4].MainNode.Neighbor_W = planetFaceQT[3].MainNode;

            // Neighbors for the back face:
            planetFaceQT[5].MainNode.Neighbor_N = planetFaceQT[2].MainNode;
            planetFaceQT[5].MainNode.Neighbor_E = planetFaceQT[3].MainNode;
            planetFaceQT[5].MainNode.Neighbor_S = planetFaceQT[0].MainNode;
            planetFaceQT[5].MainNode.Neighbor_W = planetFaceQT[1].MainNode;

            */



            // Neighborsuche geschieht über die Parents. Die müssen hier noch zugewiesen werden.
            // da es aber 6 terrains sind, brauche ich vier quadtrees mit jeweils vier neighbors.
            // als super node wird dann eine node als wurzel dienen.

            
            
           Debug.Trace("Planet generated!");
        }

        public void CreateNeighborLookupTable()
        {
            LookupNeighbors = new QuadTreeNode[6, 4];
            // Neighbors for the left face
            LookupNeighbors[(int)CubeFaceDirection.LEFT, (int)CubeEdgeDirection.NORTH] = planetFaceQT[(int)CubeFaceDirection.BOTTOM].MainNode;
            LookupNeighbors[(int)CubeFaceDirection.LEFT, (int)CubeEdgeDirection.EAST] = planetFaceQT[(int)CubeFaceDirection.BACK].MainNode;
            LookupNeighbors[(int)CubeFaceDirection.LEFT, (int)CubeEdgeDirection.SOUTH] = planetFaceQT[(int)CubeFaceDirection.TOP].MainNode;
            LookupNeighbors[(int)CubeFaceDirection.LEFT, (int)CubeEdgeDirection.WEST] = planetFaceQT[(int)CubeFaceDirection.FRONT].MainNode;
            // Neighbors for the top face
            LookupNeighbors[(int)CubeFaceDirection.TOP, (int)CubeEdgeDirection.NORTH] = planetFaceQT[(int)CubeFaceDirection.LEFT].MainNode;
            LookupNeighbors[(int)CubeFaceDirection.TOP, (int)CubeEdgeDirection.EAST] = planetFaceQT[(int)CubeFaceDirection.BACK].MainNode;
            LookupNeighbors[(int)CubeFaceDirection.TOP, (int)CubeEdgeDirection.SOUTH] = planetFaceQT[(int)CubeFaceDirection.RIGHT].MainNode;
            LookupNeighbors[(int)CubeFaceDirection.TOP, (int)CubeEdgeDirection.WEST] = planetFaceQT[(int)CubeFaceDirection.FRONT].MainNode;
            // Neighbors for the right face
            LookupNeighbors[(int)CubeFaceDirection.RIGHT, (int)CubeEdgeDirection.NORTH] = planetFaceQT[(int)CubeFaceDirection.BOTTOM].MainNode;
            LookupNeighbors[(int)CubeFaceDirection.RIGHT, (int)CubeEdgeDirection.EAST] = planetFaceQT[(int)CubeFaceDirection.FRONT].MainNode;
            LookupNeighbors[(int)CubeFaceDirection.RIGHT, (int)CubeEdgeDirection.SOUTH] = planetFaceQT[(int)CubeFaceDirection.TOP].MainNode;
            LookupNeighbors[(int)CubeFaceDirection.RIGHT, (int)CubeEdgeDirection.WEST] = planetFaceQT[(int)CubeFaceDirection.BACK].MainNode;
            // Neighbors for the bottom face
            LookupNeighbors[(int)CubeFaceDirection.BOTTOM, (int)CubeEdgeDirection.NORTH] = planetFaceQT[(int)CubeFaceDirection.LEFT].MainNode;
            LookupNeighbors[(int)CubeFaceDirection.BOTTOM, (int)CubeEdgeDirection.EAST] = planetFaceQT[(int)CubeFaceDirection.FRONT].MainNode;
            LookupNeighbors[(int)CubeFaceDirection.BOTTOM, (int)CubeEdgeDirection.SOUTH] = planetFaceQT[(int)CubeFaceDirection.RIGHT].MainNode;
            LookupNeighbors[(int)CubeFaceDirection.BOTTOM, (int)CubeEdgeDirection.WEST] = planetFaceQT[(int)CubeFaceDirection.BACK].MainNode;
            // Neighbors for the front face
            LookupNeighbors[(int)CubeFaceDirection.FRONT, (int)CubeEdgeDirection.NORTH] = planetFaceQT[(int)CubeFaceDirection.TOP].MainNode;
            LookupNeighbors[(int)CubeFaceDirection.FRONT, (int)CubeEdgeDirection.EAST] = planetFaceQT[(int)CubeFaceDirection.RIGHT].MainNode;
            LookupNeighbors[(int)CubeFaceDirection.FRONT, (int)CubeEdgeDirection.SOUTH] = planetFaceQT[(int)CubeFaceDirection.BOTTOM].MainNode;
            LookupNeighbors[(int)CubeFaceDirection.FRONT, (int)CubeEdgeDirection.WEST] = planetFaceQT[(int)CubeFaceDirection.LEFT].MainNode;
            // Neighbors for the back face
            LookupNeighbors[(int)CubeFaceDirection.BACK, (int)CubeEdgeDirection.NORTH] = planetFaceQT[(int)CubeFaceDirection.BOTTOM].MainNode;
            LookupNeighbors[(int)CubeFaceDirection.BACK, (int)CubeEdgeDirection.EAST] = planetFaceQT[(int)CubeFaceDirection.LEFT].MainNode;
            LookupNeighbors[(int)CubeFaceDirection.BACK, (int)CubeEdgeDirection.SOUTH] = planetFaceQT[(int)CubeFaceDirection.TOP].MainNode;
            LookupNeighbors[(int)CubeFaceDirection.BACK, (int)CubeEdgeDirection.WEST] = planetFaceQT[(int)CubeFaceDirection.RIGHT].MainNode;
        }

        public void Render(ref Frustum frustum)
        {
            for (int i = 0; i < planetFaceQT.Length; i++)
            {
               if (planetFaceQT[i] != null) planetFaceQT[i].Render(ref frustum);
            }
        }

        public void Destroy()
        {

        }
    }
}
