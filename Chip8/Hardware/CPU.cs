using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8.Hardware
{
	class CPU
	{
		//CHIP-8 CPU

		//Two bytes for each opcode (16 bits)
		private ushort currentOpcode;

		//RAM - Set to 4K in the constructor (4096 bytes)
		private byte[] ram;

		//CPU REGISTERS - Set to 16 bytes in the constructor (1 byte for each register. 16th is for the carry flag)
		private byte[] v;

		//INDEX REGISTER - Can contain a value from 0x000 to 0xFFF
		private ushort i;

		//PROGRAM COUNTER - Can contain a value from 0x000 to 0xFFF
		private ushort pc;

		//GRAPHICS REGISTER - Contains pixel state ranging across the screen resolution (64x32)
		public byte[] graphics;
		public bool DrawFlag { get; set; }

		//TIMER REGISTERS - Count down to zero at 60Hz
		private byte delayTimer;
		private byte soundTimer;

		//INTERPRETER STACK
		private ushort[] stack;

		//INTERPRETER STACK POINTER - points to the last used place in the stack
		private ushort sp;

		//HEX-BASED KEYPAD
		public byte[] Key { get; set; }

		//FONTSET
		private byte[] fontset;

		private const ushort appStart = 0x200;
		public CPU()
		{
			ram = new byte[4096];
			v = new byte[16];
			graphics = new byte[64 * 32];
			stack = new ushort[16];
			Key = new byte[16];

			fontset = new byte[80] {
				0xF0, 0x90, 0x90, 0x90, 0xF0,	// 0
				0x20, 0x60, 0x20, 0x20, 0x70,	// 1
				0xF0, 0x10, 0xF0, 0x80, 0xF0,	// 2
				0xF0, 0x10, 0xF0, 0x10, 0xF0,	// 3
				0x90, 0x90, 0xF0, 0x10, 0x10,	// 4
				0xF0, 0x80, 0xF0, 0x10, 0xF0,	// 5
				0xF0, 0x80, 0xF0, 0x90, 0xF0,	// 6
				0xF0, 0x10, 0x20, 0x40, 0x40,	// 7
				0xF0, 0x90, 0xF0, 0x90, 0xF0,	// 8
				0xF0, 0x90, 0xF0, 0x10, 0xF0,	// 9
				0xF0, 0x90, 0xF0, 0x90, 0x90,	// A
				0xE0, 0x90, 0xE0, 0x90, 0xE0,	// B
				0xF0, 0x80, 0x80, 0x80, 0xF0,	// C
				0xE0, 0x90, 0x90, 0x90, 0xE0,	// D
				0xF0, 0x80, 0xF0, 0x80, 0xF0,	// E
				0xF0, 0x80, 0xF0, 0x80, 0x80
			};
		}

		public void Init()
		{
			Array.Clear(ram, 0, ram.Length);
			Array.Clear(v, 0, v.Length);
			Array.Clear(graphics, 0, graphics.Length);
			Array.Clear(stack, 0, stack.Length);
			Array.Clear(Key, 0, Key.Length);

			pc = appStart;	//Start counter at 200h (application start)
			currentOpcode = 0;
			i = 0;
			sp = 0;

			LoadFontset();

			delayTimer = 0;
			soundTimer = 0;
		}

		private void LoadFontset()
		{
			for (var i = 0; i < 80; ++i)
			{
				ram[i] = fontset[i];
			}
		}

		public void LoadApplication(string path)
		{
			var appStartInt = Convert.ToInt32(appStart);
			var app = File.ReadAllBytes(path);

			for (var i = 0; i < app.Length; ++i)
			{
				ram[i + appStartInt] = app[i];
			}
		}

		public void Emulate()
		{
			//Fetch opcode
			currentOpcode = (ushort)(ram[pc] << 8 | ram[pc+1]);
			
			//Decode opcode
			switch (currentOpcode & 0xF000)
			{
				case 0x0000:
					switch (currentOpcode & 0x000F)
					{
						case 0x0000: //0x00E0 Clear the screen
							Array.Clear(graphics, 0, graphics.Length);
							pc += 2;
							break;

						case 0x000E: //0x00EE Return from subroutine
							--sp;
							pc = stack[sp];
							pc += 2;
							break;

						default:
							Console.WriteLine("Unknown Opcode [0x0000]: " + currentOpcode);
							break;
					}
					break;
				
				case 0x1000: //1NNN Jump to NNN
					pc = (ushort)(currentOpcode & 0x0FFF);
					break;
					
				case 0x2000: //2NNN Set PC to NNN
					stack[sp] = pc;
					++sp;
					pc = (ushort)(currentOpcode & 0x0FFF);
					break;
					
				case 0x3000: //3xKK Skip next opcode if Vx == KK
					if (v[(currentOpcode & 0x0F00) >> 8] == (currentOpcode & 0x00FF))
						pc += 4;
					else
						pc += 2;
					break;
				
				case 0x4000: //4xKK Skip next opcode if Vx != KK
					if (v[(currentOpcode & 0x0F00) >> 8] != (currentOpcode & 0x00FF))
						pc += 4;
					else
						pc += 2;
					break;
					
				case 0x5000: //5xy0 Skip next opcode if Vx == Vy
					if (v[(currentOpcode & 0x0F00) >> 8] == v[(currentOpcode & 0x00F0) >> 4])
						pc += 4;
					else
						pc += 2;
					break;
					
				case 0x6000: //6xKK Set Vx to KK
					v[(currentOpcode & 0x0F00) >> 8] = (byte)(currentOpcode & 0x00FF);
					pc += 2;
					break;
					
				case 0x7000: //7xKK Add KK to Vx and set Vx to the result
					v[(currentOpcode & 0x0F00) >> 8] += (byte)(currentOpcode & 0x00FF);
					pc += 2;
					break;
					
				case 0x8000:
					switch (currentOpcode & 0x000F) {
						case 0x0000: //8xy0 Set Vx = Vy
							v[(currentOpcode & 0x0F00) >> 8] = v[(currentOpcode & 0x00F0) >> 4];
							pc += 2;
							break;
							
						case 0x0001: //8xy1 Bitwise OR on Vx, Vy and set Vx to the result
							v[(currentOpcode & 0x0F00) >> 8] |= v[(currentOpcode & 0x00F0) >> 4];
							pc += 2;
							break;
							
						case 0x0002: //8xy2 Bitwise AND on Vx, Vy and set Vx to the result
							v[(currentOpcode & 0x0F00) >> 8] &= v[(currentOpcode & 0x00F0) >> 4];
							pc += 2;
							break;
							
						case 0x0003: //8xy3 Bitwise XOR on Vx, Vy and set Vx to the result
							v[(currentOpcode & 0x0F00) >> 8] ^= v[(currentOpcode & 0x00F0) >> 4];
                            pc += 2;
							break;

						case 0x0004: //8xy4 Vx = Vx + Vy, Set VF carry flag if result > 8 bits
							if (v[(currentOpcode & 0x00F0) >> 4] > v[(currentOpcode & 0x0F00) >> 8])
								v[0xF] = 1;	//Set carry flag
							else
								v[0xF] = 0;

							v[(currentOpcode & 0x0F00) >> 8] += v[(currentOpcode & 0x00F0) >> 4];
							pc += 2;
							break;

						case 0x0005: //8xy5 Vx = Vx - Vy. If Vx > Vy, Set VF to 1 else set VF to 0
							if (v[(currentOpcode & 0x0F00) >> 8] > v[(currentOpcode & 0x00F0) >> 4])
								v[0xF] = 1;
							else
								v[0xF] = 0;

							v[(currentOpcode & 0x0F00) >> 8] -= v[(currentOpcode & 0x00F0) >> 4];
							pc += 2; 
							break;

						case 0x0006: //8xy6 If the LSB of Vx is 1, set VF to 1, else set VF to 0. Divide Vx by 2
							v[0xF] = (byte)(v[(currentOpcode & 0x0F00) >> 8] & 0x1);
							v[(currentOpcode & 0x0F00) >> 8] >>= 1;
							pc += 1;
							break;

						case 0x0007: //8xy7 If Vy > Vx, Set VF to 1, else set VF to 0. Vy - Vx, store result in Vx
							if (v[(currentOpcode & 0x00F0) >> 4] > v[(currentOpcode & 0x0F00) >> 8])
								v[0xF] = 1;
							else
								v[0xF] = 0;

							v[(currentOpcode & 0x0F00) >> 8] = (byte)(v[(currentOpcode & 0x00F0) >> 4] - v[(currentOpcode & 0x0F00) >> 8]);
							pc += 2;
							break;

						case 0x000E: //8xyE If the MSB of Vx is 1, set VF to 1, else set VF to 0. Vx * 2, store result in Vx
							v[0xF] = (byte)(v[(currentOpcode & 0x0F00) >> 8] >> 7);
							v[(currentOpcode & 0x0F00) >> 8] <<= 1;
							pc += 2;
							break;
					}
					break;

				case 0x9000: //9xy0 Skip next opcode if Vx != Vy
					if (v[(currentOpcode & 0x0F00) >> 8] != v[(currentOpcode & 0x00F0) >> 4])
						pc += 4;
					else
						pc += 2;
					break;

				case 0xA000: //ANNN Set i to address NNN
					i = (ushort)(currentOpcode & 0x0FFF);
					pc += 2;
					break;

				case 0xB000: //BNNN Jump to opcode PC = NNN + V0
					pc = (ushort)((currentOpcode & 0x0FFF) + v[0]);
					break;

				case 0xC000: //CxKK Generate random number (0 - 255), AND with KKK and store result in Vx
					Random rand = new Random();
					ushort r = (ushort)rand.Next();
					v[(currentOpcode & 0x0F00) >> 8] = (byte)((r % 0xFF) & (currentOpcode & 0x00FF));
					pc += 2;
					break;

				case 0xD000: //DxyN Read N bytes from memory (starting at address in I), display as sprites at co-ordinates (Vx, Vy)
							 //Sprites are XOR'd onto the screen: if any pixels are erased, set VF to 1 otherwise 0. If outside of the display, wrap around

					ushort x = v[(currentOpcode & 0x0F00) >> 8];
					ushort y = v[(currentOpcode & 0x00F0) >> 4];
					ushort height = (ushort)(currentOpcode & 0x000F);
					ushort pixel;

					v[0xF] = 0;
					
					for (int row = 0; row < height; ++row)
					{
						pixel = ram[i + row];
						
						for (int col = 0; col < 8; ++col)
						{
							if ((pixel & (0x80 >> col)) != 0)
							{
								if (graphics[(x + col + ((y + row) * 64))] == 1)
								{
									v[0xF] = 1;
								}
								graphics[x + col + ((y + row) * 64)] ^= 1;
							}
						}
					}
					DrawFlag = true;
					pc += 2;

					break;

				case 0xE000:
					switch (currentOpcode & 0x00FF)
					{
						case 0x009E: //Ex9E Skip next opcode if key (keyboard) with Vx value is pressed
							if (Key[v[(currentOpcode & 0x0F00) >> 8]] != 0)
								pc += 4;
							else
								pc += 2;
							break;

						case 0x00A1: //ExA1 Skip next opcode if key (keyboard) with Vx is NOT pressed
							if (Key[v[(currentOpcode & 0x0F00) >> 8]] == 0)
								pc += 4;
							else
								pc += 2;
							break;
					}
					break;

				case 0xF000:
					switch (currentOpcode & 0x00FF)
					{
						case 0x0007: //Fx07 Vx = value of delay timer
							v[(currentOpcode & 0x0F00) >> 8] = delayTimer;
							pc += 2;
							break;

						case 0x000A: //Fx0A Pause until key press, store the key value in Vx
							var keyPressed = false;

							for (var i = 0; i < Key.Length; ++i)
							{
								if (Key[i] != 0)
								{
									v[(currentOpcode & 0x0F00) >> 8] = (byte)i;
									keyPressed = true;
								}
							}

							if (!keyPressed)
								return;

							pc += 2;
							break;

						case 0x0015: //Fx15 Delay timer = value of Vx
							delayTimer = v[(currentOpcode & 0x0F00) >> 8];
							pc += 2;
							break;

						case 0x0018: //Fx18 Vx = value of sound timer
							v[(currentOpcode & 0x0F00) >> 8] = soundTimer;
							pc += 2;
							break;

						case 0x001E: //Fx1E I = I + Vx
							if (i + v[(currentOpcode & 0x0F00) >> 8] > 0xFFF)
								v[0xF] = 1;
							else
								v[0xF] = 0;

							i +=  v[(currentOpcode & 0x0F00) >> 8];
							pc += 2;
							break;

						case 0x0029: //Fx29 I = Location of sprite Vx
							i = (ushort)(v[(currentOpcode & 0x0F00) >> 8] * 0x5);
							pc += 2;
							break;

						case 0x0033: //Fx33 Store BCD representation of Vx in ram at I (hundreds), I+1 (tens) and I+2 (units)
							ram[i] = (byte)(v[(currentOpcode & 0x0F00) >> 8] / 100);
							ram[i + 1] = (byte)((v[(currentOpcode & 0x0F00) >> 8] / 10) % 10);
							ram[i + 2] = (byte)((v[(currentOpcode & 0x0F00) >> 8] % 100) % 10);
							pc += 2;
							break;

						case 0x0055: //Fx55 Store V0 -> Vx in memory, starting at the address in I
							for (var n = 0; n <= v[(currentOpcode & 0x0F00) >> 8]; ++n)
							{
								ram[i + n] = v[n];
							}

							i += (ushort)(((currentOpcode & 0x0F00) >> 8) + 1);
							pc += 2;
							break;

						case 0x0065: //Fx65 Read into V0 -> Vx, starting at the address in I
							for (var n = 0; n <= v[(currentOpcode & 0x0F00) >> 8]; ++n)
							{
								v[n] = ram[i + n];
							}

							i += (ushort)(((currentOpcode & 0x0F00) >> 8) + 1);
							pc += 2;
							break;
					}
					break;

				default:
					Console.WriteLine("Unknown Opcode: 0x" + currentOpcode);
					break;
			}

			if (delayTimer > 0)
				delayTimer--;

			if (soundTimer > 0)
			{
				if (soundTimer == 1)
				{
					Console.WriteLine("BEEP!");
				}
				soundTimer--;
			}
		}
	}
}
