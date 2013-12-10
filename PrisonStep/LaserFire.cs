using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PrisonStep
{
    class LaserFire
    {
        public class LaserBlast
        {
            public Vector3 position;
            public Matrix orientation;
            public float speed;
            public float life;
            public Player.Colors color;
            public Player owner;
        }

        private PrisonGame game;

        private Player owner;

        private Model model;

        private LinkedList<LaserBlast> laserBlasts = new LinkedList<LaserBlast>();

        //private Player.Colors colorState = Player.Colors.Blue;
        //public Player.Colors ColorState { get { return colorState; } set { colorState = value; } }

        public LinkedList<LaserBlast> LaserBlasts { get { return laserBlasts; } }

        private int updateCount = 0;

        public LaserFire(PrisonGame game, Player inPlayer)
        {
            this.game = game;
            owner = inPlayer;
        }

        /// <summary>
        /// This function is called to load content into this component
        /// of our game.
        /// </summary>
        /// <param name="content">The content manager to load from.</param>
        public void LoadContent(ContentManager content)
        {
            model = content.Load<Model>("LaserBlast");
        }

        /// <summary>
        /// This function is called to update this component of our game
        /// to the current game time.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            for (LinkedListNode<LaserBlast> blastNode = LaserBlasts.First; blastNode != null; )
            {
                //blastNode is the current node
                //set nextBlast to the next node in the list
                LinkedListNode<LaserBlast> nextBlast = blastNode.Next;

                //This is the actual blast object we are working on
                LaserBlast blast = blastNode.Value;

                //Update position
                Vector3 direction = Vector3.TransformNormal(new Vector3(0, 0, 1), blast.orientation);
                blast.position += direction * blast.speed * delta;

                if ( updateCount > 10)
                {
                    switch (blast.color)
                    {
                        case (Player.Colors.Red):
                            game.RedParticleSystem.AddParticles(blast.position);
                            break;
                        case (Player.Colors.Green):
                            game.GreenParticleSystem.AddParticles(blast.position);
                            break;
                        case (Player.Colors.Blue):
                            game.BlueParticleSystem.AddParticles(blast.position);
                            break;
                    }
                    updateCount = 0;
                }

                BoundingSphere bs = new BoundingSphere();
                bs.Center = blast.position;
                bs.Radius = 5;

                for (int i = 0; i < game.PlayerPackages.Count; i++)
                {
                    bool collideWithPlayer = game.PlayerPackages[i].Player.PlayerCollision.TestForCollision(bs);
                    if (collideWithPlayer)
                    {
                        if (game.PlayerPackages[i].Player != blast.owner)
                        {
                            game.PlayerPackages[i].Player.HitByBlast(blast.color, blast.owner);
                            laserBlasts.Remove(blastNode);
                        }
                    }
                }

                //Decrease life of blast
                blast.life -= delta;
                if (blast.life <= 0)
                {
                    //When done, remove from the list
                    laserBlasts.Remove(blastNode);
                }

                blastNode = nextBlast;
                updateCount++;
            }
        }


        /// <summary>
        /// This function is called to draw this game component.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="gameTime"></param>
        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime, Camera inCamera)
        {
            foreach (LaserBlast blast in laserBlasts)
            {
                DrawModel(graphics, model, blast.orientation * Matrix.CreateTranslation(blast.position), inCamera);
            }
        }


        private void DrawModel(GraphicsDeviceManager graphics, Model model, Matrix world, Camera inCamera)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] * world;
                    effect.View = inCamera.View;
                    effect.Projection = inCamera.Projection;
                }
                mesh.Draw();
            }
        }

        public void FireLaser(Vector3 position, Matrix orientation, float speed)
        {
            LaserBlast blast = new LaserBlast();
            blast.position = position;
            blast.orientation = orientation;
            blast.speed = 3000.0f + speed;      // cm/sec
            blast.life = 2.0f;          // 2 seconds
            blast.color = owner.ColorState;
            blast.owner = owner;

            laserBlasts.AddLast(blast);
        }

    
    }
}
