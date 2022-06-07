﻿using Cosmos.System;
using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using WaveOS.Apps;
using WaveOS.Graphics;
using WaveOS.Managers;
using Mouse = Cosmos.System.MouseManager;

namespace WaveOS.GUI
{
    public class WaveTaskbar : WaveWindow
    {
        public Pen pen = new Pen(System.Drawing.Color.FromArgb(223, 223, 223));
        public Pen pen2 = new Pen(System.Drawing.Color.FromArgb(128, 128, 128));
        public Pen pen3 = new Pen(System.Drawing.Color.FromArgb(127, 127, 127));
        public Pen pen4 = new Pen(System.Drawing.Color.FromArgb(223, 223, 223));

        public List<WaveWindow> openWindows = new List<WaveWindow>();

        public TasksPanel TasksPanel;
        public WaveButton startButton;

        public WavePanel StartPanel;
        public WaveStackPanel StartButtonPanel;

        public bool showStartMenu = false;
        List<Pen> StartGradient;
        public WaveTaskbar(int X, int Y, int width, int height, Apps.WindowManager host) : base("Wave OS Taskbar", X, Y, width, height, host)
        {
            borderless = true;
            controlbox = false;
            StayOnTop = true;


            startButton = new WaveButton() { Text = "Start", X = 2, Y = 4, Width = 54, Height = 22, parent = this, Color = Aura_OS.Kernel.BlackPen,
                onClick = () => 
                {
                    showStartMenu = !showStartMenu;

                    if (showStartMenu)
                    {
                        startButton.forcePressed = true;
                    }
                    else
                    {
                        startButton.forcePressed = false;
                    }
                }
            };

            StartPanel = new WavePanel() {
                X = 3, Y = -((2 * 32) + 6), Width = 178, Height = (2 * 32) + 6,
                parent = this, ignoreTitleBar = true
            };

            StartButtonPanel = new WaveStackPanel()
            {
                X = 24,
                Y = 3,
                Width = 152,
                Height = StartPanel.Height - 6,
                DrawBorder = true,
                parent = this,
                parent2 = StartPanel
            };

            StartMenuItem rebootMenu = new StartMenuItem(152/*, new List<StartMenuItem>()
            {
                new StartMenuItem(166, null)
                {
                X = 0,
                parent = this,
                Text = "Item",
                TextAlignment = TextAlignment.Left,
                }
            }*/)
            {
                Height = 32,
                parent = this,
                Text = "Reboot",
                parent2 = StartButtonPanel,
                TextAlignment = TextAlignment.Left,
                onClick = () => { Cosmos.System.Power.Reboot(); },

            };

            List<StartMenuItem> items = new List<StartMenuItem>() {
                rebootMenu  
            };

            foreach (var item in items)
            {
                StartButtonPanel.children.Add(item);
            }

            StartButtonPanel.UpdateView();

            StartPanel.children.Add(StartButtonPanel);

            children.Add(startButton);

            TasksPanel = new TasksPanel()
            {
                X = startButton.X + startButton.Width + 2,
                Y = 4,
                parent = this,
                Width = Aura_OS.Kernel.canvas.Mode.Columns - (startButton.X + startButton.Width + 2) - 66,
                Height = 22
            };

            children.Add(TasksPanel);

            GetAllOpenWindows();

            host.WindowClosed = Host_WindowClosed;
            host.WindowOpened = (window) => GetAllOpenWindows();
            host.WindowSetActive = (window) =>
            {
                foreach (var item in TasksPanel.children)
                {
                    if(item.tag != window)
                        (item as WaveButton).forcePressed = false;
                    else
                        (item as WaveButton).forcePressed = true;
                }
            };

            host.WindowSetInActive = (window) =>
            {
                foreach (var item in TasksPanel.children)
                {
                    if (item.tag == window)
                        (item as WaveButton).forcePressed = false;
                }
            };

            StartGradient = WaveWindow.GetGradients(new Pen(System.Drawing.Color.FromArgb(0, 0, 128)), new Pen(System.Drawing.Color.FromArgb(0, 0, 255)), 50);
        }

        private void Host_WindowClosed(WaveWindow window)
        {
            GetAllOpenWindows();
        }

        void GetAllOpenWindows()
        {
            List<WaveElement> toRemove = new List<WaveElement>();
            foreach (var item in openWindows)
            {
                if (!hostWindowsContains(item))
                {
                    foreach (var item2 in TasksPanel.children)
                    {
                        if(item2.tag == item)
                        {
                            toRemove.Add(item2);
                        }
                    }
                }
            }

            foreach (var item in toRemove)
            {
                openWindows.Remove((WaveWindow)item.tag);
                TasksPanel.children.Remove(item);
            }

            TasksPanel.AlignTaskbarButtons();

            foreach (var item in Host.windows)
            {
                if(item != this && !openWindowContains(item))
                {
                    openWindows.Add(item);
                    WaveButton button = new WaveButton()
                    {
                        TextAlignment = TextAlignment.Left,
                        Text = item.title,
                        X = (160 * (openWindows.Count - 1)) + 2,
                        Y = 0,
                        Width = 160,
                        Height = 22,
                        parent = this,
                        parent2 = TasksPanel,
                        Color = Aura_OS.Kernel.BlackPen,
                        tag = item
                    };

                    button.onClick = () => { item.SetActive();
                        if(item.State == WindowState.Minimized) item.State = item.savedState; 
                        button.forcePressed = true; };
                    TasksPanel.children.Add(button);
                }
            }
        }

        bool openWindowContains(WaveWindow window)
        {
            foreach (var item in openWindows)
            {
                if (item == window) return true;
            }

            return false;
        }

        bool hostWindowsContains(WaveWindow window)
        {
            foreach (var item in Host.windows)
            {
                if (item == window) return true;
            }

            return false;
        }

        public override void Update()
        {
            base.Update();

            if (showStartMenu)
            {
                UpdateElement(StartPanel);

                if (WaveInput.WasLMBPressed())
                {
                    if (!WaveInput.IsMouseWithin(X + 3, Y - 341, 178, 341))
                    {
                        if (!WaveInput.IsMouseWithin(startButton))
                        {
                            showStartMenu = false;
                            startButton.forcePressed = false;
                        }
                    }
                    else
                        WaveInput.MouseHit = true;
                }
            }
        }

        public override void Draw()
        {
            base.Draw();

            Aura_OS.Kernel.GUI.DrawFilledRectangle(X, Y, Width, 1, 0, pen);
            Aura_OS.Kernel.GUI.DrawFilledRectangle(X, Y + 1, Width, 1, 0, Aura_OS.Kernel.WhitePen);

            DrawSystemTray();

            if (showStartMenu)
            {
                StartPanel.Draw();

                Aura_OS.Kernel.GUI.DrawVerticalGradient(StartPanel.relativeX + 3, StartPanel.relativeY + 3, 21, StartPanel.Height - 6, StartGradient);
            }
        }

        public void DrawSystemTray()
        {
            //Top shadow
            Aura_OS.Kernel.GUI.DrawFilledRectangle(Width - 60, Y + 4, 51, 1, 0, pen2);

            //Left shadow
            Aura_OS.Kernel.GUI.DrawFilledRectangle(Width - 60, Y + 4, 1, 18, 0, pen2);

            //Bottom shadow
            Aura_OS.Kernel.GUI.DrawFilledRectangle(Width - 60, Y + 22, 51, 1, 0, Aura_OS.Kernel.WhitePen);

            //Right shadow
            Aura_OS.Kernel.GUI.DrawFilledRectangle(Width - 8, Y + 4, 1, 18, 0, Aura_OS.Kernel.WhitePen);

            Aura_OS.Kernel.canvas.DrawString(GetTime(), Aura_OS.Kernel.font, Aura_OS.Kernel.BlackPen, Width - 54, Y + 6);
        }

        string GetTime()
        {
            // Time
            var hour = Cosmos.HAL.RTC.Hour;
            var minute = Cosmos.HAL.RTC.Minute;
            var strhour = hour.ToString();
            var strmin = minute.ToString();

            var intmin = Convert.ToInt32(strmin);
            var inthour = Convert.ToInt32(strhour);

            bool eh = intmin < 10;
            bool eh2 = inthour < 10;

            return (eh2 ? "0" + strhour : strhour) + ":" + (eh ? "0" + strmin : strmin);
        }
    }
}
