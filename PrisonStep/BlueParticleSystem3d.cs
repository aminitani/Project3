using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrisonStep
{

    /// <summary>
    /// SmokePlumeParticleSystem is a specialization of ParticleSystem which sends up a
    /// plume of smoke. The smoke is blown to the right by the wind.
    /// </summary>
    public class BlueParticleSystem3d : ParticleSystem3d
    {
        public BlueParticleSystem3d(int howManyEffects)
            : base(howManyEffects)
        {
        }

        /// <summary>
        /// Set up the constants that will give this particle system its behavior and
        /// properties.
        /// </summary>
        protected override void InitializeConstants()
        {
            textureFilename = "blueParticle";

            minInitialSpeed = 0;
            maxInitialSpeed = 5;

            // we don't want the particles to accelerate at all, aside from what we
            // do in our overriden InitializeParticle.
            minAcceleration = -100;
            maxAcceleration = 0;

            // long lifetime, this can be changed to create thinner or thicker smoke.
            // tweak minNumParticles and maxNumParticles to complement the effect.
            minLifetime = 3.0f;
            maxLifetime = 4.0f;

            minScale = 5.0f;
            maxScale = 10.0f;

            minNumParticles = 75;
            maxNumParticles = 200;

            // rotate
            minRotationSpeed = -MathHelper.PiOver2;
            maxRotationSpeed = MathHelper.PiOver2;
        }

        /// <summary>
        /// PickRandomDirection is overriden so that we can make the particles always 
        /// move have an initial velocity pointing up.
        /// </summary>
        /// <returns>a random direction which points basically up.</returns>
        protected override Vector3 PickParticleDirection()
        {
            float r = 0.1f;

            Vector3 v = new Vector3(RandomBetween(-r, r), 1, RandomBetween(-r, r));
            v.Normalize();

            return v;
        }

        /// <summary>
        /// InitializeParticle is overridden to add the appearance of wind.
        /// </summary>
        /// <param name="p">the particle to set up</param>
        /// <param name="where">where the particle should be placed</param>
        protected override void InitializeParticle(Particle3d p, Vector3 where)
        {
            base.InitializeParticle(p, where);

            // the base is mostly good, but we want to simulate a little bit of wind
            // heading to the right.
            p.Velocity = p.Velocity + new Vector3(ParticleSystem3d.RandomBetween(-10, 10), ParticleSystem3d.RandomBetween(-10, 10), ParticleSystem3d.RandomBetween(-10, 10));
        }
    }
}
