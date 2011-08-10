using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

namespace Device22
{
    abstract class Primitive
    {
        protected static List<Primitive> list = new List<Primitive>();
        protected static ColorRGBA gColorID = new ColorRGBA();
        protected Vector3 pos = new Vector3(0.0f, 0.0f, 0.0f);
        protected Vector3 rot = new Vector3(0.0f, 0.0f, 0.0f);
        protected Vector3 scale = new Vector3(1.0f, 1.0f, 1.0f);
        protected ColorRGBA color;
        protected int name;         // wird der name noch benötigt?

        public Primitive(int name) 
            : base()
        {
            this.name = name;
            /*this.color = Primitive.gColorID;

            Primitive.gColorID.R++;
            if (Primitive.gColorID.R > 255)
            {
                Primitive.gColorID.R = 0;
                Primitive.gColorID.G++;
                if (Primitive.gColorID.G > 255)
                {
                    Primitive.gColorID.G = 0;
                    Primitive.gColorID.B++;
                    if (Primitive.gColorID.B > 255)
                    {
                        Primitive.gColorID.B = 0;
                        Primitive.gColorID.A++;
                        if (Primitive.gColorID.A > 255)
                        {
                            Primitive.gColorID.A = 255;
                            return;
                        }
                    }
                }
            }*/
            Primitive.list.Add(this);
        }

        public static Primitive getPrimitiveByColor(ColorRGBA color)
        {
            foreach (Primitive p in Primitive.list)
            {
                if (p.color == color) { return p; }
            }
            return null;
        }

        public static void MouseMoveEvent(object sende, MouseMoveEventArgs m)
        {
            GL.PushAttrib(AttribMask.AllAttribBits);
            GL.Disable(EnableCap.Fog);
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Dither);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.LineStipple);
            GL.Disable(EnableCap.PolygonStipple);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.AlphaTest);

            foreach (Primitive p in list)
            {
                /*p.render();*/
            }

            byte[] pix = new byte[4];
            GL.ReadPixels(m.X, Device22.Game.height - m.Y, 1, 1, PixelFormat.Rgba, PixelType.UnsignedInt, pix);
            //Debug.Trace("asd : " + pix[0] + " " + pix[1] + " " + pix[2]);
            uint pixInt = BitConverter.ToUInt32(pix, 0);
            if (pixInt == uint.MaxValue) { Debug.Trace("Background selected"); }
            ColorRGBA pickedColor = new ColorRGBA(pix[0], pix[1], pix[2], pix[3]);
            //Debug.Trace(pickedColor);

            Primitive tmp = Primitive.getPrimitiveByColor(pickedColor);             // auf den 2. render durchgang packen, damit so der eine listendurchgang erspart wird
            if (tmp != null) { Debug.Trace(tmp.name + " picked."); }

            GL.PopAttrib();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public virtual void render(ref Frustum frustum)
        {
        }

        #region Setter & Getter
        public void setPosition(Vector3 pos)
        {
            this.pos = pos;
        }

        public Vector3 getPosition()
        {
            return this.pos;
        }

        public void setRotation(Vector3 rot)
        {
            this.rot = rot;
        }

        public Vector3 getRotation()
        {
            return this.rot;
        }

        public void setScale(Vector3 scale)
        {
            this.scale = scale;
        }

        public Vector3 getScale()
        {
            return this.scale;
        }

        public void setColor(ColorRGBA color)
        {
            this.color = color;
        }

        public ColorRGBA getColor()
        {
            return this.color;
        }

        public int getName()
        {
            return this.name;
        }

        public void setname(int name)
        {
            this.name = name;
        }

        public void moveX(float x)
        {
            this.pos.X += x;
        }

        public void moveY(float y)
        {
            this.pos.Y += y;
        }

        public void moveZ(float z)
        {
            this.pos.Z += z;
        }
        #endregion
    }

    class VBOPrimitive : Primitive
    {
        protected static Vbo vbo;
        private static bool wasGenerated = false;

        public VBOPrimitive(int name, VertexPositionColor[] vertices, short[] elements)
            : base(name)
        {
            if (!wasGenerated)
            {
                vbo = genVBO(vertices, elements);
                wasGenerated = true;
            }
        }

        ~VBOPrimitive()
        {
            // GL.DeleteBuffers(1, ref vbo.vboID);
            // GL.DeleteBuffers(1, ref vbo.eboID);
        }

        private static Vbo genVBO<TVertex>(TVertex[] vertices, short[] elements) where TVertex : struct
        {
            Debug.Trace("VBOCube generated -> vertices: " + vertices.Length + "; elements: " + elements.Length);

            Vbo vbo = new Vbo();
            int size;

            GL.GenBuffers(1, out vbo.vboID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * BlittableValueType.StrideOf(vertices)), vertices, BufferUsageHint.StaticDraw);
            //GL.BufferData<Vertex>(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * Vertex.SizeInBytes), vertices, BufferUsageHint.StaticDraw);
            //GL.InterleavedArrays(InterleavedArrayFormat.C4ubV3f, 0, IntPtr.Zero);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length * BlittableValueType.StrideOf(vertices) != size) { throw new ApplicationException("Vertex data not uploaded correctly"); }

            GL.GenBuffers(1, out vbo.eboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vbo.eboID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(elements.Length * sizeof(short)), elements, BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (elements.Length * sizeof(short) != size) { throw new ApplicationException("Element data not uploaded correctly"); }

            vbo.numElements = elements.Length;
            vbo.sizeOfVertices = BlittableValueType.StrideOf(vertices);
            return vbo;
        }

        public override void render(ref Frustum frustum)
        {
            Frustum.InFrustumCheck primitiveInFrustum = frustum.CubeInFrustum(pos, 1.0f);
            if (primitiveInFrustum == Frustum.InFrustumCheck.IN)
            {
                GL.PushMatrix();

                GL.Translate(this.pos.X, this.pos.Y, this.pos.Z);

                //GL.EnableClientState(ArrayCap.ColorArray);
                GL.EnableClientState(ArrayCap.VertexArray);

                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.vboID);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, vbo.eboID);

                GL.VertexPointer(3, VertexPointerType.Float, vbo.sizeOfVertices, new IntPtr(0));
                //GL.ColorPointer(4, ColorPointerType.UnsignedByte, vbo.sizeOfVertices, new IntPtr(12));
                //Debug.Trace(color.ToString());
                //GL.Color4(this.color.R, this.color.G, this.color.B, this.color.A);
                GL.Color3(this.color.R, this.color.G, this.color.B);
                //GL.Color3(255, 255, 255);

                GL.DrawElements(BeginMode.Triangles, vbo.numElements, DrawElementsType.UnsignedShort, IntPtr.Zero);

                GL.DisableClientState(ArrayCap.VertexArray);
                //GL.DisableClientState(ArrayCap.ColorArray);

                GL.Color4(1.0, 1.0, 1.0, 1.0);

                GL.PopMatrix();

                Game.Instance.polyCount += 6;
            }
        }
    }

    class VBOCube : VBOPrimitive
    {
        public VBOCube(int name) : base(name, VBOCube.vertices, VBOCube.elements) { }

        private static readonly short[] elements = new short[]
            {
                0, 1, 2, 2, 3, 0, // front face
                3, 2, 6, 6, 7, 3, // top face
                7, 6, 5, 5, 4, 7, // back face
                4, 0, 3, 3, 7, 4, // left face
                0, 1, 5, 5, 4, 0, // bottom face
                1, 5, 6, 6, 2, 1, // right face
            };

        private static readonly VertexPositionColor[] vertices = new VertexPositionColor[]
            {
                    new VertexPositionColor(-1.0f, -1.0f,  1.0f, new ColorRGBA(1f, 0f, 1f, 1f)),
                    new VertexPositionColor( 1.0f, -1.0f,  1.0f, new ColorRGBA(1f, 0f, 1f, 1f)),
                    new VertexPositionColor( 1.0f,  1.0f,  1.0f, new ColorRGBA(0f, 1f, 1f, 1f)),
                    new VertexPositionColor(-1.0f,  1.0f,  1.0f, new ColorRGBA(0f, 1f, 1f, 1f)),
                    new VertexPositionColor(-1.0f, -1.0f, -1.0f, new ColorRGBA(1f, 1f, 0f, 1f)),
                    new VertexPositionColor( 1.0f, -1.0f, -1.0f, new ColorRGBA(1f, 1f, 0f, 1f)), 
                    new VertexPositionColor( 1.0f,  1.0f, -1.0f, new ColorRGBA(1f, 0f, 1f, 1f)),
                    new VertexPositionColor(-1.0f,  1.0f, -1.0f, new ColorRGBA(1f, 0f, 1f, 1f)) 
            };
    }

    class Cube : Primitive
    {
        public Cube(int name) : base(name) { }

        public override void render(ref Frustum frustum)
        {
            Frustum.InFrustumCheck primitiveInFrustum = frustum.CubeInFrustum(pos, 1.0f);
            if (primitiveInFrustum == Frustum.InFrustumCheck.IN)
            {
                GL.Disable(EnableCap.Lighting);
                GL.PushMatrix();
                GL.Translate(this.pos.X, this.pos.Y, this.pos.Z);
                GL.Color4(this.color.R, this.color.G, this.color.B, this.color.A);

                GL.Begin(BeginMode.Quads);
                GL.Vertex3(1, 1, -1);                                        // Top Right Of The Quad (Top)
                GL.Vertex3(-1, 1, -1);                                       // Top Left Of The Quad (Top)
                GL.Vertex3(-1, 1, 1);                                        // Bottom Left Of The Quad (Top)
                GL.Vertex3(1, 1, 1);                                         // Bottom Right Of The Quad (Top)
                GL.Vertex3(1, -1, 1);                                        // Top Right Of The Quad (Bottom)
                GL.Vertex3(-1, -1, 1);                                       // Top Left Of The Quad (Bottom)
                GL.Vertex3(-1, -1, -1);                                      // Bottom Left Of The Quad (Bottom)
                GL.Vertex3(1, -1, -1);                                       // Bottom Right Of The Quad (Bottom)
                GL.Vertex3(1, 1, 1);                                         // Top Right Of The Quad (Front)
                GL.Vertex3(-1, 1, 1);                                        // Top Left Of The Quad (Front)
                GL.Vertex3(-1, -1, 1);                                       // Bottom Left Of The Quad (Front)
                GL.Vertex3(1, -1, 1);                                        // Bottom Right Of The Quad (Front)
                GL.Vertex3(1, -1, -1);                                       // Top Right Of The Quad (Back)
                GL.Vertex3(-1, -1, -1);                                      // Top Left Of The Quad (Back)
                GL.Vertex3(-1, 1, -1);                                       // Bottom Left Of The Quad (Back)
                GL.Vertex3(1, 1, -1);                                        // Bottom Right Of The Quad (Back)
                GL.Vertex3(-1, 1, 1);                                        // Top Right Of The Quad (Left)
                GL.Vertex3(-1, 1, -1);                                       // Top Left Of The Quad (Left)
                GL.Vertex3(-1, -1, -1);                                      // Bottom Left Of The Quad (Left)
                GL.Vertex3(-1, -1, 1);                                       // Bottom Right Of The Quad (Left)
                GL.Vertex3(1, 1, -1);                                        // Top Right Of The Quad (Right)
                GL.Vertex3(1, 1, 1);                                         // Top Left Of The Quad (Right)
                GL.Vertex3(1, -1, 1);                                        // Bottom Left Of The Quad (Right)
                GL.Vertex3(1, -1, -1);                                       // Bottom Right Of The Quad (Right)
                GL.End();

                GL.Color4(1.0, 1.0, 1.0, 1.0);
                GL.PopMatrix();
                GL.Enable(EnableCap.Lighting);

                Game.Instance.polyCount += 6;
            }
        }
    }
}