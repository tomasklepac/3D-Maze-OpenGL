using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MazeProject.Graphics.OpenGL
{
    /// <summary>
    /// Represents a compiled OpenGL shader program and provides utility methods for setting uniforms.
    /// </summary>
    public class Shader
    {
        /// <summary>
        /// OpenGL handle of the linked shader program.
        /// </summary>
        public readonly int Handle;

        /// <summary>
        /// Loads and compiles a vertex and fragment shader, then links them into a shader program.
        /// </summary>
        /// <param name="vertexPath">Path to the vertex shader source file.</param>
        /// <param name="fragmentPath">Path to the fragment shader source file.</param>
        public Shader(string vertexPath, string fragmentPath)
        {
            // Load shader source code
            string vertexShaderSource = File.ReadAllText(vertexPath);
            string fragmentShaderSource = File.ReadAllText(fragmentPath);

            // Compile vertex shader
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);
            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int vertexStatus);
            if (vertexStatus != (int)All.True)
                throw new Exception($"Vertex shader compilation failed:\n{GL.GetShaderInfoLog(vertexShader)}");

            // Compile fragment shader
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);
            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out int fragmentStatus);
            if (fragmentStatus != (int)All.True)
                throw new Exception($"Fragment shader compilation failed:\n{GL.GetShaderInfoLog(fragmentShader)}");

            // Link shaders into a program
            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);
            GL.LinkProgram(Handle);

            // Cleanup temporary shader objects
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        /// <summary>
        /// Sets this shader program as the current one to be used by OpenGL.
        /// </summary>
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        /// <summary>
        /// Gets the location of a uniform variable by name.
        /// </summary>
        /// <param name="name">The uniform variable name.</param>
        /// <returns>Integer location of the uniform, or -1 if not found.</returns>
        public int GetUniformLocation(string name)
        {
            return GL.GetUniformLocation(Handle, name);
        }

        /// <summary>
        /// Sets a 4x4 matrix uniform.
        /// </summary>
        public void SetMatrix4(string name, Matrix4 matrix)
        {
            int location = GetUniformLocation(name);
            if (location != -1)
                GL.UniformMatrix4(location, false, ref matrix);
        }

        /// <summary>
        /// Sets a float uniform.
        /// </summary>
        public void SetFloat(string name, float value)
        {
            int location = GetUniformLocation(name);
            if (location != -1)
                GL.Uniform1(location, value);
        }

        /// <summary>
        /// Sets a 2D vector uniform.
        /// </summary>
        public void SetVector2(string name, Vector2 vector)
        {
            int location = GetUniformLocation(name);
            if (location != -1)
                GL.Uniform2(location, vector);
        }
    }
}
