using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using OpenTKVoxelEngine_Shader;
using OpenTKVoxelEngine_Utils;
using OpenTKVoxelEngine_Texture;
using OpenTKVoxelEngine_Camera;

namespace OpenTKVoxelEngine_EngineWindow
{
    public class EngineWindow : GameWindow
    {
        private bool renderWireframe;
        private Shader shader; // Shader
        private Texture texture1; // Texture 1
        private Texture texture2; // Texture 2
        private Camera camera; // Camera
        private int vertexBufferObject; // VBO
        private int vertexArrayObject; // VAO
        private int elementBufferObject; // EBO
        private Stopwatch timer;
        private Vector2 lastPos;
        private bool firstMove;

        private readonly float[] vertices =
        {
            // positions          // texture coords
             -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
              0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
              0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
              0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
             -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
             -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

             -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
              0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
              0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
              0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
             -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
             -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

             -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
             -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
             -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
             -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
             -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
             -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

              0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
              0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
              0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
              0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
              0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
              0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

             -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
              0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
              0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
              0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
             -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
             -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

             -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
              0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
              0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
              0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
             -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
             -0.5f,  0.5f, -0.5f,  0.0f, 1.0f
        };

        private readonly uint[] indices = // Starting from 0!
        {
            // Clockwise
            0, 1, 3, // first triangle
            1, 2, 3  // second triangle
        };

        public EngineWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings) { }

        protected override void OnLoad()
        {
            base.OnLoad();
            Console.WriteLine("VoxelEngine - OnLoading...");
            Console.WriteLine($"Graphics API: '{API}' / Version: '{APIVersion}'");

            renderWireframe = false;
            timer = new Stopwatch();
            timer.Start();
            firstMove = true;

            // Clear the color buffers with the provided RGBA values.
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            // Bind the VBO.
            vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Bind the VAO.
            vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            // Bind EBO
            elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            // Construct and initialize the shaders.
            shader = new Shader(Utility.GetRootDirectory("Shaders/shader.vert"), Utility.GetRootDirectory("Shaders/shader.frag"));
            shader.Use();
            shader.SetInt("texture1", 0);
            shader.SetInt("texture2", 1);

            // Link the vertex attribute (Positions).
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Link the vertex attribute (Texture Coords).
            int texCoordLocation = shader.GetAttribLocation("aTexCoord");
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(texCoordLocation);

            // Create the first texture and use it.
            texture1 = Texture.LoadFromFile(Utility.GetRootDirectory("Resources/Textures/wallTexture.jpg"));
            texture1.Use(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0);

            // Create the second texture and use it.
            texture2 = Texture.LoadFromFile(Utility.GetRootDirectory("Resources/Textures/awesomeface.png"));
            texture2.Use(OpenTK.Graphics.OpenGL4.TextureUnit.Texture1);

            // Now initialize the camera, so that it is 3 units back from where the rectangle is.
            // Also give it a proper aspect ratio.
            camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);

            // Confine the cursor inside the window.
            CursorGrabbed = true;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            // Enable the Z-Buffer depth test.
            GL.Enable(EnableCap.DepthTest);

            // Clears the image buffer.
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            // Bind the VAO.
            GL.BindVertexArray(vertexArrayObject);

            // Call the textures to use them.
            texture1.Use(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0);
            texture2.Use(OpenTK.Graphics.OpenGL4.TextureUnit.Texture1);

            // Bind the shader.
            shader.Use();

            // Set the model matrix.
            Matrix4 model = Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(timer.ElapsedMilliseconds / 10f));

            // And set the uniform matrices to the vertex shader, by using the camera view and projection matrix.
            shader.SetMatrix4("model", model);
            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());

            // Draw the VAO data, starting from the same index as the declared position in the Vertex Shader.
            //GL.DrawElements(PrimitiveType.Triangles, vertices.Length, DrawElementsType.UnsignedInt, 0);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            // Handle whether to render in wireframe view or not.
            if (renderWireframe) GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            else GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            // Swap buffers to present the rendered frame.
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (KeyboardState.IsKeyDown(Keys.F1)) renderWireframe = true;
            else renderWireframe = false;

            //if (KeyboardState.IsKeyDown(Keys.Escape))
            //    Close();

            // Skip executing if the application is not focused.
            if (!IsFocused) { return; }

            KeyboardState input = KeyboardState.GetSnapshot();

            // Change the camera speed if key pressed during frame.
            if (input.IsKeyDown(Keys.LeftShift)) camera._cameraSpeed = 2 * Camera.CAMERA_DEFAULT_SPEED;
            else camera._cameraSpeed = Camera.CAMERA_DEFAULT_SPEED;
            
            if (input.IsKeyDown(Keys.W))
            {
                camera.Position += camera.Forward * camera._cameraSpeed * (float)args.Time; //Forward
            }

            if (input.IsKeyDown(Keys.S))
            {
                camera.Position -= camera.Forward * camera._cameraSpeed * (float)args.Time; //Backwards
            }

            if (input.IsKeyDown(Keys.A))
            {
                camera.Position -= Vector3.Normalize(Vector3.Cross(camera.Forward, camera.Up)) * camera._cameraSpeed * (float)args.Time; //Left
            }

            if (input.IsKeyDown(Keys.D))
            {
                camera.Position += Vector3.Normalize(Vector3.Cross(camera.Forward, camera.Up)) * camera._cameraSpeed * (float)args.Time; //Right
            }

            if (input.IsKeyDown(Keys.Space))
            {
                camera.Position += camera.Up * camera._cameraSpeed * (float)args.Time; //Up
            }

            if (input.IsKeyDown(Keys.LeftControl))
            {
                camera.Position -= camera.Up * camera._cameraSpeed * (float)args.Time; //Down
            }

            // Get the mouse state.
            MouseState mouse = MouseState;

            if (firstMove)
            {
                lastPos = new Vector2(mouse.X, mouse.Y);
                firstMove = false;
            }
            else
            {
                // Calculate the offset of the mouse position.
                float deltaX = mouse.X - lastPos.X;
                float deltaY = mouse.Y - lastPos.Y;
                lastPos = new Vector2(mouse.X, mouse.Y);

                // Apply the camera pitch and yaw (pitch clamped in the camera class)
                camera.Yaw += deltaX * camera._sensitivity;
                camera.Pitch -= deltaY * camera._sensitivity; // reversed since y-coordinates range from bottom to top.
            }

        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);

            if (camera != null)
            {
                // Need to update the aspect ratio once the window has been resized.
                camera.AspectRatio = Size.X / (float)Size.Y;
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            camera.Fov -= e.OffsetY;
        }

        protected override void OnUnload()
        {
            Console.WriteLine("VoxelEngine - OnUnloading...");

            // Binding a buffer to 0 basically sets it to null, so any calls that modify a buffer without binding one first will result in a crash.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(vertexBufferObject);
            GL.UseProgram(0);

            // Delete all the resources
            GL.DeleteBuffer(vertexBufferObject);
            GL.DeleteBuffer(vertexArrayObject);
            GL.DeleteBuffer(elementBufferObject);

            // Cleanup the shaders
            shader.Dispose();

            base.OnUnload();
        }

    }
}
