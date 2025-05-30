using MazeProject.Graphics.OpenGL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MazeProject.Graphics.Rendering
{
    /// <summary>
    /// Renders a textured floor with PBR lighting and displacement.
    /// </summary>
    public class FloorRenderer
    {
        private int _vao, _vbo;
        private Shader _shader;

        private Texture _albedo;
        private Texture _normalMap;
        private Texture _aoMap;
        private Texture _roughnessMap;
        private Texture _metallicMap;
        private Texture _heightMap;

        /// <summary>
        /// Initializes the floor geometry and loads shader + textures.
        /// </summary>
        /// <param name="mapWidth">Width of the maze in tiles.</param>
        /// <param name="mapHeight">Height of the maze in tiles.</param>
        public FloorRenderer(int mapWidth, int mapHeight)
        {
            float width = mapWidth * 2f;
            float height = mapHeight * 2f;

            // Vertex layout: position (3), normal (3), texCoord (2)
            float[] vertices =
            {
                // Bottom-left
                0f,    1.5f,     0f,     0f, 1f, 0f,   0f, 0f,
                // Bottom-right
                width, 1.5f,     0f,     0f, 1f, 0f,   width / 2f, 0f,
                // Top-right
                width, 1.5f, height,     0f, 1f, 0f,   width / 2f, height / 2f,
                // Top-left
                0f,    1.5f, height,     0f, 1f, 0f,   0f, height / 2f,
            };

            uint[] indices = { 0, 1, 2, 2, 3, 0 };

            // Setup VAO, VBO, EBO
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            int ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            // Position attribute
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

            // Normal attribute
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

            // Texture coordinate attribute
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));

            GL.BindVertexArray(0); // Unbind

            // Load shader
            _shader = new Shader(
                Path.Combine(AppContext.BaseDirectory, "Graphics", "Shaders", "vertexShader.vert"),
                Path.Combine(AppContext.BaseDirectory, "Graphics", "Shaders", "fragmentShader.frag")
            );

            _shader.Use();

            // Load textures
            _albedo = new Texture(Path.Combine(AppContext.BaseDirectory, "Graphics", "Textures", "grass_albedo.png"));
            _normalMap = new Texture(Path.Combine(AppContext.BaseDirectory, "Graphics", "Textures", "grass_normal.png"));
            _aoMap = new Texture(Path.Combine(AppContext.BaseDirectory, "Graphics", "Textures", "grass_ao.png"));
            _roughnessMap = new Texture(Path.Combine(AppContext.BaseDirectory, "Graphics", "Textures", "grass_roughness.png"));
            _metallicMap = new Texture(Path.Combine(AppContext.BaseDirectory, "Graphics", "Textures", "grass_metallic.png"));
            _heightMap = new Texture(Path.Combine(AppContext.BaseDirectory, "Graphics", "Textures", "grass_height.png"));
        }

        /// <summary>
        /// Renders the floor with correct lighting and perspective.
        /// </summary>
        /// <param name="view">Camera view matrix.</param>
        /// <param name="projection">Camera projection matrix.</param>
        /// <param name="cameraPos">Camera world position.</param>
        /// <param name="cameraFront">Camera forward vector.</param>
        public void Render(Matrix4 view, Matrix4 projection, Vector3 cameraPos, Vector3 cameraFront)
        {
            _shader.Use();

            // Model matrix: shifts floor down by 1.5 units (floor height offset)
            Matrix4 model = Matrix4.CreateTranslation(0f, -1.5f, 0f);

            GL.UniformMatrix4(_shader.GetUniformLocation("model"), false, ref model);
            GL.UniformMatrix4(_shader.GetUniformLocation("view"), false, ref view);
            GL.UniformMatrix4(_shader.GetUniformLocation("projection"), false, ref projection);

            // Flashlight calculations (camera-held spotlight)
            Vector3 lightPos = cameraPos + new Vector3(0f, 0.35f, 0f); // slightly above camera
            Vector3 lightDir = Vector3.Normalize(cameraFront + new Vector3(0f, MathF.Tan(MathHelper.DegreesToRadians(-2f)), 0f));

            // Upload lighting uniforms
            GL.Uniform3(_shader.GetUniformLocation("lightPos"), lightPos);
            GL.Uniform3(_shader.GetUniformLocation("viewPos"), cameraPos);
            GL.Uniform3(_shader.GetUniformLocation("lightColor"), new Vector3(1f));
            GL.Uniform3(_shader.GetUniformLocation("objectColor"), new Vector3(1f)); // white floor
            GL.Uniform3(_shader.GetUniformLocation("lightDirection"), lightDir);
            GL.Uniform1(_shader.GetUniformLocation("cutOff"), MathF.Cos(MathHelper.DegreesToRadians(40f)));
            GL.Uniform1(_shader.GetUniformLocation("outerCutOff"), MathF.Cos(MathHelper.DegreesToRadians(45f)));

            // Bind all texture maps to texture units
            _albedo.Use(TextureUnit.Texture0);
            _normalMap.Use(TextureUnit.Texture1);
            _roughnessMap.Use(TextureUnit.Texture2);
            _metallicMap.Use(TextureUnit.Texture3);
            _aoMap.Use(TextureUnit.Texture4);
            _heightMap.Use(TextureUnit.Texture5);

            // Upload texture unit bindings to shader
            GL.Uniform1(_shader.GetUniformLocation("texture0"), 0);
            GL.Uniform1(_shader.GetUniformLocation("normalMap"), 1);
            GL.Uniform1(_shader.GetUniformLocation("roughnessMap"), 2);
            GL.Uniform1(_shader.GetUniformLocation("metallicMap"), 3);
            GL.Uniform1(_shader.GetUniformLocation("aoMap"), 4);
            GL.Uniform1(_shader.GetUniformLocation("heightMap"), 5);

            // Render the floor quad
            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
        }

        /// <summary>
        /// Frees GPU memory used by the floor's VAO and VBO.
        /// </summary>
        public void Dispose()
        {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
        }
    }
}
