using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EETetris
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Game
	{


		#region Variables
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		Texture2D gui;
		Texture2D[] blocks;
		SpriteFont text;
		Rectangle gui_rect;
		int x;
		int y;

		bool shadows = false;

		int score = 0;

		bool[,] screen;
		int[,] col;
		bool canSwitch = true;

		private int __grfon = 0;
		int graphicOn
		{
			get
			{
				return __grfon;
			}
			set
			{
				//Set it to the value
				__grfon = value;

				//Min value
				if (__grfon < 0) __grfon = 0;

				//Max value ( 0 - 2)
				if (__grfon > 2) __grfon = 0;
			}
		}
		bool graphicBasic{ get { return graphicOn == 0; } set { } }
		bool graphicBeta { get { return graphicOn == 1; } set { } }
		bool graphicTile { get { return graphicOn == 2; } set { } }
		bool[,] block;
		bool[,] heldBlock;
		bool[,,] pieces;
		int[] cols;
		int heldBlockCol = 0;
		int blockCol = 0;

		private float _cd = 1.0f;
		private float _cdt = 0.0f;
		float speedMultiplier { get { return _cdt; } set { if (value < 0.7f) { _cdt = 0.7f; } } }
		float countDuration { get {
			return _cd - speedMultiplier; } set { _cd = value; } }
		float currentTime = 0f;

		float cD = 0.0f;
		float cT = 0.0f;

		public int lastScore = 0;

		int __using = 0;
		int usingPiece { get { return __using; } set { __using = value; SetBlock(value); x = 4; y = 0; } }
		int held = 0;

		System.Random r;
		#endregion

		public Game1()
			: base()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Switch held and current piece
		/// </summary>
		public void SwitchHeld()
		{
			//If we haven't already switched
			if (canSwitch)
			{
				canSwitch = false;

				//If our held is null, we'll set it to something random
				if (held == -1)
					held = r.Next(0, pieces.GetLength(0));

				int p_tmp = held;

				//Switch held for block
				heldBlock = new bool[4, 4];
				for (int i = 0; i < block.GetLength(0); i++)
					for (int s = 0; s < block.GetLength(1); s++)
						heldBlock[i, s] = block[i, s];
				heldBlockCol = blockCol;

				block = new bool[4, 4];
				blockCol = 0;

				held = usingPiece;
				usingPiece = p_tmp;
			}
		}

		/// <summary>
		/// Increase the Y of the tetris block. Also handles when the block falls down
		/// </summary>
		/// <returns></returns>
		public bool IncreaseY()
		{
			bool go = true;

			for (int i = 0; i < block.GetLength(0); i++)
				for (int s = 0; s < block.GetLength(1); s++)
					if (block[i, s])
					{
						int curX = x + i;
						int curY = y + s;

						if (curY + 1 > 19)
							go = false;
						else if (screen[curX, curY + 1])
							go = false;
					}

			if (go)
				y++;
			else
			{
				canSwitch = true;
				//Set board
				for (int i = 0; i < block.GetLength(0); i++)
					for (int s = 0; s < block.GetLength(1); s++)
						if (block[i, s])
						{
							int curX = x + i;
							int curY = y + s;

							screen[curX, curY] = true;
							col[curX, curY] = blockCol;
						}

				x = 4;
				y = 0;

				//Reset piece type
				SetBlock(r.Next(0, pieces.GetLength(0)));

				int multiplier = 0;

				bool looking = true;
				while (looking)
				{
					if (multiplier != 0)
					{
						if (multiplier == 1)
						{
							score += 100;
							lastScore += 100;
						}
						else
						{
							score += 200 * multiplier;
							lastScore += 200 * multiplier;
						}
						
						if (lastScore > 1000)
						{
							lastScore -= 1000;
							speedMultiplier += 0.2f;// 025f;
						}
					}

					looking = false;

					//Check if there are any full lines
					for (int yp = 0; yp < screen.GetLength(1); yp++)
					{
						bool f = true;
						for (int xp = 0; xp < screen.GetLength(0); xp++)
						{
							if (!screen[xp, yp])
								f = false;
						}

						if (f)
						{
							//Merge the upper layer of the screen downwards
							for (int yx = yp; yx > 0; yx--)
								for (int xp = 0; xp < screen.GetLength(0); xp++)
								{
									screen[xp, yx] = screen[xp, yx - 1];
								}
							looking = true;
							multiplier++;
						}
					}
				}

				for (int i = 0; i < block.GetLength(0); i++)
					for(int s = 0; s < block.GetLength(1); s++)
						if(block[i, s]) //If blocks overlap, then we'll restart
							if (screen[x + i, y + s] == block[i, s])
							{
								Hotkeys.AddScore(score);
								DoInit();
								return go;
							}
			}

			return go;
		}

		/// <summary>
		/// Set the current piece to a different one
		/// </summary>
		/// <param name="piece"></param>
		public void SetBlock(int piece)
		{
			__using = piece;
			blockCol = cols[piece];

			//Set block to false
			for (int i = 0; i < block.GetLength(0); i++)
				for (int s = 0; s < block.GetLength(1); s++)
					block[i, s] = false;

			//Set block to piece
			for (int i = 0; i < pieces.GetLength(1); i++)
				for (int s = 0; s < pieces.GetLength(2); s++)
				{
					block[i, s] = pieces[piece, i, s];
				}
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			DoInit();

			base.Initialize();
		}

		public void DoInit()
		{

			#region Random
			//Init random & seeds
			r = new System.Random();
			r.Next(0, 1000);
			#endregion

			graphicBeta = true;

			//Clear held
			cD = 0.0f;
			cT = 0.0f;
			lastScore = 0;
			_cdt = 0.0f;
			heldBlockCol = 0;
			heldBlock = null;
			_cd = 1.0f;
			_cdt = 0.0f;
			countDuration = 1.0f;
			currentTime = 0f;
			held = 0;
			__using = 0;

			score = 0;

			Hotkeys.Init();

			// TODO: Add your initialization logic here
			//288 x 352
			graphics.PreferredBackBufferWidth = 288;
			graphics.PreferredBackBufferHeight = 352;
			graphics.ApplyChanges();

			x = 4;
			y = 0;
			held = -1;

			#region Init screen blocks
			screen = new bool[10, 20];
			col = new int[10, 20];
			for (int i = 0; i < screen.GetLength(0); i++)
				for (int s = 0; s < screen.GetLength(1); s++)
				{
					screen[i, s] = false;
					col[i, s] = 0;
				}
			#endregion

			#region Init block (the selected blocks)
			block = new bool[4, 4];
			for (int i = 0; i < block.GetLength(0); i++)
				for (int s = 0; s < block.GetLength(1); s++)
					block[i, s] = false;
			#endregion

			#region Init possible tetris pieces
			pieces = new bool[7, 4, 4];
			cols = new int[pieces.GetLength(0)];
			#region Set colors
			for (int n = 0; n < cols.GetLength(0); n++)
				switch (n)
				{
					case 0:
						cols[n] = 2;
						break;
					case 1:
						cols[n] = 1;
						break;
					case 2:
						cols[n] = 5;
						break;
					case 3:
						cols[n] = 0;
						break;
					case 4:
						cols[n] = 3;
						break;
					case 5:
						cols[n] = 4;
						break;
					case 6:
						cols[n] = 6;
						break;
				}
			#endregion

			#region Set pieces to false
			for (int n = 0; n < pieces.GetLength(0); n++)
				for (int i = 0; i < pieces.GetLength(1); i++)
					for (int s = 0; s < pieces.GetLength(2); s++)
					{
						pieces[n, i, s] = false;
					}
			#endregion

			#region Set pieces
			for (int n = 0; n < pieces.GetLength(0); n++)
				switch (n)
				{
					case 0:
						for (int i = 1; i < 3; i++)
							for (int s = 1; s < 3; s++)
								pieces[n, i, s] = true;
						break;
					case 1:
						pieces[n, 2, 0] = true;
						for (int i = 0; i < 3; i++)
							pieces[n, i, 1] = true;
						break;
					case 2:
						pieces[n, 0, 0] = true;
						for (int i = 0; i < 3; i++)
							pieces[n, i, 1] = true;
						break;
					case 3:
						pieces[n, 1, 0] = true;
						pieces[n, 1, 1] = true;
						pieces[n, 2, 1] = true;
						pieces[n, 2, 2] = true;
						break;
					case 4:
						pieces[n, 0, 1] = true;
						pieces[n, 1, 1] = true;
						pieces[n, 1, 2] = true;
						pieces[n, 2, 2] = true;
						break;
					case 5:
						for (int i = 0; i < 4; i++)
							pieces[n, 1, i] = true;
						break;
					case 6:
						pieces[n, 2, 1] = true;
						for (int i = 1; i < 4; i++)
							pieces[n, i, 2] = true;
						break;
				}
			#endregion
			#endregion

			//The original game sets the first block to square
			SetBlock(0);
		}

		/// <summary>
		/// Get a specific snip of a picture
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="wid"></param>
		/// <param name="hei"></param>
		/// <param name="main"></param>
		/// <returns></returns>
		public Texture2D GetSnip(int x, int y, int wid, int hei, Texture2D main)
		{
			//http://gamedev.stackexchange.com/questions/35358/create-a-texture2d-from-larger-image
			Texture2D originalTexture = main;
			Rectangle sourceRectangle = new Rectangle(x, y, wid, hei);

			Texture2D cropTexture = new Texture2D(GraphicsDevice, sourceRectangle.Width, sourceRectangle.Height);
			Color[] data = new Color[sourceRectangle.Width * sourceRectangle.Height];
			originalTexture.GetData(0, sourceRectangle, data, 0, data.Length);
			cropTexture.SetData(data);
			return cropTexture;
		}

		/// <summary>
		/// Rotate the piece
		/// </summary>
		/// <param name="amt"></param>
		public void Rotate(int amt = 1)
		{
			//Actually rotate x amount of times
			for (int c = 0; c < amt; c++)
			{
				object[,] conv = new object[block.GetLength(0), block.GetLength(1)];
				for (int i = 0; i < conv.GetLength(0); i++)
					for (int n = 0; n < conv.GetLength(1); n++)
						conv[i, n] = (object)block[i, n];

				object[,] back = RotateMatrix(conv, 4);
				for (int i = 0; i < back.GetLength(0); i++)
					for (int n = 0; n < back.GetLength(1); n++)
						block[i, n] = (bool)back[i, n];
			}

			//Make sure blocks aren't colliding after the rotation
			for (int i = 0; i < block.GetLength(0); i++)
				for (int s = 0; s < block.GetLength(1); s++)
					if (block[i, s])
					{
						int curX = x + i;
						int curY = y + s;

						//If the rotation was invalid: The rotation was invalid. Go back to the original way we were
						if ((curX < 0 || curX > 9) || (curY < 0 || curY > 19))
						{
							Rotate(4 - amt);
							return;
						}
						//First we check the bounds, then we'll check the screen position

						if (screen[curX, curY])
						{
							Rotate(4 - amt);
							return;
						}
					}
		}

		//stackoverflow
		public object[,] RotateMatrix(object[,] matrix, int n)
		{
			object[,] ret = new object[n, n];

			for (int i = 0; i < n; ++i)
			{
				for (int j = 0; j < n; ++j)
				{
					ret[i, j] = matrix[n - j - 1, i];
				}
			}

			return ret;
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			//Load the GUI
			gui = Content.Load<Texture2D>("gui");
			gui_rect = new Rectangle(0, 0, gui.Width, gui.Height);

			text = Content.Load<SpriteFont>("chat");

			//Get the blocks
			Texture2D blockUse = Content.Load<Texture2D>("blocks");

			//Snip out the specific amount of required blocks
			blocks = new Texture2D[24];

			for (int y = 0; y < 3; y++)
				for (int x = 0; x < 8; x++)
					blocks[x + (y * 8)] = GetSnip(x * 16, y * 16, 16, 16, blockUse);

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
			var k = Keyboard.GetState();

			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || k.IsKeyDown(Keys.Escape))
				Exit();

			//KEY PRESS TEMPLATE
			/*
			int num = 7;
			if (k.IsKeyDown(Keys.X)) {
				if(keys[num]) {
					keys[num] = false;
					//key press code
				}
			} else {
				keys[num] = true; 
			}
			 */

			#region Switch between block sets
			if (Hotkeys.HandleKeyPress(Hotkeys.BetaBasic(k), 0))
			{

				graphicOn++;

			}
			#endregion

			#region ROTATE LEF
			if (Hotkeys.HandleKeyPress(Hotkeys.RotateLeft(k), 1))
				Rotate(1);
			#endregion

			#region ROTATE RIGHT
			if (Hotkeys.HandleKeyPress(Hotkeys.RotateRight(k), 2))
				Rotate(3);
			#endregion

			#region FASTER FALLING
			if (k.IsKeyDown(Keys.S) || k.IsKeyDown(Keys.Down)) countDuration = 0.1f; else countDuration = 1f;
			#endregion

			#region C [ SWITCH HELD TO HOTBAR ]
			if (Hotkeys.HandleKeyPress(Hotkeys.HoldSlot(k), 4))
				SwitchHeld();
			#endregion

			if (Hotkeys.HandleKeyPress(Hotkeys.Shadows(k), 8))
			{
				shadows = !shadows;
			}

			if (Hotkeys.HandleKeyPress(Hotkeys.TetrisRight(k), 5))
			{
				bool go = true;

				for (int i = 0; i < block.GetLength(0); i++)
					for (int s = 0; s < block.GetLength(1); s++)
						if (block[i, s])
						{
							int curX = x + i;
							int curY = y + s;

							if (curX + 1 > 9)
								go = false;
							else if (screen[curX + 1, curY])
								go = false;
						}

				if (go)
					x += 1;
			}

			if (Hotkeys.HandleKeyPress(Hotkeys.TetrisLeft(k), 6))
			{
				bool go = true;

				for (int i = 0; i < block.GetLength(0); i++)
					for (int s = 0; s < block.GetLength(1); s++)
						if (block[i, s])
						{
							int curX = x + i;
							int curY = y + s;

							if (curX - 1 < 0)
								go = false;
							else if (screen[curX - 1, curY])
								go = false;
						}

				if (go)
					x -= 1;
			}

			currentTime += (float)gameTime.ElapsedGameTime.TotalSeconds; //Time passed since last Update()
			cT += (float)gameTime.ElapsedGameTime.TotalSeconds;

			if (currentTime >= countDuration)
			{
				currentTime = 0.0f;
				IncreaseY();
			}

			if (Hotkeys.HandleKeyPress(Hotkeys.InstantDrop(k), 7))
					while (IncreaseY())
					{
						//Keep increasing untill the tetris piece has fallen and hit the board.
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
			spriteBatch.Begin();

			spriteBatch.Draw(gui, gui_rect, Color.White);

			int modifier = (graphicOn* 8);

			//Shadows before actual blocks
			if (shadows)
			{
				int ampy = y;
				bool hitGround = false;
				int minusBonus = 0;
				while (!hitGround)
				{
					ampy++;

					if (ampy == 20)
						hitGround = true;
					else
					{
						for (int i = 0; i < block.GetLength(0); i++)
							for (int s = 0; s < block.GetLength(1); s++)
								if(block[i, s])
								if (ampy + s >= 20 || x + i >= 9)
								{
									while ((ampy + s) - minusBonus >= 20)
									{
										minusBonus++;
									}
									//hitGround = true;
								}
								else if (screen[x + i, ampy + s])
								{
									hitGround = true;
								}
					}
				}

				if (ampy == 20)
					ampy -= minusBonus + 1;
				else
					ampy--;


				for (int i = 0; i < block.GetLength(0); i++)
					for (int s = 0; s < block.GetLength(1); s++)
						if (block[i, s])
							spriteBatch.Draw(blocks[modifier + 7], new Rectangle(((x + i) * 16) + 16, ((ampy + s) * 16) + 16, 16, 16), Color.White);
			}

			for (int i = 0; i < block.GetLength(0); i++)
				for (int s = 0; s < block.GetLength(1); s++)
					if (block[i, s])
						spriteBatch.Draw(blocks[modifier + blockCol], new Rectangle(((x + i) * 16) + 16, ((y + s) * 16) + 16, 16, 16), Color.White);
			
			for (int i = 0; i < screen.GetLength(0); i++ )
				for (int s = 0; s < screen.GetLength(1); s++)
				{
					if (screen[i, s])
					{
						spriteBatch.Draw(blocks[modifier + col[i, s]], new Rectangle((i * 16) + 16, (s * 16) + 16, 16, 16), Color.White);
					}
				}

			//GUI held 13x17
			if(heldBlock != null)
			for (int i = 0; i < heldBlock.GetLength(0); i++)
				for (int s = 0; s < heldBlock.GetLength(1); s++)
					if (heldBlock[i, s])
						spriteBatch.Draw(blocks[modifier + heldBlockCol], new Rectangle((i * 16) + (16 * 13), (s * 16) + (16 * 16), 16, 16), Color.White);

			spriteBatch.DrawString(text, score.ToString(), new Vector2(12 * 16, (13 * 16) - 2), Color.White);

			spriteBatch.End();
			base.Draw(gameTime);
		}
	}
}
