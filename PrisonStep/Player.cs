using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace PrisonStep
{
    /// <summary>
    /// This class describes our player in the game. 
    /// </summary>
    public class Player
    {
        #region Fields

        /// <summary>
        /// This is a range from the door center that is considered
        /// to be under the door.  This much either side.
        /// </summary>
        private const float DoorUnderRange = 40;

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

        /// <summary>
        /// The player orientation as a simple angle
        /// </summary>
        private float orientation = 1.6f;

        /// <summary>
        /// The player transformation matrix. Places the player where they need to be.
        /// </summary>
        private Matrix transform;
        public Matrix Transform { get { return transform; } }

        /// <summary>
        /// The rotation rate in radians per second when player is rotating
        /// </summary>
        private float panRate = 2;

        /// <summary>
        /// The player move rate in centimeters per second
        /// </summary>
        private float moveRate = 500;

        /// <summary>
        /// Id for a door we are opening or 0 if none.
        /// </summary>
        private int openDoor = 0;

        /// <summary>
        /// Keeps track of the last game pad state
        /// </summary>
        GamePadState lastGPS;

        /// <summary>
        /// Tells if the player has chosen to wield the bazooka or not.
        /// </summary>
        private bool wieldBazooka = false;
        public bool WieldBazooka { get { return wieldBazooka; } }

        /// <summary>
        /// Tells if the player has chosen to crouch;
        /// </summary>
        private bool crouch = false;
        public bool Crouch { get { return crouch; } }

        /// <summary>
        /// Tells us if the bazooka is raised and we are aiming
        /// </summary>
        private bool aiming = false;
        public bool Aiming { get { return aiming; } }

        /// <summary>
        /// Stores the angle we are in when we start aiming
        /// </summary>
        private float aimOrient = 0;
        public float AimOrient { get { return aimOrient; } }

        /// <summary>
        /// Stores the angle we are aiming at;
        /// </summary>
        private float aimAngle = 0;
        public float AimAngle { get { return aimAngle; } }

        /// <summary>
        /// The previous keyboard state
        /// </summary>
        KeyboardState lastKeyboardState;

        private enum States { Start, StanceStart, Stance, Turn, TurnLoopStart, TurnLoop, WalkStart, WalkLoopStart, WalkLoop, 
            StanceRaised, CrouchBazooka, WalkStartBazooka, WalkLoopStartBazooka, WalkLoopBazooka, TurnBazooka, TurnLoopStartBazooka, TurnLoopBazooka, Aim }
        private States state = States.Start;

        /// <summary>
        /// Our animated model
        /// </summary>
        private AnimatedModel victoria;

        private AnimatedModel pies;

        //private AnimatedModel bazooka;

        private Dictionary<string, List<Vector2>> regions = new Dictionary<string, List<Vector2>>();
        public Dictionary<string, List<Vector2>> Regions { get { return regions; } }

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
            victoria = new AnimatedModel(game, "Victoria");

            victoria.AddAssetClip("dance", "Victoria-dance");
            victoria.AddAssetClip("stance", "Victoria-stance");
            victoria.AddAssetClip("walk", "Victoria-walk");
            victoria.AddAssetClip("walkstart", "Victoria-walkstart");
            victoria.AddAssetClip("walkloop", "Victoria-walkloop");
            victoria.AddAssetClip("leftturn", "Victoria-leftturn");
            victoria.AddAssetClip("rightturn", "Victoria-rightturn");

            victoria.AddAssetClip("crouchbazooka", "Victoria-crouchbazooka");
            victoria.AddAssetClip("lowerbazooka", "Victoria-lowerbazooka");
            victoria.AddAssetClip("raisebazooka", "Victoria-raisebazooka");
            victoria.AddAssetClip("walkloopbazooka", "Victoria-walkloopbazooka");
            victoria.AddAssetClip("walkstartbazooka", "Victoria-walkstartbazooka");
            SetPlayerTransform();

            playerCollision = new BoundingCylinder(game, location);
        }

        public void Initialize()
        {
            lastGPS = GamePad.GetState(PlayerIndex.One);
        }

        /// <summary>
        /// Set the value of transform to match the current location
        /// and orientation.
        /// </summary>
        private void SetPlayerTransform()
        {
            transform = Matrix.CreateRotationY(orientation);
            transform.Translation = location;
        }


        public void LoadContent(ContentManager content)
        {
            victoria.LoadContent(content);
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

            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            float speed = 0;
            float pan = 0;
            float strafe = 0;
            float newOrientation;
            float deltaAngle;

            do
            {
                double delta = deltaTotal;

                //
                // State machine
                //

                switch (state)
                {
                    case States.Start:
                        state = States.StanceStart;
                        delta = 0;
                        break;

                    case States.StanceStart:
                        victoria.PlayClip("stance");
                        location.Y = 0;
                        if (!wieldBazooka)
                        {
                            state = States.Stance;
                        }
                        else if (wieldBazooka)
                        {
                            victoria.PlayClip("raisebazooka");
                            state = States.StanceRaised;
                        }
                        break;

                    case States.Stance:
                        speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                        pan = GetDesiredTurnRate(ref keyboardState, ref gamePadState);
                        strafe = GetDesiredStrafe(ref keyboardState, ref gamePadState);

                        if (speed > 0)
                        {
                            // We need to leave the stance state and start walking
                            victoria.PlayClip("walkloop");
                            victoria.Player.Speed = speed;
                            state = States.WalkLoop;
                        }

                        if (pan != 0)
                        {
                            victoria.Player.Speed = pan;
                            state = States.TurnLoopStart;
                        }

                        break;

                    case States.TurnLoopStart:
                        if (pan > 0)
                        {
                            victoria.PlayClip("rightturn").Speed = GetDesiredTurnRate(ref keyboardState, ref gamePadState);
                            state = States.TurnLoop;
                            break;
                        }
                        else if (pan < 0)
                        {
                            victoria.PlayClip("leftturn").Speed = GetDesiredTurnRate(ref keyboardState, ref gamePadState);
                            state = States.TurnLoop;
                            break;
                        }
                        state = States.TurnLoop;

                        break;

                    case States.TurnLoop:
                        if (delta > victoria.Player.Clip.Duration - victoria.Player.Time)
                        {
                            delta = victoria.Player.Clip.Duration - victoria.Player.Time;

                            // The clip is done after this update
                            state = States.TurnLoopStart;
                        }

                        strafe = GetDesiredStrafe(ref keyboardState, ref gamePadState);
                        pan = GetDesiredTurnRate(ref keyboardState, ref gamePadState);
                        if (pan == 0)
                        {
                            delta = 0;
                            state = States.StanceStart;
                        }
                        else
                        {
                            victoria.Player.Speed = pan;
                        }
                        break;

                    case States.WalkStart:
                        if (delta > victoria.Player.Clip.Duration - victoria.Player.Time)
                        {
                            delta = victoria.Player.Clip.Duration - victoria.Player.Time;

                            // The clip is done after this update
                            state = States.WalkLoopStart;
                        }

                        strafe = GetDesiredStrafe(ref keyboardState, ref gamePadState);
                        speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);

                        if (speed == 0)
                        {
                            delta = 0;
                            state = States.StanceStart;
                        }
                        else
                        {
                            victoria.Player.Speed = speed;
                        }

                        break;

                    case States.WalkLoopStart:
                        victoria.PlayClip("walkloop").Speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                        state = States.WalkLoop;
                        break;

                    case States.WalkLoop:
                        location.Y = 0;
                        if (delta > victoria.Player.Clip.Duration - victoria.Player.Time)
                        {
                            delta = victoria.Player.Clip.Duration - victoria.Player.Time;

                            // The clip is done after this update
                            state = States.WalkLoopStart;
                        }

                        strafe = GetDesiredStrafe(ref keyboardState, ref gamePadState);
                        speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                        if (speed == 0)
                        {
                            delta = 0;
                            state = States.StanceStart;
                        }
                        else
                        {
                            victoria.Player.Speed = speed;
                        }

                        break;

                    case States.StanceRaised:
                        speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                        pan = GetDesiredTurnRate(ref keyboardState, ref gamePadState);
                        strafe = GetDesiredStrafe(ref keyboardState, ref gamePadState);

                        location.Y = 0;

                        if (speed > 0)
                        {
                            // We need to leave the stance state and start walking
                            victoria.PlayClip("lowerbazooka");
                            victoria.Player.Speed = speed;
                            state = States.WalkLoopBazooka;
                        }

                        if (pan != 0 && !aiming)
                        {
                            victoria.Player.Speed = pan;
                            victoria.PlayClip("lowerbazooka");
                            state = States.TurnLoopStartBazooka;
                        }

                        if(aiming)
                        {
                            pan = 0;
                            state = States.Aim;
                        }

                        if (!wieldBazooka)
                        {
                            victoria.PlayClip("lowerbazooka");
                            state = States.Stance;
                        }

                        if (crouch)
                        {
                            victoria.PlayClip("crouchbazooka");
                            state = States.CrouchBazooka;
                        }

                        break;

                    case States.Aim:
                        if (!keyboardState.IsKeyDown(Keys.LeftShift))
                        {
                            pan = 0;
                            state = States.StanceRaised;
                        }
                        else
                        {
                            pan = 0;
                            state = States.Aim;
                        }

                        break;

                    case States.CrouchBazooka:
                        if (delta > victoria.Player.Clip.Duration - victoria.Player.Time)
                        {
                            delta = victoria.Player.Clip.Duration - victoria.Player.Time;

                            // The clip is done after this update
                            crouch = false;
                            victoria.PlayClip("raisebazooka");
                            state = States.StanceRaised;
                        }

                        break;

                    case States.TurnLoopStartBazooka:
                        if (pan > 0)
                        {
                            victoria.PlayClip("rightturn").Speed = GetDesiredTurnRate(ref keyboardState, ref gamePadState);
                            state = States.TurnLoopBazooka;
                            break;
                        }
                        else if (pan < 0)
                        {
                            victoria.PlayClip("leftturn").Speed = GetDesiredTurnRate(ref keyboardState, ref gamePadState);
                            state = States.TurnLoopBazooka;
                            break;
                        }
                        state = States.TurnLoopBazooka;

                        break;

                    case States.TurnLoopBazooka:
                        if (delta > victoria.Player.Clip.Duration - victoria.Player.Time)
                        {
                            delta = victoria.Player.Clip.Duration - victoria.Player.Time;

                            // The clip is done after this update
                            state = States.TurnLoopStartBazooka;
                        }

                        strafe = GetDesiredStrafe(ref keyboardState, ref gamePadState);
                        pan = GetDesiredTurnRate(ref keyboardState, ref gamePadState);
                        if (pan == 0)
                        {
                            delta = 0;
                            state = States.StanceStart;
                        }
                        else
                        {
                            victoria.Player.Speed = pan;
                        }
                        break;

                    case States.WalkStartBazooka:
                        if (delta > victoria.Player.Clip.Duration - victoria.Player.Time)
                        {
                            delta = victoria.Player.Clip.Duration - victoria.Player.Time;

                            // The clip is done after this update
                            victoria.PlayClip("walkstartbazooka");
                            state = States.WalkLoopStartBazooka;
                        }

                        speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                        strafe = GetDesiredStrafe(ref keyboardState, ref gamePadState);

                        if (speed == 0)
                        {
                            delta = 0;
                            state = States.StanceStart;
                        }
                        else
                        {
                            victoria.Player.Speed = speed;
                        }

                        break;

                    case States.WalkLoopStartBazooka:
                        victoria.PlayClip("walkloopbazooka").Speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                        state = States.WalkLoopBazooka;
                        break;

                    case States.WalkLoopBazooka:
                        location.Y = 0;
                        if (delta > victoria.Player.Clip.Duration - victoria.Player.Time)
                        {
                            delta = victoria.Player.Clip.Duration - victoria.Player.Time;

                            // The clip is done after this update
                            state = States.WalkLoopStartBazooka;
                        }

                        strafe = GetDesiredStrafe(ref keyboardState, ref gamePadState);
                        speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                        if (speed == 0)
                        {
                            delta = 0;
                            state = States.StanceStart;
                        }
                        else
                        {
                            victoria.Player.Speed = speed;
                        }

                        break;
                }

                // 
                // State update
                //

                if (!aiming)
                {
                    orientation += GetDesiredTurnRate(ref keyboardState, ref gamePadState) * (float)delta;
                }
                else
                {
                    aimAngle += GetDesiredTurnRate(ref keyboardState, ref gamePadState) * (float)delta;
                }

                victoria.Update(delta);

                //
                // Part 1:  Compute a new orientation
                //

                Matrix deltaMatrix = victoria.DeltaMatrix;
                deltaAngle = (float)Math.Atan2(deltaMatrix.Backward.X, deltaMatrix.Backward.Z);
                newOrientation = orientation + deltaAngle;

                if (keyboardState.IsKeyDown(Keys.LeftShift) && lastKeyboardState.IsKeyDown(Keys.LeftShift) && (state == States.Aim || state == States.StanceRaised))
                {
                    if (!aiming)
                    {
                        aimOrient = orientation;
                        aimAngle = orientation;
                    }
                    aiming = true;

                    if (aimOrient - aimAngle > 1.222f)
                    {
                        aimAngle = aimOrient - 1.222f;
                    }

                    if (aimOrient - aimAngle < -1.222f)
                    {
                        aimAngle = aimOrient + 1.222f;
                    }

                }
                else
                {
                    aiming = false;
                    int spineInd = victoria.Model.Bones["Bip01 Spine1"].Index;
                    victoria.AbsoTransforms[spineInd].Backward = new Vector3(1, 0, 0);
                }

                //
                // Part 2:  Compute a new location
                //

                // We are likely rotated from the angle the model expects to be in
                // Determine that angle.
                Matrix rootMatrix = victoria.RootMatrix;
                float actualAngle = (float)Math.Atan2(rootMatrix.Backward.X, rootMatrix.Backward.Z);
                Vector3 newLocation = location + Vector3.TransformNormal(victoria.DeltaPosition + new Vector3(strafe, 0, 0),
                               Matrix.CreateRotationY(newOrientation - actualAngle));

                //
                // Update the orientation
                //

                orientation = newOrientation;

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
                    location = newLocation;
                }

                SetPlayerTransform();

                bool collisionCamera = false;
                camera.Center = location + new Vector3(0,100,0);
                Vector3 newCameraLocation = location + new Vector3(300, 100, 0);
                camera.Eye = newCameraLocation;
                string regionCamera = TestRegion(newCameraLocation);

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

            if (keyboardState.IsKeyDown(Keys.D1) && lastKeyboardState.IsKeyUp(Keys.D1))
            {
                if (wieldBazooka)
                {
                    wieldBazooka = false;
                }
                else if (!wieldBazooka)
                {
                    wieldBazooka = true;
                }
            }

            if (keyboardState.IsKeyDown(Keys.LeftControl) && lastKeyboardState.IsKeyUp(Keys.LeftControl) && wieldBazooka)
            {
                if (crouch)
                {
                    crouch = false;
                }
                else if (!crouch)
                {
                    crouch = true;
                }
            }

            if (keyboardState.IsKeyDown(Keys.A))
            {
                //transform *= Matrix.CreateTranslation(new Vector3(
            }

            playerCollision.Update(gameTime, location);

            lastKeyboardState = keyboardState;
        }


        /// <summary>
        /// This function is called to draw the player.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="gameTime"></param>
        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime, Camera inCamera)
        {
            Matrix transform = Matrix.CreateRotationY(orientation);
            transform.Translation = location;

            if (wieldBazooka)
            {
                int handInd;
                Matrix boneMat;

                handInd = victoria.Model.Bones["Bip01 R Hand"].Index;

                boneMat = victoria.AbsoTransforms[handInd] * transform;

                Matrix bazTransform =
                    Matrix.CreateRotationX(MathHelper.ToRadians(109.5f)) *
                    Matrix.CreateRotationY(MathHelper.ToRadians(9.7f)) *
                    Matrix.CreateRotationZ(MathHelper.ToRadians(72.9f)) *
                    Matrix.CreateTranslation(-9.6f, 11.85f, 21.1f) *
                    boneMat;

                game.BazTransform = bazTransform;


            }
            else
            {
                game.BazTransform = Matrix.CreateTranslation(0, -100, 0);
            }

            victoria.Draw(graphics, gameTime, transform, inCamera.View, inCamera.Projection);
            game.Bazooka.Draw(graphics, gameTime, game.BazTransform, inCamera.View, inCamera.Projection);

        }

        private float GetDesiredSpeed(ref KeyboardState keyboardState, ref GamePadState gamePadState)
        {
            if (keyboardState.IsKeyDown(Keys.W))
                return 1;
            if (keyboardState.IsKeyDown(Keys.S))
                return -1;

            float speed = gamePadState.ThumbSticks.Right.Y;

            return speed;
        }

        private float GetDesiredTurnRate(ref KeyboardState keyboardState, ref GamePadState gamePadState)
        {
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                return panRate;
            }

            if (keyboardState.IsKeyDown(Keys.Right))
            {
                return -panRate;
            }

            return -gamePadState.ThumbSticks.Right.X * panRate;
        }

        private float GetDesiredStrafe(ref KeyboardState keyboardState, ref GamePadState gamePadState)
        {
            if (keyboardState.IsKeyDown(Keys.A))
                return 1;

            if (keyboardState.IsKeyDown(Keys.D))
                return -1;

            float speed = gamePadState.ThumbSticks.Right.Y;

            // I'm not allowing you to walk backwards
            //if (speed < 0)
            //    speed = 0;

            return speed;
        }

        public void PlayerAim()
        {
            if (aiming)
            {
                victoria.BoneTransforms[victoria.Model.Bones["Bip01 Spine1"].Index] = Matrix.CreateRotationX(aimOrient - aimAngle) * victoria.BindTransforms[victoria.Model.Bones["Bip01 Spine1"].Index];
            }
        }
    }
}
