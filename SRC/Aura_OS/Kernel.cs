﻿/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Kernel.cs, the main init class + main loop class
* PROGRAMMER(S):    Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using Cosmos.Core.Memory;
using Cosmos.HAL;
using Cosmos.System;
using Cosmos.System.ExtendedASCII;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using Aura_OS.Processing;
using System.Drawing;
using Aura_OS.System;
using System.IO;
using Aura_OS.System.Processing;
using Aura_OS.System.Graphics.UI.GUI;
using Aura_OS.System.Graphics;
using Aura_OS.System.Processing.Interpreter;
using Aura_OS.System.Processing.Interpreter.Commands;
using Aura_OS.System.Processing.Processes;

namespace Aura_OS
{
    public class Kernel
    {
        public static Dictionary<string, string> EnvironmentVariables;
        public static string ComputerName = "aura-pc";
        public static string userLogged = "root";
        public static string userLevelLogged = "admin";
        public static bool Running = false;
        public static string Version = "0.7.0";
        public static string Revision = VersionInfo.revision;
        public static string langSelected = "en_US";
        public static string BootTime = "01/01/1970";

        public static string Clipboard { get; internal set; }

        public static string CurrentVolume = @"0:\";
        public static string CurrentDirectory = @"0:\";

        public static bool NetworkConnected = false;
        public static bool NetworkTransmitting = false;

        //FILES
        public static Bitmap programLogo;
        public static Bitmap errorLogo;

        public static Bitmap AuraLogo2;
        public static Bitmap AuraLogo;
        public static Bitmap AuraLogoWhite;
        public static Bitmap CosmosLogo;

        public static Bitmap wallpaper;

        public static PCScreenFont font;
        public static PCScreenFont fontTerminal;

        //GRAPHICS
        public static uint screenWidth = 1920;
        public static uint screenHeight = 1080;

        public static Canvas Canvas;

        public static Color WhiteColor = Color.White;
        public static Color BlackColor = Color.Black;
        public static Color avgColPen = Color.PowderBlue;

        //WIN95 Colors
        public static Color Gray = Color.FromArgb(0xff, 0xdf, 0xdf, 0xdf);
        public static Color DarkGrayLight = Color.FromArgb(0xff, 0xc0, 0xc0, 0xc0);
        public static Color DarkGray = Color.FromArgb(0xff, 0x80, 0x80, 0x80);
        public static Color DarkBlue = Color.FromArgb(0xff, 0x00, 0x00, 0x80);
        public static Color Pink = Color.FromArgb(0xff, 0xe7, 0x98, 0xde);

        // Managers
        public static ProcessManager ProcessManager;
        public static System.Input.MouseManager MouseManager;
        public static ApplicationManager ApplicationManager;
        public static PackageManager PackageManager;
        public static Explorer Explorer;

        // Textmode Console
        public static System.Graphics.UI.CUI.Console TextmodeConsole;

        public static int FreeCount = 0;

        private static int _frames = 0;
        private static int _fps = 0;
        private static int _deltaT = 0;

        public static CosmosVFS VirtualFileSystem;

        public static string CommandOutput = "";
        public static bool Redirect = false;

        public static void BeforeRun()
        {
            EnvironmentVariables = new Dictionary<string, string>();
            VirtualFileSystem = new CosmosVFS();

            //Start Filesystem
            VFSManager.RegisterVFS(VirtualFileSystem);

            //START PROCESSES
            CustomConsole.WriteLineInfo("Starting process manager...");
            ProcessManager = new ProcessManager();
            ProcessManager.Initialize();

            CustomConsole.WriteLineInfo("Loading files...");
            Files.LoadFiles();

            CustomConsole.WriteLineInfo("Checking for boot.bat script...");
            if (File.Exists(CurrentDirectory + "boot.bat"))
            {
                CustomConsole.WriteLineOK("Detected boot.bat, executing script...");

                Batch.Execute(CurrentDirectory + "boot.bat");
            }

            CustomConsole.WriteLineInfo("Starting Canvas...");

            //START GRAPHICS
            Canvas = FullScreenCanvas.GetFullScreenCanvas(new Mode(screenWidth, screenHeight, ColorDepth.ColorDepth32));
            Canvas.DrawImage(AuraLogoWhite, (int)((screenWidth / 2) - (AuraLogoWhite.Width / 2)), (int)((screenHeight / 2) - (AuraLogoWhite.Height / 2)));
            Canvas.Display();

            CustomConsole.BootConsole = new(0, 0, (int)screenWidth, (int)screenHeight);
            CustomConsole.BootConsole.DrawBackground = false;

            TextmodeConsole = null;

            CustomConsole.WriteLineInfo("Loading icons...");
            Files.LoadImages();

            CustomConsole.WriteLineInfo("Starting package manager...");
            PackageManager = new PackageManager();
            PackageManager.Initialize();

            CustomConsole.WriteLineInfo("Starting application manager...");
            ApplicationManager = new ApplicationManager();
            CustomConsole.WriteLineInfo("Registering applications...");
            ApplicationManager.LoadApplications();

            CustomConsole.WriteLineInfo("Starting mouse manager...");
            MouseManager = new System.Input.MouseManager();

            CustomConsole.WriteLineInfo("Starting Explorer...");
            Explorer = new Explorer();
            Explorer.Initialize();
            ProcessManager.Start(Explorer);

            CustomConsole.WriteLineInfo("Starting mouse...");
            Cosmos.System.MouseManager.ScreenWidth = screenWidth;
            Cosmos.System.MouseManager.ScreenHeight = screenHeight;

            //Load Localization
            CustomConsole.WriteLineInfo("Initializing localization...");
            Encoding.RegisterProvider(CosmosEncodingProvider.Instance);
            KeyboardManager.SetKeyLayout(new Sys.ScanMaps.USStandardLayout());

            CustomConsole.WriteLineInfo("Initializing ASCII encoding...");
            global::System.Console.InputEncoding = Encoding.ASCII;
            global::System.Console.OutputEncoding = Encoding.ASCII;

            CustomConsole.WriteLineInfo("Try cleaning memory...");
            FreeCount = Heap.Collect();
            CustomConsole.WriteLineInfo("Cosmos Memory Manager works.");

            BootTime = Time.MonthString() + "/" + Time.DayString() + "/" + Time.YearString() + ", " + Time.TimeString(true, true, true);

            CustomConsole.WriteLineOK("Aura Operating System boot sequence done.");

            Running = true;
        }

        public static void Run()
        {
            try
            {
                if (_deltaT != RTC.Second)
                {
                    _fps = _frames;
                    _frames = 0;
                    _deltaT = RTC.Second;
                }

                _frames++;

                FreeCount = Heap.Collect();

                ProcessManager.Update();

                MouseManager.Update();
                MouseManager.DrawRightClick();

                DrawCursor(Sys.MouseManager.X, Sys.MouseManager.Y);

                Canvas.DrawString("Aura Operating System [" + Version + "." + Revision + "]", font, WhiteColor, 2, 0);
                Canvas.DrawString("fps=" + _fps, font, WhiteColor, 2, font.Height);

                Canvas.Display();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    Crash.StopKernel(ex.Message, ex.InnerException.Message, "0x00000000", "0");
                }
                else
                {
                    Crash.StopKernel("Fatal dotnet exception occured.", ex.Message, "0x00000000", "0");
                }
            }
        }

        public static void DrawCursor(uint x, uint y)
        {
            Canvas.DrawImageAlpha(ResourceManager.GetImage("00-cursor.bmp"), (int)x, (int)y);
        }
    }
}
