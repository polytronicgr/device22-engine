using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Device22
{
    class Particle
    {
        private Vector3 position;
        private int color;
        private float size;
        private float life;
        private float angle;
        private Vector3 velocity;
        private Texture texture;

        public Particle()
        {
            color = ColorFunctions.ARGB((byte)255, (byte)255, (byte)255, (byte)255);
            size = 0.0f;
            life = 0.0f;
            angle = 0.0f;
        }

        public void init(Vector3 pos, Vector3 vel, float lifespan, float size, float angle, int color, string texName)
        {
            this.position = pos;
            this.velocity = vel;

            if (lifespan <= 0.0f)
            {
                Console.WriteLine("Class Particle: No Lifespan < 0 is permitted");
                return;
            }

            this.life = lifespan;
            this.size = size;
            this.angle = angle;
            this.color = color;

            if(texName != null)
            {
                this.texture = new Texture();
                Texture.loadFromFile(texName);
            }
        }

        public bool isAlive() { return (this.life > 0.0f); }

        public void update(float dt)
        {
            if (this.isAlive() == false)
            {
                this.life = (float)Math2.randomNumber(0.0d, 2.0d);
                this.position = new Vector3(0f, 0f, 0f);
                return;
            }

            //this.position.Add(this.velocity.X * dt, this.velocity.Y * dt, this.velocity.Z * dt);
            //this.position.Add(0f, -((float)(Earth.Gravity)) * dt, 0f);
            this.life -= dt;

            if (this.angle != 0.0f)
                this.texture.SetRotation(this.texture.GetRotationAngle() + (this.angle * dt));
        }

        public void render(float dt)
        {
            if (this.isAlive() == false)
                return;
            if (this.texture.GetID() == -9999)
                return;

            GL.DepthMask(false);

            byte red, green, blue;

            if (this.life < 1.0f)
            {
                red = (byte)(ColorFunctions.GetR(this.color) * this.life);
                green = (byte)(ColorFunctions.GetG(this.color) * this.life);
                blue = (byte)(ColorFunctions.GetB(this.color) * this.life);
            }
            else
            {
                red = (byte)(ColorFunctions.GetR(this.color));
                green = (byte)(ColorFunctions.GetG(this.color));
                blue = (byte)(ColorFunctions.GetB(this.color));
            }

            GL.Color3(red, green, blue);

            this.texture.Bind();

            GL.PushMatrix();
            GL.Translate(this.position.X, this.position.Y, this.position.Z);

            float halfSize = this.size * 0.5f;

            GL.Begin(BeginMode.Quads);
                GL.TexCoord2(0.0d, 1.0);
                GL.Vertex3(-halfSize, halfSize, 0.0f);
                GL.TexCoord2(0.0f, 0.0f);
                GL.Vertex3(-halfSize, -halfSize, 0.0f);
                GL.TexCoord2(1.0f, 0.0f);
                GL.Vertex3(-halfSize, -halfSize, 0.0f);
                GL.TexCoord2(1.0f, 1.0f);
                GL.Vertex3(halfSize, halfSize, 0.0f);
            GL.End();
            GL.PopMatrix();

            GL.DepthMask(true);
        }
    }
}