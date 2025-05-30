using MazeProject.Graphics.OpenGL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MazeProject.Graphics.Rendering
{
    /// <summary>
    /// Renders a dynamic top-down minimap showing nearby map tiles and the player’s orientation.
    /// </summary>
    public class MiniMapRenderer
    {
        private int _vao;
        private int _vbo;
        private Shader _shader = null!;

        private readonly float _tileSize = 1.0f;

        private const int TILE_VERTICES = 6; // 2 triangles per tile (quad)

        /// <summary>
        /// Initializes OpenGL buffers and loads the minimap shader.
        /// </summary>
        public MiniMapRenderer()
        {
            _shader = new Shader(
                Path.Combine(AppContext.BaseDirectory, "Graphics", "Shaders", "miniMap.vert"),
                Path.Combine(AppContext.BaseDirectory, "Graphics", "Shaders", "miniMap.frag")
            );

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, 10000 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);

            // Position (2 floats), Color (3 floats)
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);
        }

        /// <summary>
        /// Renders the minimap with visible map tiles and a directional triangle representing the player.
        /// </summary>
        public void Render(char[,] map, int mapWidth, int mapHeight, Vector3 playerPos, float playerYaw)
        {
            // Convert player position from world units to tile space
            float playerTileX = playerPos.X / 2f + 0.5f;
            float playerTileY = playerPos.Z / 2f + 0.5f;

            Vector2 forward = new Vector2(
                MathF.Cos(MathHelper.DegreesToRadians(playerYaw)),
                MathF.Sin(MathHelper.DegreesToRadians(playerYaw))
            );

            const int visibleViewSize = 7;         // Half-size of map view (15x15 grid total)
            const int paddedViewSize = visibleViewSize + 2;

            var vertices = new List<float>();

            for (int dy = -paddedViewSize; dy <= paddedViewSize; dy++)
            {
                for (int dx = -paddedViewSize; dx <= paddedViewSize; dx++)
                {
                    int mapX = (int)playerTileX + dx;
                    int mapY = (int)playerTileY + dy;
                    if (mapX < 0 || mapX >= mapWidth || mapY < 0 || mapY >= mapHeight)
                        continue;

                    char tile = map[mapY, mapX];

                    Vector3 color = tile switch
                    {
                        >= 'o' and <= 'z' => new Vector3(0.2f, 0.2f, 0.2f), // walls
                        >= 'A' and <= 'G' => new Vector3(0.0f, 0.5f, 1.0f), // doors
                        >= 'T' and <= 'Z' => new Vector3(1.0f, 1.0f, 1.0f), // items
                        _ => new Vector3(0.3f, 0.7f, 0.3f),                // floor
                    };

                    float x = mapX - playerTileX + visibleViewSize;
                    float y = mapY - playerTileY + visibleViewSize;

                    AddQuad(vertices, x, y, _tileSize, color);
                }
            }

            // Draw red triangle representing player's direction
            Vector2 center = new Vector2(visibleViewSize, visibleViewSize);
            float size = 0.8f;

            // Right vector = perpendicular to forward
            Vector2 right = new Vector2(-forward.Y, forward.X);

            Vector2 tip = center + forward * size * 0.6f;                         // front tip
            Vector2 left = center - forward * size * 0.5f + right * size * 0.5f; // left corner
            Vector2 rightPt = center - forward * size * 0.5f - right * size * 0.5f; // right corner

            Vector3 red = new Vector3(1f, 0f, 0f);
            vertices.AddRange(new float[] { tip.X, tip.Y, red.X, red.Y, red.Z });
            vertices.AddRange(new float[] { left.X, left.Y, red.X, red.Y, red.Z });
            vertices.AddRange(new float[] { rightPt.X, rightPt.Y, red.X, red.Y, red.Z });

            // Upload vertex data
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * sizeof(float), vertices.ToArray(), BufferUsageHint.DynamicDraw);

            // Use shader
            _shader.Use();

            // Circular mask center and radius
            _shader.SetVector2("uCenter", new Vector2(100, 100));
            _shader.SetFloat("uRadius", 100f);

            // Build transformation matrix for map rotation around player
            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, (visibleViewSize * 2 + 1), (visibleViewSize * 2 + 1), 0, -1, 1);
            Matrix4 rotation = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(-playerYaw - 90f));
            Matrix4 centerTranslate = Matrix4.CreateTranslation(-(visibleViewSize + 0.5f), -(visibleViewSize + 0.5f), 0);
            Matrix4 reverseCenter = Matrix4.CreateTranslation((visibleViewSize + 0.5f), (visibleViewSize + 0.5f), 0);
            Matrix4 final = centerTranslate * rotation * reverseCenter * projection;

            _shader.SetMatrix4("uProjection", final);

            // Draw map
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.DepthTest);

            GL.Viewport(0, 0, 200, 200); // draw in bottom-left corner
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Count / 5);

            // Restore default viewport
            GL.Viewport(0, 0, 1280, 720);
            GL.Enable(EnableCap.DepthTest);
        }

        /// <summary>
        /// Adds a colored quad (2 triangles) to the vertex buffer at the given map tile position.
        /// </summary>
        private void AddQuad(List<float> data, float x, float y, float size, Vector3 color)
        {
            float x0 = x;
            float y0 = y;
            float x1 = x + size;
            float y1 = y + size;

            // First triangle
            data.AddRange(new float[] { x0, y0, color.X, color.Y, color.Z });
            data.AddRange(new float[] { x1, y0, color.X, color.Y, color.Z });
            data.AddRange(new float[] { x1, y1, color.X, color.Y, color.Z });

            // Second triangle
            data.AddRange(new float[] { x0, y0, color.X, color.Y, color.Z });
            data.AddRange(new float[] { x1, y1, color.X, color.Y, color.Z });
            data.AddRange(new float[] { x0, y1, color.X, color.Y, color.Z });
        }
    }
}
