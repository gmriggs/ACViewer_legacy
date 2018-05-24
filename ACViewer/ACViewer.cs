using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using ACE.Diag;
using ACE.Diag.Network;

namespace ACViewer
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class ACViewer : Game
    {
        GraphicsDeviceManager graphics;

        public static ACViewer Instance;
        public Client Client;
        public GameState GameState { get => Client.GameState; }

        public BlockRange Landblocks;
        public Player Player;

        public Render.Render Render;
        public Camera Camera;

        public ACViewer(Client client)
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Instance = this;
            Client = client;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            var windowWidth = 1280;
            var windowHeight = 720;

            graphics.PreferredBackBufferWidth = windowWidth;
            graphics.PreferredBackBufferHeight = windowHeight;
            if (windowWidth == 1920)
            {
                Window.IsBorderless = true;
                Window.Position = new Point(0, 0);
            }

            graphics.IsFullScreen = false;

            graphics.ApplyChanges();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Player = new Player();
            var landblock = Player.GetLandblock();

            ACData.Init();
            LoadLandblock(landblock.Raw);
        }

        public void LoadLandblock(uint landblockId)
        {
            Console.WriteLine("Landblock: " + landblockId.ToString("X8"));
            Render = new Render.Render();
            Landblocks = new BlockRange(landblockId);
            Render.Init();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Client != null && Client.IsUpdated)
            {
                Render.Setup.BuildPlayer();
                Render.Setup.BuildCreatures();
            }

            Render.Camera.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            Render.Draw();

            base.Draw(gameTime);
        }
    }
}
