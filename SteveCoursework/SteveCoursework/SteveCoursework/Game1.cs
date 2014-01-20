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

namespace SteveCoursework
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        #region Variables
        //Create Models & transformation matrices 
        private Model spaceShip;
        private Model enemyShip1;
        private Model enemyShip2;
        private Matrix[] spaceShipTransforms;
        private Matrix[] enemyShip1Transforms;
        private Matrix[] enemyShip2Transforms;

        //Creates textures
        Texture2D backgroundTexture;
        Texture2D splashScreen;
        Texture2D controlsScreen;
        Texture2D gameOverScreen;
        int screenWidth;
        int screenHeight;

        //Imports a font
        private SpriteFont fontToUse;

        //Game state which changes throughout the game
        private int GameState = 0;

        // The aspect ratio determines how to scale 3d to 2d projection.
        private float aspectRatio;

        private BasicEffect basicEffect;
        private int bulletTimer = 0;
        // Set the position of the model in world space, and set the rotation.
        private Vector3 mdlPosition = Vector3.Zero;
        private Vector3 mdlRotation = Vector3.Zero;
        private float mdlRot = 0.0f;
        private Vector3 mdlVelocity = Vector3.Zero;

        //Sets the number of enemies there are in the array
        private Enemy[] enemySpaceShip1 = new Enemy[5];
        private Enemy[] enemySpaceShip2 = new Enemy[3];

        private SoundEffect engineSound, firingSound; //sound effects for the game that are WAV files
        private Song themeMusic; //Games main theme music
        private float soundTimer = 0.0f;

        // create an array of laser bullets
        private Model mdlLaser;
        private Matrix[] mdlLaserTransforms;
        private Laser[] laserList = new Laser[GameConstants.NumLasers];

        private Random random = new Random();

        private KeyboardState lastState; //Get the last keyboard state
        private GamePadState gamePadState;
        private int hitCount;

        //Creates camera objects
        private Camera thirdPersonCamera;
        private Camera topDownCamera;
        private Camera camera;

        private int playerHealth = 100; //Health
        private int score = 0; //Player score

        #endregion

        public void InitializeTransform()
        {
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
        }

        private void MoveModel()
        {

            KeyboardState keyboardState = Keyboard.GetState();

            if (GameState == 3 && keyboardState.IsKeyDown(Keys.Enter) || GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed && gamePadState.Buttons.Start == ButtonState.Released)//When game over occurs allow the player to press enter to start again
            {
                GameState = 2;
                MediaPlayer.Play(themeMusic);
                gamePadState = GamePad.GetState(PlayerIndex.One);
            }

            if (GameState == 3 && keyboardState.IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed && gamePadState.Buttons.Back == ButtonState.Released)//if player presses esc at the game over screen, go back to menu
            {
                GameState = 0;
                score = 0;
                gamePadState = GamePad.GetState(PlayerIndex.One);
            }

            if (GameState == 2)
            {

                if (playerHealth == 0)

                {//When player health = 0 show game over screen
                    GameState = 3;
                    MediaPlayer.Stop();
                }

                if (keyboardState.IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed && gamePadState.Buttons.Back == ButtonState.Released)
                {//If player presses esc go back to menu screen
                    GameState = 0;
                    MediaPlayer.Stop();
                    gamePadState = GamePad.GetState(PlayerIndex.One);
                }

                // Create some velocity if the right trigger is down.
                Vector3 mdlVelocityAdd = Vector3.Zero;

                mdlVelocityAdd.X = 2.0f;

                mdlRotation.X = -(float)Math.Sin(mdlRot);
                mdlRotation.Y = -(float)Math.Cos(mdlRot);

                //Changes the camera when the 'C' key is pressed 
                if (keyboardState.IsKeyDown(Keys.C) && lastState != keyboardState || GamePad.GetState(PlayerIndex.One).Buttons.Y == ButtonState.Pressed && gamePadState.Buttons.Y == ButtonState.Released)
                {
                    if (camera == thirdPersonCamera)
                    {
                        camera = topDownCamera;
                    }
                    else
                    {
                        camera = thirdPersonCamera;
                    }
                    gamePadState = GamePad.GetState(PlayerIndex.One);
                }


                if (keyboardState.IsKeyDown(Keys.A) || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X < 0.0f)
                {
                   //lets the player move left when 'A' is pressed. 
                    // adds a slight roation to the ship in the direction it is moving
                    mdlVelocityAdd.X *= -0.05f;
                    mdlVelocity += mdlVelocityAdd;
                    if (soundTimer == 0.0f)
                    {
                        //plays engine sound when moving
                        engineSound.Play(0.1f, 0.0f, 0.0f);
                    }

                    soundTimer += 0.1f;
                    //allows the sound to play through entirely and resets the timer to play again 
                    if (soundTimer > 4.4f)
                    {
                        soundTimer = 0.0f;
                    }
                    if (mdlRot <= 0.20f)
                    {
                        mdlRot -= -1.0f * 0.20f;
                    }
                }

                if (keyboardState.IsKeyDown(Keys.D) || GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X > 0.0f)
                {
                    //lets the player move left when 'D' is pressed. 
                    // adds a slight roation to the ship in the direction it is moving
                    mdlVelocityAdd.X *= 0.05f;
                    mdlVelocity += mdlVelocityAdd;

                    if (soundTimer == 0.0f)
                    {
                        engineSound.Play(0.1f, 0.0f, 0.0f);
                    }

                    soundTimer += 0.1f;
                    //allows the sound to play through entirely and resets the timer to play again 
                    if (soundTimer > 4.4f)
                    {
                        soundTimer = 0.0f;
                    }
                    if (mdlRot >= -0.20f)
                    {
                        mdlRot -= 1.0f * 0.20f;
                    }

                }

                if (keyboardState.IsKeyUp(Keys.A) && (keyboardState.IsKeyUp(Keys.D)) && GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X == 0.0f)
                {

                    mdlRot = 0.0f; //resets the rotation of the ship back to zero when no keys are being pressed
                    mdlVelocity = Vector3.Zero; //Stops the velocity so the ship doesn't slide
                    gamePadState = GamePad.GetState(PlayerIndex.One);
                }

                //are we shooting?
                if (keyboardState.IsKeyDown(Keys.Space) || lastState.IsKeyDown(Keys.Space) || GamePad.GetState(PlayerIndex.One).Triggers.Right > 0.0f )
                {
                    //add another bullet.  Find an inactive bullet slot and use it
                    //if all bullets slots are used, ignore the user input
                    bulletTimer++;
                    if (bulletTimer >= 30 || bulletTimer <= 60)
                    {
                        for (int i = 0; i < GameConstants.NumLasers; i++)
                        {
                            if (!laserList[i].isActive)
                            {
                                Vector3 laserDir = Vector3.Zero;
                                laserDir.Z = -2;
                                Matrix shipTransform = Matrix.CreateRotationY(mdlRot);
                                laserList[i].direction = laserDir;//shipTransform.Forward;
                                laserList[i].speed = GameConstants.LaserSpeedAdjustment;
                                laserList[i].position = mdlPosition + laserList[i].direction;
                                laserList[i].isActive = true;
                                firingSound.Play(0.1f, 0.0f, 0.0f);

                                break; //exit the loop     
                            }
                        }
                        bulletTimer = 0;
                    }

                }
                if (keyboardState.IsKeyDown(Keys.M) && lastState.IsKeyUp(Keys.M) || GamePad.GetState(PlayerIndex.One).Buttons.B == ButtonState.Pressed  && gamePadState.Buttons.B == ButtonState.Released)
                {
                    //checks to see if the audio is playing, if so mutes when 'M' key is pressed
                    if (MediaPlayer.IsMuted == false)
                    {
                        MediaPlayer.IsMuted = true;
                        
                    }
                    else 
                    {
                        //if the audio is currently muted it is then unmuted
                        MediaPlayer.IsMuted = false;
                    }
                    lastState = keyboardState;
                    gamePadState = GamePad.GetState(PlayerIndex.One);
                }

            }
            if (keyboardState.IsKeyDown(Keys.Enter) && GameState == 0 && lastState.IsKeyUp(Keys.Enter) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed && gamePadState.Buttons.Back == ButtonState.Released)
            {
                //If the game is at the main splash screen, move onto the controls page
                GameState = 1;
                MediaPlayer.Stop();
                score = 0;
                lastState = keyboardState;
                gamePadState = GamePad.GetState(PlayerIndex.One);
            }

            if (GameState == 1 && keyboardState.IsKeyDown(Keys.Enter) && lastState.IsKeyUp(Keys.Enter) || GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed && gamePadState.Buttons.Start == ButtonState.Released)
            {
                //Games the game is at the controls page, move onto main game
                GameState = 2; 
                    MediaPlayer.Play(themeMusic);
                    MediaPlayer.IsRepeating = true;
                    MediaPlayer.Volume = 0.9f;
                    lastState = keyboardState;
            }
            gamePadState = GamePad.GetState(PlayerIndex.One);
            lastState = keyboardState;
        }

        public Matrix[] SetupEffectTransformDefaults(Model myModel)
        {
            Matrix[] absoluteTransforms = new Matrix[myModel.Bones.Count];
            myModel.CopyAbsoluteBoneTransformsTo(absoluteTransforms);

            foreach (ModelMesh mesh in myModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    //Enables lighting effects for the game
                    effect.EnableDefaultLighting();
                    effect.DirectionalLight0.DiffuseColor = new Vector3(10, 10, 10.8f);
                    effect.DirectionalLight0.Direction = new Vector3(1, -1, -1);
                    effect.SpecularColor = new Vector3(0, 0, 0.1f); //adds a slight blue tinge to make the objects stand out 
                    effect.Projection = camera.projectionMatrix;
                    effect.View = camera.camViewMatrix;
                }
            }
            return absoluteTransforms;
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.IsMouseVisible = false;
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
            this.IsMouseVisible = false;
            Window.Title = "Space: Because It's Easier Than Land 2";
            InitializeTransform();

            //Sets the cameras position for the top down camera 
            Vector3 cameraPos1 = new Vector3(0.0f, 0.0f, -20.0f);

            //Sets up both cameras
            thirdPersonCamera = new Camera(graphics, new Vector3(0.0f, 55.0f, 80.0f), Vector3.Zero, 0, 0);
            topDownCamera = new Camera(graphics, new Vector3(0.0f, 180.0f,10.0f), cameraPos1, 0, 0);



            //Sets the inital camera when the game is loaded 
            camera = topDownCamera;
            InitializeEffect();

            base.Initialize();
        }

        private void InitializeEffect()
        {
            //Set up the projection and world matrices
            basicEffect = new BasicEffect(graphics.GraphicsDevice);
            basicEffect.Projection = camera.projectionMatrix;
            basicEffect.World = camera.worldMatrix;
        }

        private void SetupCameraViews(Model myModel)
        {
            foreach (ModelMesh mesh in myModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.Projection = camera.projectionMatrix;
                    effect.View = camera.camViewMatrix;

                }
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;

            //Loads the font used in the game
            fontToUse = Content.Load<SpriteFont>(".\\Font\\Font");

            //Load all 2D textures used
            backgroundTexture = Content.Load<Texture2D>(".\\Models\\space_background");
            splashScreen = Content.Load<Texture2D>(".\\Models\\SplashScreen");
            controlsScreen = Content.Load<Texture2D>(".\\Models\\Controls");
            gameOverScreen = Content.Load<Texture2D>(".\\Models\\gameover");

            //Loads Music and Sound Effects
            themeMusic = Content.Load<Song>(".\\Audio\\Danger Zone");
            engineSound = Content.Load<SoundEffect>(".\\Audio\\engine");
            firingSound = Content.Load<SoundEffect>(".\\Audio\\laser");

            // Loads game models and sets up transforms
            spaceShip = Content.Load<Model>(".\\Models\\SPACESHIP");
            spaceShipTransforms = SetupEffectTransformDefaults(spaceShip);
            mdlLaser = Content.Load<Model>(".\\Models\\bullet");
            mdlLaserTransforms = SetupEffectTransformDefaults(mdlLaser);
            enemyShip1 = Content.Load<Model>(".\\Models\\shipshite");
            enemyShip1Transforms = SetupEffectTransformDefaults(enemyShip1);
            enemyShip2 = Content.Load<Model>(".\\Models\\ship");
            enemyShip2Transforms = SetupEffectTransformDefaults(enemyShip2);

                for (int i = 0; i < enemySpaceShip1.Length; i++)
                {
                    Random rPos = new Random();
                    enemySpaceShip1[i] = new Enemy(); //Creates enemies for the length of the array
                    enemySpaceShip1[i].position = new Vector3(rPos.Next(-50, 50), 0, -110);
                    enemySpaceShip1[i].speed = 0.4f; //Sets the speed of the enemies
                    enemySpaceShip1[i].isActive = true;
                }
               
                for (int i = 0; i < enemySpaceShip2.Length; i++)
                {
                    Random rPos = new Random();
                    enemySpaceShip2[i] = new Enemy(); //Creates enemies for the length of the array
                    enemySpaceShip2[i].position = new Vector3(rPos.Next(-50, 50), 0, -110);
                    enemySpaceShip2[i].speed = 0.6f;//Sets the speed of the enemies
                    enemySpaceShip2[i].isActive = true;
                }
            
            screenWidth = graphics.GraphicsDevice.PresentationParameters.BackBufferWidth;
            screenHeight = graphics.GraphicsDevice.PresentationParameters.BackBufferHeight;

        }

        private Model LoadModel(string assetName, out Texture2D[] textures)
        {
            //loads and textures models
            Model newModel = Content.Load<Model>(assetName);
            textures = new Texture2D[newModel.Meshes.Count];
            int i = 0;
            foreach (ModelMesh mesh in newModel.Meshes)
                foreach (BasicEffect currentEffect in mesh.Effects)
                    textures[i++] = currentEffect.Texture;
            return newModel;

            Model enemy1 = Content.Load<Model>(assetName);
            textures = new Texture2D[enemy1.Meshes.Count];
            int i1 = 0;
            foreach (ModelMesh mesh in enemy1.Meshes)
                foreach (BasicEffect currentEffect in mesh.Effects)
                    textures[i++] = currentEffect.Texture;
            return enemy1;

            Model enemy2 = Content.Load<Model>(assetName);
            textures = new Texture2D[enemy1.Meshes.Count];
            int i2 = 0;
            foreach (ModelMesh mesh in enemy1.Meshes)
                foreach (BasicEffect currentEffect in mesh.Effects)
                    textures[i++] = currentEffect.Texture;
            return enemy1;

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
            mdlPosition.Z = 30.0f;
            // Allows the game to exit
            if (GameState == 0)
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                    this.Exit();
            }

            MoveModel();
            // TODO: Add your update logic here
            // Add velocity to the current position.
            mdlPosition += mdlVelocity;

            // Bleed off velocity over time.
            mdlVelocity *= 0.95f;

            if (GameState == 2)
            {
                //Updates the score once per frame (score will also be increased by shooting enemies
                score++;
            }

            for (int i = 0; i < enemySpaceShip1.Length; i++)
            if (GameState == 0)
            {
                enemySpaceShip1[i].isActive = false;
            }
            for (int i = 0; i < enemySpaceShip2.Length; i++)
                if (GameState == 0)
                {
                    enemySpaceShip2[i].isActive = false;
                }

            for (int i = 0; i < laserList.Length; i++)
                if (GameState == 0)
                {
                    laserList[i].isActive = false;
                }

            if (GameState == 2)
            {
                //Creates a random number that is used for the pos of enemies 
                Random r = new Random();

                for (int i = 0; i < enemySpaceShip1.Length; i++)
                {
                    enemySpaceShip1[i].position.Z += enemySpaceShip1[i].speed;

                    if (enemySpaceShip1[i].position.Z >= 60)
                    {
                        enemySpaceShip1[i].speed = r.Next(1, 3);
                        enemySpaceShip1[i].position.Z = -140.0f;
                        enemySpaceShip1[i].position.X += r.Next(-50, 50);//puts the enemies on a random pos 
                        enemySpaceShip1[i].isActive = true;
                    }
                }

                for (int i = 0; i < enemySpaceShip2.Length; i++)
                {
                    enemySpaceShip2[i].position.Z += enemySpaceShip2[i].speed;

                    if (enemySpaceShip2[i].position.Z >= 60)
                    {
                        enemySpaceShip2[i].speed = r.Next(1, 2);
                        enemySpaceShip2[i].position.Z = -140.0f;
                        enemySpaceShip2[i].position.X += r.Next(-50, 50);
                        enemySpaceShip2[i].isActive = true;
                    }
                }

                if (camera == thirdPersonCamera) //bounding values for third person camera
                {
                    if (mdlPosition.X >= 50.0f) { mdlPosition.X = 49.0f; }
                    if (mdlPosition.X <= -50.0f) { mdlPosition.X = -49.0f; }
                }

                if (camera == topDownCamera) //bounding values for top down camera
                {
                    if (mdlPosition.X >= 100.0f) { mdlPosition.X = 99.0f; }
                    if (mdlPosition.X <= -100.0f) { mdlPosition.X = -99.0f; }
                }

                float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds; //converts time to delta time

                for (int i = 0; i < GameConstants.NumLasers; i++)
                {
                    if (laserList[i].isActive)
                    {
                        laserList[i].Update(timeDelta);
                    }
                }

                BoundingSphere playerSphere =
                 new BoundingSphere(mdlPosition,
                          spaceShip.Meshes[0].BoundingSphere.Radius * 1.0f);

                //Check for collisions
                for (int i = 0; i < enemySpaceShip1.Length; i++)
                {
                    if (enemySpaceShip1[i].isActive)
                    {
                        BoundingSphere enemySphereA =
                          new BoundingSphere(enemySpaceShip1[i].position, enemyShip1.Meshes[0].BoundingSphere.Radius *
                                        1.0f);
                        for (int k = 0; k < laserList.Length; k++)
                        {
                            if (laserList[k].isActive)
                            {
                                BoundingSphere laserSphere = new BoundingSphere(
                                  laserList[k].position, mdlLaser.Meshes[0].BoundingSphere.Radius *
                                         1.0f);
                                if (enemySphereA.Intersects(laserSphere))
                                {
                                    enemySpaceShip1[i].isActive = false;//deletes the enemy ship 
                                    laserList[k].isActive = false;//deletes the laser
                                    hitCount++;
                                    score += 100;//adds 100 onto the players score
                                    break; //no need to check other bullets
                                }
                            }
                            if (enemySphereA.Intersects(playerSphere)) //Check collision between Dalek and Tardis
                            {
                                playerHealth -= 10; //takes 10 from the players health if collides with enemy
                                enemySpaceShip1[i].isActive = false;//deletes the enemy ship 
                                laserList[k].isActive = false;//deletes the laser 
                                break; //no need to check other bullets
                            }
                        }
                    }
                }
                //Check for collisions
                for (int i = 0; i < enemySpaceShip2.Length; i++)
                {
                    if (enemySpaceShip2[i].isActive)
                    {
                        BoundingSphere enemySphereB =
                          new BoundingSphere(enemySpaceShip2[i].position, enemyShip2.Meshes[0].BoundingSphere.Radius *
                                        1.5f);
                        for (int k = 0; k < laserList.Length; k++)
                        {
                            if (laserList[k].isActive)
                            {
                                BoundingSphere laserSphere = new BoundingSphere(
                                  laserList[k].position, mdlLaser.Meshes[0].BoundingSphere.Radius *
                                         1.0f);
                                if (enemySphereB.Intersects(laserSphere))
                                {
                                    enemySpaceShip2[i].isActive = false;//deletes the enemy ship 
                                    laserList[k].isActive = false;//deletes the laser object
                                    score += 100;//Adds 100 on to the players score
                                    hitCount++;

                                    break; //no need to check other bullets
                                }
                            }
                            if (enemySphereB.Intersects(playerSphere)) //Check collision between Dalek and Tardis
                            {
                                playerHealth -= 10; //takes 10 from the players health if collides with enemy
                                enemySpaceShip2[i].isActive = false;
                                laserList[k].isActive = false;
                                break; //no need to check other bullets
                            }
                        }
                    }
                }
            }
            base.Update(gameTime);
         }
                    


                
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (GameState == 0)
            {
                //draws the splash screen
                spriteBatch.Begin();
                Rectangle screenRectangle = new Rectangle(0, 0, screenWidth, screenHeight);
                spriteBatch.Draw(splashScreen, screenRectangle, Color.White);
                spriteBatch.End();
            }

            if (GameState == 1)
            {
                //draws the controls page
                spriteBatch.Begin();
                Rectangle screenRectangle = new Rectangle(0, 0, screenWidth, screenHeight);
                spriteBatch.Draw(controlsScreen, screenRectangle, Color.White);
                spriteBatch.End();
            }

            if (GameState == 3)
            {
                spriteBatch.Begin();
                Rectangle screenRectangle = new Rectangle(0, 0, screenWidth, screenHeight);
                spriteBatch.Draw(gameOverScreen, screenRectangle, Color.White);
               
                spriteBatch.End();
                writeText("Score: " + score, new Vector2(350, 280), Color.Black);
            }

            if (GameState == 2)
            {
                //Draws main game objects
                spriteBatch.Begin();
                DrawScenery();
                //draws the game background
                spriteBatch.End();

                writeText("Score: " + score, new Vector2(20, 20), Color.White);//shows the player score on screen   
                writeText("Health: " + playerHealth, new Vector2(650, 20), Color.White);//shows the players health on screen
                
                for (int i = 0; i < GameConstants.NumLasers; i++)//draws the laser
                {
                    if (laserList[i].isActive)
                    {
                        Matrix laserTransform = Matrix.CreateScale(GameConstants.LaserScalar) * Matrix.CreateTranslation(laserList[i].position);
                        DrawModel(mdlLaser, laserTransform, mdlLaserTransforms, "");
                    }
                }

                // TODO: Add your drawing code here
                Matrix modelTransform = Matrix.CreateRotationZ(mdlRot) * Matrix.CreateTranslation(mdlPosition);
                DrawModel(spaceShip, modelTransform, spaceShipTransforms, "");//draws the spaceship

                for (int i = 0; i < enemySpaceShip1.Length; i++)//draws enemy1
                {
                    if (enemySpaceShip1[i].isActive)
                    {
                        Matrix enemy1Transform = Matrix.CreateTranslation(enemySpaceShip1[i].position);
                        DrawModel(enemyShip1, enemy1Transform, enemyShip1Transforms, "");
                    }
                }

                for (int i = 0; i < enemySpaceShip2.Length; i++)//draws enemy2
                {
                    if (enemySpaceShip2[i].isActive)
                    {
                        Matrix enemy2Transform = Matrix.CreateTranslation(enemySpaceShip2[i].position);
                        DrawModel(enemyShip2, enemy2Transform, enemyShip2Transforms, "");
                    }
                }
            }
            base.Draw(gameTime);
        }

        public void DrawModel(Model model, Matrix modelTransform, Matrix[] absoluteBoneTransforms, String type)
        {

            if (GameState == 2)
            {
                this.basicEffect.View = camera.camViewMatrix;

                //Draw the model, a model can have multiple meshes, so loop
                foreach (ModelMesh mesh in model.Meshes)
                {
                    this.basicEffect.View = camera.camViewMatrix;
                    //This is where the mesh orientation is set
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.View = camera.camViewMatrix;
                        effect.World = absoluteBoneTransforms[mesh.ParentBone.Index] * modelTransform;
                        effect.EnableDefaultLighting();

                    }
                    //Draw the mesh, will use the effects set above.
                    mesh.Draw();
                }
            }
        }

        private void DrawScenery()
        {
            //method for drawing main game scenery
            Rectangle screenRectangle = new Rectangle(0, 0, screenWidth, screenHeight);
            spriteBatch.Draw(backgroundTexture, screenRectangle, Color.White);
        }

        private void writeText(string msg, Vector2 msgPos, Color msgColour)
        {
            //method for creating a piece of text to draw on screen
            spriteBatch.Begin();
            string output = msg;
            Vector2 FontOrigin = fontToUse.MeasureString(output) / 2;
            Vector2 FontPos = msgPos;
            spriteBatch.DrawString(fontToUse, output, FontPos, msgColour);
            spriteBatch.End();
        }
    }


}