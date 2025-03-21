using System;
using System.Threading;
using System.Diagnostics;

namespace TetrisExam
{
    class Program
    {
        //Map or Background
        const int mapSizeX = 10;
        const int mapSizeY = 20;
        static char[,] bg = new char[mapSizeY, mapSizeX];

        static int score = 0;

        //Variables for Hold
        const int holdSizeX = 6;
        const int holdSizeY = mapSizeY;
        static int holdIndex = -1;
        static char holdChar;

        const int upNextSize = 6;

        static ConsoleKeyInfo input;

        //Current Information
        static int currentX = 0;
        static int currentY = 0;
        static char currentChar = 'O';

        static int currentRot = 0;

        //Block and Bags
        static int[] bag;
        static int[] nextBag;

        static int bagIndex;
        static int currentIndex;


        // Timer, etc
        static int maxTime = 20;
        static int timer = 0;
        static int amount = 0;

        #region Assets

        readonly static string characters = "OILJSZT";
        readonly static int[,,,] positions =
            {
        {
        {{0,0},{1,0},{0,1},{1,1}},
        {{0,0},{1,0},{0,1},{1,1}},
        {{0,0},{1,0},{0,1},{1,1}},
        {{0,0},{1,0},{0,1},{1,1}}
        },

        {
        {{2,0},{2,1},{2,2},{2,3}},
        {{0,2},{1,2},{2,2},{3,2}},
        {{1,0},{1,1},{1,2},{1,3}},
        {{0,1},{1,1},{2,1},{3,1}},
        },
        {
        {{1,0},{1,1},{1,2},{2,2}},
        {{1,2},{1,1},{2,1},{3,1}},
        {{1,1},{2,1},{2,2},{2,3}},
        {{2,1},{2,2},{1,2},{0,2}}
        },

        {
        {{2,0},{2,1},{2,2},{1,2}},
        {{1,1},{1,2},{2,2},{3,2}},
        {{2,1},{1,1},{1,2},{1,3}},
        {{0,1},{1,1},{2,1},{2,2}}
        },

        {
        {{2,1},{1,1},{1,2},{0,2}},
        {{1,0},{1,1},{2,1},{2,2}},
        {{2,1},{1,1},{1,2},{0,2}},
        {{1,0},{1,1},{2,1},{2,2}}
        },
        {
        {{0,1},{1,1},{1,2},{2,2}},
        {{1,0},{1,1},{0,1},{0,2}},
        {{0,1},{1,1},{1,2},{2,2}},
        {{1,0},{1,1},{0,1},{0,2}}
        },

        {
        {{0,1},{1,1},{1,0},{2,1}},
        {{1,0},{1,1},{2,1},{1,2}},
        {{0,1},{1,1},{1,2},{2,1}},
        {{1,0},{1,1},{0,1},{1,2}}
        }
        };
        #endregion

        static void Main()
        {
            Console.CursorVisible = false;
            Console.Clear(); // Clear once at the beginning

            Thread inputThread = new Thread(Input);
            inputThread.Start();

            //For generating empty background
            for (int y = 0; y < mapSizeY; y++)
                for (int x = 0; x < mapSizeX; x++)
                    bg[y, x] = '-';

            //To generate bag / current block
            bag = GenerateBag();
            nextBag = GenerateBag();
            NewBlock();

            while (true)
            {
                //To force block to go down
                if (timer >= maxTime)
                {
                    if (!Collision(currentIndex, bg, currentX, currentY + 1, currentRot)) currentY++;
                    else BlockDownCollision();

                    timer = 0;
                }
                timer++;

                InputTextriz();
                input = new ConsoleKeyInfo();

                //Render Game board
                char[,] view = RenderView();

                //Render currently held block
                char[,] hold = RenderHold();

                //Render upcoming blocks
                char[,] next = RenderNextBlocks();

                //Print everything to the screen
                // Remove the Console.Clear() line here
                Colour(view, hold, next);

                Thread.Sleep(20); //In order not to overload the processor
            }
        }

        static int[] GenerateBag()
        {
            // This is not my code, source https://stackoverflow.com/questions/108819/best-way-to-randomize-an-array-with-net
            Random random = new Random();
            int n = 7;
            int[] ret = { 0, 1, 2, 3, 4, 5, 6 };
            while (n > 1)
            {
                int k = random.Next(n--);
                int temp = ret[n];
                ret[n] = ret[k];
                ret[k] = temp;

            }
            return ret;

        }

        static void Input()
        {
            while (true)
            {
                input = Console.ReadKey(true);
            }
        }

        static void NewBlock()
        {
            //Check if new bag is needed
            if (bagIndex >= 7)
            {
                bagIndex = 0;
                bag = nextBag;
                nextBag = GenerateBag();
            }

            //Reset everything
            currentY = 0;
            currentX = 4;
            currentChar = characters[bag[bagIndex]];
            currentIndex = bag[bagIndex];

            //Check if the next block position collides, If it does its gameover
            if (Collision(currentIndex, bg, currentX, currentY, currentRot) && amount > 0)
            {
                GameOver();
            }
            bagIndex++;
            amount++;
        }


        static bool Collision(int index, char[,] bg, int x, int y, int rot)
        {
            for (int i = 0; i < positions.GetLength(2); i++)
            {
                int checkX = positions[index, rot, i, 0] + x;
                int checkY = positions[index, rot, i, 1] + y;

                // Check if out of bounds (left, right, bottom)
                if (checkX < 0 || checkX >= mapSizeX || checkY >= mapSizeY)
                    return true;

                // Check if colliding with existing block
                if (checkY >= 0 && bg[checkY, checkX] != '-')
                    return true;
            }
            return false;
        }

        static void BlockDownCollision()
        {
            //Adding blocks from current to background 
            for (int i = 0; i < positions.GetLength(2); i++)
            {
                bg[positions[currentIndex, currentRot, i, 1] + currentY, positions[currentIndex, currentRot, i, 0] + currentX] = currentChar;
            }

            while (true)
            {
                // Check for line
                int lineY = Line(bg);

                // If a line is detected
                if (lineY != -1)
                {
                    ClearLine(lineY);

                    continue;
                }
                break;
            }
            //New Block
            NewBlock();
        }
        static void InputTextriz()
        {
            switch (input.Key)
            {
                //Going left
                case ConsoleKey.A:
                case ConsoleKey.LeftArrow:
                    if (!Collision(currentIndex, bg, currentX - 1, currentY, currentRot)) currentX -= 1;
                    break;

                //Going right
                case ConsoleKey.D:
                case ConsoleKey.RightArrow:
                    if (!Collision(currentIndex, bg, currentX + 1, currentY, currentRot)) currentX += 1;
                    break;

                //Rotating the block
                case ConsoleKey.W:
                case ConsoleKey.UpArrow:
                    int newRot = currentRot + 1;
                    if (newRot >= 4) newRot = 0;
                    if (!Collision(currentIndex, bg, currentX, currentY, newRot)) currentRot = newRot;

                    break;

                //Moving the block instantly downwards 
                case ConsoleKey.Spacebar:
                    int i = 0;
                    while (true)
                    {
                        i++;
                        if (Collision(currentIndex, bg, currentX, currentY + i, currentRot))
                        {
                            currentY += i - 1;
                            break;
                        }
                    }
                    score += i + 1;
                    break;

                //Stopping the game
                case ConsoleKey.Escape:
                    Environment.Exit(1);
                    break;

                //Command to hold the block
                case ConsoleKey.Enter:

                    //If there's no block being held
                    if (holdIndex == -1)
                    {
                        holdIndex = currentIndex;
                        holdChar = currentChar;
                        NewBlock();
                    }

                    //If there is
                    else
                    {
                        if (!Collision(holdIndex, bg, currentX, currentY, 0)) //Checking for collision
                        {
                            //Switching current block with held block
                            int c = currentIndex;
                            char ch = currentChar;
                            currentIndex = holdIndex;
                            currentChar = holdChar;
                            holdIndex = c;
                            holdChar = ch;
                        }
                    }
                    break;

                // Going down faster
                case ConsoleKey.S:
                case ConsoleKey.DownArrow:
                    timer = maxTime;
                    break;

                case ConsoleKey.R:
                    Restart();
                    break;

                default:
                    break;

            }
        }

        static char[,] RenderView()
        {
            char[,] view = new char[mapSizeY, mapSizeX];

            //To make view equal to background
            for (int y = 0; y < mapSizeY; y++)
                for (int x = 0; x < mapSizeX; x++)
                    view[y, x] = bg[y, x];

            //Overlay current
            for (int i = 0; i < positions.GetLength(2); i++)
            {
                int posY = positions[currentIndex, currentRot, i, 1] + currentY;
                int posX = positions[currentIndex, currentRot, i, 0] + currentX;

                // Make sure we're within bounds
                if (posY >= 0 && posY < mapSizeY && posX >= 0 && posX < mapSizeX)
                {
                    view[posY, posX] = currentChar;
                }
            }
            return view;
        }


        static char[,] RenderHold()
        {
            char[,] hold = new char[holdSizeY, holdSizeX];

            for (int y = 0; y < holdSizeY; y++)
                for (int x = 0; x < holdSizeX; x++)
                    hold[y, x] = ' ';

            //If there is a block held
            if (holdIndex != -1)
            {
                //Overlays blocks from hold
                for (int i = 0; i < positions.GetLength(2); i++)
                {
                    hold[positions[holdIndex, 0, i, 1] + 1, positions[holdIndex, 0, i, 0] + 1] = holdChar;
                }
            }
            return hold;
        }


        static char[,] RenderNextBlocks()
        {
            char[,] next = new char[mapSizeY, upNextSize];
            for (int y = 0; y < mapSizeY; y++)
                for (int x = 0; x < upNextSize; x++)
                    next[y, x] = ' ';

            int nextBagIndex = 0;
            for (int i = 0; i < 3; i++) // For next 3 blocks
            {

                for (int l = 0; l < positions.GetLength(2); l++)
                {
                    if (i + bagIndex >= 7) //If access to the next bag is needed
                        next[positions[nextBag[nextBagIndex], 0, l, 1] + 5 * i, positions[nextBag[nextBagIndex], 0, l, 0] + 1] = characters[nextBag[nextBagIndex]];
                    else
                        next[positions[bag[bagIndex + i], 0, l, 1] + 5 * i, positions[bag[bagIndex + i], 0, l, 0] + 1] = characters[bag[bagIndex + i]];
                }
                if (i + bagIndex >= 7) nextBagIndex++;
            }
            return next;

        }

        static void Restart()
        {
            //Got this code from a friend
            var applicationPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            Process.Start(applicationPath);
            Environment.Exit(Environment.ExitCode);
        }

        static void ClearLine(int lineY)
        {
            score += 40;
            //Clear the said line
            for (int x = 0; x < mapSizeX; x++) bg[lineY, x] = '-';

            //Loop through all blocks above line 
            for (int y = lineY - 1; y > 0; y--)
            {
                for (int x = 0; x < mapSizeX; x++)
                {
                    //Moving each character down
                    char character = bg[y, x];
                    if (character != '-')
                    {
                        bg[y, x] = '-';
                        bg[y + 1, x] = character;
                    }
                }
            }
        }

        static int Line(char[,] bg)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                bool i = true;
                for (int x = 0; x < mapSizeX; x++)
                {
                    if (bg[y, x] == '-')
                    {
                        i = false;
                    }
                }
                if (i) return y;
            }

            return -1;
        }

        static char[,] previousScreen = null;

        static void Colour(char[,] view, char[,] hold, char[,] next)
        {
            // Initialize the previous screen buffer if it's the first frame
            if (previousScreen == null)
            {
                previousScreen = new char[mapSizeY, holdSizeX + mapSizeX + upNextSize];
                for (int y = 0; y < mapSizeY; y++)
                    for (int x = 0; x < holdSizeX + mapSizeX + upNextSize; x++)
                        previousScreen[y, x] = ' ';

                // Force a full redraw on first frame
                Console.Clear();
            }

            // Draw a border around the game area
            Console.SetCursorPosition(holdSizeX - 1, 0);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write('+');
            Console.SetCursorPosition(holdSizeX + mapSizeX, 0);
            Console.Write('+');

            for (int y = 0; y < mapSizeY; y++)
            {
                // Draw left border
                Console.SetCursorPosition(holdSizeX - 1, y);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write('|');

                // Draw right border
                Console.SetCursorPosition(holdSizeX + mapSizeX, y);
                Console.Write('|');

                for (int x = 0; x < holdSizeX + mapSizeX + upNextSize; x++)
                {
                    char i = ' ';

                    if (x < holdSizeX)
                        i = hold[y, x];
                    else if (x >= holdSizeX && x < holdSizeX + mapSizeX)
                    {
                        // Ensure background is always shown
                        i = view[y, (x - holdSizeX)];

                        // If the character is empty space, show the background grid
                        if (i == ' ')
                            i = '-';
                    }
                    else if (x >= holdSizeX + mapSizeX && x < holdSizeX + mapSizeX + upNextSize)
                        i = next[y, x - holdSizeX - mapSizeX];

                    // Only update the character if it has changed
                    if (i != previousScreen[y, x])
                    {
                        Console.SetCursorPosition(x, y);

                        //Block Colours
                        switch (i)
                        {
                            case 'O':
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.Write(i);
                                break;
                            case 'I':
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write(i);
                                break;
                            case 'T':
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.Write(i);
                                break;
                            case 'S':
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write(i);
                                break;
                            case 'Z':
                                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                                Console.Write(i);
                                break;
                            case 'L':
                                Console.ForegroundColor = ConsoleColor.DarkCyan;
                                Console.Write(i);
                                break;
                            case 'J':
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.Write(i);
                                break;
                            case '-':
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                Console.Write(i);
                                break;
                            default:
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                Console.Write(i);
                                break;
                        }

                        // Update the buffer
                        previousScreen[y, x] = i;
                    }
                }
            }

            // Draw bottom border
            Console.SetCursorPosition(holdSizeX - 1, mapSizeY);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write('+');
            for (int x = 0; x < mapSizeX; x++)
            {
                Console.Write('-');
            }
            Console.Write('+');

            // Display score
            Console.SetCursorPosition(holdSizeX + mapSizeX + 3, 1);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("Score: " + score + "   ");

            // Display controls
            Console.SetCursorPosition(holdSizeX + mapSizeX + 3, 3);
            Console.Write("Controls:");
            Console.SetCursorPosition(holdSizeX + mapSizeX + 3, 4);
            Console.Write("←→: Move");
            Console.SetCursorPosition(holdSizeX + mapSizeX + 3, 5);
            Console.Write("↑: Rotate");
            Console.SetCursorPosition(holdSizeX + mapSizeX + 3, 6);
            Console.Write("↓: Down");
            Console.SetCursorPosition(holdSizeX + mapSizeX + 3, 7);
            Console.Write("Space: Drop");
            Console.SetCursorPosition(holdSizeX + mapSizeX + 3, 8);
            Console.Write("Enter: Hold");
            Console.SetCursorPosition(holdSizeX + mapSizeX + 3, 9);
            Console.Write("R: Restart");
            Console.SetCursorPosition(holdSizeX + mapSizeX + 3, 10);
            Console.Write("Esc: Exit");
        }


        static void GameOver()
        {
            Environment.Exit(1);
        }
    }
}