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
    public class Camera
    {
        public float near;
        public float far;
        private Vector3 eye;
        private Vector3 target;
        private Vector3 up;
        private Matrix4 camMatrix;
        private float pitch;
        private float facing;
        private float kSpeed;
        private float[] mSpeed = new float[2];
        public bool mousing = false;
        private Point lastMousePos = new Point();

        public Camera(float near, float far, float moveSpeed = 0.5f)
        {
            this.near = near;
            this.far = far;
            this.eye = new Vector3(5.0f, 5.0f, 5.0f);
            this.target = new Vector3(-0.5f, -0.5f, -0.5f);
            this.up = new Vector3(0.0f, 1.0f, 0.0f);
            this.kSpeed = moveSpeed;
            this.camMatrix = Matrix4.Identity;
            this.pitch = 0.0f;
            this.facing = 0.0f;

            this.lastMousePos = new Point(Device22.Game.width / 2, Device22.Game.height / 2);

            int middleX = Device22.Game.width >> 1;
            int middleY = Device22.Game.height >> 1;
            //Device22.Program.Game.mouse.setPosition(new Point(middleX, middleY));

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Device22.Game.width / (float)Device22.Game.height, near, far);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }

        public void update(Point mousepos)
        {
            mSpeed[0] *= 0.9f;
            mSpeed[1] *= 0.9f;
            if (this.mousing)
            {
                int nXDiff = (mousepos.X - this.lastMousePos.X);
                int nYDiff = (mousepos.Y - this.lastMousePos.Y);

                if (nXDiff != 0)
                {
                    mSpeed[0] += nXDiff / 1000f;
                }
                if (nYDiff != 0)
                {
                    mSpeed[1] += nYDiff / 1000f;
                }
                lastMousePos = mousepos;
            }
            facing += mSpeed[0];
            pitch += mSpeed[1];
            target = new Vector3((float)Math.Cos(facing), -pitch, (float)Math.Sin(facing));
            Matrix4 modelview = Matrix4.LookAt(eye, eye + target, up);
            GL.LoadMatrix(ref modelview);
        }

        public void moveUp()
        {
            eye.X += (float)Math.Cos(facing) * kSpeed;
            eye.Z += (float)Math.Sin(facing) * kSpeed;
        }

        public void moveDown()
        {
            eye.X -= (float)Math.Cos(facing) * kSpeed;
            eye.Z -= (float)Math.Sin(facing) * kSpeed;
        }

        public void moveLeft()
        {
            eye.X -= (float)Math.Cos(facing + Math.PI / 2) * kSpeed;
            eye.Z -= (float)Math.Sin(facing + Math.PI / 2) * kSpeed;
        }

        public void moveRight()
        {
            eye.X += (float)Math.Cos(facing + Math.PI / 2) * kSpeed;
            eye.Z += (float)Math.Sin(facing + Math.PI / 2) * kSpeed;
        }

        public float getMoveSpeed()
        {
            return this.kSpeed;
        }

        public void setMoveSpeed(float moveSpeed)
        {
            this.kSpeed = moveSpeed;
        }

        public Point getLastMousePos()
        {
            return this.lastMousePos;
        }

        public void setLastMousePos(Point mousePos)
        {
            this.lastMousePos = mousePos;
        }

        public Vector3 getPostion() { return this.eye; }

        public float calcDistanceToVertexPoint(Vector3 v)
        {
            float dx, dy, dz, distance;
            dx = v.X - eye.X;
            dy = v.Y - eye.Y;
            dz = v.Z - eye.Z;
            distance = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
            return distance;
        }
    }
}
