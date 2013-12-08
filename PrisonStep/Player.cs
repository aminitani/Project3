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

        private float exterminateDelay = 0.0f;

        private float moveSpeed = 100.0f;
        private float panRate = 2;

        private Camera camera;
        public Camera Camera { get { return camera; } }

        /// <summary>
        /// Game that uses this player
        /// </summary>
        private PrisonGame game;

        //
        // Player location information.  We keep a x/z location (y stays zero)
        // and an orientation (which way we are looking).
        //

        /// <summary>
        /// Player location in the prison. Only x/z are important. y still stay zero
        /// unless we add some flying or jumping behavior later on.
        /// </summary>
        private Vector3 location = new Vector3(0, 0, 0);
        public Vector3 Location { get { return location; } set { location = value; } }

        private int Head;
        private int Arm2;
        private int Eye;

        private float eyeRot = 0.0f;
        private float armRot = 0.0f;
        private float headRot = 0.0f;

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

        private enum States {Start}
        private States state = States.Start;

        /// <summary>
        /// Our animated model
        /// </summary>
        private AnimatedModel dalek;

        private string playerRegion;

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
            SetPlayerTransform();

            playerCollision = new BoundingCylinder(game, location);
        }

        public void Initialize()
        {
        }

        /// <summary>
        /// Set the value of transform to match the current location
        /// and orientation.
        /// </summary>
        private void SetPlayerTransform()
        {
            transform = Matrix.CreateRotationY(horizontalOrientation);
            transform.Translation = location;
        }


        public void LoadContent(ContentManager content)
        {
            dalek.LoadContent(content);

            Head = dalek.Model.Bones.IndexOf(dalek.Model.Bones["Head"]);
            Arm2 = dalek.Model.Bones.IndexOf(dalek.Model.Bones["Arm2"]);
            Eye = dalek.Model.Bones.IndexOf(dalek.Model.Bones["Eye"]);
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

            float strafe = 0;
            float newOrientation = horizontalOrientation;
            float deltaAngle = 0;
            float turnRate = 0;

            do
            {
                double delta = deltaTotal;

                dalek.Update(delta);

                //
                // Part 1:  Compute a new orientation
                //

                //Matrix deltaMatrix = dalek.DeltaMatrix;
                //deltaAngle = (float)Math.Atan2(deltaMatrix.Backward.X, deltaMatrix.Backward.Z);
                //newOrientation = horizontalOrientation + deltaAngle;

                //
                // Part 2:  Compute a new location
                //

                // We are likely rotated from the angle the model expects to be in
                // Determine that angle.
                //Matrix rootMatrix = dalek.RootMatrix;
                //float actualAngle = (float)Math.Atan2(rootMatrix.Backward.X, rootMatrix.Backward.Z);
                //Vector3 newLocation = location + Vector3.TransformNormal(dalek.DeltaPosition + new Vector3(strafe, 0, 0),
                //               Matrix.CreateRotationY(newOrientation - actualAngle));

                //
                // Update the orientation
                //

                //horizontalOrientation = newOrientation;

                //
                // Update the location
                //


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
        }


        public void RequestMovement(float horizontal, float vertical, double deltaTime)
        {
            Vector3 newlocation = location + (vertical * Facing() + horizontal * Right()) * moveSpeed * (float)deltaTime;
            location = newlocation;
        }


        public void RequestRotation(float horizontal, float vertical, double deltaTime)
        {
            horizontalOrientation += -horizontal * panRate * (float)deltaTime;
        }


        public void RequestShoot()
        {
        }


        /// <summary>
        /// This function is called to draw the player.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="gameTime"></param>
        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime, Camera inCamera)
        {
            Matrix transform = Matrix.CreateRotationY(horizontalOrientation);
            transform.Translation = location;

            dalek.Draw(graphics, gameTime, transform, inCamera.View, inCamera.Projection);

        }

        public Vector3 Facing()
        {
            Vector3 ret = new Vector3((float)Math.Sin(horizontalOrientation), 0.0f, (float)Math.Cos(horizontalOrientation));
            ret.Normalize();
            return ret;
        }

        public Vector3 Right()
        {
            Vector3 ret = Vector3.Cross(Facing(), new Vector3(0, 1, 0));
            //already normal
            ret.Normalize();
            return ret;
        }


    }
}
