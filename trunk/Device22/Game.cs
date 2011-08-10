using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Input;

namespace Device22
{
    sealed class Game : GameWindow
    {
        #region Singleton
        //static Game instance = null;
        //static readonly object padlock = new object();

        //public static Game Instance
        //{
        //    get
        //    {
        //        lock (padlock)
        //        {
        //            if (instance == null)
        //            {
        //                instance = new Game();
        //                return instance;
        //            }
        //            //return instance;
        //            return null;
        //        }
        //    }
        //}
        static Game instance = null;

        public static Game Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Game();
                    return instance;
                }
                return instance;
            }
        } 
        #endregion

        private static bool[] keys = new bool[256];                             // Array Used For The Keyboard Routine
        private static bool[] mouseKeys = new bool[8];
        private static bool[] keyPressed = new bool[8];
        private static bool fullscreen = false;                                 // Fullscreen Flag, Set To Fullscreen Mode By Default

        private Camera camera;
        public Camera Camera
        { get { return camera; } }

        private InputController input;
        private Frustum frustum;
        public static int height = 800;
        public static int width = 800;

        public int polyCount = 0;

        private int maxCubes = 1;
        private VBOCube[] cube;

        private Planet earth;

        public static TrueTypeFont font;

        public struct MouseFunctions
        {
            public static void setPosition(Point pos) { Cursor.Position = pos; }
            public static Point getPosition() { return Cursor.Position; }
            public static void hideCursor() { Cursor.Hide(); }
            public static void showCursor() { Cursor.Show(); }
        }

        public Game()
            : base(width, height, GraphicsMode.Default, "Device 22")
        {
            VSync = VSyncMode.On;
            Cursor.Hide();
        }

        protected override void OnUnload(EventArgs e)
        {
            // Unload all VBOs and EBOs
            if (earth != null) { earth.Destroy(); }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            string version = GL.GetString(StringName.Version);
            int major = (int)version[0];
            int minor = (int)version[2];
            if (major <= 1 && minor < 5)
            {
                System.Windows.Forms.MessageBox.Show("You need at least OpenGL 1.5 to run this example. Aborting.", "Extensions not supported",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                this.Exit();
            }

            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.ClearDepth(1.0d);
            GL.DepthFunc(DepthFunction.Less);
            GL.MatrixMode(MatrixMode.Projection);
                GL.ShadeModel(ShadingModel.Smooth);
            GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
                GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
                GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);
                GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.ColorMaterial);
            GL.FrontFace(FrontFaceDirection.Cw);
            GL.Enable(EnableCap.CullFace);
            //GL.Enable(EnableCap.Lighting);

            Debug.On = false;

            camera = new Camera(0.1f, 2000.0f);
            camera.Position(0, 0, 1,   0, 0, 0,   0, 1, 0);

            GL.Enable(EnableCap.Light0);
            GL.Light(LightName.Light0, LightParameter.Position, new Vector4(new Vector3(0, 0, 0)));
            GL.Light(LightName.Light0, LightParameter.Ambient, new Color4(255, 255, 255, 0));
            GL.Light(LightName.Light0, LightParameter.Diffuse, new Color4(255, 100, 0, 0));

            MouseFunctions.setPosition(new Point(Game.width >> 1, Game.height >> 1));
            Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(mouseDownEvents);
            //Mouse.Move += new EventHandler<MouseMoveEventArgs>(Primitive.MouseMoveEvent);             // Object picking

            font = new TrueTypeFont(new Font("Times New Roman", 20f, FontStyle.Bold), true);
            font.color = new Vector3(1.0f, 0.0f, 1.0f);

            if (!loadContent()) { Core.ErrorExit("Error loading content"); }
        }

        private bool loadContent()
        {
            //Core.Shader.Init();

            frustum = new Frustum();

            cube = new VBOCube[10];
            for (int i = 0; i < maxCubes; i++)
            {
                cube[i] = new VBOCube(i);
                cube[i].setColor(new ColorRGBA(180, 20, 20, 0));
                cube[i].setPosition(new Vector3((float)Math2.randomNumber(0, 25), 0, (float)Math2.randomNumber(0, 25)));
            }
            cube[0].setPosition(Vector3.Zero);

            earth = new Planet();
            
            return true;
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)(Width / Height), camera.near, camera.far);
            GL.LoadMatrix(ref projection);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            base.OnResize(e);
        }

        private void mouseDownEvents(object sender, MouseButtonEventArgs m)
        {
            if (m.Button == MouseButton.Right)
            {
                keyPressed[(int)MouseButton.Right] = true;
            }
            if (m.Button == MouseButton.Left)
            {
                keyPressed[(int)MouseButton.Left] = true;
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            float speed = camera.Speed * 2.0f;

            if (Keyboard[Key.Escape])
                Exit();

            if (Keyboard[Key.W])
                camera.MoveView(speed);

            if (Keyboard[Key.S])
                camera.MoveView(-speed);

            if (Keyboard[Key.A])
                camera.StrafeView(-speed);

            if (Keyboard[Key.D])
                camera.StrafeView(speed);

            if (Keyboard[Key.P])
                Core.ChangeWireframeMode();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            
            //Debug.Trace("update");

            // reset polygon count
            polyCount = 0;

            GL.Color3(255, 255, 255);
            //GL.ClearColor(0.2f, 0.2f, 0.8f, 0.0f);
            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            //GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.MatrixMode(MatrixMode.Modelview);

            camera.Update();
            GL.LoadIdentity();
            camera.Look();
            frustum.CalculateFrustum();

            for (int i = 0; i < maxCubes; i++)
            {
                //cube[i].render(ref frustum);
            }
            
            earth.Render(ref frustum);

            // font.drawString(5, 1.5f, "Hello World\nI am glad to meet you.", .01f, .01f, TrueTypeFont.ALIGN_LEFT);

            Title = String.Format("FPS={0:0}", Core.Fps.Get(e.Time)) + String.Format(" Polygons rendered={0:0}", polyCount);

            //GL.Flush();
            SwapBuffers();
        }
        
        [STAThread]
        static void Main()
        {
            using (Game game = Game.Instance)
            {
                // If game == null?? if game != null eher oder?
                if (game == null) { Core.ErrorExit("Only one instance of this Game can be started at the same time. Get some real cake, not just a lie!"); }      // Welches Exit() reicht?
                game.Run();
            }
        }
    }
}