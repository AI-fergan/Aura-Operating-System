/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Window class
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

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
        public Panel TopBar;

        public bool HasBorders;
        public bool HasCloseButton;
        public bool HasMinimizeButton;

        public Window(int x, int y, int width, int height) : base(x, y, width, height)
        {
            HasBorders = false;
        }

        public Window(string name, int x, int y, int width, int height, bool hasCloseButton = true, bool hasMinimizeButton = true) : base(x, y, width, height)
        {
            Icon = ResourceManager.GetImage("16-program.bmp");
            Name = name;
            HasCloseButton = hasCloseButton;
            HasMinimizeButton = hasMinimizeButton;
            HasBorders = true;

            if (HasCloseButton)
            {
                Close = new Button(ResourceManager.GetImage("16-close.bmp"), Width - 20, 5);
                Close.NoBackground = true;
                Close.NoBorder = true;
                Close.HasTransparency = true;
                Children.Add(Close);
            }

            if (HasMinimizeButton)
            {
                Minimize = new Button(ResourceManager.GetImage("16-minimize.bmp"), Width - 38, 5);
                Minimize.NoBackground = true;
                Minimize.NoBorder = true;
                Minimize.HasTransparency = true;
                Children.Add(Minimize);
            }

            if (HasBorders)
            {
                TopBar = new Panel(Kernel.DarkBlue, Kernel.Pink, 3, 3, Width - 5, 18);
                Children.Add(TopBar);
            }
        }

        public override void Update()
        {
            if (HasBorders)
            {
                TopBar.X = X + 3;
                TopBar.Y = Y + 3;
            }
            if (HasCloseButton)
            {
                Close.X = X + Width - 20;
                Close.Y = Y + 5;
            }
            if (HasMinimizeButton)
            {
                Minimize.X = X + Width - 38;
                Minimize.Y = Y + 5;
            }
        }

        public override void Draw()
        {
            Clear();

            DrawLine(Kernel.Gray, 0, 0, Width - 1, 0); // Top
            DrawLine(Kernel.WhiteColor, 0, 0 + 1, Width - 1, 0 + 1);

            DrawLine(Kernel.Gray, 0, 0, 0, Height - 1); // Left
            DrawLine(Kernel.WhiteColor, 0 + 1, 0, 0 + 1, Height - 1);

            DrawLine(Kernel.DarkGray, 0, Height - 1, Width - 1, Height - 1); // Bottom
            DrawLine(Kernel.BlackColor, 0, Height, Width, Height);

            DrawLine(Kernel.DarkGray, Width - 1, 0, Width - 1, Height - 1); // Right
            DrawLine(Kernel.BlackColor, Width, 0, Width, Height);

            if (HasBorders)
            {
                TopBar.Draw();
                DrawString(Name, PCScreenFont.Default, Kernel.WhiteColor, 5, 4);

                if (HasCloseButton)
                {
                    Close.Draw();
                }

                if (HasMinimizeButton)
                {
                    Minimize.Draw();
                }
            }
        }
    }
}