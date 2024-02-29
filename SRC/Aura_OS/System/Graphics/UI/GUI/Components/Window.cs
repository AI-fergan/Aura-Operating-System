/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Window class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Graphics.UI.GUI.Skin;
using Cosmos.System;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Aura_OS.System.Graphics.UI.GUI.Components
{
    public class Window : Component
    {
        public Bitmap Icon;
        public string Name;
        public Button Close;
        public Button Minimize;
        public Button Maximize;
        public Panel TopBar;

        public bool HasBorders;
        public bool HasCloseButton;
        public bool HasMinimizeButton;
        public bool HasMaximizeButton;

        public Window(int x, int y, int width, int height) : base(x, y, width, height)
        {
            Frame = Kernel.ThemeManager.GetFrame("window");

            HasBorders = false;
        }

        public Window(string name, int x, int y, int width, int height, bool hasCloseButton = true, bool hasMinimizeButton = true) : base(x, y, width, height)
        {
            Frame = Kernel.ThemeManager.GetFrame("window");

            Icon = Kernel.ResourceManager.GetIcon("16-program.bmp");
            Name = name;
            HasCloseButton = hasCloseButton;
            HasMinimizeButton = hasMinimizeButton;
            HasBorders = true;

            if (HasBorders)
            {
                TopBar = new Panel(Color.Transparent, 3, 3, Width - 5, 18);
                TopBar.Background = false;
                TopBar.Borders = false;
                TopBar.Text = name;
                AddChild(TopBar);
            }

            if (HasCloseButton)
            {
                Close = new Button(Kernel.ResourceManager.GetIcon("16-close.bmp"), Width - 20, 5);
                Close.Frame = Kernel.ThemeManager.GetFrame("window.close.normal");
                Close.NoBackground = true;
                AddChild(Close);
            }

            if (HasMinimizeButton)
            {
                Minimize = new Button(Kernel.ResourceManager.GetIcon("16-minimize.bmp"), Width - 38, 5);
                Minimize.Frame = Kernel.ThemeManager.GetFrame("window.minimize.normal");
                Minimize.NoBackground = true;
                AddChild(Minimize);
            }

            // Force maximize for taskbar actions
            HasMaximizeButton = true;

            if (HasMaximizeButton)
            {
                Maximize = new Button(Kernel.ResourceManager.GetIcon("16-minimize.bmp"), Width - 60, 5);
                Maximize.Frame = Kernel.ThemeManager.GetFrame("window.minimize.normal");
                Maximize.Visible = false;
                Maximize.NoBackground = true;
                AddChild(Maximize);
            }
        }

        public override void Draw()
        {
            base.Draw();

            if (HasBorders)
            {
                TopBar.Draw(this);

                if (HasCloseButton)
                {
                    Close.Draw(this);
                }

                if (HasMinimizeButton)
                {
                    Minimize.Draw(this);
                }
            }
        }
    }
}