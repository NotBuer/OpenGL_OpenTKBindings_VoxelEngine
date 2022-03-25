using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using OpenTKVoxelEngine_Shader;

namespace OpenTKVoxelEngine_EngineWindow
{
    public class EngineWindow : GameWindow
    {
        private bool renderWireframe;
        private Shader shader; // Shader itself
        private int vertexBufferObject; // VBO
        private int vertexArrayObject; // VAO
        private int elementBufferObject; // EBO

        private readonly float[] vertices =
        {
            0.5f,  0.5f, 0.0f, // top right
            0.5f, -0.5f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f  // top left
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

            // Clear the color buffers with the provided RGBA values.
            GL.ClearColor(.2f, .3f, .3f, 1f);

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

            // Link the vertex attribute.
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Construct and initialize the shaders.
            shader = new Shader("shader.vert", "shader.frag");
            shader.Use();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            DeliverFrame();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (KeyboardState.IsKeyDown(Keys.F1))
                renderWireframe = true;
            else
                renderWireframe = false;

            if (KeyboardState.IsKeyDown(Keys.Escape))
                Close();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
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

        private void DeliverFrame()
        {
            // Clears the image buffer.
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Bind the shader.
            shader.Use();

            // Bind the VAO.
            GL.BindVertexArray(vertexArrayObject);

            if (renderWireframe)
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            else
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            // Draw the VAO data, starting from the same index as the declared position in the Vertex Shader.
            GL.DrawElements(PrimitiveType.Triangles, vertices.Length, DrawElementsType.UnsignedInt, 0);
            //GL.DrawArrays(PrimitiveType.Triangles, shader.GetAttribLocation("aPosition"), 3);

            // Swap buffers to present the rendered frame.
            SwapBuffers();
        }

    }
}
