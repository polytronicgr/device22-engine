using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

namespace Device22
{
    public class Debug
    {
        public static bool On = true;

        public static void Trace(string info)
        {
            if (!On) return;
            Debug.Trace(info);
        }

        public class Warning
        {
            public static void Obsolete()
            {
                Debug.Trace("Warning: Function is obsolete!");
            }
        }

        public class BoundingBox
        {
            public static void Render(Vector3[] bb)
            {
                GL.Disable(EnableCap.Lighting);
                GL.Color4(1.0, 0.0, 1.0, 1.0);
                GL.Begin(BeginMode.Lines);
                    GL.Vertex3(bb[0].X, bb[0].Y, bb[0].Z);
                    GL.Vertex3(bb[1].X, bb[1].Y, bb[1].Z);
                    GL.Vertex3(bb[0].X, bb[0].Y, bb[0].Z);
                    GL.Vertex3(bb[2].X, bb[2].Y, bb[2].Z);
                    GL.Vertex3(bb[1].X, bb[1].Y, bb[1].Z);
                    GL.Vertex3(bb[3].X, bb[3].Y, bb[3].Z);
                    GL.Vertex3(bb[2].X, bb[2].Y, bb[2].Z);
                    GL.Vertex3(bb[3].X, bb[3].Y, bb[3].Z);
                    GL.Vertex3(bb[0].X, bb[0].Y, bb[0].Z);
                    GL.Vertex3(bb[4].X, bb[4].Y, bb[4].Z);
                    GL.Vertex3(bb[1].X, bb[1].Y, bb[1].Z);
                    GL.Vertex3(bb[5].X, bb[5].Y, bb[5].Z);
                    GL.Vertex3(bb[2].X, bb[2].Y, bb[2].Z);
                    GL.Vertex3(bb[6].X, bb[6].Y, bb[6].Z);
                    GL.Vertex3(bb[3].X, bb[3].Y, bb[3].Z);
                    GL.Vertex3(bb[7].X, bb[7].Y, bb[7].Z);
                    GL.Vertex3(bb[4].X, bb[4].Y, bb[4].Z);
                    GL.Vertex3(bb[5].X, bb[5].Y, bb[5].Z);
                    GL.Vertex3(bb[4].X, bb[4].Y, bb[4].Z);
                    GL.Vertex3(bb[6].X, bb[6].Y, bb[6].Z);
                    GL.Vertex3(bb[5].X, bb[5].Y, bb[5].Z);
                    GL.Vertex3(bb[7].X, bb[7].Y, bb[7].Z);
                    GL.Vertex3(bb[6].X, bb[6].Y, bb[6].Z);
                    GL.Vertex3(bb[7].X, bb[7].Y, bb[7].Z);
                GL.End();
                GL.Color4(1.0, 1.0, 1.0, 1.0);
                GL.Enable(EnableCap.Lighting);
            }
        }
    }
}
