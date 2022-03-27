using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

using OpenTKVoxelEngine_Core;

namespace OpenTKVoxelEngine_Shader
{
    public class Shader : IDisposable
    {
        private bool disposedValue = false;
        private int handle;

        private readonly Dictionary<string, int> _uniformLocations;

        public int Handle { get => handle; }

        public Shader(string vertexPath, string fragmentPath)
        {
            int vertexShader;
            int fragmentShader;

            string vertexShaderSource = File.ReadAllText(vertexPath);
            string fragmentShaderSource = File.ReadAllText(fragmentPath);

            // Create the vertex shader object and replace the source code in a shader object.
            vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            CompileShader(vertexShader);

            // Create the fragment shader object and replace the source code in a shader object.
            fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            CompileShader(fragmentShader);

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

            // Cache all shader uniform locations.
            // First, get the number of active uniforms in the shader.
            GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, out int numberOfUniforms);

            // Allocate, the dictionary to hold the locations.
            _uniformLocations = new Dictionary<string, int>(numberOfUniforms);

            for (int i = 0; i < numberOfUniforms; i++)
            {
                // Get the name of this uniform
                string key = GL.GetActiveUniform(handle, i, out _, out _);

                // Get the location
                int location = GL.GetUniformLocation(handle, key);

                // Then add to the dictionary.
                _uniformLocations.Add(key, location);
            }

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

        public void SetInt(string name, int value)
        {
            int location = GL.GetUniformLocation(handle, name);
            GL.Uniform1(location, value);
        }

        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(handle);
            GL.UniformMatrix4(_uniformLocations[name], true, ref data);
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
