using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using PrisonStep;

namespace PrisonStep
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PrisonGame : Microsoft.Xna.Framework.Game
    {

        #region Fields

        /// <summary>
        /// This graphics device we are drawing on in this assignment
        /// </summary>
        GraphicsDeviceManager graphics;

        /// <summary>
        /// Splash screen states
        /// </summary>
        public enum GameState { splash, game, results };
        GameState current = GameState.splash;

        /// <summary>
        /// A reference to the audio engine we use
        /// </summary>
        AudioEngine audioEngine;

        /// <summary>
        /// The loaded audio wave bank
        /// </summary>
        WaveBank waveBank;

        /// <summary>
        /// The loaded audio sound bank
        /// </summary>
        SoundBank soundBank;
        public SoundBank SoundBank { get { return soundBank; } }

        /// <summary>
        /// Stores the last keyboard state for the game.
        /// </summary>
        KeyboardState lastKeyboardState;
        GamePadState lastGPS1;
        GamePadState lastGPS2;

        private Ground ground;
        public Ground Ground { get { return ground; } }

        private Model wall;

        private List<PlayerPackage> playerPackages = new List<PlayerPackage>();
        public List<PlayerPackage> PlayerPackages { get { return playerPackages; } }

        //Particle effects
        private RedParticleSystem3d redParticleSystem;
        public RedParticleSystem3d RedParticleSystem { get { return redParticleSystem; } }

        private BlueParticleSystem3d blueParticleSystem;
        public BlueParticleSystem3d BlueParticleSystem { get { return blueParticleSystem; } }

        private GreenParticleSystem3d greenParticleSystem;
        public GreenParticleSystem3d GreenParticleSystem { get { return greenParticleSystem; } }

        private DExpParticleSystem3d dalekExpParticleSystem;
        public DExpParticleSystem3d DalekExpParticleSystem { get { return dalekExpParticleSystem; } }

        private Fluid fluid;
        public Fluid Fluid { get { return fluid;} }

        #endregion

        #region Properties

        #endregion

        /// <summary>
        /// random number generator
        /// </summary>
        private Random randNum = new Random();
        public Random RandNum { get { return randNum; } }

        /// <summary>
        /// Score and UI fonts
        /// </summary>
        private SpriteFont uIFont;
        public SpriteFont UIFont { get { return uIFont; } }
        private SpriteBatch spriteBatch;
        public SpriteBatch SpriteBatch { get { return spriteBatch; } }
        private Texture2D crosshairTexture;
        public Texture2D CrosshairTexture { get { return crosshairTexture; } }
        private Texture2D elementsPlusTexture;
        public Texture2D ElementsPlusTexture { get { return elementsPlusTexture; } }
        private Texture2D ringTexture;
        public Texture2D RingTexture { get { return ringTexture; } }
        private Texture2D healthBar;
        public Texture2D HealthBar { get { return healthBar; } }

        private Skybox skybox;


        /// <summary>
        /// Constructor
        /// </summary>
        public PrisonGame()
        {
            // XNA startup
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            skybox = new Skybox(this);

            // player
            for (int i = 0; i < 4; i++)
            {
                PlayerPackage templayerPackage = new PlayerPackage();
                
                Camera tempCam = new Camera(graphics);
                tempCam.Eye = new Vector3(800, 180, 1053);
                tempCam.Center = new Vector3(275, 90, 1053);
                tempCam.FieldOfView = MathHelper.ToRadians(42);
                templayerPackage.Camera = tempCam;

                templayerPackage.Spawn = new Vector3(1000, 0, 0);

                Player templayer = new Player(this, tempCam, i);
                templayer.Location = templayerPackage.Spawn;
                templayerPackage.Player = templayer;

                //FIX ME
                Interface templayerInterface = null;
                if (i == 0)
                {
                    templayerInterface = new Interface(this, templayer, PlayerIndex.One);
                    /*add keyboard support for the first player*/
                    templayerInterface.AllowKeyboard = true;
                }
                else if (i == 1)
                {
                    templayerInterface = new Interface(this, templayer, PlayerIndex.Two);
                }
                else if (i == 2)
                {
                    templayerInterface = new Interface(this, templayer, PlayerIndex.Three);
                }
                else if (i == 3)
                {
                    templayerInterface = new Interface(this, templayer, PlayerIndex.Four);
                }

                templayerPackage.PlayerInterface = templayerInterface;

                playerPackages.Add(templayerPackage);
            }

            ground = new Ground(this);

            //Particle system
            redParticleSystem = new RedParticleSystem3d(100);
            redParticleSystem.Blended = false;

            blueParticleSystem = new BlueParticleSystem3d(100);
            blueParticleSystem.Blended = false;

            greenParticleSystem = new GreenParticleSystem3d(100);
            greenParticleSystem.Blended = false;

            dalekExpParticleSystem = new DExpParticleSystem3d(100);
            dalekExpParticleSystem.Blended = false;

            fluid = new Fluid(this);

            // Some basic setup for the display window
            this.IsMouseVisible = true;
			this.Window.AllowUserResizing = false;
			this.graphics.PreferredBackBufferWidth = 1024;
			this.graphics.PreferredBackBufferHeight = 768;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            foreach (PlayerPackage pp in playerPackages)
            {
                pp.Player.Initialize();
            }
            ground.Initialize();
            skybox.Initialize();
            fluid.Initialize();
            

            //This section is lifted from the learning XNA 4.0 book. Partition the screen into two disjoint halves.
            Viewport vp1 = GraphicsDevice.Viewport;
            Viewport vp2 = GraphicsDevice.Viewport;
            Viewport vp3 = GraphicsDevice.Viewport;
            Viewport vp4 = GraphicsDevice.Viewport;
            vp1.Height = (GraphicsDevice.Viewport.Height / 2);
            vp1.Width = (GraphicsDevice.Viewport.Width / 2);

            vp2.Y = vp1.Height;
            vp2.Height = vp1.Height;
            vp2.Width = vp1.Width;

            vp3.X = vp1.Width;
            vp3.Width = vp1.Width;
            vp3.Height = vp1.Height;

            vp4.X = vp1.Width;
            vp4.Y = vp1.Height;
            vp4.Width = vp1.Width;
            vp4.Height = vp1.Height;


            //FIX ME
            playerPackages[0].Camera.Viewport = vp1;
            playerPackages[1].Camera.Viewport = vp2;
            playerPackages[2].Camera.Viewport = vp3;
            playerPackages[3].Camera.Viewport = vp4;

            foreach (PlayerPackage pp in playerPackages)
            {
                pp.Camera.Initialize();
            }

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            audioEngine = new AudioEngine("Content\\DDRAudio.xgs");
            waveBank = new WaveBank(audioEngine, "Content\\Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, "Content\\Sound Bank.xsb");

            skybox.LoadContent(Content);

            wall = Content.Load<Model>("borked");

            foreach (PlayerPackage pp in playerPackages)
            {
                pp.Player.LoadContent(Content);
            }
            ground.LoadContent(Content);

            //Particle system contents
            redParticleSystem.LoadContent(Content);
            blueParticleSystem.LoadContent(Content);
            greenParticleSystem.LoadContent(Content);
            dalekExpParticleSystem.LoadContent(Content);

            fluid.LoadContent(Content);


            spriteBatch = new SpriteBatch(GraphicsDevice);
            uIFont = Content.Load<SpriteFont>("UIFont");
            crosshairTexture = Content.Load<Texture2D>("crosshair");
            elementsPlusTexture = Content.Load<Texture2D>("ElementSelectPlus");
            ringTexture = Content.Load<Texture2D>("ring");
            healthBar = Content.Load<Texture2D>("healthBar");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            base.Update(gameTime);

            audioEngine.Update();

            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState1 = GamePad.GetState(PlayerIndex.One);
            GamePadState gamePadState2 = GamePad.GetState(PlayerIndex.Two);

            //
            // Update game components
            //
            if (current == GameState.splash)
            {
                if (keyboardState.IsKeyDown(Keys.Tab) && lastKeyboardState.IsKeyUp(Keys.Tab))
                    current = GameState.game;
                if ((gamePadState1.IsButtonDown(Buttons.Start) && lastGPS1.IsButtonUp(Buttons.Start)) || (gamePadState2.IsButtonDown(Buttons.Start) && lastGPS2.IsButtonUp(Buttons.Start)))
                    current = GameState.game;
            }
            else if (current == GameState.game)
            {
                if (keyboardState.IsKeyDown(Keys.Tab) && lastKeyboardState.IsKeyUp(Keys.Tab))
                    current = GameState.results;
                if ((gamePadState1.IsButtonDown(Buttons.Start) && lastGPS1.IsButtonUp(Buttons.Start)) || (gamePadState2.IsButtonDown(Buttons.Start) && lastGPS2.IsButtonUp(Buttons.Start)))
                    current = GameState.results;

                foreach (PlayerPackage pp in playerPackages)
                {
                    pp.PlayerInterface.Update(gameTime);
                    if (pp.Player.Location.Y >= 0)
                    {
                        fluid.Disturb(pp.Player.Location);
                    }
                }

                skybox.Update(gameTime);

                foreach (PlayerPackage pp in playerPackages)
                {
                    pp.Player.Update(gameTime);
                }

                ground.Update(gameTime);

                foreach (PlayerPackage pp in playerPackages)
                {
                    pp.Camera.Update(gameTime);
                }

                //particle systems
                redParticleSystem.Update(gameTime.ElapsedGameTime.TotalSeconds);
                blueParticleSystem.Update(gameTime.ElapsedGameTime.TotalSeconds);
                greenParticleSystem.Update(gameTime.ElapsedGameTime.TotalSeconds);
                dalekExpParticleSystem.Update(gameTime.ElapsedGameTime.TotalSeconds);

                fluid.Update(gameTime);
            }
            else if (current == GameState.results)
            {
                if (keyboardState.IsKeyDown(Keys.Tab) && lastKeyboardState.IsKeyUp(Keys.Tab))
                    current = GameState.splash;
                if ((gamePadState1.IsButtonDown(Buttons.Start) && lastGPS1.IsButtonUp(Buttons.Start)) || (gamePadState2.IsButtonDown(Buttons.Start) && lastGPS2.IsButtonUp(Buttons.Start)))
                    current = GameState.splash;
            }

            lastKeyboardState = keyboardState;
            lastGPS1 = gamePadState1;
            lastGPS2 = gamePadState2;
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (current == GameState.splash)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(uIFont, "If the aliens win, everyone will die.", new Vector2(10, 10), Color.White);
                spriteBatch.End();
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }
            else if (current == GameState.game)
            {
                graphics.GraphicsDevice.Clear(Color.Black);

                foreach (PlayerPackage pp in playerPackages)
                {
                    if (pp.Player.State != Player.States.Died)
                    {
                        GraphicsDevice.Viewport = pp.Camera.Viewport;
                        DrawGame(gameTime, pp.Camera);

                        spriteBatch.Begin();
                        spriteBatch.Draw(healthBar, new Vector2(10, 10), new Rectangle(healthBar.Height / 2, healthBar.Width, healthBar.Width, healthBar.Height / 2), Color.Black);
                        spriteBatch.Draw(healthBar, new Vector2(10, 10), new Rectangle(healthBar.Height, healthBar.Width, (int)(healthBar.Width * pp.Player.Health / pp.Player.MaxHealth), healthBar.Height / 2), Color.Red);
                        spriteBatch.Draw(crosshairTexture, new Vector2(GraphicsDevice.Viewport.Width / 2 - crosshairTexture.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2 - crosshairTexture.Height / 2), Color.White);
                        spriteBatch.Draw(elementsPlusTexture, new Vector2(10, graphics.GraphicsDevice.Viewport.Height - 10 - elementsPlusTexture.Height), Color.White);
                        if (pp.Player.ColorState == Player.Colors.Blue)
                            spriteBatch.Draw(ringTexture, new Vector2(10 + 44, graphics.GraphicsDevice.Viewport.Height - 10 - elementsPlusTexture.Height + 1), Color.White);
                        else if (pp.Player.ColorState == Player.Colors.Red)
                            spriteBatch.Draw(ringTexture, new Vector2(10 + 1, graphics.GraphicsDevice.Viewport.Height - 10 - elementsPlusTexture.Height + 44), Color.White);
                        else //if (pp.Player.ColorState == Player.Colors.Green)
                        {
                            spriteBatch.Draw(ringTexture, new Vector2(10 + elementsPlusTexture.Width - 1 - ringTexture.Width, graphics.GraphicsDevice.Viewport.Height - 10 - elementsPlusTexture.Height + 44), Color.White);
                        }
 
                         spriteBatch.End();
                        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    }
                    else
                    {
                        GraphicsDevice.Viewport = pp.Camera.Viewport;
                        DrawGame(gameTime, pp.Camera);

                        spriteBatch.Begin();
                        spriteBatch.DrawString(uIFont, "Respawn in: " + ((int)pp.Player.DeathTimer+1).ToString(), new Vector2(230, 184), Color.White);
                        //spriteBatch.Draw(crosshairTexture, new Vector2(GraphicsDevice.Viewport.Width / 2 - crosshairTexture.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2 - crosshairTexture.Height / 2), Color.White);
                        spriteBatch.End();
                        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    }
                }
            }
            else if (current == GameState.results)
            {
                foreach (PlayerPackage pp in playerPackages)
                {
                    GraphicsDevice.Viewport = pp.Camera.Viewport;

                    spriteBatch.Begin();
                    spriteBatch.DrawString(uIFont, "Kills: " + pp.Player.Kills.ToString(), new Vector2(10, 30), Color.White);
                    spriteBatch.DrawString(uIFont, "Deaths: " + pp.Player.Deaths.ToString(), new Vector2(10, 50), Color.White);
                    spriteBatch.End();
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                }
            }
        }

        public void DrawGame(GameTime gameTime, Camera inCamera)
        {
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            skybox.Draw(graphics, gameTime, inCamera);
            DrawModel(graphics, wall, Matrix.CreateRotationY((float)Math.PI) * Matrix.CreateScale(1000, 1000, 1000), inCamera);
            ground.Draw(graphics, gameTime, inCamera);

            foreach (PlayerPackage pp in playerPackages)
            {
                pp.Player.Draw(graphics, gameTime, inCamera);
            }

            fluid.Draw(graphics.GraphicsDevice, inCamera);

            redParticleSystem.Draw(GraphicsDevice, inCamera);
            blueParticleSystem.Draw(GraphicsDevice, inCamera);
            greenParticleSystem.Draw(GraphicsDevice, inCamera);
            dalekExpParticleSystem.Draw(GraphicsDevice, inCamera);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.BlendState = BlendState.Opaque;

            base.Draw(gameTime);

        }

        public void DrawModel(GraphicsDeviceManager graphics, Model model, Matrix world, Camera inCamera)
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


    }
}
