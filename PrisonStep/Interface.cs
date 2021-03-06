﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

/***************************************************
 * Interface class
 * instantiate in PrisonGame after player is instantiated
 * Arguments:
 *  -Game: the one and only prison game
 *  -Player: The player instantiation the interface passes commands to. 
 *      Should be the player instantiated directly before this!
 *  -playerControllerIndex: The controller number we take input from. EG: Controller 1
 *  */


namespace PrisonStep
{
    public class Interface
    {
        /// <summary>
        /// The game in which this interface exists
        /// </summary>
        PrisonGame game;

        /// <summary>
        /// The player instantiation this interface controls
        /// </summary>
        Player player;

        /// <summary>
        /// The player index to indicate what controller to take instruction from
        /// </summary>
        PlayerIndex index;

        /// <summary>
        /// Holds the last state the gamepad was in
        /// </summary>
        GamePadState lastGamepadState;

        private bool allowKeyboard;

        public bool AllowKeyboard { get { return allowKeyboard; } set { allowKeyboard = value; } }


        public Interface(PrisonGame game, Player player, PlayerIndex playerControllerIndex)
        {
            this.game = game;
            this.player = player;
            this.index = playerControllerIndex;
        }

        public void Update(GameTime gameTime)
        {
            double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;

            if (GamePad.GetState(index).Triggers.Right > 0)
            {
                //type is float
                player.AttemptShoot();
            }

            if (GamePad.GetState(index).Triggers.Left > 0)
            {
                //type is float
                //Call a function from the player to raise a shield
                //pass float to function
            }

            if (GamePad.GetState(index).ThumbSticks.Right != Vector2.Zero)
            {
                //type is vector2
                //Call a function from the player change the camera angle
                //pass Xfloat, Yfloat, and gameTime to function
                player.AttemptRotation(GamePad.GetState(index).ThumbSticks.Right.X,
                    GamePad.GetState(index).ThumbSticks.Right.Y,
                    deltaTime);
            }

            if (GamePad.GetState(index).ThumbSticks.Left != Vector2.Zero)
            {
                //type is vector2
                //Call a function from the player to move themself
                //pass Xfloat, Yfloat, and gameTime to function
                player.AttempMovement(GamePad.GetState(index).ThumbSticks.Left.X,
                    GamePad.GetState(index).ThumbSticks.Left.Y,
                    deltaTime);
            }

            if (GamePad.GetState(index).DPad.Left == ButtonState.Pressed
                && lastGamepadState.DPad.Left != ButtonState.Pressed)
            {
                //no type to pass
                //Call a function from the player to make them switch to element 1
                player.ChangeColor(Player.Colors.Red);
            }

            if (GamePad.GetState(index).DPad.Up == ButtonState.Pressed
                && lastGamepadState.DPad.Up != ButtonState.Pressed)
            {
                //no type to pass
                //Call a function from the player to make them switch to element 2
                player.ChangeColor(Player.Colors.Blue);
            }

            if (GamePad.GetState(index).DPad.Right == ButtonState.Pressed
                && lastGamepadState.DPad.Right != ButtonState.Pressed)
            {
                //no type to pass
                //Call a function from the player to make them switch to element 3
                player.ChangeColor(Player.Colors.Green);
            }

            if (GamePad.GetState(index).Buttons.A == ButtonState.Pressed
                && lastGamepadState.Buttons.A != ButtonState.Pressed)
            {
                //no type to pass
                //Call a function from the player to jump
                player.AttemptJump();
            }

            if (GamePad.GetState(index).Buttons.B == ButtonState.Pressed
                && lastGamepadState.Buttons.B != ButtonState.Pressed)
            {
                //no type to pass
                //Call a function from the player to make them yell "Exterminate!"
                player.AttempToYellExterminate();
            }
                


            if (allowKeyboard)
            {
                KeyboardState keyboardState = Keyboard.GetState();
                if (keyboardState.IsKeyDown(Keys.W))
                {
                    player.AttempMovement(0, 1,deltaTime);
                }
                if (keyboardState.IsKeyDown(Keys.S))
                {
                    player.AttempMovement(0, -1, deltaTime);
                }
                if (keyboardState.IsKeyDown(Keys.A))
                {
                    player.AttempMovement(-1, 0, deltaTime);
                }
                if (keyboardState.IsKeyDown(Keys.D))
                {
                    player.AttempMovement(1, 0, deltaTime);
                }
                if (keyboardState.IsKeyDown(Keys.Up))
                {
                    player.AttemptRotation(0, .5f,deltaTime);
                }
                if (keyboardState.IsKeyDown(Keys.Down))
                {
                    player.AttemptRotation(0, -.5f, deltaTime);
                }
                if (keyboardState.IsKeyDown(Keys.Left))
                {
                    player.AttemptRotation(-.5f, 0, deltaTime);
                }
                if (keyboardState.IsKeyDown(Keys.Right))
                {
                    player.AttemptRotation(.5f, 0, deltaTime);
                }
                if (keyboardState.IsKeyDown(Keys.Space))
                {
                    player.AttemptShoot();
                }
                //if (keyboardState.IsKeyDown(Keys.CapsLock))
                //    game.Fluid.Disturb(new Vector3(0, 0, 0));

                if (keyboardState.IsKeyDown(Keys.LeftControl))
                {
                    player.AttemptJump();
                }

                if (keyboardState.IsKeyDown(Keys.D1))
                {
                    player.ChangeColor(Player.Colors.Red);
                }
                if (keyboardState.IsKeyDown(Keys.D2))
                {
                    player.ChangeColor(Player.Colors.Green);
                }
                if (keyboardState.IsKeyDown(Keys.D3))
                {
                    player.ChangeColor(Player.Colors.Blue);
                }
            };
            lastGamepadState = GamePad.GetState(index);
        }


    }
}
