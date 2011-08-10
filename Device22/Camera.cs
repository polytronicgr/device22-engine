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
        public float near = 0.1f;
        public float far = 100.0f;
        private Vector3 position;
        private Vector3 view;
        private Vector3 up;
        private Vector3 strafe;

        private float speed = 0.0f;
        public float Speed
        { get { return speed; } }

        private static float currentRotX = 0.0f;
        private static float lastRotX = 0.0f;


        public Camera(float near, float far, float speed = 1.0f)
        {
            this.near = near;
            this.far = far;
            this.speed = speed;
            position = Vector3.Zero;
            view = new Vector3(0.0f, 1.0f, 0.5f);
            up = new Vector3(0.0f, 0.0f, 1.0f);
        }

        public void Position(float positionX, float positionY, float positionZ,
                             float viewX,     float viewY,     float viewZ,
                             float upX,       float upY,       float upZ)
        {
            position = new Vector3(positionX, positionY, positionZ);
            view = new Vector3(viewX, viewY, viewZ);
            up = new Vector3(upX, upY, upZ);
        }

        public void SetViewByMouse()
        {
            int middleX = Game.Instance.Width >> 1;
            int middleY = Game.Instance.Height >> 1;
            float angleY = 0.0f;
            float angleZ = 0.0f;

            currentRotX = 0.0f;

            Point mousePos = Game.MouseFunctions.getPosition();

            Game.MouseFunctions.setPosition(new Point(middleX, middleY));
            angleY = (float)((middleX - mousePos.X) / 500.0f);
            angleZ = (float)((middleY - mousePos.Y) / 500.0f);

            lastRotX = 0.0f;
            lastRotX = currentRotX;         // warum?

            currentRotX += angleZ;

            Vector3 pVector = Vector3.Subtract(view, position);        // perpendicular vector from the view vector and up vector.
            Vector3 vAxis = Vector3.Cross(pVector, up);

            if (currentRotX > 1.0f)
            {
                currentRotX = 1.0f;

                if (lastRotX != 1.0f)
                {
                    RotateView(1.0f - lastRotX, vAxis.X, vAxis.Y, vAxis.Z);
                }
            }
            else if(currentRotX < -1.0f)
            {
                currentRotX = -1.0f;

                if (lastRotX != -1.0)
                {
                    RotateView(-1.0f - lastRotX, vAxis.X, vAxis.Y, vAxis.Z);
                }
            }
            else
            {
                RotateView(angleZ, vAxis.X, vAxis.Y, vAxis.Z);
            }
            
            RotateView(angleY, 0, 1, 0);
        }

        public void RotateView(float angle, float x, float y, float z)
        {
            Vector3 newView = new Vector3();
            // Get the view vector (The direction we are facing)
            Vector3 currView = Vector3.Subtract(view, position);

            // Calculate the sine and cosine of the angle once
            float cosTheta = (float)Math.Cos(angle);
            float sinTheta = (float)Math.Sin(angle);

            // Find the new x position for the new rotated point
            newView.X = (cosTheta + (1 - cosTheta) * x * x)     * currView.X;
            newView.X += ((1 - cosTheta) * x * y - z * sinTheta)* currView.Y;
            newView.X += ((1 - cosTheta) * x * z + y * sinTheta)* currView.Z;

            // Find the new y position for the new rotated point
            newView.Y = ((1 - cosTheta) * x * y + z * sinTheta) * currView.X;
            newView.Y += (cosTheta + (1 - cosTheta) * y * y)    * currView.Y;
            newView.Y += ((1 - cosTheta) * y * z - x * sinTheta)* currView.Z;

            // Find the new z position for the new rotated point
            newView.Z = ((1 - cosTheta) * x * z - y * sinTheta) * currView.X;
            newView.Z += ((1 - cosTheta) * y * z + x * sinTheta)* currView.Y;
            newView.Z += (cosTheta + (1 - cosTheta) * z * z)    * currView.Z;

            // Now we just add the newly rotated vector to our position to set
	        // our new rotated view of our camera.
            view = Vector3.Add(position, newView);
        }

        public void StrafeView(float speed)
        {
            position.X += strafe.X * speed;
            position.Z += strafe.Z * speed;

            view.X += strafe.X * speed;
            view.Z += strafe.Z * speed;
        }

        public void MoveView(float speed)
        {
            Vector3 currView = Vector3.Subtract(view, position);
            currView.Normalize();

            position.X += currView.X * speed;
            position.Y += currView.Y * speed;
            position.Z += currView.Z * speed;

            view.X += currView.X * speed;
            view.Y += currView.Y * speed;
            view.Z += currView.Z * speed;
        }

        public void Update()
        {
            Vector3 currView = Vector3.Subtract(view, position);
            Vector3 cross = Vector3.Cross(currView, up);
            cross.Normalize();
            strafe = cross;

            SetViewByMouse();

            //calculateFrameRate();     // obsolete?
        }

        public void Look()
        {
            Matrix4 lookat = Matrix4.LookAt(position, view, up);
            //GL.MultMatrix(ref lookat);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);
        }

        public float calcDistanceToVertexPoint(Vector3 v)
        {
            float dx, dy, dz, distance;
            dx = v.X - position.X;
            dy = v.Y - position.Y;
            dz = v.Z - position.Z;
            distance = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
            return distance;
        }
    }
}
