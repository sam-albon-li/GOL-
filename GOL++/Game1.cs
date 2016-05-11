using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace GOL
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        //Number of times a second update/draw are called
        const float UpdateRate = 60f;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Rectangle viewportRect;
        Rectangle titleSafeAreaRect;

        Texture2D background;
        Menus.LoadedMenu mainMenu;
        SpriteFont font1;

        Objects.GameObject customPointer;
        Objects.GameOfLife GOL;
        float updateGOLRate = 60;
        float UpdateGOLRate
        {
            get { return updateGOLRate; }
            set
            {
                if (value < 1)
                {
                    updateGOLRate = 1;
                }
                else if (value > UpdateRate + 1)
                {
                    updateGOLRate = UpdateRate + 1;
                }
                else
                {
                    updateGOLRate = value;
                }
            }
        }
        int updateCounter = 0;

        long generation = 0;
        bool onMenu;
        bool custom;
        bool paused;
        bool running;
        GamePadState previousGamePadState = GamePad.GetState(PlayerIndex.One);

        SoundEffect begin;
        SoundEffect loop;
        SoundEffectInstance music;

        float score = 0f;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            SetResolution(1280, 720);
            //graphics.ToggleFullScreen();

            GOL = new Objects.GameOfLife(Content.Load<Texture2D>("Sprites\\square"), Content.Load<Texture2D>("Sprites\\grid"),
                                         Content.Load<Texture2D>("Sprites\\ControllerButtons\\b"), Content.Load<Texture2D>("Sprites\\ControllerButtons\\a"), 
                                         Content.Load<Texture2D>("Sprites\\ControllerButtons\\x"), Content.Load<Texture2D>("Sprites\\Menu\\gameover"),
                                         Content.Load<Texture2D>("Sprites\\Menu\\rules"),
                                         40, 40, titleSafeAreaRect);
            customPointer = new Objects.GameObject(Content.Load<Texture2D>("Sprites\\customPointer"), new Vector2(GOL.DrawAreaRect.X, GOL.DrawAreaRect.Y), 1f);

            background = Content.Load<Texture2D>("Sprites\\background");
            font1 = Content.Load<SpriteFont>("Sprites\\font1");

            onMenu = true;
            mainMenu = new Menus.LoadedMenu(Content.Load<Texture2D>("Sprites\\Menu\\customGame"),
                                            Content.Load<Texture2D>("Sprites\\Menu\\gliderGun"),
                                            Content.Load<Texture2D>("Sprites\\Menu\\block1"),
                                            Content.Load<Texture2D>("Sprites\\Menu\\block2"),
                                            Content.Load<Texture2D>("Sprites\\Menu\\block3"),
                                            Content.Load<Texture2D>("Sprites\\Menu\\leave"), viewportRect, 1f);

            this.TargetElapsedTime = TimeSpan.FromSeconds(1.0f / UpdateRate);

            begin = Content.Load<SoundEffect>("Music\\begin");
            loop = Content.Load<SoundEffect>("Music\\loop");
            music = begin.CreateInstance();
            music.Play();

            // TODO: use this.Content to load your game content here
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
            if (onMenu)
            {
                MainMenu(mainMenu.UpdateLinearMenu(previousGamePadState));
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed && previousGamePadState.Buttons.Back == ButtonState.Released)
                {
                    if (paused)
                    {
                        onMenu = false;
                        paused = false;
                    }
                }
            }
            else
            {
                if (GamePad.GetState(PlayerIndex.One).DPad.Up == ButtonState.Pressed)
                {
                    UpdateGOLRate += 1 / 5f;
                }
                else if (GamePad.GetState(PlayerIndex.One).DPad.Down == ButtonState.Pressed)
                {
                    UpdateGOLRate -= 1 / 5f;
                }

                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed && previousGamePadState.Buttons.Back == ButtonState.Released)
                {
                    onMenu = true;
                    paused = true;
                    if (GOL.GameOver)
                    {
                        GOL.GameOver = false;
                        GOL.Load(99);
                    }
                }

                if (!custom)
                {
                    //if A is pressed, running is set to false or true
                    if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed && previousGamePadState.Buttons.Start == ButtonState.Released)
                    {
                        if (running)
                        {
                            running = false;
                        }
                        else
                        {
                            running = true;
                        }

                    }

                    //Updates GOL
                    if (running)
                    {
                        if (!GOL.GameOver)
                        {
                            if (updateCounter == 0)
                            {
                                GOL.Update();
                                generation++;
                            }

                            updateCounter++;
                            if (updateCounter > UpdateRate / UpdateGOLRate)
                            {
                                updateCounter = 0;
                            }

                            score = 0;
                            for (int i = 0; i < GOL.Width; i++)
                            {
                                for (int j = 0; j < GOL.Height; j++)
                                {
                                    if (GOL.CurrentGOL[i, j])
                                    {
                                        score++;
                                    }
                                }
                            }
                            score *= generation;
                        }
                    }
                }
                else
                {
                    customPointer.Update(GOL.DrawAreaRect);
                    GOL.CustomUpdate(new Point((int)(customPointer.Position.X),
                                               (int)(customPointer.Position.Y)));
                    if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed)
                    {
                        custom = false;
                        running = true;
                        customPointer.Alive = false;
                    }
                }
            }

            base.Update(gameTime);
            previousGamePadState = GamePad.GetState(PlayerIndex.One);

            //Check if begin has finished looping
            if (music.State == SoundState.Stopped)
            {
                music = loop.CreateInstance();
                music.IsLooped = true;
                music.Play();
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkBlue);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            spriteBatch.Draw(background, viewportRect, Color.White);

            GOL.Draw(spriteBatch);

            if (!onMenu)
            {
                Vector2 stringDrawLocation = new Vector2(titleSafeAreaRect.X, titleSafeAreaRect.Y);
                spriteBatch.DrawString(font1, "Generation: " + generation.ToString(), stringDrawLocation, Color.White);
                stringDrawLocation.Y += 50;
                string updateRateString = "Speed: ";
                if (updateGOLRate > 60)
                {
                    updateRateString += "60+";
                }
                else
                {
                    updateRateString += Math.Floor(UpdateGOLRate).ToString();
                }
                spriteBatch.DrawString(font1, updateRateString, stringDrawLocation, Color.White);
                stringDrawLocation.Y += 50;
                spriteBatch.DrawString(font1, "Score: " + Math.Floor(score).ToString(), stringDrawLocation, Color.White);
                spriteBatch.Draw(GOL.Rules, GOL.RulesRect, Color.White);

                if (customPointer.Alive)
                {
                    spriteBatch.Draw(customPointer.Sprite, customPointer.Position, null, Color.White, 0f, customPointer.Centre, 1f, SpriteEffects.None, 0f);
                }
            }
            else
            {
                mainMenu.DrawMenu(spriteBatch);
            }

            spriteBatch.End();
        }

        void SetResolution(int nWidth, int nHeight)
        {
            //Change resolution
            graphics.PreferredBackBufferWidth = nWidth;
            graphics.PreferredBackBufferHeight = nHeight;
            graphics.ApplyChanges();

            viewportRect = new Rectangle(0, 0, nWidth, nHeight);
            titleSafeAreaRect = new Rectangle((int)(nHeight * 0.1f), (int)(nHeight * 0.1f), (int)(nWidth - (nHeight * 0.2f)), (int)(nHeight * 0.8f));
        }

        void MainMenu(int buttonSelection)
        {
            if (buttonSelection == 5)
            {
                this.Exit();
            }
            else if (buttonSelection != 99)
            {
                customPointer.Alive = true;
                GOL.Load(99);
                GOL.Load(0);
                GOL.Load(buttonSelection);
                paused = false;
                onMenu = false;
                custom = true;
                generation = 0;
                UpdateGOLRate = UpdateRate;
            }
        }
    }
}
