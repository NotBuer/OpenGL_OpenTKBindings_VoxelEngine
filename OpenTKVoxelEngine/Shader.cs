﻿using System;
using System.IO;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace OpenTKVoxelEngine_Shader
{
    public class Shader : IDisposable
    {
        private bool disposedValue = false;
        private int handle;

        public Shader(string vertexPath, string fragmentPath)
        {
            int vertexShader;
            int fragmentShader;

            // Read the vertex shader source code and store it in a string.
            string vertexShaderSource;
            using (StreamReader reader = new StreamReader(vertexPath, Encoding.UTF8))
            {
                vertexShaderSource = reader.ReadToEnd();
            }

            // Read the fragment shader source code and store it in a string.
            string fragmentShaderSource;
            using (StreamReader reader = new StreamReader(fragmentPath, Encoding.UTF8))
            {
                fragmentShaderSource = reader.ReadToEnd();
            }

            // Create the vertex shader object and replace the source code in a shader object.
            vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            CompileShader(vertexShader);

            // Create the fragment shader object and replace the source code in a shader object.
            fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            CompileShader(fragmentShader);

            //// Compile the vertex shader object.
            //GL.CompileShader(vertexShader);
            //string infoLogVert = GL.GetShaderInfoLog(vertexShader);
            //if (infoLogVert != string.Empty)
            //{
            //    Console.WriteLine(infoLogVert);
            //}

            //// Compile the fragment shader object.
            //GL.CompileShader(fragmentShader);
            //string infoLogFrag = GL.GetShaderInfoLog(fragmentShader);
            //if (infoLogFrag != string.Empty)
            //{
            //    Console.WriteLine(infoLogFrag);
            //}

            // Then create the program object and set it.
            handle = GL.CreateProgram();
            GL.AttachShader(handle, vertexShader);
            GL.AttachShader(handle, fragmentShader);
            LinkProgram(handle);

            // Cleanup
            GL.DetachShader(handle, vertexShader);
            GL.DetachShader(handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);
        }

        private static void CompileShader(int shader)
        {
            // Try to compile the shader
            GL.CompileShader(shader);

            // Check for compilation errors
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int code);
            if (code != (int)All.True)
            {
                // We can use 'GL.GetShaderInfoLog(shader)' to get information about the error.
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}). \n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program)
        {
            // We link the program
            GL.LinkProgram(program);

            // Check for linking errors.
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int code);
            if (code != (int)All.True)
            {
                // We can use 'GL.GetProgramInfoLog(program)' to get information about the error.
                string infoLog = GL.GetProgramInfoLog(program);
                throw new Exception($"Error occurred whilst linking Program({program}). \n\n{infoLog}");
            }
        }

        public void Use()
        {
            GL.UseProgram(handle);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(handle, attribName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(handle);
                disposedValue = true;
            }
        }

        ~Shader()
        {
            GL.DeleteProgram(handle);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
