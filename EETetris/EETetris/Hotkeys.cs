using Microsoft.Xna.Framework.Input;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EETetris
{
	public static class Hotkeys
	{
		private static KeySetup ks;

		public static void Init()
		{
			ks = new KeySetup();
			keys = new bool[8];

			for (int i = 0; i < keys.Length; i++)
				keys[i] = true;
		}

		public static bool[] keys;

		public static bool TetrisLeft(KeyboardState n) { return IsInUse(ks.TetrisLeft, n); }
		public static bool TetrisRight(KeyboardState n) { return IsInUse(ks.TetrisRight, n); }
		public static bool BetaBasic(KeyboardState n) { return IsInUse(ks.BetaBasic, n); }
		public static bool HoldSlot(KeyboardState n) { return IsInUse(ks.HoldSlot, n); }
		public static bool InstantDrop(KeyboardState n) { return IsInUse(ks.InstantDrop, n); }
		public static bool FastDrop(KeyboardState n) { return IsInUse(ks.FastDrop, n); }
		public static bool RotateLeft(KeyboardState n) { return IsInUse(ks.RotateLeft, n); }
		public static bool RotateRight(KeyboardState n) { return IsInUse(ks.RotateRight, n); }
		public static void AddScore(int s)
		{
			ks.Scores.Add(s);
			if (ks.BestScore < s)
				ks.BestScore = s;
			ks.Save();
		}

		private static bool IsInUse(System.Collections.Generic.List<Keys> l, KeyboardState n)
		{
			for (int i = 0; i < l.Count; i++)
				if (n.IsKeyDown(l[i]))
					return true;
			return false;
		}

		public static bool HandleKeyPress(bool returnIfSuceed, int keyId)
		{
			if (returnIfSuceed)
			{
				if (keys[keyId])
				{
					keys[keyId] = false;
					return true;
				}
			}
			else keys[keyId] = true;
			return false;
		}
	}

	public class KeySetup
	{
		public KeySetup(bool reset = true)
		{
			TetrisLeft = new System.Collections.Generic.List<Keys>();
			TetrisRight = new System.Collections.Generic.List<Keys>();
			BetaBasic = new System.Collections.Generic.List<Keys>();

			HoldSlot = new System.Collections.Generic.List<Keys>();

			InstantDrop = new System.Collections.Generic.List<Keys>();
			FastDrop = new System.Collections.Generic.List<Keys>();

			RotateLeft = new System.Collections.Generic.List<Keys>();
			RotateRight = new System.Collections.Generic.List<Keys>();

			Scores = new System.Collections.Generic.List<int>();

			if(reset)
			if (!System.IO.File.Exists("setup.json"))
			{
				TetrisLeft.Add(Keys.Left);
				TetrisLeft.Add(Keys.A);

				TetrisRight.Add(Keys.Right);
				TetrisRight.Add(Keys.D);

				BetaBasic.Add(Keys.L);

				HoldSlot.Add(Keys.C);

				InstantDrop.Add(Keys.Space);
				FastDrop.Add(Keys.S);

				RotateLeft.Add(Keys.Q);
				RotateRight.Add(Keys.E);

				Save();
			}
			else
			{
				Load();
			}
		}

		public int BestScore = 0;
		public System.Collections.Generic.List<int> Scores;

		public System.Collections.Generic.List<Keys> TetrisLeft;
		public System.Collections.Generic.List<Keys> TetrisRight;
		public System.Collections.Generic.List<Keys> BetaBasic;
		public System.Collections.Generic.List<Keys> HoldSlot;
		public System.Collections.Generic.List<Keys> InstantDrop;
		public System.Collections.Generic.List<Keys> FastDrop;
		public System.Collections.Generic.List<Keys> RotateLeft;
		public System.Collections.Generic.List<Keys> RotateRight;

		public void Save()
		{
			System.IO.File.WriteAllText("setup.json", JsonConvert.SerializeObject(this));
		}

		public void Load()
		{
			KeySetup get = new KeySetup(false);
			get = JsonConvert.DeserializeObject<KeySetup>(System.IO.File.ReadAllText("setup.json"));

			this.TetrisLeft = get.TetrisLeft;
			this.TetrisRight = get.TetrisRight;

			this.BetaBasic = get.BetaBasic;
			this.HoldSlot = get.HoldSlot;

			this.InstantDrop = get.InstantDrop;
			this.FastDrop = get.FastDrop;

			this.RotateLeft = get.RotateLeft;
			this.RotateRight = get.RotateRight;

			this.Scores = get.Scores;
			this.BestScore = get.BestScore;
		}
	}
}
