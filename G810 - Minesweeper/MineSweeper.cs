using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Windows.Forms;

namespace G810___Minesweeper
{
    static class MineSweeper
    {
        #region variablen konstruktor und eigenschaften

        static int[,] map;
        static bool[,] isBomb;
        static bool[,] isFlag = new bool[13, 6];
        static int[,] display;
        static int bombs = 13;
        static int covered = 11 * 4;
        static int wins = 0;
        static bool gameRunning;
        static bool firstMove = true;
        static string keyboardLayout = "US";
        static bool setBackground = false;
        public static byte[,] colors =
            {
                // Bomben in der Umgebung
                {000,000,000},  //0
                {128,000,128},  //1
                {255,255,000},  //2
                {000,128,000},  //3
                {000,255,255},  //4
                {000,127,255},  //5
                {000,000,255},  //6

                // Bombe
                {000,000,255},  //7

                //Clear
                {255,255,255},   //8

                //nicht Spielfeld
                {255,200,200},    //9

                //Flag
                {255,000,255},   //10

                //New Game Key
                {255,000,000},   //11

                //Game Lost background
                {000,000,255},   //12

                //Game Won background
                {000,255,255},   //13

                //New Game background
                {255,160,160},   //14
        };

        static MineSweeper()
        {
            //Initialisiert die SDK
            if (!LogitechGSDK.LogiLedInit())Console.Write("Not connected to GSDK");

            newGame();
        }

        static public int Wins
        {
            get { return wins; }
            set
            {
                wins = value;
                printLogiLED();
            }
        }

        static public int Bombs
        {
            get { return bombs; }
            set
            {
                bombs = value;
            }
        }

        static public string KeyboardLayout
        {
            get { return keyboardLayout; }
            set
            {
                if (value != "DE" && value != "US")
                {
                    throw new Exception("Only German or Us layout allowed. (DE or US)");
                }
                keyboardLayout = value;
            }
        }

        #endregion

        static public void newGame()
        {
            //damit man mit jeder taste starten kanN
            InterceptKeys.last = "Add";


            display = new int[21, 6];
            for (int i = 0; i < 21; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (i > 0 && i < 12 && j > 0 && j < 5)
                    {
                        display[i, j] = 8;
                        if (keyboardLayout == "US")
                        {
                            if (i == 1 && j == 4)
                            {
                                display[i, j] = 9;
                            }
                        }
                    }
                    else
                    {
                        display[i, j] = 9;
                    }
                }
            }

            if (keyboardLayout == "US")
            {
                covered = 43;
            }
            else
            {
                covered = 44;
            }

            firstMove = true;
            genBombs();

            genMap();
            isFlag = new bool[13, 6];

            colors[9, 0] = colors[14, 0];
            colors[9, 1] = colors[14, 1];
            colors[9, 2] = colors[14, 2];
            gameRunning = true;

            Thread.Sleep(100);

            setBackground = true;
            printLogiLED();
        }


        static public void keyPressed(int i)
        {
            if (!gameRunning)
            {
                var principalForm = Application.OpenForms.OfType<Form1>().Single();
                principalForm.UpdateDisplay();
                principalForm.ResetWatch();
                newGame();
            }
            //Restart Game wenn num pad plus gedrückt wird
            else if (i == 99)
            {
                var principalForm = Application.OpenForms.OfType<Form1>().Single();
                principalForm.UpdateDisplay();
                principalForm.StopWatchDefeat();
                principalForm.ResetWatch();
                newGame();
            }
            //Tastendruck nicht annehmen wenn Flag da ist
            else if (display[(i % 11) + 1, (i / 11) + 1] == 10)
            {
            }
            else if (display[(i % 11) + 1, (i / 11) + 1] <= 6 && display[(i % 11) + 1, (i / 11) + 1] >= 0)
            {
                uncoverFlags(i % 11, i / 11);
            }
            //damit man nicht beim ersten zug bombe aufdeckt
            else if (firstMove)
            {
                if (isBomb[(i % 11) + 1, (i / 11) + 1])
                {
                    MoveBomb((i % 11) + 1, (i / 11) + 1);
                }

                //start timer
                var principalForm = Application.OpenForms.OfType<Form1>().Single();
                principalForm.UpdateDisplay();
                principalForm.StartWatch();


                firstMove = false;
                uncover(i % 11, i / 11);
                printLogiLED();
            }
            else
            {
                uncover(i % 11, i / 11);
                printLogiLED();
                //Console.Write(print());
            }
        }

        static private void MoveBomb(int x, int y)
        {
            Random r = new Random();
            for (int i = 1; i <= 1; i++)
            {
                int a = r.Next(1, isBomb.GetLength(0) - 1);
                int b = r.Next(1, isBomb.GetLength(1) - 1);
                if (keyboardLayout == "US")
                {
                    if (y == 4 && x == 1)
                    {
                        i--;
                        continue;
                    }
                }
                if (isBomb[a, b]) i--;
                else isBomb[a, b] = true;
            }

            isBomb[x, y] = false;
            genMap();
        }

        static public void SetFlag(int i)
        {
            //Auch bei setflag damit man auch restarten kann wenn shift gedrückt ist
            if (i == 99)
            {
                var principalForm = Application.OpenForms.OfType<Form1>().Single();
                principalForm.UpdateDisplay();
                principalForm.StopWatchDefeat();
                principalForm.ResetWatch();
                newGame();
            }
            //Flag wegnehmen wen schon da ist
            else if (display[(i % 11) + 1, (i / 11) + 1] == 10)
            {
                display[(i % 11) + 1, (i / 11) + 1] = 8;
                isFlag[(i % 11) + 1, (i / 11) + 1] = false;
            }
            //Flag platzieren wenn das Feld leer ist
            else if (display[(i % 11) + 1, (i / 11) + 1] == 8)
            {
                display[(i % 11) + 1, (i / 11) + 1] = 10;
                isFlag[(i % 11) + 1, (i / 11) + 1] = true;
            }
            printLogiLED();
            //Console.Write(print());
        }


        static private void genBombs()
        {
            isBomb = new bool[13, 6];
            Random r = new Random();
            for (int i = 1; i <= bombs; i++)
            {
                int x = r.Next(1, isBomb.GetLength(0) - 1);
                int y = r.Next(1, isBomb.GetLength(1) - 1);
                if (keyboardLayout == "US")
                {
                    if (y == 4 && x == 1)
                    {
                        i--;
                        continue;
                    }
                }
                if (isBomb[x, y]) i--;
                else isBomb[x, y] = true;
            }
        }

        static private void genMap()
        {
            map = new int[11, 4];

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (keyboardLayout == "US")
                    {
                        if (i == 0 && j == 3)
                        {
                            map[i, j] = 8;
                            continue;
                        }
                    }
                    if (isBomb[i + 1, j + 1]) map[i, j] = 7;
                    else
                    {
                        switch (j)
                        {
                            case 0:
                            case 1:
                                if (isBomb[i + 1, j]) map[i, j]++;
                                if (isBomb[i + 2, j]) map[i, j]++;
                                if (isBomb[i, j + 1]) map[i, j]++;
                                if (isBomb[i + 2, j + 1]) map[i, j]++;
                                if (isBomb[i, j + 2]) map[i, j]++;
                                if (isBomb[i + 1, j + 2]) map[i, j]++;
                                break;
                            case 2:
                                if (isBomb[i + 1, j]) map[i, j]++;
                                if (isBomb[i + 2, j]) map[i, j]++;
                                if (isBomb[i, j + 1]) map[i, j]++;
                                if (isBomb[i + 2, j + 1]) map[i, j]++;
                                if (isBomb[i + 1, j + 2]) map[i, j]++;
                                if (isBomb[i + 2, j + 2]) map[i, j]++;
                                break;
                            case 3:
                                if (isBomb[i, j]) map[i, j]++;
                                if (isBomb[i + 1, j]) map[i, j]++;
                                if (isBomb[i, j + 1]) map[i, j]++;
                                if (isBomb[i + 2, j + 1]) map[i, j]++;
                                break;
                        }
                    }
                }
            }
        }

        static private string print()
        {
            string s = "";
            for (int i = 0; i < display.GetLength(1); i++)
            {
                for (int j = 0; j < display.GetLength(0); j++)
                {
                    if (j == 0)
                    {
                        switch (i - 1)
                        {
                            case 0:
                                s += "";
                                break;
                            case 1:
                                s += " ";
                                break;
                            case 2:
                                s += "  ";
                                break;
                            case 3:
                                s += " ";
                                break;
                            default:
                                break;
                        }
                    }
                    if (display[j, i] == 7) s += "X ";
                    else if (display[j, i] == 8) s += "- ";
                    else s += display[j, i] + " ";
                }
                s += "\n";
            }
            return s;
        }

        static private void printLogiLED()
        {
            byte[] logiLED = new byte[LogitechGSDK.LOGI_LED_BITMAP_SIZE];

            
            for (int i = 0; i < display.GetLength(1); i++)
            {
                for (int j = 0; j < display.GetLength(0); j++)
                {
                    if (display[j, i] >= 0 && display[j, i] < colors.GetLength(0))
                    {
                        colorToByte(logiLED, (i * 21 + j) * 4, colors[display[j, i], 0], colors[display[j, i], 1], colors[display[j, i], 2]);
                    }
                    else
                    {
                        colorToByte(logiLED, (i * 21 + j) * 4, 255, 255, 255);
                    }
                }
            }

            //LEGENDE
            //numPad 1-3 = 4*21 + 18-20
            colorToByte(logiLED, (4 * 21 + 17) * 4, colors[1, 0], colors[1, 1], colors[1, 2]);
            colorToByte(logiLED, (4 * 21 + 18) * 4, colors[2, 0], colors[2, 1], colors[2, 2]);
            colorToByte(logiLED, (4 * 21 + 19) * 4, colors[3, 0], colors[3, 1], colors[3, 2]);
            //numPad 4-6 = 3*21 + 18-20
            colorToByte(logiLED, (3 * 21 + 17) * 4, colors[4, 0], colors[4, 1], colors[4, 2]);
            colorToByte(logiLED, (3 * 21 + 18) * 4, colors[5, 0], colors[5, 1], colors[5, 2]);
            colorToByte(logiLED, (3 * 21 + 19) * 4, colors[6, 0], colors[6, 1], colors[6, 2]);

            //New Game
            colorToByte(logiLED, 248, colors[11, 0], colors[11, 1], colors[11, 2]);

            //reste score
            //colorToByte(logiLED, 60, colors[13, 0], colors[13, 1], colors[13, 2]);

            //score
            /*for (int i = 0; i < wins; i++)
            {
                if (i >= 12) break;
                colorToByte(logiLED, i * 4 + 4, colors[12,0], colors[12, 1], colors[12, 2]);
            }*/

            if (setBackground)
            {
                setBackground = false;
                LogitechGSDK.LogiLedSetLighting(Convert.ToInt32((Convert.ToDouble(colors[9, 2]) / 255.0) * 100), Convert.ToInt32((Convert.ToDouble(colors[9, 1]) / 255.0) * 100), Convert.ToInt32((Convert.ToDouble(colors[9, 0]) / 255.0) * 100));
            }
            LogitechGSDK.LogiLedSetLightingFromBitmap(logiLED);
        }

        static private void colorToByte(byte[] logiLED, int start, byte blue, byte green, byte red)
        {
            logiLED[start] = blue;
            logiLED[start + 1] = green;
            logiLED[start + 2] = red;
            logiLED[start + 3] = byte.MaxValue;
        }

        static private void uncover(int x, int y)
        {
            //Abbruch wenn x oder y out of range sind
            if (x >= map.GetLength(0) || y >= map.GetLength(1) || x < 0 || y < 0) return;
            //Abbruch wenn das Feld schon aufgedeckt wurde
            if (display[x + 1, y + 1] != 8) return;

            int m = map[x, y];


            if (m < 8 && m >= 0)
            {
                display[x + 1, y + 1] = m;

                //damit man gleich nach aufdecken theoretisch flaggen kann
                InterceptKeys.last = "empty";

                if (--covered <= bombs && m != 7)
                {
                    victory();
                    return;
                }
            }
            if (m == 7)
            {
                colors[9, 0] = colors[12, 0];
                colors[9, 1] = colors[12, 1];
                colors[9, 2] = colors[12, 2];

                //stop timer defeat
                var principalForm = Application.OpenForms.OfType<Form1>().Single();
                principalForm.UpdateDisplay();
                principalForm.StopWatchDefeat();

                gameOver();
            }
            else if (m == 0)
            {
                switch (y)
                {
                    case 0:
                    case 1:
                        uncover(x, y - 1);
                        uncover(x + 1, y - 1);
                        uncover(x - 1, y);
                        uncover(x + 1, y);
                        uncover(x - 1, y + 1);
                        uncover(x, y + 1);
                        break;
                    case 2:
                        uncover(x, y - 1);
                        uncover(x + 1, y - 1);
                        uncover(x - 1, y);
                        uncover(x + 1, y);
                        uncover(x, y + 1);
                        uncover(x + 1, y + 1);
                        break;
                    case 3:
                        uncover(x - 1, y - 1);
                        uncover(x, y - 1);
                        uncover(x - 1, y);
                        uncover(x + 1, y);
                        break;
                }
            }
        }

        static private void uncoverFlags(int x, int y)
        {
            int sourroundingFlags = 0;
            switch (y)
            {
                case 0:
                case 1:
                    if (isFlag[x + 1, y]) sourroundingFlags++;
                    if (isFlag[x + 2, y]) sourroundingFlags++;
                    if (isFlag[x, y + 1]) sourroundingFlags++;
                    if (isFlag[x + 2, y + 1]) sourroundingFlags++;
                    if (isFlag[x, y + 2]) sourroundingFlags++;
                    if (isFlag[x + 1, y + 2]) sourroundingFlags++;
                    break;
                case 2:
                    if (isFlag[x + 1, y]) sourroundingFlags++;
                    if (isFlag[x + 2, y]) sourroundingFlags++;
                    if (isFlag[x, y + 1]) sourroundingFlags++;
                    if (isFlag[x + 2, y + 1]) sourroundingFlags++;
                    if (isFlag[x + 1, y + 2]) sourroundingFlags++;
                    if (isFlag[x + 2, y + 2]) sourroundingFlags++;
                    break;
                case 3:
                    if (isFlag[x, y]) sourroundingFlags++;
                    if (isFlag[x + 1, y]) sourroundingFlags++;
                    if (isFlag[x, y + 1]) sourroundingFlags++;
                    if (isFlag[x + 2, y + 1]) sourroundingFlags++;
                    break;
            }

            if (display[x + 1, y + 1] <= sourroundingFlags)
            {
                switch (y)
                {
                    case 0:
                    case 1:
                        if (!isFlag[x + 1, y]) uncover(x, y - 1);
                        if (!isFlag[x + 2, y]) uncover(x + 1, y - 1);
                        if (!isFlag[x, y + 1]) uncover(x - 1, y);
                        if (!isFlag[x + 2, y + 1]) uncover(x + 1, y);
                        if (!isFlag[x, y + 2]) uncover(x - 1, y + 1);
                        if (!isFlag[x + 1, y + 2]) uncover(x, y + 1);
                        break;
                    case 2:
                        if (!isFlag[x + 1, y]) uncover(x, y - 1);
                        if (!isFlag[x + 2, y]) uncover(x + 1, y - 1);
                        if (!isFlag[x, y + 1]) uncover(x - 1, y);
                        if (!isFlag[x + 2, y + 1]) uncover(x + 1, y);
                        if (!isFlag[x + 1, y + 2]) uncover(x, y + 1);
                        if (!isFlag[x + 2, y + 2]) uncover(x + 1, y + 1);
                        break;
                    case 3:
                        if (!isFlag[x, y]) uncover(x - 1, y - 1);
                        if (!isFlag[x + 1, y]) uncover(x, y - 1);
                        if (!isFlag[x, y + 1]) uncover(x - 1, y);
                        if (!isFlag[x + 2, y + 1]) uncover(x + 1, y);
                        break;
                }
            }

            printLogiLED();
        }

        static private void gameOver()
        {
            //damit man nicht new game spammen kann
            InterceptKeys.last = "empty";
            for (int i = 0; i < isBomb.GetLength(0); i++)
            {
                for (int j = 0; j < isBomb.GetLength(1); j++)
                {
                    if (isBomb[i, j]) display[i, j] = 7;

                }
            }

            setBackground = true;
            printLogiLED();

            gameRunning = false;
        }

        static private void victory()
        {
            //stop timer victory
            wins++;
            var principalForm = Application.OpenForms.OfType<Form1>().Single();
            principalForm.UpdateDisplay();
            principalForm.StopWatchVictory();
            
            colors[9, 0] = colors[13, 0];
            colors[9, 1] = colors[13, 1];
            colors[9, 2] = colors[13, 2];

            var systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string[] lines = { "Wins: " + wins, "Bombs: " + bombs, "Layout: " + keyboardLayout };
            var file = Path.Combine(systemPath, "G810 Minesweeper/config.txt");
            File.WriteAllLines(file, lines);

            gameOver();
        }
    }
}
