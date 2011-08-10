using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Device22
{
    class InputController
    {
        private Camera camera;

        public InputController() { }

        public void setCamera(Camera camera)
        {
            this.camera = camera;
        }

        public Camera getCamera()
        {
            return this.camera;
        }

        public bool update(bool[] keys)
        {
            try
            {
                this.keyboardInput(keys);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Input error: " + ex.ToString());
                return false;
            }
        }

        public void keyboardInput(bool[] keys)
        {
            if (keys[(int)Keys.W])
            {
                //camera.moveView(camera.getMoveSpeed());
            }
            if (keys[(int)Keys.A])
            {
                //camera.strafeView(-camera.getMoveSpeed());
            }
            if (keys[(int)Keys.S])
            {
                //camera.moveView(-camera.getMoveSpeed());
            }
            if (keys[(int)Keys.D])
            {
                //camera.strafeView(camera.getMoveSpeed());
            }
        }
    }
}
