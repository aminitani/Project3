using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PrisonStep
{
    public class PlayerPackage
    {
        private Player player;
        private Camera camera;
        private Interface playerInterface;
        private Vector3 spawn;

        public Player Player { get { return player; } set { player = value; } }
        public Camera Camera { get { return camera; } set { camera = value; } }
        public Interface PlayerInterface { get { return playerInterface; } set { playerInterface = value; } }
        public Vector3 Spawn { get { return spawn; } set { spawn = value; } }

        public PlayerPackage()
        {
        }


    }
}
