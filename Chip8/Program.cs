using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.Window;
using SFML;
using Chip8.Hardware;

namespace Chip8
{
	class Program
	{
		private static CPU cpu;
		static void Main(string[] args)
		{
			RenderWindow window = new RenderWindow(new VideoMode(640, 320), "Chip 8!");
			window.SetFramerateLimit(60);
			window.Closed += new EventHandler(OnClose);
			window.KeyPressed += new EventHandler<KeyEventArgs>(KeyDown);
			window.KeyReleased += new EventHandler<KeyEventArgs>(KeyUp);

			cpu = new CPU();
			cpu.Init();
			cpu.LoadApplication("tetris.c8");

			RectangleShape shape = new RectangleShape(new Vector2f(10, 10));
			var y = 0;
			var x = 0;

			while (window.IsOpen())
			{
				window.DispatchEvents();
				cpu.Emulate();
				if(cpu.DrawFlag)
				{
					y = 0;

					for (int i = 0; i < cpu.graphics.Length; ++i)
					{
						shape.Position = new Vector2f(x * 10, y * 10);
						++x;
						if(cpu.graphics[i] != 0)
						{
							shape.FillColor = new Color(255, 255, 255);
						} else
						{
							shape.FillColor = new Color(100, 100, 100);
						}

						if (x == 64)
						{
							x = 0;
							y += 1;
						}
						window.Draw(shape);
					}
				}

				window.Display();
			}
		}

		static void OnClose(object sender, EventArgs e)
		{
			RenderWindow window = (RenderWindow)sender;
			window.Close();
		}

		static void KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Code)
			{
				case Keyboard.Key.Num1:
					cpu.Key[0x1] = 1;
					break;

				case Keyboard.Key.Num2:
					cpu.Key[0x2] = 1;
					break;

				case Keyboard.Key.Num3:
					cpu.Key[0x3] = 1;
					break;

				case Keyboard.Key.Num4:
					cpu.Key[0xC] = 1;
					break;

				case Keyboard.Key.Q:
					cpu.Key[0x4] = 1;
					break;

				case Keyboard.Key.W:
					cpu.Key[0x5] = 1;
					break;

				case Keyboard.Key.E:
					cpu.Key[0x6] = 1;
					break;

				case Keyboard.Key.R:
					cpu.Key[0xD] = 1;
					break;

				case Keyboard.Key.A:
					cpu.Key[0x7] = 1;
					break;

				case Keyboard.Key.S:
					cpu.Key[0x8] = 1;
					break;

				case Keyboard.Key.D:
					cpu.Key[0x9] = 1;
					break;

				case Keyboard.Key.F:
					cpu.Key[0xE] = 1;
					break;

				case Keyboard.Key.Z:
					cpu.Key[0xA] = 1;
					break;

				case Keyboard.Key.X:
					cpu.Key[0x0] = 1;
					break;

				case Keyboard.Key.C:
					cpu.Key[0xB] = 1;
					break;

				case Keyboard.Key.V:
					cpu.Key[0xF] = 1;
					break;
			}
		}

		static void KeyUp(object sender, KeyEventArgs e)
		{
			switch (e.Code)
			{
				case Keyboard.Key.Num0:
					cpu.Key[0x1] = 0;
					break;

				case Keyboard.Key.Num2:
					cpu.Key[0x2] = 0;
					break;

				case Keyboard.Key.Num3:
					cpu.Key[0x3] = 0;
					break;

				case Keyboard.Key.Num4:
					cpu.Key[0xC] = 0;
					break;

				case Keyboard.Key.Q:
					cpu.Key[0x4] = 0;
					break;

				case Keyboard.Key.W:
					cpu.Key[0x5] = 0;
					break;

				case Keyboard.Key.E:
					cpu.Key[0x6] = 0;
					break;

				case Keyboard.Key.R:
					cpu.Key[0xD] = 0;
					break;

				case Keyboard.Key.A:
					cpu.Key[0x7] = 0;
					break;

				case Keyboard.Key.S:
					cpu.Key[0x8] = 0;
					break;

				case Keyboard.Key.D:
					cpu.Key[0x9] = 0;
					break;

				case Keyboard.Key.F:
					cpu.Key[0xE] = 0;
					break;

				case Keyboard.Key.Z:
					cpu.Key[0xA] = 0;
					break;

				case Keyboard.Key.X:
					cpu.Key[0x0] = 0;
					break;

				case Keyboard.Key.C:
					cpu.Key[0xB] = 0;
					break;

				case Keyboard.Key.V:
					cpu.Key[0xF] = 0;
					break;
			}
		}
	}
}
