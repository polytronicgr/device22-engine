using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Device22
{
    public class Texture
    {
        int filterFar;
        int filterNear;
        Bitmap bitmap;
        int ID = -1;
        float angle = 0.0f;

        public Texture()
        {
        }

        public static int loadFromFile(string url) { return loadFromFile(url, (int)TextureMinFilter.Linear, (int)TextureMagFilter.Linear); }

        public static int loadFromFile(string url, int filter_far, int filter_near)
        {
            try
            {
                Texture tex = new Texture();
                tex.bitmap = Texture.loadBMP(url);
                if (tex.bitmap == null) { return -1; }
                GL.GenTextures(1, out tex.ID);
                tex.Bind();
                
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, filter_far);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, filter_near);

                GL.GetTexParameter(TextureTarget.Texture2D, GetTextureParameter.TextureMagFilter, out tex.filterFar);
                GL.GetTexParameter(TextureTarget.Texture2D, GetTextureParameter.TextureMinFilter, out tex.filterNear);

                Rectangle rect = new Rectangle(0, 0, tex.bitmap.Width, tex.bitmap.Height);
                System.Drawing.Imaging.BitmapData bitmapdata = tex.bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, tex.bitmap.Width, tex.bitmap.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, bitmapdata.Scan0);
                
                tex.bitmap.Dispose();
                return tex.ID;
            }
            catch(ArgumentException)
            {
                Debug.Trace("Class Texture: Error creating Bitmap: " + url);
                return -1;
            }
        }

        private static Bitmap loadBMP(string fileName)
        {
            if (fileName == null || fileName == string.Empty)
            {                  // Make Sure A Filename Was Given
                Debug.Trace("Class Texture: No filename was provided");
                return null;                                                    // If Not Return Null
            }

            string fileName1 = string.Format("Data{0}{1}",                      // Look For Data\Filename
                Path.DirectorySeparatorChar, fileName);
            string fileName2 = string.Format("{0}{1}{0}{1}Data{1}{2}",          // Look For ..\..\Data\Filename
                "..", Path.DirectorySeparatorChar, fileName);

            // Make Sure The File Exists In One Of The Usual Directories
            if (!File.Exists(fileName) && !File.Exists(fileName1) && !File.Exists(fileName2))
            {
                Debug.Trace("Class Texture: Bitmap: " + fileName + " does not exist");
                return null;                                                    // If Not Return Null
            }

            if (File.Exists(fileName))
            {                                         // Does The File Exist Here?
                return new Bitmap(fileName);                                    // Load The Bitmap
            }
            else if (File.Exists(fileName1))
            {                                   // Does The File Exist Here?
                return new Bitmap(fileName1);                                   // Load The Bitmap
            }
            else if (File.Exists(fileName2))
            {                                   // Does The File Exist Here?
                return new Bitmap(fileName2);                                   // Load The Bitmap
            }
            Debug.Trace("Class Texture: Error loading Bitmap: " + fileName);
            return null;                                                        // If Load Failed Return Null
        }

        public static void Bind(int ID)
        {
            if (ID == -1)
                return;

            GL.MatrixMode(MatrixMode.Texture);
            GL.LoadIdentity();

            GL.BindTexture(TextureTarget.Texture2D, ID);
            GL.MatrixMode(MatrixMode.Modelview);
        }

        public void Bind()
        {
            if (this.ID == -1)
                return;

            GL.MatrixMode(MatrixMode.Texture);
            GL.LoadIdentity();

            GL.Translate(0.5f, 0.5f, 0.0f);
            GL.Rotate(this.angle, 0.0f, 0.0f, 1.0f);
            GL.Translate(-0.5f, -0.5f, 0.0f);

            GL.BindTexture(TextureTarget.Texture2D, this.ID);
            GL.MatrixMode(MatrixMode.Modelview);
        }

        public void SetRotation(float rotAngle) { this.angle = rotAngle; }
        public float GetRotationAngle() { return this.angle; }
        public int GetID() { return this.ID; }
    }
}
