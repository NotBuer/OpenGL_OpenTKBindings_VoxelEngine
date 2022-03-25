﻿using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using OpenTKVoxelEngine_Shader;
using OpenTKVoxelEngine_EngineWindow;

namespace OpenTKVoxelEngine_Core
{
    public static class Core
    {

        public const int WIDTH = 800;
        public const int HEIGHT = 600;

        public static void Main()
        {

            // Create a new instance of engine window and set it.
            EngineWindow engineWindow = new EngineWindow(GameWindowSettings.Default, NativeWindowSettings.Default)
            {
                Size = new Vector2i(WIDTH, HEIGHT),
                Title = "VoxelEngine",
                VSync = VSyncMode.Adaptive
                
            };

            // Initialize the engine window.
            engineWindow.Run();

        }

    }

}