using MazeProject.Graphics.OpenGL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MazeProject.Graphics.Rendering
{
    /// <summary>
    /// Renders and animates rotating collectible items (diamonds) with full PBR support.
    /// </summary>
    public class ItemRenderer
    {
        private int _vao, _vbo;
        private Shader _shader;

        private Texture _albedoMap;
        private Texture _normalMap;
        private Texture _roughnessMap;
        private Texture _metallicMap;
        private Texture _aoMap;
        private Texture _heightMap;

        private char[,] _map;
        private List<Vector3> _positions;

        private int _modelLoc, _viewLoc, _projLoc;

        /// <summary>
        /// Number of collected items.
        /// </summary>
        public int CollectedCount { get; private set; } = 0;

        /// <summary>
        /// Initializes item renderer and sets up geometry, shaders, and textures.
        /// </summary>
        public ItemRenderer(List<(int x, int y)> positions, char[,] map)
        {
            _positions = new List<Vector3>();
            _map = map;

            foreach (var (x, y) in positions)
                _positions.Add(new Vector3(x * 2f, 0.5f, y * 2f));

            // Octahedron model – 8 triangles (3 vertices each) = 24 total vertices
            // Layout: position.xyz, texcoord.uv, normal.xyz, tangent.xyz, bitangent.xyz
            float[] vertices = {
                // Triangle 1
                 0f, 0.6f, 0f,   0.5f, 1f,   0f, 0.707f, -0.707f,   1f, 0f, 0f,   0f, 1f, 0f,
                -0.3f, 0.3f, 0f, 0f, 0.5f,   0f, 0.707f, -0.707f,   1f, 0f, 0f,   0f, 1f, 0f,
                 0f, 0.3f, -0.3f, 1f, 0.5f,  0f, 0.707f, -0.707f,   1f, 0f, 0f,   0f, 1f, 0f,
                // Triangle 2
                 0f, 0.6f, 0f,   0.5f, 1f,   0.707f, 0.707f, 0f,   0f, 0f, 1f,   0f, 1f, 0f,
                 0f, 0.3f, -0.3f, 0f, 0.5f,  0.707f, 0.707f, 0f,   0f, 0f, 1f,   0f, 1f, 0f,
                 0.3f, 0.3f, 0f, 1f, 0.5f,   0.707f, 0.707f, 0f,   0f, 0f, 1f,   0f, 1f, 0f,
                // Triangle 3
                 0f, 0.6f, 0f,   0.5f, 1f,   0f, 0.707f, 0.707f,   -1f, 0f, 0f,   0f, 1f, 0f,
                 0.3f, 0.3f, 0f, 0f, 0.5f,   0f, 0.707f, 0.707f,   -1f, 0f, 0f,   0f, 1f, 0f,
                 0f, 0.3f, 0.3f, 1f, 0.5f,   0f, 0.707f, 0.707f,   -1f, 0f, 0f,   0f, 1f, 0f,
                // Triangle 4
                 0f, 0.6f, 0f,   0.5f, 1f,   -0.707f, 0.707f, 0f,  0f, 0f, -1f,  0f, 1f, 0f,
                 0f, 0.3f, 0.3f, 0f, 0.5f,   -0.707f, 0.707f, 0f,  0f, 0f, -1f,  0f, 1f, 0f,
                -0.3f, 0.3f, 0f, 1f, 0.5f,   -0.707f, 0.707f, 0f,  0f, 0f, -1f,  0f, 1f, 0f,
                // Triangle 5
                 0f, 0.0f, 0f,   0.5f, 0f,   0f, -0.707f, -0.707f,  1f, 0f, 0f,   0f, 1f, 0f,
                 0f, 0.3f, -0.3f, 0f, 0.5f,  0f, -0.707f, -0.707f,  1f, 0f, 0f,   0f, 1f, 0f,
                -0.3f, 0.3f, 0f, 1f, 0.5f,   0f, -0.707f, -0.707f,  1f, 0f, 0f,   0f, 1f, 0f,
                // Triangle 6
                 0f, 0.0f, 0f,   0.5f, 0f,   0.707f, -0.707f, 0f,  0f, 0f, 1f,   0f, 1f, 0f,
                -0.3f, 0.3f, 0f, 0f, 0.5f,   0.707f, -0.707f, 0f,  0f, 0f, 1f,   0f, 1f, 0f,
                 0f, 0.3f, 0.3f, 1f, 0.5f,   0.707f, -0.707f, 0f,  0f, 0f, 1f,   0f, 1f, 0f,
                // Triangle 7
                 0f, 0.0f, 0f,   0.5f, 0f,   0f, -0.707f, 0.707f,  -1f, 0f, 0f,  0f, 1f, 0f,
                 0f, 0.3f, 0.3f, 0f, 0.5f,   0f, -0.707f, 0.707f,  -1f, 0f, 0f,  0f, 1f, 0f,
                 0.3f, 0.3f, 0f, 1f, 0.5f,   0f, -0.707f, 0.707f,  -1f, 0f, 0f,  0f, 1f, 0f,
                // Triangle 8
                 0f, 0.0f, 0f,   0.5f, 0f,   -0.707f, -0.707f, 0f, 0f, 0f, -1f,  0f, 1f, 0f,
                 0.3f, 0.3f, 0f, 0f, 0.5f,   -0.707f, -0.707f, 0f, 0f, 0f, -1f,  0f, 1f, 0f,
                 0f, 0.3f, -0.3f, 1f, 0.5f,  -0.707f, -0.707f, 0f, 0f, 0f, -1f,  0f, 1f, 0f
            };

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            int stride = 14 * sizeof(float); // 14 floats per vertex

            // Setup vertex attributes: position (0), texcoord (2), normal (1), tangent (3), bitangent (4)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 5 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, stride, 8 * sizeof(float));
            GL.EnableVertexAttribArray(3);

            GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, stride, 11 * sizeof(float));
            GL.EnableVertexAttribArray(4);

            GL.BindVertexArray(0);

            _shader = new Shader(
                Path.Combine(AppContext.BaseDirectory, "Graphics", "Shaders", "vertexShader.vert"),
                Path.Combine(AppContext.BaseDirectory, "Graphics", "Shaders", "fragmentShader.frag")
            );
            _shader.Use();

            // Texture bindings for samplers
            GL.Uniform1(_shader.GetUniformLocation("texture0"), 0);
            GL.Uniform1(_shader.GetUniformLocation("normalMap"), 1);
            GL.Uniform1(_shader.GetUniformLocation("roughnessMap"), 2);
            GL.Uniform1(_shader.GetUniformLocation("metallicMap"), 3);
            GL.Uniform1(_shader.GetUniformLocation("aoMap"), 4);
            GL.Uniform1(_shader.GetUniformLocation("heightMap"), 5);

            // Load textures
            _albedoMap = new Texture(Path.Combine(AppContext.BaseDirectory, "Graphics", "Textures", "diamond_albedo.png"));
            _normalMap = new Texture(Path.Combine(AppContext.BaseDirectory, "Graphics", "Textures", "diamond_normal.png"));
            _roughnessMap = new Texture(Path.Combine(AppContext.BaseDirectory, "Graphics", "Textures", "diamond_roughness.png"));
            _metallicMap = new Texture(Path.Combine(AppContext.BaseDirectory, "Graphics", "Textures", "diamond_metallic.png"));
            _aoMap = new Texture(Path.Combine(AppContext.BaseDirectory, "Graphics", "Textures", "diamond_ao.png"));
            _heightMap = new Texture(Path.Combine(AppContext.BaseDirectory, "Graphics", "Textures", "diamond_height.png"));

            _modelLoc = _shader.GetUniformLocation("model");
            _viewLoc = _shader.GetUniformLocation("view");
            _projLoc = _shader.GetUniformLocation("projection");
        }

        /// <summary>
        /// Renders all visible and uncollected items in the scene.
        /// </summary>
        public void Render(Matrix4 view, Matrix4 projection, Vector3 cameraPos, Vector3 cameraFront, double time)
        {
            _shader.Use();

            // Bind PBR textures
            _albedoMap.Use(TextureUnit.Texture0);
            _normalMap.Use(TextureUnit.Texture1);
            _roughnessMap.Use(TextureUnit.Texture2);
            _metallicMap.Use(TextureUnit.Texture3);
            _aoMap.Use(TextureUnit.Texture4);
            _heightMap.Use(TextureUnit.Texture5);

            // Upload camera matrices
            GL.UniformMatrix4(_viewLoc, false, ref view);
            GL.UniformMatrix4(_projLoc, false, ref projection);

            // Lighting (flashlight)
            Vector3 lightDir = Vector3.Normalize(cameraFront + new Vector3(0f, MathF.Tan(MathHelper.DegreesToRadians(-2f)), 0f));
            Vector3 lightPos = cameraPos + new Vector3(0f, 0.35f, 0f);

            GL.Uniform3(_shader.GetUniformLocation("lightPos"), lightPos);
            GL.Uniform3(_shader.GetUniformLocation("viewPos"), cameraPos);
            GL.Uniform3(_shader.GetUniformLocation("lightColor"), new Vector3(1f));
            GL.Uniform3(_shader.GetUniformLocation("objectColor"), new Vector3(1f));
            GL.Uniform3(_shader.GetUniformLocation("lightDirection"), lightDir);
            GL.Uniform1(_shader.GetUniformLocation("cutOff"), MathF.Cos(MathHelper.DegreesToRadians(40f)));
            GL.Uniform1(_shader.GetUniformLocation("outerCutOff"), MathF.Cos(MathHelper.DegreesToRadians(45f)));

            GL.BindVertexArray(_vao);

            // Draw all remaining collectible items
            for (int i = _positions.Count - 1; i >= 0; i--)
            {
                Vector3 pos = _positions[i];

                // Animated bounce and rotation
                float bounce = MathF.Sin((float)(time * 2.5)) * 0.1f;
                float angle = (float)(time * 100.0);

                Vector3 center = pos + new Vector3(0f, 0.3f + bounce, 0f);
                Matrix4 model = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(angle)) *
                                Matrix4.CreateTranslation(center);

                GL.UniformMatrix4(_modelLoc, false, ref model);

                // Check for player proximity — collect if close enough
                if ((cameraPos - center).Length < 1.0f)
                {
                    Vector2i tile = new((int)(pos.X / 2f), (int)(pos.Z / 2f));
                    _map[tile.Y, tile.X] = ' '; // Clear tile on map
                    _positions.RemoveAt(i);     // Remove item from list
                    CollectedCount++;
                    continue;
                }

                GL.DrawArrays(PrimitiveType.Triangles, 0, 24);
            }

            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Releases GPU resources used by this renderer.
        /// </summary>
        public void Dispose()
        {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
        }
    }
}
