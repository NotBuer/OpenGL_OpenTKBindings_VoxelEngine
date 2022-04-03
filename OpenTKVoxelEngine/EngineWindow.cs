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
        private Camera camera; // Camera
        private int elementBufferObject; // EBO
        private Stopwatch timer;
        private Vector2 lastPos;
        private bool firstMove;

        private readonly Vector3 _lightPos = new Vector3(1.2f, 1.0f, 2.0f);
        private int _vertexBufferObject;
        private int _vaoModel;
        private int _vaoLamp;
        private Shader _lampShader;
        private Shader _lightingShader;

        private readonly float[] vertices =
        {
            // positions
            -0.5f, -0.5f, -0.5f, // Front face
             0.5f, -0.5f, -0.5f, 
             0.5f,  0.5f, -0.5f, 
             0.5f,  0.5f, -0.5f, 
            -0.5f,  0.5f, -0.5f, 
            -0.5f, -0.5f, -0.5f, 

            -0.5f, -0.5f,  0.5f, // Back face
             0.5f, -0.5f,  0.5f, 
             0.5f,  0.5f,  0.5f, 
             0.5f,  0.5f,  0.5f, 
            -0.5f,  0.5f,  0.5f, 
            -0.5f, -0.5f,  0.5f, 

            -0.5f,  0.5f,  0.5f, // Left face
            -0.5f,  0.5f, -0.5f, 
            -0.5f, -0.5f, -0.5f, 
            -0.5f, -0.5f, -0.5f, 
            -0.5f, -0.5f,  0.5f, 
            -0.5f,  0.5f,  0.5f, 

             0.5f,  0.5f,  0.5f, // Right face
             0.5f,  0.5f, -0.5f, 
             0.5f, -0.5f, -0.5f, 
             0.5f, -0.5f, -0.5f, 
             0.5f, -0.5f,  0.5f, 
             0.5f,  0.5f,  0.5f, 

            -0.5f, -0.5f, -0.5f, // Bottom face
             0.5f, -0.5f, -0.5f, 
             0.5f, -0.5f,  0.5f, 
             0.5f, -0.5f,  0.5f, 
            -0.5f, -0.5f,  0.5f, 
            -0.5f, -0.5f, -0.5f, 

            -0.5f,  0.5f, -0.5f, // Top face
             0.5f,  0.5f, -0.5f, 
             0.5f,  0.5f,  0.5f, 
             0.5f,  0.5f,  0.5f, 
            -0.5f,  0.5f,  0.5f, 
            -0.5f,  0.5f, -0.5f, 
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

            // Enable the Z-Buffer depth test.
            GL.Enable(EnableCap.DepthTest);

            // Bind the VBO.
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            _lightingShader = new Shader(Utility.GetRootDirectory("Shaders/shader.vert"), 
                Utility.GetRootDirectory("Shaders/lighting.frag"));
            _lampShader = new Shader(Utility.GetRootDirectory("Shaders/shader.vert"), 
                Utility.GetRootDirectory("Shaders/shader.frag"));


            {
                // Initialize the VAO for the model.
                _vaoModel = GL.GenVertexArray();
                GL.BindVertexArray(_vaoModel);
                int vertexLocationModel = _lightingShader.GetAttribLocation("aPosition");
                GL.EnableVertexAttribArray(vertexLocationModel);
                GL.VertexAttribPointer(vertexLocationModel, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            }

            {
                // Initialize the VAO for the lamp.
                _vaoLamp = GL.GenVertexArray();
                GL.BindVertexArray(_vaoLamp);
                int vertexLocationLamp = _lampShader.GetAttribLocation("aPosition");
                GL.EnableVertexAttribArray(vertexLocationLamp);
                GL.VertexAttribPointer(vertexLocationLamp, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            }
            
            //// Bind EBO
            //elementBufferObject = GL.GenBuffer();
            //GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            //GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            // Now initialize the camera, so that it is 3 units back from where the rectangle is.
            // Also give it a proper aspect ratio.
            camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);

            // Confine the cursor inside the window.
            CursorGrabbed = true;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            // Clears the image buffer.
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Draw the model.
            GL.BindVertexArray(_vaoModel);
            _lightingShader.Use();

            // Matrix4.Identity is used as the matrix, since we just want to draw it at (0,0,0).
            _lightingShader.SetMatrix4("model", Matrix4.Identity);
            _lightingShader.SetMatrix4("view", camera.GetViewMatrix());
            _lightingShader.SetMatrix4("projection", camera.GetProjectionMatrix());

            _lightingShader.SetVector3("objectColor", new Vector3(1f, 0.5f, 0.31f));
            _lightingShader.SetVector3("lightColor", new Vector3(1f, 1f, 1f));

            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);


            // Draw the lamp, this is mostly the same as for the model cube.
            GL.BindVertexArray(_vaoLamp);
            _lampShader.Use();

            Matrix4 lampMatrix = Matrix4.CreateScale(0.2f);
            lampMatrix *= Matrix4.CreateTranslation(_lightPos);

            _lampShader.SetMatrix4("model", lampMatrix);
            _lampShader.SetMatrix4("view", camera.GetViewMatrix());
            _lampShader.SetMatrix4("projection", camera.GetProjectionMatrix());
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
            GL.DeleteBuffer(_vertexBufferObject);
            GL.UseProgram(0);

            // Delete all the resources
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_vaoModel);
            GL.DeleteBuffer(_vaoLamp);
            GL.DeleteBuffer(elementBufferObject);

            // Cleanup the shaders
            _lampShader.Dispose();
            _lightingShader.Dispose();

            base.OnUnload();
        }

    }
}
