using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace tom_foolery
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _orangeSquare;
        private Rectangle _squareRect;
        private Vector2 _squarePosition;
        private const float SquareSpeed = 200f;

        private bool _isJumping;
        private const float JumpVelocity = -500f;
        private const float Gravity = 1000f;
        private float _yVelocity;

        private const int SquareSize = 80;

        private SoundEffect _footstepSound;
        private SoundEffectInstance _footstepInstance;

        private bool _isMoving;

        private Texture2D _bulletTexture;
        private Rectangle _bulletRect;
        private List<Vector2> _bullets;
        private const float BulletSpeed = 1000f;
        private const float BulletSpawnInterval = 0.1f; 
        private float _timeSinceLastBullet;

        private Texture2D _enemyTexture;
        private Texture2D _enemyFadingTexture;
        private Rectangle _enemyRect;
        private Vector2 _enemyPosition;
        private Vector2 _enemyVelocity;
        private bool _isEnemyHit;
        private float _enemyFadeAlpha;
        private float _enemyFadeRate = 0.5f; 
        private SoundEffect _fadeSound;

        private Random _random; 

        private List<SoundEffect> _bulletSounds;
        private SoundEffect _hitSound;

        private float _screenShakeAmount;
        private const float MaxScreenShakeAmount = 10f;
        private const float ScreenShakeDecayFactor = 0.9f;

        private bool _gameEnding;
        private float _endDelay = 2f; 
        private float _endTimer; 
        private const int EnemySize = 80;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _squarePosition = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height - SquareSize); // Adjusted position
            _isJumping = false;

            _enemyPosition = new Vector2(GraphicsDevice.Viewport.Width - EnemySize, GraphicsDevice.Viewport.Height - EnemySize); // Adjusted enemy position
            _enemyVelocity = new Vector2(50f, 50f); // Initial enemy velocity

            _bullets = new List<Vector2>();
            _timeSinceLastBullet = 0f;

            _random = new Random(); // Initialize the Random object

            _bulletSounds = new List<SoundEffect>(); // Initialize the list of bullet sounds

            _enemyFadeAlpha = 1f; // Set enemy alpha to fully opaque

            _screenShakeAmount = 0f; // Initialize screen shake amount to zero

            _gameEnding = false;
            _endTimer = 0f;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _orangeSquare = new Texture2D(GraphicsDevice, 1, 1);
            _orangeSquare.SetData(new[] { Color.Orange });

            _squareRect = new Rectangle(0, 0, SquareSize, SquareSize); 

            
            _bulletTexture = Content.Load<Texture2D>("bulletSheet");
            _bulletRect = new Rectangle(0, 0, 20, 10);

            _enemyTexture = Content.Load<Texture2D>("miley"); 
            _enemyFadingTexture = Content.Load<Texture2D>("tacolol"); 
            _enemyRect = new Rectangle(0, 0, EnemySize, EnemySize);

           
            _footstepSound = Content.Load<SoundEffect>("teef");
            _footstepInstance = _footstepSound.CreateInstance();
            _footstepInstance.Volume = 0.5f;
            _footstepInstance.IsLooped = true;

         
            _bulletSounds.Add(Content.Load<SoundEffect>("bullet1"));
            _bulletSounds.Add(Content.Load<SoundEffect>("bullet2"));
            _bulletSounds.Add(Content.Load<SoundEffect>("bullet3"));

           
            _hitSound = Content.Load<SoundEffect>("ded");

         
            _fadeSound = Content.Load<SoundEffect>("ded");

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            KeyboardState keyboardState = Keyboard.GetState();
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!_isEnemyHit && !_gameEnding)
            {
                _isMoving = false;

                if (keyboardState.IsKeyDown(Keys.A) && _squarePosition.X > 0)
                {
                    _squarePosition.X -= SquareSpeed * delta;
                    _isMoving = true;
                }
                else if (keyboardState.IsKeyDown(Keys.D) && _squarePosition.X < GraphicsDevice.Viewport.Width - _squareRect.Width)
                {
                    _squarePosition.X += SquareSpeed * delta;
                    _isMoving = true;
                }

                if (_isMoving)
                {
                    if (_footstepInstance.State != SoundState.Playing)
                    {
                        _footstepInstance.Play(); 
                    }
                }
                else
                {
                    _footstepInstance.Stop(); 
                }

                if (keyboardState.IsKeyDown(Keys.W) && !_isJumping)
                {
                    _isJumping = true;
                    _yVelocity = JumpVelocity;
                }

                if (_isJumping)
                {
                    _squarePosition.Y += _yVelocity * delta;
                    _yVelocity += Gravity * delta;

                    if (_squarePosition.Y >= GraphicsDevice.Viewport.Height - _squareRect.Height)
                    {
                        _squarePosition.Y = GraphicsDevice.Viewport.Height - _squareRect.Height;
                        _isJumping = false;
                        _yVelocity = 0;
                    }
                }

                // Shooting behavior
                if (keyboardState.IsKeyDown(Keys.Space))
                {
                    _timeSinceLastBullet += delta;

                 
                    if (_timeSinceLastBullet >= BulletSpawnInterval)
                    {
                        _bullets.Add(_squarePosition + new Vector2(_squareRect.Width, _squareRect.Height / 2) - new Vector2(0f, _bulletRect.Height / 2));
                        _timeSinceLastBullet = 0f;

                        
                        int randomIndex = _random.Next(_bulletSounds.Count);
                        _bulletSounds[randomIndex]?.Play();

            
                        _screenShakeAmount = MaxScreenShakeAmount;

                       
                        foreach (Vector2 bullet in _bullets)
                        {
                            Rectangle bulletRect = new Rectangle((int)bullet.X, (int)bullet.Y, _bulletRect.Width, _bulletRect.Height);
                            if (bulletRect.Intersects(_enemyRect))
                            {
                                _isEnemyHit = true;

                             
                                _hitSound.Play();

                            
                                _enemyFadeAlpha = 1f;

                               
                                _footstepInstance.Stop();

                                _fadeSound.Play();



                                break;
                            }
                        }
                    }
                }

                for (int i = _bullets.Count - 1; i >= 0; i--)
                {
                    _bullets[i] += new Vector2(BulletSpeed * delta, 0f);

                    if (_bullets[i].X > GraphicsDevice.Viewport.Width)
                    {
                        _bullets.RemoveAt(i);
                    }
                }

                _screenShakeAmount *= ScreenShakeDecayFactor;

            
                if (_enemyPosition.X < 0 || _enemyPosition.X > GraphicsDevice.Viewport.Width - EnemySize)
                {
                    _enemyVelocity.X *= -1;
                }

                if (_enemyPosition.Y < 0 || _enemyPosition.Y > GraphicsDevice.Viewport.Height - EnemySize)
                {
                    _enemyVelocity.Y *= -1;
                }
            }
            else if (_isEnemyHit && !_gameEnding)
            {
                // If the enemy is hit, fade it into another image
                _enemyFadeAlpha -= _enemyFadeRate * delta;
                _enemyFadeAlpha = MathHelper.Clamp(_enemyFadeAlpha, 0f, 1f);

                if (_enemyFadeAlpha <= 0f)
                {
                    
                    _gameEnding = true;
                }
            }
            else if (_gameEnding)
            {
                _endTimer += delta;

                if (_endTimer >= _endDelay)
                {
                    // THIS KILLS THE GAME HAHAHAH BLOOD, GORE AHAHAHAHAHAHAHAHAHHAHHAH
                    Exit();
                }
            }

            _squareRect.X = (int)_squarePosition.X;
            _squareRect.Y = (int)_squarePosition.Y;

            _enemyRect.X = (int)_enemyPosition.X;
            _enemyRect.Y = (int)_enemyPosition.Y;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Matrix.CreateTranslation(new Vector3(_screenShakeAmount, _screenShakeAmount, 0))); // Apply screen shake offset
            _spriteBatch.Draw(_orangeSquare, _squareRect, Color.White);
            foreach (Vector2 bullet in _bullets)
            {
                _spriteBatch.Draw(_bulletTexture, bullet, null, Color.White, 0f, Vector2.Zero, new Vector2(1.5f, 1.5f), SpriteEffects.None, 0f); // Make bullets bigger
            }

            if (!_isEnemyHit)
            {
               // this is the fade effect.
                Color enemyColor = new Color(Color.White, _enemyFadeAlpha);
                _spriteBatch.Draw(_enemyTexture, _enemyRect, enemyColor);
            }
            else
            {
                Color enemyColor = new Color(Color.White, _enemyFadeAlpha);
                _spriteBatch.Draw(_enemyFadingTexture, _enemyRect, enemyColor);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        static void Main()
        {
            using (var game = new Game1())
                game.Run();
        }
    }
}
