using System;
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

        public const int WIDTH = 1920;
        public const int HEIGHT = 1080;

        public static void Main()
        {

            // Create the NativeWindowSettings instance and initialize the parameters
            NativeWindowSettings nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(WIDTH, HEIGHT),
                Title = "VoxelEngine",
                Flags = ContextFlags.ForwardCompatible,
                //Location = new Vector2i(WIDTH / 2, HEIGHT / 2),
            };

            // Create a EngineWindow instance to handle our window, but create it inside an using statement,
            // this allow us to run the code once at the very begining, and later on after the setup, everythig is cleared
            // in the memory, avoiding any possible memory leaks.
            using (EngineWindow window = new EngineWindow(GameWindowSettings.Default, nativeWindowSettings))
            {
                window.VSync = VSyncMode.Off;
                window.Run();
            }

        }

    }

}
