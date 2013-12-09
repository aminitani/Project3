using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PrisonStep
{
    public class Ground
    {
        /// <summary>
        /// Current position
        /// </summary>
        private Vector3 position = Vector3.Zero;
        private PrisonGame game;
        private int scale = 8;
        private Model model;

        public Vector3 Position { get { return position; } set { position = value; } }
        public int Scale { get { return scale; } set { scale = value; } }

        public Ground(PrisonGame inGame)
        {
            game = inGame;
        }

        public void Initialize()
        {
        }

        public void Update(GameTime gameTime) { }

        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime, Camera inCamera)
        {
            float width = 2290;
            float height = 2290;
            for (int i = 0; i < scale; i++)
            {
                for (int j = 0; j < scale; j++)
                {
                    DrawModel(graphics, model, Matrix.CreateTranslation(position - new Vector3(0.5f * (scale - 1) * width, 0, (0.5f * scale - 0.5f) * height) + new Vector3(i * width, 0, j * height)), gameTime, inCamera);
                }
            }
            //DrawModel(graphics, model, Matrix.CreateTranslation(position), gameTime, inCamera);
            //DrawModel(graphics, model, Matrix.CreateTranslation(position + new Vector3(2290, 0, 0)), gameTime, inCamera);
            //DrawModel(graphics, model, Matrix.CreateTranslation(position + new Vector3(-2290, 0, 0)), gameTime, inCamera);
            //DrawModel(graphics, model, Matrix.CreateTranslation(position + new Vector3(0, 0, 2290)), gameTime, inCamera);
            //DrawModel(graphics, model, Matrix.CreateTranslation(position + new Vector3(0, 0, -2290)), gameTime, inCamera);
            //DrawModel(graphics, model, Matrix.CreateTranslation(position + new Vector3(2290, 0, -2290)), gameTime, inCamera);
            //DrawModel(graphics, model, Matrix.CreateTranslation(position + new Vector3(-2290, 0, -2290)), gameTime, inCamera);
            //DrawModel(graphics, model, Matrix.CreateTranslation(position + new Vector3(2290, 0, 2290)), gameTime, inCamera);
            //DrawModel(graphics, model, Matrix.CreateTranslation(position + new Vector3(-2290, 0, 2290)), gameTime, inCamera);
        }

        private void DrawModel(GraphicsDeviceManager graphics, Model model, Matrix world, GameTime gameTime, Camera inCamera)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = transforms[mesh.ParentBone.Index] * world;
                    effect.View = inCamera.View;
                    effect.Projection = inCamera.Projection;
                }
                mesh.Draw();
            }

        }

        public void LoadContent(ContentManager content)
        {
            model = content.Load<Model>("ground");
        }


    }
}