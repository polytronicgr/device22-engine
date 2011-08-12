using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Device22
{
    public class QuadTreeNode
    {
        public int ID;

        // the 4 childs
        public QuadTreeNode NW = null;
        public QuadTreeNode NE = null;
        public QuadTreeNode SW = null;
        public QuadTreeNode SE = null;

        // Refs to the adjacent nodes
        public QuadTreeNode Neighbor_N = null;
        public QuadTreeNode Neighbor_E = null;
        public QuadTreeNode Neighbor_S = null;
        public QuadTreeNode Neighbor_W = null;

        // used to find all neighbours, even if they are childs to an other node
        public QuadTreeNode Parent = null;

        private int level;
        public int Level
        { get { return level; } }
        // 2D Bounding Box vertices for quadtree generation
        private Vector3[] boundingV2D;
        public Vector3[] BoundingBox2D
        { get { return boundingV2D; } }
        public Vector3[] BoundingBox3D;
        /*private Vector3 centerV;
        public Vector3 CenterVertex
        { get { return centerV; } }*/

        // patch assigned to node, if node is a leaf
        public TerrainPatch Patch;

        public QuadTreeNode(int nodeLevel, Vector3 topLeftBBV, Vector3 topRightBBV, Vector3 bottomLeftBBV, Vector3 bottomRightBBV)
        {
            level = nodeLevel;
            boundingV2D = new Vector3[4] { topLeftBBV, topRightBBV, bottomLeftBBV, bottomRightBBV };
            BoundingBox3D = new Vector3[8];
        }

        // used to get the children at runtime
        public QuadTreeNode GetChild(int nodeDirection)
        {
            if ((nodeDirection < 1) && (nodeDirection > 4)) return null;
            if (nodeDirection == 1) return NW;
            if (nodeDirection == 2) return NE;
            if (nodeDirection == 3) return SW;
            return SE;
        }

        public bool HasChilds()
        {
            if (NW != null) return true;
            if (NE != null) return true;
            if (SW != null) return true;
            if (SE != null) return true;
            return false;
        }

        public void Destroy()
        {
            if (Patch != null)
            {
                Patch.Destroy();
            }
        }
    }
}
