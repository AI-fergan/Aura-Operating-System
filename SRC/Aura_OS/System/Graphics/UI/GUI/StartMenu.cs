﻿/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Start menu
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using Cosmos.System;
using Aura_OS.System.Graphics.UI.GUI.Components;
using Aura_OS.System.Processing.Processes;

namespace Aura_OS.System.Graphics.UI.GUI
{
    public class StartMenu : Window
    {
        private List<Button> buttons;
        private Button Shutdown;
        private Button Reboot;
        private Panel Ribbon;
        private Bitmap Logo;

        private bool Clicked = false;

        public StartMenu(int x, int y, int width, int height) : base(x, y, width, height)
        {
            Frame = Kernel.ThemeManager.GetFrame("window.borderless");

            Logo = Kernel.AuraLogo2;
            buttons = new List<Button>();

            // Ribbon
            Ribbon = new Panel(Kernel.DarkBlue, 3, 3, 23, height - 6);
            AddChild(Ribbon);

            // Shutdown
            Shutdown = new Button(Kernel.ResourceManager.GetIcon("24-shutdown.bmp"), "Shut Down.", 3 + Ribbon.Width, Height - 32 - 4, Width - Ribbon.Width - 3, 35);
            Shutdown.NoBorder = true;
            Shutdown.Click = new Action(() =>
            {
                Power.Shutdown();
            });
            AddChild(Shutdown);

            // Reboot
            Reboot = new Button(Kernel.ResourceManager.GetIcon("24-reboot.bmp"), "Reboot.", 3 + Ribbon.Width, Height - 64 - 4, Width - Ribbon.Width - 3, 35);
            Reboot.NoBorder = true;
            Reboot.Click = new Action(() =>
            {
                Power.Reboot();
            });
            AddChild(Reboot);

            // App buttons
            buttons.Clear();
            int buttonY = 0;
            foreach (var applicationConfig in Kernel.ApplicationManager.ApplicationTemplates)
            {
                Bitmap icon = null;

                if (applicationConfig.Template.Name.StartsWith("Terminal"))
                {
                    icon = Kernel.ResourceManager.GetIcon("24-terminal.bmp");
                }
                else if (applicationConfig.Template.Name.StartsWith("Explorer"))
                {
                    icon = Kernel.ResourceManager.GetIcon("24-explorer.bmp");
                }
                else
                {
                    icon = Kernel.ResourceManager.GetIcon("24-program.bmp");
                }

                var button = new Button(icon, applicationConfig.Template.Name, 3 + Ribbon.Width, 3 +  buttonY + 1, Width - Ribbon.Width - 3, 35);
                button.NoBorder = true;
                button.Click = new Action(() =>
                {
                    Kernel.ApplicationManager.StartApplication(applicationConfig);
                    Explorer.ShowStartMenu = false;
                });
                buttons.Add(button);
                AddChild(button);
                buttonY += 32;
            }

            Visible = false;
        }

        public override void Update()
        {
            base.Update();

            // Applications buttons
            foreach (var button in buttons)
            {
                if (button.IsInside((int)MouseManager.X, (int)MouseManager.Y))
                {
                    button.BackColor = Color.DarkBlue;
                    button.TextColor = Color.White;
                }
                else
                {
                    button.BackColor = Color.LightGray;
                    button.TextColor = Color.Black;
                }

                button.Update();
            }

            // Shutdown + Reboot buttons
            Shutdown.Update();
            Reboot.Update();

            if (Shutdown.IsInside((int)MouseManager.X, (int)MouseManager.Y))
            {
                Shutdown.BackColor = Color.DarkBlue;
                Shutdown.TextColor = Color.White;
            }
            else
            {
                Shutdown.BackColor = Color.LightGray;
                Shutdown.TextColor = Color.Black;
            }

            if (Reboot.IsInside((int)MouseManager.X, (int)MouseManager.Y))
            {
                Reboot.BackColor = Color.DarkBlue;
                Reboot.TextColor = Color.White;
            }
            else
            {
                Reboot.BackColor = Color.LightGray;
                Reboot.TextColor = Color.Black;
            }
        }

        public override void Draw()
        {
            base.Draw();

            Ribbon.Draw(this);

            DrawImage(Logo, 5, Ribbon.Height - 66);

            foreach (var button in buttons)
            {
                button.Draw(this);
            }

            Shutdown.Draw(this);
            Reboot.Draw(this);
        }
    }
}