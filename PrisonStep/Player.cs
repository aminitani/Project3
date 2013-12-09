using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace PrisonStep
{
    /// <summary>
    /// This class describes our player in the game. 
    /// </summary>
    public class Player
    {
        #region Fields

        const int MAXHEALTH = 100;

        private float moveSpeed = 1000.0f;
        private float panRate = 4;

        private Camera camera;
        public Camera Camera { get { return camera; } }

        /// <summary>
        /// Game that uses this player
        /// </summary>
        private PrisonGame game;

        /// <summary>
        /// Player location in the prison. Only x/z are important. y still stay zero
        /// unless we add some flying or jumping behavior later on.
        /// </summary>
        private Vector3 location = new Vector3(0, 0, 0);
        public Vector3 Location { get { return location; } set { location = value; } }

        private Vector3 momentum = Vector3.Zero;
        //update calculates this only for the sake of momentum
        private Vector3 velocity = Vector3.Zero;
        private Vector3 lastPosition = Vector3.Zero;
        //this one holds the vertical velocity while jumping
        private float verticalocity = 0.0f;

        private int headex;
        private int arm2Index;
        private int eyendex;

        private float eyeRot = 0.0f;
        private float armRot = 0.0f;
        private float headRot = 0.0f;

        private int deaths = 0;
        public int Deaths { get { return deaths; } set { deaths = value; } }
        private int kills = 0;
        public int Kills { get { return kills; } set { kills = value; } }

        public enum Colors { Red, Green, Blue }

        private Colors colorState = Colors.Red;
        public Colors ColorState { get { return colorState; } }

        /// <summary>
        /// The player orientation as a simple angle
        /// </summary>
        private float horizontalOrientation = (float)Math.PI / 2;
        private float verticalOrientation = 0.0f;
        private float verMaxRot = (float)Math.PI / 4.0f;
        private float verMinRot = -(float)Math.PI / 4.0f;

        /// <summary>
        /// The player transformation matrix. Places the player where they need to be.
        /// </summary>
        private Matrix transform;
        public Matrix Transform { get { return transform; } }

        private enum States { Normal, Jumped, Died }
        private States state = States.Normal;

        /// <summary>
        /// Our animated model
        /// </summary>
        private AnimatedModel dalek;

        private string playerRegion;

        /// <summary>
        /// the player's score
        /// </summary>
        private int score = 0;
        public int Score { get { return score; } set { score = value; } }

        private LaserFire laserFire;

        private float laserDelay = 0.0f;
        private float exterminateDelay = 0.0f;

        private int health = MAXHEALTH;
        public int Health { get { return health; } }

        private PlayerColorSparkle colorSparkle;

        /// <summary>
        /// The collision cylinder for the player
        /// </summary>
        private BoundingCylinder playerCollision;
        public BoundingCylinder PlayerCollision { get { return playerCollision; } }



        #endregion


        public Player(PrisonGame game, Camera inCamera)
        {
            this.game = game;
            this.camera = inCamera;
            dalek = new AnimatedModel(game, "dalek");

            this.colorSparkle = new PlayerColorSparkle(this, game);

            playerCollision = new BoundingCylinder(game, location);
            laserFire = new LaserFire(game, this);

            SetPlayerTransform();
        }

        public void Initialize()
        {
            lastPosition = location;
        }

        /// <summary>
        /// Set the value of transform to match the current location
        /// and orientation.
        /// </summary>
        private void SetPlayerTransform()
        {
            transform = Matrix.CreateRotationX(verticalOrientation) * Matrix.CreateRotationY(horizontalOrientation);
            transform.Translation = location;
        }


        public void LoadContent(ContentManager content)
        {
            dalek.LoadContent(content);

            headex = dalek.Model.Bones.IndexOf(dalek.Model.Bones["Head"]);
            arm2Index = dalek.Model.Bones.IndexOf(dalek.Model.Bones["Arm2"]);
            eyendex = dalek.Model.Bones.IndexOf(dalek.Model.Bones["Eye"]);

            laserFire.LoadContent(content);
        }

        public string TestRegion(Vector3 v3)
        {
            // Convert to a 2D Point
            float x = v3.X;
            float y = v3.Z;

            return "";
        }

        public void Update(GameTime gameTime)
        {
            double deltaTotal = gameTime.ElapsedGameTime.TotalSeconds;

            velocity = (location - lastPosition) / (float)deltaTotal;
            velocity.Y = 0;

            switch (state)
            {
                case States.Normal:
                    break;

                case States.Jumped:
                    Vector3 newlocation = location + momentum * (float)deltaTotal;
                    if ((newlocation - Vector3.Zero).Length() < 7000.0f)
                        location = newlocation;

                    location += new Vector3(0, verticalocity * (float)deltaTotal, 0);
                    if (location.Y <= 0)
                    {
                        location.Y = 0;
                        state = States.Normal;
                    }
                    else
                        verticalocity -= 981 * (float)deltaTotal;
                    break;

                case States.Died:
                    break;
            }

            laserFire.Update(gameTime);

            colorSparkle.Update(gameTime);

            if (laserDelay < 0.0f)
            {
                laserDelay = 0.0f;
            }
            else if (laserDelay > 0.0f)
            {
                laserDelay -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (exterminateDelay < 0.0f)
            {
                exterminateDelay = 0.0f;
            }
            else if (exterminateDelay > 0.0f)
            {
                exterminateDelay -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            do
            {
                double delta = deltaTotal;

                dalek.Update(delta);

                dalek.BoneTransforms[eyendex] = Matrix.CreateRotationX(verticalOrientation) * dalek.BindTransforms[eyendex];
                dalek.BoneTransforms[arm2Index] = Matrix.CreateRotationX(verticalOrientation) * dalek.BindTransforms[arm2Index];
                dalek.ComputeAbsoluteTransforms();

                bool collision = false;     // Until we know otherwise

                //string region = TestRegion(newLocation);
                string region = "lol";
                playerRegion = region;

                if (region == "")
                {
                    // If not in a region, we have stepped out of bounds
                    collision = true;
                }

                if (!collision)
                {
                    //location = newLocation;
                }

                SetPlayerTransform();

                //bool collisionCamera = false;


                //camera.Center = location + new Vector3(0, 100, 0);
                //Vector3 newCameraLocation = location + new Vector3(300, 100, 0);
                //camera.Eye = newCameraLocation;
                camera.Center = location + new Vector3(0, 90, 0) + 1000 * Facing();
                Vector3 newCameraLocation = location + new Vector3(0, 120, 0) +
                    -70.0f * Right() - 200.0f * Facing();
                camera.Eye = newCameraLocation;

                //string regionCamera = TestRegion(newCameraLocation);

                /*if (regionCamera == "")
                {
                    // If not in a region, we have stepped out of bounds
                    collisionCamera = true;
                }*/

                /*if (!collisionCamera)
                {
                    game.Camera.Eye = newCameraLocation;
                }
                else
                {
                    int cameraDistance = 0;
                    regionCamera = "playerLoc";
                    while (regionCamera != "")
                    {
                        newCameraLocation = Vector3.Transform(new Vector3(0, 180, cameraDistance), transform);
                        cameraDistance -= 1;
                        regionCamera = TestRegion(newCameraLocation);
                    }
                    game.Camera.Eye = newCameraLocation;
                }*/
                deltaTotal -= delta;
            } while (deltaTotal > 0);


            //do other keyboard based actions

            playerCollision.Update(gameTime, location);

            lastPosition = location;
        }


        public void AttempMovement(float horizontal, float vertical, double deltaTime)
        {
            Vector3 newlocation;
            if(state == States.Jumped)
                newlocation = location + (vertical * FacingWithoutY() + horizontal * Right()) * moveSpeed/3 * (float)deltaTime;
            else
                newlocation = location + (vertical * FacingWithoutY() + horizontal * Right()) * moveSpeed * (float)deltaTime;
            if( (newlocation - Vector3.Zero).Length() < 7000.0f)
                location = newlocation;
        }


        public void AttemptRotation(float horizontal, float vertical, double deltaTime)
        {
            horizontalOrientation += -horizontal * panRate * (float)deltaTime;

            float newVertOrientation = verticalOrientation + -vertical * panRate * (float)deltaTime;
            if (newVertOrientation < verMaxRot && newVertOrientation > verMinRot)
                verticalOrientation = newVertOrientation;
        }


        public void AttemptShoot()
        {
            //can't fire more than 4/sec, wait for delay
            if (laserDelay > 0.0f)
                return;

            Matrix tempransform = Matrix.CreateRotationX(verticalOrientation) * Matrix.CreateRotationY(horizontalOrientation);
            //0 added speed for now, won't make a big difference
            laserFire.FireLaser(location + Vector3.Transform(dalek.AbsoTransforms[arm2Index].Translation, Matrix.CreateRotationY(horizontalOrientation)) + 100 * Facing(), tempransform, 0);
            game.SoundBank.PlayCue("tx0_fire1");

            //only fire one per quarter second
            laserDelay = 0.25f;
        }

        public void AttemptJump()
        {
            if (state != States.Normal)
                return;

            momentum = velocity;
            verticalocity = 500.0f;
            state = States.Jumped;
        }

        public void AttempToYellExterminate()
        {
            if (exterminateDelay > 0.0f)
                return;

            game.SoundBank.PlayCue("exterminate");

            exterminateDelay = 2.0f;
        }


        /// <summary>
        /// This function is called to draw the player.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="gameTime"></param>
        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime, Camera inCamera)
        {
            Matrix tempransform = Matrix.CreateRotationY(horizontalOrientation);
            tempransform.Translation = location;

            dalek.Draw(graphics, gameTime, tempransform, inCamera.View, inCamera.Projection);

            laserFire.Draw(graphics, gameTime, inCamera);
        }

        public Vector3 Facing()
        {
            //Vector3 ret = new Vector3((float)Math.Sin(horizontalOrientation), 0.0f, (float)Math.Cos(horizontalOrientation));
            Vector3 ret = transform.Backward;
            ret.Normalize();
            return ret;
        }

        public Vector3 FacingWithoutY()
        {
            Vector3 ret = new Vector3((float)Math.Sin(horizontalOrientation), 0.0f, (float)Math.Cos(horizontalOrientation));
            ret.Normalize();
            return ret;
        }

        public Vector3 Right()
        {
            Vector3 ret = transform.Left;
            //already normal
            ret.Normalize();
            return ret;
        }

        public void ChangeColor(Player.Colors inColor)
        {
            colorState = inColor;
            laserFire.ColorState = inColor;
        }

        private void Die()
        {
            deaths += 1;
        }

        public void IncrementHealth(int healthInc)
        {
            health += healthInc;
            if (health <= 0)
            {
                health = 0;
                Die();
            }
            else if (health >= 100)
            {
                health = 100;
            }
        }

        public void HitByBlast(Colors inColor)
        {
            switch (inColor)
            {
                case Colors.Red:
                    if (colorState == Colors.Blue)
                    {
                        IncrementHealth(-5);
                    }
                    else if (colorState == Colors.Green)
                    {
                        IncrementHealth(-20);
                    }
                    else
                    {
                        IncrementHealth(-10);
                    }
                    break;
                case Colors.Blue:
                    if (colorState == Colors.Blue)
                    {
                        IncrementHealth(-10);
                    }
                    else if (colorState == Colors.Green)
                    {
                        IncrementHealth(-5);
                    }
                    else
                    {
                        IncrementHealth(-20);
                    }
                    break;
                case Colors.Green:
                    if (colorState == Colors.Blue)
                    {
                        IncrementHealth(-20);
                    }
                    else if (colorState == Colors.Green)
                    {
                        IncrementHealth(-10);
                    }
                    else
                    {
                        IncrementHealth(-5);
                    }
                    break;
            }
        }
    }
}
