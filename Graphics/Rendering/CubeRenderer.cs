using MazeProject.Graphics.OpenGL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MazeProject.Graphics.Rendering
{
    /// <summary>
    /// Renders a textured cube with PBR lighting using multiple texture maps.
    /// </summary>
    public class CubeRenderer
    {
        private int _vao;
        private int _vbo;
        private int _ibo;

        private Shader _shader;

        private Texture _wallTexture;
        private Texture _normalMap;
        private Texture _roughnessMap;
        private Texture _metallicMap;
        private Texture _aoMap;
        private Texture _heightMap;

        private int _modelLocation;
        private int _viewLocation;
        private int _projectionLocation;
        private int _lightPosLocation;
        private int _viewPosLocation;
        private int _lightColorLocation;
        private int _objectColorLocation;
        private int _lightDirectionLocation;
        private int _cutOffLocation;
        private int _outerCutOffLocation;

        // Vertex data format: position (3), normal (3), texCoords (2)
        private readonly float[] _vertices =
        {
            // Back face
            -1f, -1.5f, -1f,  0f, 0f, -1f,  0f, 0f,
             1f, -1.5f, -1f,  0f, 0f, -1f,  1f, 0f,
             1f,  1.5f, -1f,  0f, 0f, -1f,  1f, 1f,
            -1f,  1.5f, -1f,  0f, 0f, -1f,  0f, 1f,

            // Front face
            -1f, -1.5f,  1f,  0f, 0f, 1f,  0f, 0f,
             1f, -1.5f,  1f,  0f, 0f, 1f,  1f, 0f,
             1f,  1.5f,  1f,  0f, 0f, 1f,  1f, 1f,
            -1f,  1.5f,  1f,  0f, 0f, 1f,  0f, 1f,

            // Left face
            -1f, -1.5f, -1f, -1f, 0f, 0f,  0f, 0f,
            -1f,  1.5f, -1f, -1f, 0f, 0f,  0f, 1f,
            -1f,  1.5f,  1f, -1f, 0f, 0f,  1f, 1f,
            -1f, -1.5f,  1f, -1f, 0f, 0f,  1f, 0f,

            // Right face
             1f, -1.5f, -1f,  1f, 0f, 0f,  0f, 0f,
             1f,  1.5f, -1f,  1f, 0f, 0f,  0f, 1f,
             1f,  1.5f,  1f,  1f, 0f, 0f,  1f, 1f,
             1f, -1.5f,  1f,  1f, 0f, 0f,  1f, 0f,

            // Bottom face
            -1f, -1.5f, -1f,  0f, -1f, 0f,  0f, 0f,
             1f, -1.5f, -1f,  0f, -1f, 0f,  1f, 0f,
             1f, -1.5f,  1f,  0f, -1f, 0f,  1f, 1f,
            -1f, -1.5f,  1f,  0f, -1f, 0f,  0f, 1f,

            // Top face
            -1f,  1.5f, -1f,  0f, 1f, 0f,  0f, 0f,
             1f,  1.5f, -1f,  0f, 1f, 0f,  1f, 0f,
             1f,  1.5f,  1f,  0f, 1f, 0f,  1f, 1f,
            -1f,  1.5f,  1f,  0f, 1f, 0f,  0f, 1f
        };

        private readonly uint[] _indices =
        {
            0, 1, 2, 2, 3, 0,    // Back
            4, 5, 6, 6, 7, 4,    // Front
            8, 9,10,10,11, 8,    // Left
           12,13,14,14,15,12,    // Right
           16,17,18,18,19,16,    // Bottom
           20,21,22,22,23,20     // Top
        };

        /// <summary>
        /// Initializes the cube renderer: buffers, attributes, shader and textures.
        /// </summary>
        public CubeRenderer()
        {
            // Generate and bind VBO
            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            // Generate and bind IBO
            _ibo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            // Generate and bind VAO
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);

            // Setup vertex attributes: position (0), normal (1), texCoords (2)
            int stride = 8 * sizeof(float);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));

            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));

            // Load shader
            _shader = new Shader(
                Path.Combine(AppContext.BaseDirectory, "Graphics", "Shaders", "vertexShader.vert"),
                Path.Combine(AppContext.BaseDirectory, "Graphics", "Shaders", "fragmentShader.frag")
            );

            // Load PBR textures
            _wallTexture = new Texture(Path.Combine(AppContext.BaseDirectory, "Graphics", "Textures", "wall_albedo.png"));
            _normalMap = new Texture(Path.Combine(AppContext.BaseDirectory, "Graphics", "Textures", "wall_normal.png"));
            _roughnessMap = new Texture(Path.Combine(AppContext.BaseDirectory, "Graphics", "Textures", "wall_roughness.png"));
            _metallicMap = new Texture(Path.Combine(AppContext.BaseDirectory, "Graphics", "Textures", "wall_metallic.png"));
            _aoMap = new Texture(Path.Combine(AppContext.BaseDirectory, "Graphics", "Textures", "wall_ao.png"));
            _heightMap = new Texture(Path.Combine(AppContext.BaseDirectory, "Graphics", "Textures", "wall_height.png"));

            // Get shader uniform locations
            _shader.Use();
            _modelLocation = _shader.GetUniformLocation("model");
            _viewLocation = _shader.GetUniformLocation("view");
            _projectionLocation = _shader.GetUniformLocation("projection");

            _lightPosLocation = _shader.GetUniformLocation("lightPos");
            _viewPosLocation = _shader.GetUniformLocation("viewPos");
            _lightColorLocation = _shader.GetUniformLocation("lightColor");
            _objectColorLocation = _shader.GetUniformLocation("objectColor");

            _lightDirectionLocation = _shader.GetUniformLocation("lightDirection");
            _cutOffLocation = _shader.GetUniformLocation("cutOff");
            _outerCutOffLocation = _shader.GetUniformLocation("outerCutOff");

            GL.BindVertexArray(0); // Unbind VAO
        }

        /// <summary>
        /// Renders a cube at the specified position using camera and lighting data.
        /// </summary>
        public void RenderAt(Vector3 position, Matrix4 view, Matrix4 projection, Vector3 cameraPos, Vector3 cameraFront)
        {
            _shader.Use();

            // Bind all relevant textures to texture units
            _wallTexture.Use(TextureUnit.Texture0);
            _normalMap.Use(TextureUnit.Texture1);
            _roughnessMap.Use(TextureUnit.Texture2);
            _metallicMap.Use(TextureUnit.Texture3);
            _aoMap.Use(TextureUnit.Texture4);
            _heightMap.Use(TextureUnit.Texture5);

            Matrix4 model = Matrix4.CreateTranslation(position);

            // Upload matrices
            GL.UniformMatrix4(_modelLocation, false, ref model);
            GL.UniformMatrix4(_viewLocation, false, ref view);
            GL.UniformMatrix4(_projectionLocation, false, ref projection);

            // Calculate flashlight position and direction
            Vector3 lightPos = cameraPos + new Vector3(0f, 0.35f, 0f); // flashlight offset
            Vector3 lightDir = Vector3.Normalize(cameraFront + new Vector3(0f, MathF.Tan(MathHelper.DegreesToRadians(-2f)), 0f));

            // Upload lighting uniforms
            GL.Uniform3(_lightPosLocation, lightPos);
            GL.Uniform3(_viewPosLocation, cameraPos);
            GL.Uniform3(_lightColorLocation, new Vector3(1f));
            GL.Uniform3(_objectColorLocation, new Vector3(1.0f, 0.3f, 0.5f));
            GL.Uniform3(_lightDirectionLocation, lightDir);
            GL.Uniform1(_cutOffLocation, MathF.Cos(MathHelper.DegreesToRadians(40f)));
            GL.Uniform1(_outerCutOffLocation, MathF.Cos(MathHelper.DegreesToRadians(45f)));

            // Upload texture bindings (match to samplers in shader)
            GL.Uniform1(_shader.GetUniformLocation("normalMap"), 1);
            GL.Uniform1(_shader.GetUniformLocation("roughnessMap"), 2);
            GL.Uniform1(_shader.GetUniformLocation("metallicMap"), 3);
            GL.Uniform1(_shader.GetUniformLocation("aoMap"), 4);
            GL.Uniform1(_shader.GetUniformLocation("heightMap"), 5);

            // Draw the cube
            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        /// <summary>
        /// Releases all GPU resources used by the cube renderer.
        /// </summary>
        public void Dispose()
        {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ibo);
        }
    }
}
