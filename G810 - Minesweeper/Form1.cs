﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Timers;
using System.Threading;

namespace G810___Minesweeper
{
    public partial class Form1 : Form
    {
        public static System.Timers.Timer aTimer = new System.Timers.Timer();
        private static readonly Stopwatch timer = new Stopwatch();

        public Form1()
        {

            InitializeComponent();
            ActiveControl = label3;

            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 1000;
            aTimer.Enabled = false;
            aTimer.Stop();
            
            label5.Text = BestTime(MineSweeper.KeyboardLayout, MineSweeper.Bombs);
            UpdateTimer();
            label3.Text = MineSweeper.Wins.ToString();

            if (MineSweeper.KeyboardLayout == "US")
            {
                comboBox1.SelectedIndex = 1;
            }
            else
            {
                comboBox1.SelectedIndex = 0;
            }

            numericUpDown1.Value = Convert.ToDecimal(MineSweeper.Bombs);

            // Define the border style of the form to a dialog box.
            FormBorderStyle = FormBorderStyle.FixedDialog;

            // Set the MaximizeBox to false to remove the maximize box.
            MaximizeBox = false;

            // Set the MinimizeBox to false to remove the minimize box.
            MinimizeBox = false;

            // Set the start position of the form to the center of the screen.
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public void UpdateDisplay()
        {
            label3.Text = MineSweeper.Wins.ToString();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            MineSweeper.Bombs = Convert.ToInt32(numericUpDown1.Value);
            MineSweeper.newGame();

            label5.Text = BestTime(MineSweeper.KeyboardLayout, MineSweeper.Bombs);

            StopWatchDefeat();
            ResetWatch();

            UpdateFile();
        }

        string BestTime(string layout, int bombs)
        {
            string[] US = { "", "", "", "", "", "5: 30:00", "6: 30:00", "7: 30:00", "8: 30:00", "9: 30:00", "10: 30:00", "11: 30:00", "12: 30:00", "13: 30:00", "14: 30:00", "15: 30:00", "16: 30:00", "17: 30:00", "18: 30:00", "19: 30:00", "20: 30:00", "21: 30:00", "22: 30:00", "23: 30:00", "24: 30:00", "25: 30:00" };
            string best = "";

            var systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var file = Path.Combine(systemPath, "G810 Minesweeper/config.txt");
            var fileUS = Path.Combine(systemPath, "G810 Minesweeper/US.txt");
            var fileDE = Path.Combine(systemPath, "G810 Minesweeper/DE.txt");

            int a = 0;

            if (layout == "US")
            {
                try
                {
                    string skip = bombs + ": ";
                    string line = File.ReadLines(fileUS).Skip(bombs).Take(1).First();
                    a = line.IndexOf(skip);
                    best = line.Substring(a + skip.Length);
                    int min = Convert.ToInt32(best.Substring(0, 2));
                    int sek = Convert.ToInt32(best.Substring(3, 2));
                }
                catch
                {
                    File.WriteAllLines(fileUS, US);
                }
                if (best.Length != 5 || best.Substring(2, 1) != ":")
                {
                    File.WriteAllLines(fileUS, US);
                }
            }
            else if(layout == "DE")
            {
                try
                {
                    string skip = bombs + ": ";
                    string line = File.ReadLines(fileDE).Skip(bombs).Take(1).First();
                    a = line.IndexOf(skip);
                    best = line.Substring(a + skip.Length);
                    int min = Convert.ToInt32(best.Substring(0, 2));
                    int sek = Convert.ToInt32(best.Substring(3, 2));
                }
                catch
                {
                    File.WriteAllLines(fileDE, US);
                }
                if (best.Length != 5 || best.Substring(2, 1) != ":")
                {
                    File.WriteAllLines(fileDE, US);
                }
            }

            

            return best;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox1.SelectedIndex == 1)
            {
                MineSweeper.KeyboardLayout = "US";
                MineSweeper.newGame();
            }
            else
            {
                MineSweeper.KeyboardLayout = "DE";
                MineSweeper.newGame();
            }

            StopWatchDefeat();
            ResetWatch();
            
            label5.Text = BestTime(MineSweeper.KeyboardLayout, MineSweeper.Bombs);

            UpdateFile();
        }

        void UpdateFile()
        {
            var systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string[] lines = { "Wins: " + MineSweeper.Wins, "Bombs: " + MineSweeper.Bombs, "Layout: " + MineSweeper.KeyboardLayout };
            var file = Path.Combine(systemPath, "G810 Minesweeper/config.txt");
            File.WriteAllLines(file, lines);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ActiveControl = label3;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            numericUpDown1.Value = 13;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            numericUpDown1.Value = 10;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            numericUpDown1.Value = 7;
        }

        void numericUpDown1_MouseWheel(object sender, MouseEventArgs e)
        {
            ((HandledMouseEventArgs)e).Handled = true;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (timer.Elapsed.Minutes >= 30)
            {
                MineSweeper.newGame();
                StopWatchDefeat();
                ResetWatch();
            }
            fnUpdate_Label("Hello!");
            //UpdateTimer();
        }

        public void StopWatchVictory()
        {
            timer1.ForeColor = System.Drawing.Color.Green;
            timer.Stop();
            aTimer.Enabled = false;
            fnUpdate_Label("Hello!");

            string[] file = new string[26];

            string best = BestTime(MineSweeper.KeyboardLayout, MineSweeper.Bombs);
            if(Convert.ToInt32(best.Substring(0, 2)) * 60 + Convert.ToInt32(best.Substring(3, 2)) >= timer.Elapsed.Minutes * 60 + timer.Elapsed.Seconds)
            {
                for(int i = 0; i < 26; i++)
                {
                    if(i <= 5)
                    {
                        file[i] = "";
                    }
                    else
                    {
                        file[i] = i + ": " + BestTime(MineSweeper.KeyboardLayout, i);
                    }
                }
                file[MineSweeper.Bombs] = MineSweeper.Bombs.ToString() + ": " + string.Format("{0:00}:{1:00}",timer.Elapsed.Minutes,timer.Elapsed.Seconds);

                var systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                var fileUS = Path.Combine(systemPath, "G810 Minesweeper/US.txt");
                var fileDE = Path.Combine(systemPath, "G810 Minesweeper/DE.txt");

                if (MineSweeper.KeyboardLayout == "US")
                {
                    File.WriteAllLines(fileUS, file);
                }
                else
                {
                    File.WriteAllLines(fileDE, file);
                }

                label5.Text = BestTime(MineSweeper.KeyboardLayout, MineSweeper.Bombs);
            }
        }

        public void StopWatchDefeat()
        {
            timer1.ForeColor = System.Drawing.Color.Red;
            timer.Stop();
            aTimer.Enabled = false;
        }

        public void StartWatch()
        {
            timer1.ForeColor = System.Drawing.Color.Black;
            timer.Reset();
            aTimer.Enabled = true;
            timer.Start();
        }


        public void ResetWatch()
        {
            timer1.ForeColor = System.Drawing.Color.Black;
            timer.Reset();
            UpdateTimer();
        }

        private void UpdateTimer()
        {
            timer1.Text = GetTimeString(timer.Elapsed);
        }

        private string GetTimeString(TimeSpan elapsed)
        {
            string result = string.Empty;

            result = string.Format("{0:00}:{1:00}",
                elapsed.Minutes,
                elapsed.Seconds);

            return result;
        }

        public delegate void Update_label_delegate(string msg);

        public void fnUpdate_Label(string msg)
        {
            if (timer1.InvokeRequired)
            {
                timer1.BeginInvoke(new Update_label_delegate(fnUpdate_Label), new Object[] { msg });
            }
            else
            {
                timer1.Text = GetTimeString(timer.Elapsed);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if ((MessageBox.Show("Are you sure you want to Reset. All wins and best times will be lost.", "Reset",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes))
            {
                string[] lines = { "Wins: 0", "Bombs: 13", "Layout: US" };
                string[] US = { "", "", "", "", "", "5: 30:00", "6: 30:00", "7: 30:00", "8: 30:00", "9: 30:00", "10: 30:00", "11: 30:00", "12: 30:00", "13: 30:00", "14: 30:00", "15: 30:00", "16: 30:00", "17: 30:00", "18: 30:00", "19: 30:00", "20: 30:00", "21: 30:00", "22: 30:00", "23: 30:00", "24: 30:00", "25: 30:00" };

                var systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                var directory = Path.Combine(systemPath, "G810 Minesweeper");

                var file = Path.Combine(systemPath, "G810 Minesweeper/config.txt");
                var fileUS = Path.Combine(systemPath, "G810 Minesweeper/US.txt");
                var fileDE = Path.Combine(systemPath, "G810 Minesweeper/DE.txt");

                File.WriteAllLines(file, lines);
                File.WriteAllLines(fileUS, US);
                File.WriteAllLines(fileDE, US);

                MineSweeper.Wins = 0;
                MineSweeper.Bombs = 13;
                MineSweeper.KeyboardLayout = "US";

                comboBox1.SelectedIndex = 1;
                numericUpDown1.Value = 13;
                label3.Text = MineSweeper.Wins.ToString();

                MineSweeper.newGame();

                StopWatchDefeat();
                ResetWatch();
            }
        }
    }
}
