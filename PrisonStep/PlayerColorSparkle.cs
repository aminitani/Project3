using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PrisonStep
{
    public class PlayerColorSparkle
    {
        private float time = 0;
        private float sparkleDistance = 100;
        private Player player;
        private PrisonGame game;
        private int updateCount = 0;

        public PlayerColorSparkle(Player inPlayer, PrisonGame inGame)
        {
            this.player = inPlayer;
            this.game = inGame;
        }

        public void LoadContent(ContentManager Content)
        {
        }

        public void Initialize()
        {
        }

        public void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            time += delta;
            time %= ((float)Math.PI * 2);
            updateCount++;

            if (updateCount > 15)
            {

                Vector3 sparkleLoc = player.Location + new Vector3(0, 80, 0);
                sparkleLoc += new Vector3(0, 4*sparkleDistance/5 * (float)Math.Sin(time), 0);
                sparkleLoc += new Vector3(sparkleDistance * (float)Math.Cos(time), 0, sparkleDistance *(float)Math.Sin(time));
                switch (player.ColorState)
                {
                    case Player.Colors.Red:
                        game.RedParticleSystem.AddParticles(sparkleLoc);
                        break;
                    case Player.Colors.Green:
                        game.GreenParticleSystem.AddParticles(sparkleLoc);
                        break;
                    case Player.Colors.Blue:
                        game.BlueParticleSystem.AddParticles(sparkleLoc);
                        break;
                }
                updateCount = 0;
            }
        }

    }
}
