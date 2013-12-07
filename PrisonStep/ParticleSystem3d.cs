﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


/*
 * This code was lifted directly from Owen.
 */
namespace PrisonStep
{
    /// <summary>
    /// ParticleSystem is an abstract class that provides the basic functionality to
    /// create a particle effect. Different subclasses will have different effects,
    /// such as fire, explosions, and plumes of smoke. To use these subclasses, 
    /// simply call AddParticles, and pass in where the particles should exist
    /// </summary>
    public abstract class ParticleSystem3d
    {
        private bool blended = true;

        public bool Blended { get { return blended; } set { blended = value; } }

        // the texture this particle system will use.
        private Texture2D texture;

        private Effect effect;

        // this number represents the maximum number of effects this particle system
        // will be expected to draw at one time. this is set in the constructor and is
        // used to calculate how many particles we will need.
        private int howManyEffects;

        // The list of live particles
        private LinkedList<Particle3d> liveParticles = new LinkedList<Particle3d>();

        // A list of available particles
        private LinkedList<Particle3d> availableParticles = new LinkedList<Particle3d>();

        /// <summary>
        /// returns the number of particles that are available for a new effect.
        /// </summary>
        public int FreeParticleCount { get { return availableParticles.Count; } }


        // This region of values control the "look" of the particle system, and should 
        // be set by deriving particle systems in the InitializeConstants method. The
        // values are then used by the virtual function InitializeParticle. Subclasses
        // can override InitializeParticle for further
        // customization.
        #region constants to be set by subclasses

        /// <summary>
        /// minNumParticles and maxNumParticles control the number of particles that are
        /// added when AddParticles is called. The number of particles will be a random
        /// number between minNumParticles and maxNumParticles.
        /// </summary>
        protected int minNumParticles;
        protected int maxNumParticles;

        /// <summary>
        /// this controls the texture that the particle system uses. It will be used as
        /// an argument to ContentManager.Load.
        /// </summary>
        protected string textureFilename;

        /// <summary>
        /// minInitialSpeed and maxInitialSpeed are used to control the initial velocity
        /// of the particles. The particle's initial speed will be a random number 
        /// between these two. The direction is determined by the function 
        /// PickRandomDirection, which can be overriden.
        /// </summary>
        protected float minInitialSpeed;
        protected float maxInitialSpeed;

        /// <summary>
        /// minAcceleration and maxAcceleration are used to control the acceleration of
        /// the particles. The particle's acceleration will be a random number between
        /// these two. By default, the direction of acceleration is the same as the
        /// direction of the initial velocity.
        /// </summary>
        protected float minAcceleration;
        protected float maxAcceleration;

        /// <summary>
        /// minRotationSpeed and maxRotationSpeed control the particles' angular
        /// velocity: the speed at which particles will rotate. Each particle's rotation
        /// speed will be a random number between minRotationSpeed and maxRotationSpeed.
        /// Use smaller numbers to make particle systems look calm and wispy, and large 
        /// numbers for more violent effects.
        /// </summary>
        protected float minRotationSpeed;
        protected float maxRotationSpeed;

        /// <summary>
        /// minLifetime and maxLifetime are used to control the lifetime. Each
        /// particle's lifetime will be a random number between these two. Lifetime
        /// is used to determine how long a particle "lasts." Also, in the base
        /// implementation of Draw, lifetime is also used to calculate alpha and scale
        /// values to avoid particles suddenly "popping" into view
        /// </summary>
        protected float minLifetime;
        protected float maxLifetime;

        /// <summary>
        /// to get some additional variance in the appearance of the particles, we give
        /// them all random scales. the scale is a value between minScale and maxScale,
        /// and is additionally affected by the particle's lifetime to avoid particles
        /// "popping" into view.
        /// </summary>
        protected float minScale;
        protected float maxScale;

        #endregion

        /// <summary>
        /// Constructs a new ParticleSystem.
        /// </summary>
        /// <param name="game">The host for this particle system. The game keeps the 
        /// content manager and sprite batch for us.</param>
        /// <param name="howManyEffects">the maximum number of particle effects that
        /// are expected on screen at once.</param>
        /// <remarks>it is tempting to set the value of howManyEffects very high.
        /// However, this value should be set to the minimum possible, because
        /// it has a large impact on the amount of memory required, and slows down the
        /// Update and Draw functions.</remarks>
        protected ParticleSystem3d(int howManyEffects)
        {
            this.howManyEffects = howManyEffects;
        }

        /// <summary>
        /// override the base class's Initialize to do some additional work; we want to
        /// call InitializeConstants to let subclasses set the constants that we'll use.
        /// 
        /// also, the particle array and freeParticles queue are set up here.
        /// </summary>
        private void Initialize()
        {
            InitializeConstants();

            // calculate the total number of particles we will ever need, using the
            // max number of effects and the max number of particles per effect.
            // once these particles are allocated, they will be reused, so that
            // we don't put any pressure on the garbage collector.
            for (int i = 0; i < howManyEffects * maxNumParticles; i++)
            {
                availableParticles.AddLast(new Particle3d());
            }
        }

        /// <summary>
        /// this abstract function must be overriden by subclasses of ParticleSystem.
        /// It's here that they should set all the constants marked in the region
        /// "constants to be set by subclasses", which give each ParticleSystem its
        /// specific flavor.
        /// </summary>
        protected abstract void InitializeConstants();

        /// <summary>
        /// Override the base class LoadContent to load the texture. once it's
        /// loaded, calculate the origin.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            Initialize();

            // make sure sub classes properly set textureFilename.
            if (string.IsNullOrEmpty(textureFilename))
            {
                string message = "textureFilename wasn't set properly, so the " +
                    "particle system doesn't know what texture to load. Make " +
                    "sure your particle system's InitializeConstants function " +
                    "properly sets textureFilename.";
                throw new InvalidOperationException(message);
            }

            // load the texture....
            texture = content.Load<Texture2D>(textureFilename);
            effect = content.Load<Effect>("ParticleEffect");
            CreateMesh();
        }

        private VertexPositionNormalTexture[] vertices;
        private int[] indices = { 0, 3, 2, 0, 2, 1 };

        private void CreateMesh()
        {
            vertices = new VertexPositionNormalTexture[4];
            vertices[0].Position = new Vector3(-1, -1, 0);
            vertices[1].Position = new Vector3(1, -1, 0);
            vertices[2].Position = new Vector3(1, 1, 0);
            vertices[3].Position = new Vector3(-1, 1, 0);

            vertices[0].TextureCoordinate = new Vector2(0, 1);
            vertices[1].TextureCoordinate = new Vector2(1, 1);
            vertices[2].TextureCoordinate = new Vector2(1, 0);
            vertices[3].TextureCoordinate = new Vector2(0, 0);
        }

        public void Activate(GraphicsDevice device)
        {
        }


        /// <summary>
        /// AddParticles's job is to add an effect somewhere on the screen. If there 
        /// aren't enough particles in the freeParticles queue, it will use as many as 
        /// it can. This means that if there not enough particles available, calling
        /// AddParticles will have no effect.
        /// </summary>
        /// <param name="where">where the particle effect should be created</param>
        public void AddParticles(Vector3 where)
        {
            // the number of particles we want for this effect is a random number
            // somewhere between the two constants specified by the subclasses.
            int numParticles = Random.Next(minNumParticles, maxNumParticles);

            // create that many particles, if you can.
            for (int i = 0; i < numParticles && availableParticles.Count > 0; i++)
            {
                // Remove the node from the list of available particles
                LinkedListNode<Particle3d> node = availableParticles.First;
                availableParticles.Remove(node);

                // Initialize the particle
                Particle3d p = node.Value;
                InitializeParticle(p, where);

                // Add to the list of live particles
                liveParticles.AddLast(node);
            }
        }

        /// <summary>
        /// InitializeParticle randomizes some properties for a particle, then
        /// calls initialize on it. It can be overriden by subclasses if they 
        /// want to modify the way particles are created. For example, 
        /// SmokePlumeParticleSystem overrides this function make all particles
        /// accelerate to the right, simulating wind.
        /// </summary>
        /// <param name="p">the particle to initialize</param>
        /// <param name="where">the position on the screen that the particle should be
        /// </param>
        protected virtual void InitializeParticle(Particle3d p, Vector3 where)
        {
            // Determine the initial particle direction
            Vector3 direction = PickParticleDirection();

            // pick some random values for our particle
            float velocity = RandomBetween(minInitialSpeed, maxInitialSpeed);
            float acceleration = RandomBetween(minAcceleration, maxAcceleration);
            float lifetime = RandomBetween(minLifetime, maxLifetime);
            float scale = RandomBetween(minScale, maxScale);
            float rotationSpeed = RandomBetween(minRotationSpeed, maxRotationSpeed);
            float orientation = RandomBetween(0, (float)Math.PI * 2);

            // then initialize it with those random values. initialize will save those,
            // and make sure it is marked as active.
            p.Initialize(where, velocity * direction, acceleration * direction,
                          lifetime, scale, rotationSpeed, orientation);
        }


        // a random number generator that the whole system can share.
        private static Random random = new Random();
        public static Random Random
        {
            get { return random; }
        }


        //  a handy little function that gives a random float between two
        // values. This will be used in several places in the sample, in particilar in
        // ParticleSystem.InitializeParticle.
        public static float RandomBetween(float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        /// <summary>
        /// PickRandomDirection is used by InitializeParticles to decide which direction
        /// particles will move. The default implementation is a random vector in a
        /// circular pattern.
        /// </summary>
        protected virtual Vector3 PickParticleDirection()
        {
            Vector3 v = new Vector3(RandomBetween(-1, 1), RandomBetween(-1, 1), RandomBetween(-1, 1));
            v.Normalize();

            return v;
        }

        /// <summary>
        /// overriden from DrawableGameComponent, Update will update all of the active
        /// particles.
        /// </summary>
        public void Update(double deltaTime)
        {
            // calculate dt, the change in the since the last frame. the particle
            // updates will use this value.
            float delta = (float)deltaTime;

            for (LinkedListNode<Particle3d> node = liveParticles.First; node != null; )
            {
                LinkedListNode<Particle3d> nextNode = node.Next;
                node.Value.Update(delta);
                if (!node.Value.Active)
                {
                    liveParticles.Remove(node);
                    availableParticles.AddLast(node);
                }

                node = nextNode;
            }
        }

        /// <summary>
        /// overriden from DrawableGameComponent, Draw will use ParticleSampleGame's 
        /// sprite batch to render all of the active particles.
        /// </summary>
        public void Draw(GraphicsDevice device, Camera camera)
        {
            effect.Parameters["View"].SetValue(camera.View);
            effect.Parameters["Projection"].SetValue(camera.Projection);
            effect.Parameters["Texture"].SetValue(texture);

            Matrix invView = camera.View;
            invView.Translation = Vector3.Zero;
            invView = Matrix.Invert(invView);

            BlendState blendState = new BlendState();
            blendState.ColorBlendFunction = BlendFunction.Add;
            blendState.ColorSourceBlend = Blend.SourceAlpha;

            if (blended)
            {
                // Smoke is often alpha blended
                blendState.ColorDestinationBlend = Blend.InverseSourceAlpha;
            }
            else
            {
                // Explosions are often additive
                blendState.ColorDestinationBlend = Blend.One;
            }

            device.BlendState = blendState;
            device.DepthStencilState = DepthStencilState.DepthRead;

            foreach (Particle3d p in liveParticles)
            {
                // Life time as a value from 0 to 1
                float normalizedLifetime = p.Age / p.Lifetime;

                float alpha = 4 * normalizedLifetime * (1 - normalizedLifetime);

                // make particles grow as they age. they'll start at 75% of their size,
                // and increase to 100% once they're finished.
                float scale = p.Scale * (.75f + .25f * normalizedLifetime);

                Matrix world = Matrix.CreateScale(scale) * Matrix.CreateRotationZ(p.Orientation) * invView * Matrix.CreateTranslation(p.Position);
                effect.Parameters["World"].SetValue(world);
                effect.Parameters["Alpha"].SetValue(alpha);


                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    device.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);
                }

            }

            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
        }
    }
}