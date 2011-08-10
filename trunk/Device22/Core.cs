using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Glu = OpenTK.Graphics;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;

namespace Device22
{
    struct Byte4
    {
        public byte R, G, B, A;

        public Byte4(ColorRGBA color)
        {
            R = (byte)color.R;
            G = (byte)color.G;
            B = (byte)color.B;
            A = (byte)color.A;
        }

        public uint ToUInt32()
        {
            byte[] temp = new byte[] { this.R, this.G, this.B, this.A };
            return BitConverter.ToUInt32(temp, 0);
        }

        public override string ToString()
        {
            return this.R + ", " + this.G + ", " + this.B + ", " + this.A;
        }
    }

    struct Vbo
    {
        public int vboID;
        public int eboID;
        public int numElements;
        public int sizeOfVertices;
    }

    struct VertexPositionColor
    {
        public Vector3 Position;    // 12 bytes
        public Byte4 Color;         // 4 bytes

        public const byte SizeInBytes = 16;

        public VertexPositionColor(float x, float y, float z, ColorRGBA color)
        {
            Position = new Vector3(x, y, z);
            Color = new Byte4(color);
        }
    }

    public struct ColorRGB
    {
        public float R, G, B;

        public ColorRGB(int R, int G, int B) { this.R = R / 255f; this.G = G / 255f; this.B = B / 255f; }

        public ColorRGB(float R, float G, float B) { this.R = R; this.G = G; this.B = B; }

        public void Set(int R, int G, int B) { this.R = R / 255f; this.G = G / 255f; this.B = B / 255f; }

        public void Set(float R, float G, float B) { this.R = R; this.G = G; this.B = B; }

        public static bool operator ==(ColorRGB c1, ColorRGB c2) { return ((c1.R == c2.R) && (c1.G == c2.G) && (c1.B == c2.B)); }

        public static bool operator !=(ColorRGB c1, ColorRGB c2) { return ((c1.R != c2.R) && (c1.G != c2.G) && (c1.B != c2.B)); }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return "R:" + this.R + ", G:" + this.G + ", B:" + this.B;
        }
    }

    public struct ColorRGBA
    {
        public float R, G, B, A;

        public ColorRGBA(float R, float G, float B, float A) { this.R = R; this.G = G; this.B = B; this.A = A; }

        public ColorRGBA(int R, int G, int B, int A) { this.R = R / 255f; this.G = G / 255f; this.B = B / 255f; this.A = A / 255f; }

        public void Set(float R, float G, float B, float A) { this.R = R; this.G = G; this.B = B; this.A = A; }

        public void Set(int R, int G, int B, int A) { this.R = R / 255f; this.G = G / 255f; this.B = B / 255f; this.A = A / 255f; }

        public static bool operator ==(ColorRGBA c1, ColorRGBA c2) { return ((c1.R == c2.R) && (c1.G == c2.G) && (c1.B == c2.B) && (c1.A == c2.A)); }

        public static bool operator !=(ColorRGBA c1, ColorRGBA c2) { return ((c1.R != c2.R) && (c1.G != c2.G) && (c1.B != c2.B) && (c1.A != c2.A)); }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return "R:" + this.R + ", G:" + this.G + ", B:" + this.B + ", A:" + this.A;
        }
    }

    public class ColorFunctions
    {
        public static byte ARGB(byte A, byte R, byte G, byte B)
        {
            return ((byte)((A & 0xFF) << 24 | (R & 0xFF) << 16 | (G & 0xFF) << 8 | (B & 0xFF)));
        }

        public static int GetA(int c)
        {
            return (byte)((c >> 24) & 0xFF);
        }

        public static int GetR(int c)
        {
            return (byte)((c >> 16) & 0xFF);
        }

        public static int GetG(int c)
        {
            return (byte)((c >> 8) & 0xFF);
        }

        public static int GetB(int c)
        {
            return (byte)(c & 0xFF);
        }
    }

    public class Core
    {
        public static void ErrorExit(string message = "An Error occured")
        {
            MessageBox.Show(message);
            Application.Exit();
            Environment.Exit(-1);
            Game.Instance.Exit();
        }

        private static bool wireframed = false;
        public static void SetWireframeMode(bool mode)
        {
            if (mode)
            {
                wireframed = true;
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            } else {
                wireframed = false;
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }
        }

        public static void ChangeWireframeMode()
        {
            if (!wireframed)
            {
                wireframed = true;
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            } else {
                wireframed = false;
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }
        }

        public static void pushState()
        {
            GL.PushClientAttrib(ClientAttribMask.ClientAllAttribBits);              // wird das hier benötigt?
            GL.PushMatrix();
        }

        public static void popState()
        {
            GL.PopClientAttrib();
            GL.PopMatrix();
        }
        
        /*
        public static class Fps
        {
            private static float frameInterval = 0.0f;
            public static float FrameInterval
            { get { return frameInterval; } }
            private static float framesPerSecond = 0.0f;
            private static float frameTime = 0.0f;
            private static float lastTime = 0.0f;

            public static void Get(double time, ref int fps)
            {
                float currTime = (float)time * 1000.0f;
                frameInterval = currTime - frameTime;
                frameTime = currTime;

                framesPerSecond++;

                if (currTime - lastTime > 1.0f)
                {
                    lastTime = currTime;

                    fps = (int)framesPerSecond;
                    framesPerSecond = 0;
                }
            }
        }
         * */

        public static class Fps
        {
            static double _time = 0.0, _frames = 0.0;
            static int _fps = 0;

            public static int Get(double time)
            {
                _time += time;
                if (_time < 1.0)
                {
                    _frames++;
                    return _fps;
                }
                else
                {
                    _fps = (int)_frames;
                    _time = 0.0;
                    _frames = 0.0;
                    return _fps;
                }
            }
        }

        public static bool IsExtAvailable(String ext)
        {
            return GL.GetString(StringName.Extensions).Contains(ext);
        }

        public static bool useGLSL = false;
        public static bool CheckGLSLSupport()
        {
            if (useGLSL) return true;
            return IsExtAvailable("GL_EXT_geometry_shader4") && IsExtAvailable("GL_EXT_gpu_shader4");
        }

        public class Shader
        {
            private int VertexShaderObject;
            private int FragmentShaderObject;
            private int ProgramObject;
            private bool active;
            public bool isActive
            {
                get { return active; }
            }

            public Shader(string VSUrl, string FSUrl)
            {
                // First call Core.Shader.Init();
                if ((!Core.useGLSL) || (!Core.CheckGLSLSupport())) { return; }

                active = false;

                using (StreamReader sr = new StreamReader(VSUrl))
                {
                    VertexShaderObject = GL.CreateShader(ShaderType.VertexShader);
                    GL.ShaderSource(VertexShaderObject, sr.ReadToEnd());
                    GL.CompileShader(VertexShaderObject);
                }

                ErrorCode err;
                err = GL.GetError();
                if (err != ErrorCode.NoError)
                    Debug.Trace("Vertex Shader: " + err);

                string LogInfo;
                GL.GetShaderInfoLog(VertexShaderObject, out LogInfo);
                if (LogInfo.Length > 0 && !LogInfo.Contains("hardware"))
                    Debug.Trace("Vertex Shader failed!\nLog:\n" + LogInfo);
                else
                    Debug.Trace("Vertex Shader compiled without complaint.");

                // Load&Compile Fragment Shader

                using (StreamReader sr = new StreamReader(FSUrl))
                {
                    FragmentShaderObject = GL.CreateShader(ShaderType.FragmentShader);
                    GL.ShaderSource(FragmentShaderObject, sr.ReadToEnd());
                    GL.CompileShader(FragmentShaderObject);
                }
                GL.GetShaderInfoLog(FragmentShaderObject, out LogInfo);

                err = GL.GetError();
                if (err != ErrorCode.NoError)
                    Debug.Trace("Fragment Shader: " + err);

                if (LogInfo.Length > 0 && !LogInfo.Contains("hardware"))
                    Debug.Trace("Fragment Shader failed!\nLog:\n" + LogInfo);
                else
                    Debug.Trace("Fragment Shader compiled without complaint.");

                // Link the Shaders to a usable Program
                ProgramObject = GL.CreateProgram();
                GL.AttachShader(ProgramObject, VertexShaderObject);
                GL.AttachShader(ProgramObject, FragmentShaderObject);

                // link it all together
                GL.LinkProgram(ProgramObject);

                err = GL.GetError();
                if (err != ErrorCode.NoError)
                    Debug.Trace("LinkProgram: " + err);

                GL.UseProgram(ProgramObject);

                err = GL.GetError();
                if (err != ErrorCode.NoError)
                    Debug.Trace("UseProgram: " + err);

                // flag ShaderObjects for delete when not used anymore
                GL.DeleteShader(VertexShaderObject);
                GL.DeleteShader(FragmentShaderObject);

                int temp;
                GL.GetProgram(ProgramObject, ProgramParameter.LinkStatus, out temp);
                Debug.Trace("Linking Program (" + ProgramObject + ") " + ((temp == 1) ? "succeeded." : "FAILED!"));
                if (temp != 1)
                {
                    GL.GetProgramInfoLog(ProgramObject, out LogInfo);
                    Debug.Trace("Program Log:\n" + LogInfo);
                }

                Debug.Trace("End of Shader build. GL Error: " + GL.GetError());

                GL.UseProgram(0);
            }

            public bool SetUniform(float val, string varname = null, int index = -1)
            {
                if (!Core.useGLSL) { return false; }
                if ((varname == null) && (index == -1)) { return false; }

                int loc;
                if (varname != null)
                {
                    loc = GL.GetUniformLocation(ProgramObject, varname);
                }
                else
                {
                    loc = index;
                }

                GL.Uniform1(loc, val);
                
                return true;
            }

            public bool SetUniform(Vector2 val, string varname = null, int index = -1)
            {
                if (!Core.useGLSL) { return false; }
                if ((varname == null) && (index == -1)) { return false; }

                int loc;
                if (varname != null)
                {
                    loc = GL.GetUniformLocation(ProgramObject, varname);
                }
                else
                {
                    loc = index;
                }

                GL.Uniform2(loc, val);

                return true;
            }

            public bool SetUniform(Vector3 val, string varname = null, int index = -1)
            {
                if (!Core.useGLSL) { return false; }
                if ((varname == null) && (index == -1)) { return false; }

                int loc;
                if (varname != null)
                {
                    loc = GL.GetUniformLocation(ProgramObject, varname);
                }
                else
                {
                    loc = index;
                }

                GL.Uniform3(loc, val);

                return true;
            }

            public bool SetUniform(Vector4 val, string varname = null, int index = -1)
            {
                if (!Core.useGLSL) { return false; }
                if ((varname == null) && (index == -1)) { return false; }

                int loc;
                if (varname != null)
                {
                    loc = GL.GetUniformLocation(ProgramObject, varname);
                }
                else
                {
                    loc = index;
                }

                GL.Uniform4(loc, val);

                return true;
            }
            
            public void Activate()
            {
                if (!Core.useGLSL) { return; }
                GL.UseProgram(this.ProgramObject);
                active = true;
            }

            public void Deactivate()
            {
                if (!Core.useGLSL) { return; }
                GL.UseProgram(0);
                active = false;
            }

            public static void Init()
            {
                Core.useGLSL = true;
            }
        }
    }
}
