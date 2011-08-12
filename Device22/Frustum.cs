using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Device22
{
    public class Frustum
    {
        public enum InFrustumCheck
        {
            IN = 0,
            INTERSECT = 1,
            OUT = 2
        }

        enum FrustumSide
        {
            RIGHT = 0,		// The RIGHT side of the frustum
            LEFT = 1,		// The LEFT	 side of the frustum
            BOTTOM = 2,		// The BOTTOM side of the frustum
            TOP = 3,		// The TOP side of the frustum
            BACK = 4,		// The BACK	side of the frustum
            FRONT = 5	    // The FRONT side of the frustum
        }

        enum PlaneData
        {
            A = 0,				// The X value of the plane's normal
            B = 1,				// The Y value of the plane's normal
            C = 2,				// The Z value of the plane's normal
            D = 3				// The distance the plane is from the origin
        }

        private float[,] FrustumModel = new float[6, 4];

        public Frustum()
        {
        }

        private void NormalizePlane(float[,] frustum, int side)
        {
            float magnitude = (float)Math.Sqrt(frustum[side, (int)PlaneData.A] * frustum[side, (int)PlaneData.A] +
                                           frustum[side, (int)PlaneData.B] * frustum[side, (int)PlaneData.B] +
                                           frustum[side, (int)PlaneData.C] * frustum[side, (int)PlaneData.C]);

            frustum[side, (int)PlaneData.A] /= magnitude;
            frustum[side, (int)PlaneData.B] /= magnitude;
            frustum[side, (int)PlaneData.C] /= magnitude;
            frustum[side, (int)PlaneData.D] /= magnitude;
        }

        public void CalculateFrustum()
        {
            float[] proj = new float[16];								// This will hold our projection matrix
            float[] modl = new float[16];								// This will hold our modelview matrix
            float[] clip = new float[16];                  				// This will hold the clipping planes

            // glGetFloatv() is used to extract information about our OpenGL world.
            // Below, we pass in GL_PROJECTION_MATRIX to abstract our projection matrix.
            // It then stores the matrix into an array of [16].
            GL.GetFloat(GetPName.ProjectionMatrix, proj);

            // By passing in GL_MODELVIEW_MATRIX, we can abstract our model view matrix.
            // This also stores it in an array of [16].
            GL.GetFloat(GetPName.ModelviewMatrix, modl);

            // Now that we have our modelview and projection matrix, if we combine these 2 matrices,
            // it will give us our clipping planes.  To combine 2 matrices, we multiply them.

            clip[0] = modl[0] * proj[0] + modl[1] * proj[4] + modl[2] * proj[8] + modl[3] * proj[12];
            clip[1] = modl[0] * proj[1] + modl[1] * proj[5] + modl[2] * proj[9] + modl[3] * proj[13];
            clip[2] = modl[0] * proj[2] + modl[1] * proj[6] + modl[2] * proj[10] + modl[3] * proj[14];
            clip[3] = modl[0] * proj[3] + modl[1] * proj[7] + modl[2] * proj[11] + modl[3] * proj[15];

            clip[4] = modl[4] * proj[0] + modl[5] * proj[4] + modl[6] * proj[8] + modl[7] * proj[12];
            clip[5] = modl[4] * proj[1] + modl[5] * proj[5] + modl[6] * proj[9] + modl[7] * proj[13];
            clip[6] = modl[4] * proj[2] + modl[5] * proj[6] + modl[6] * proj[10] + modl[7] * proj[14];
            clip[7] = modl[4] * proj[3] + modl[5] * proj[7] + modl[6] * proj[11] + modl[7] * proj[15];

            clip[8] = modl[8] * proj[0] + modl[9] * proj[4] + modl[10] * proj[8] + modl[11] * proj[12];
            clip[9] = modl[8] * proj[1] + modl[9] * proj[5] + modl[10] * proj[9] + modl[11] * proj[13];
            clip[10] = modl[8] * proj[2] + modl[9] * proj[6] + modl[10] * proj[10] + modl[11] * proj[14];
            clip[11] = modl[8] * proj[3] + modl[9] * proj[7] + modl[10] * proj[11] + modl[11] * proj[15];

            clip[12] = modl[12] * proj[0] + modl[13] * proj[4] + modl[14] * proj[8] + modl[15] * proj[12];
            clip[13] = modl[12] * proj[1] + modl[13] * proj[5] + modl[14] * proj[9] + modl[15] * proj[13];
            clip[14] = modl[12] * proj[2] + modl[13] * proj[6] + modl[14] * proj[10] + modl[15] * proj[14];
            clip[15] = modl[12] * proj[3] + modl[13] * proj[7] + modl[14] * proj[11] + modl[15] * proj[15];

            // Now we actually want to get the sides of the frustum.  To do this we take
            // the clipping planes we received above and extract the sides from them.

            // This will extract the RIGHT side of the frustum
            FrustumModel[(int)FrustumSide.RIGHT, (int)PlaneData.A] = (clip[3]) - clip[0];
            FrustumModel[(int)FrustumSide.RIGHT, (int)PlaneData.B] = (clip[7]) - clip[4];
            FrustumModel[(int)FrustumSide.RIGHT, (int)PlaneData.C] = (clip[11]) - clip[8];
            FrustumModel[(int)FrustumSide.RIGHT, (int)PlaneData.D] = (clip[15]) - clip[12];

            // Now that we have a normal (A,B,C) and a distance (D) to the plane,
            // we want to normalize that normal and distance.

            // Normalize the RIGHT side
            NormalizePlane(FrustumModel, (int)FrustumSide.RIGHT);

            // This will extract the LEFT side of the frustum
            FrustumModel[(int)FrustumSide.LEFT, (int)PlaneData.A] = (clip[3]) + clip[0];
            FrustumModel[(int)FrustumSide.LEFT, (int)PlaneData.B] = (clip[7]) + clip[4];
            FrustumModel[(int)FrustumSide.LEFT, (int)PlaneData.C] = (clip[11]) + clip[8];
            FrustumModel[(int)FrustumSide.LEFT, (int)PlaneData.D] = (clip[15]) + clip[12];

            // Normalize the LEFT side
            NormalizePlane(FrustumModel, (int)FrustumSide.LEFT);

            // This will extract the BOTTOM side of the frustum
            FrustumModel[(int)FrustumSide.BOTTOM, (int)PlaneData.A] = (clip[3]) + clip[1];
            FrustumModel[(int)FrustumSide.BOTTOM, (int)PlaneData.B] = (clip[7]) + clip[5];
            FrustumModel[(int)FrustumSide.BOTTOM, (int)PlaneData.C] = (clip[11]) + clip[9];
            FrustumModel[(int)FrustumSide.BOTTOM, (int)PlaneData.D] = (clip[15]) + clip[13];

            // Normalize the BOTTOM side
            NormalizePlane(FrustumModel, (int)FrustumSide.BOTTOM);

            // This will extract the TOP side of the frustum
            FrustumModel[(int)FrustumSide.TOP, (int)PlaneData.A] = (clip[3]) - clip[1];
            FrustumModel[(int)FrustumSide.TOP, (int)PlaneData.B] = (clip[7]) - clip[5];
            FrustumModel[(int)FrustumSide.TOP, (int)PlaneData.C] = (clip[11]) - clip[9];
            FrustumModel[(int)FrustumSide.TOP, (int)PlaneData.D] = (clip[15]) - clip[13];

            // Normalize the TOP side
            NormalizePlane(FrustumModel, (int)FrustumSide.TOP);

            // This will extract the BACK side of the frustum
            FrustumModel[(int)FrustumSide.BACK, (int)PlaneData.A] = (clip[3]) - clip[2];
            FrustumModel[(int)FrustumSide.BACK, (int)PlaneData.B] = (clip[7]) - clip[6];
            FrustumModel[(int)FrustumSide.BACK, (int)PlaneData.C] = (clip[11]) - clip[10];
            FrustumModel[(int)FrustumSide.BACK, (int)PlaneData.D] = (clip[15]) - clip[14];

            // Normalize the BACK side
            NormalizePlane(FrustumModel, (int)FrustumSide.BACK);

            // This will extract the FRONT side of the frustum
            FrustumModel[(int)FrustumSide.FRONT, (int)PlaneData.A] = (clip[3]) + clip[2];
            FrustumModel[(int)FrustumSide.FRONT, (int)PlaneData.B] = (clip[7]) + clip[6];
            FrustumModel[(int)FrustumSide.FRONT, (int)PlaneData.C] = (clip[11]) + clip[10];
            FrustumModel[(int)FrustumSide.FRONT, (int)PlaneData.D] = (clip[15]) + clip[14];

            // Normalize the FRONT side
            NormalizePlane(FrustumModel, (int)FrustumSide.FRONT);
        }

        public bool PointInFrustum(Vector3 vPoint)
        {
            for (int i = 0; i < 6; i++)
            {
                // Calculate the plane equation and check if the point is behind a side of the frustum
                if (FrustumModel[i, (int)PlaneData.A] * vPoint.X + FrustumModel[i, (int)PlaneData.B] * vPoint.Y + FrustumModel[i, (int)PlaneData.C] * vPoint.Z + FrustumModel[i, (int)PlaneData.D] < 0)
                {
                    // The point was behind a side, so it ISN'T in the frustum
                    return false;
                }
            }

            // The point was inside of the frustum (In front of ALL the sides of the frustum)
            return true;
        }

        public InFrustumCheck CubeInFrustum(TerrainPatch patch)
        {
            return InFrustumCheck.IN;
            int totalIn = 0;
            // Test all 6 sides against all 8 corners
            for (int s = 0; s < 6; s++)
            {
                int inCount = 8;
                int cIn = 1;
                for (int c = 0; c < 8; c++)
                {
                    // test corner against all planes
                    if (FrustumModel[s, (int)PlaneData.A] * patch.BoundingBoxV[c].X + FrustumModel[s, (int)PlaneData.B] * patch.BoundingBoxV[c].Y + FrustumModel[s, (int)PlaneData.C] * patch.BoundingBoxV[c].Z + FrustumModel[s, (int)PlaneData.D] < 0)
                    {
                        cIn = 0;
                        inCount--;
                    }

                    // if all points where outside, return false
                    if (inCount == 0) { return InFrustumCheck.OUT; }

                    // check if all points where on the right side of the plane
                    totalIn += cIn;
                }
            }
            // if all points where inside, return true
            if (totalIn == 6) { return InFrustumCheck.IN; }
            // at this point the cube must be partly inside
            return InFrustumCheck.INTERSECT;
        }

        //
        // Summary:
        //     Check if a cube (with 8 vertices) is inside the frustum, intersecting it or outside
        //
        // Parameters:
        //   BoundingBox3D:
        //     Array with 8 elements representing the cubes boundingbox
        //
        // Returns:
        //     The InFrustumCheck element describing the boundingbox behavior to the frustum (IN, INTERSECT or OUT)
    public InFrustumCheck CubeInFrustum(Vector3[] BoundingBox3D)
    {
        return InFrustumCheck.IN;
        int totalIn = 0;
        // Test all 6 sides against all 8 corners
        for (int s = 0; s < 6; s++)
        {
            int inCount = 8;
            int cIn = 1;
            for (int c = 0; c < 8; c++)
            {
                // test corner against all planes
                if (FrustumModel[s, (int)PlaneData.A] * BoundingBox3D[c].X + FrustumModel[s, (int)PlaneData.B] * BoundingBox3D[c].Y + FrustumModel[s, (int)PlaneData.C] * BoundingBox3D[c].Z + FrustumModel[s, (int)PlaneData.D] < 0)
                {
                    cIn = 0;
                    inCount--;
                }

                // if all points where outside, return false
                if (inCount == 0) { return InFrustumCheck.OUT; }

                // check if all points where on the right side of the plane
                totalIn += cIn;
            }
        }
        // if all points where inside, return true
        if (totalIn == 6) { return InFrustumCheck.IN; }
        // at this point the cube must be partly inside
        return InFrustumCheck.INTERSECT;
        }

        public InFrustumCheck CubeInFrustum(Vector3 vCenter, float size)
        {
            for (int i = 0; i < 6; i++)
            {
                if (FrustumModel[i, (int)PlaneData.A] * (vCenter.X - size) + FrustumModel[i, (int)PlaneData.B] * (vCenter.Y - size) + FrustumModel[i, (int)PlaneData.C] * (vCenter.Z - size) + FrustumModel[i, (int)PlaneData.D] >= 0) continue;
                if (FrustumModel[i, (int)PlaneData.A] * (vCenter.X + size) + FrustumModel[i, (int)PlaneData.B] * (vCenter.Y - size) + FrustumModel[i, (int)PlaneData.C] * (vCenter.Z - size) + FrustumModel[i, (int)PlaneData.D] >= 0) continue;
                if (FrustumModel[i, (int)PlaneData.A] * (vCenter.X - size) + FrustumModel[i, (int)PlaneData.B] * (vCenter.Y + size) + FrustumModel[i, (int)PlaneData.C] * (vCenter.Z - size) + FrustumModel[i, (int)PlaneData.D] >= 0) continue;
                if (FrustumModel[i, (int)PlaneData.A] * (vCenter.X + size) + FrustumModel[i, (int)PlaneData.B] * (vCenter.Y + size) + FrustumModel[i, (int)PlaneData.C] * (vCenter.Z - size) + FrustumModel[i, (int)PlaneData.D] >= 0) continue;
                if (FrustumModel[i, (int)PlaneData.A] * (vCenter.X - size) + FrustumModel[i, (int)PlaneData.B] * (vCenter.Y - size) + FrustumModel[i, (int)PlaneData.C] * (vCenter.Z + size) + FrustumModel[i, (int)PlaneData.D] >= 0) continue;
                if (FrustumModel[i, (int)PlaneData.A] * (vCenter.X + size) + FrustumModel[i, (int)PlaneData.B] * (vCenter.Y - size) + FrustumModel[i, (int)PlaneData.C] * (vCenter.Z + size) + FrustumModel[i, (int)PlaneData.D] >= 0) continue;
                if (FrustumModel[i, (int)PlaneData.A] * (vCenter.X - size) + FrustumModel[i, (int)PlaneData.B] * (vCenter.Y + size) + FrustumModel[i, (int)PlaneData.C] * (vCenter.Z + size) + FrustumModel[i, (int)PlaneData.D] >= 0) continue;
                if (FrustumModel[i, (int)PlaneData.A] * (vCenter.X + size) + FrustumModel[i, (int)PlaneData.B] * (vCenter.Y + size) + FrustumModel[i, (int)PlaneData.C] * (vCenter.Z + size) + FrustumModel[i, (int)PlaneData.D] >= 0) continue;
                return InFrustumCheck.OUT;
            }
            return InFrustumCheck.IN;
        }
    }
}
