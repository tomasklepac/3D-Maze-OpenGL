using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using MazeProject.Graphics.Rendering;
using MazeProject.Core;

namespace MazeProject.App
{
    /// <summary>
    /// The main application window — controls rendering, updating logic, and user input.
    /// </summary>
    public class MainWindow : GameWindow
    {
        // === Map data ===
        private char[,] _map = null!;
        private int _mapWidth;
        private int _mapHeight;
        private (int x, int y) _start;

        // === Rendering components ===
        private CubeRenderer _cubeRenderer = null!;
        private FloorRenderer _floorRenderer = null!;
        private MiniMapRenderer _miniMap = null!;
        private ItemRenderer _itemRenderer = null!;

        // === Camera ===
        private Camera _camera = null!;

        // === State ===
        private Matrix4 _projection;
        private double _fpsTimeAccumulator = 0.0;
        private int _frameCount = 0;
        private float _time = 0f;

        /// <summary>
        /// Initializes a new instance of the MainWindow.
        /// </summary>
        public MainWindow(GameWindowSettings gws, NativeWindowSettings nws)
            : base(gws, nws)
        {
        }

        /// <summary>
        /// Called once when the window is loaded — initializes the entire game state.
        /// </summary>
        protected override void OnLoad()
        {
            base.OnLoad();
            VSync = VSyncMode.On;

            LoadMapAndItems();
            SetupCamera();
            SetupOpenGL();
            CreateRenderers();
            SetupProjectionMatrix();
        }

        /// <summary>
        /// Loads the maze map and collectible item positions from file.
        /// </summary>
        private void LoadMapAndItems()
        {
            _miniMap = new MiniMapRenderer();
            _map = MapLoader.LoadMap("map.txt", out _mapWidth, out _mapHeight, out _start);

            var itemPositions = MapLoader.FindCollectibles(_map, _mapWidth, _mapHeight);
            _itemRenderer = new ItemRenderer(itemPositions, _map);
        }

        /// <summary>
        /// Sets up the camera at the player's start position.
        /// </summary>
        private void SetupCamera()
        {
            _camera = new Camera(new Vector3(_start.x * 2f, 1.7f, _start.y * 2f));
        }

        /// <summary>
        /// Configures global OpenGL state: background color, depth testing, cursor mode.
        /// </summary>
        private void SetupOpenGL()
        {
            GL.ClearColor(0.01f, 0.01f, 0.05f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            CursorState = CursorState.Grabbed;
        }

        /// <summary>
        /// Creates the main renderers for floor and walls.
        /// </summary>
        private void CreateRenderers()
        {
            _cubeRenderer = new CubeRenderer();
            _floorRenderer = new FloorRenderer(_mapWidth, _mapHeight);
        }

        /// <summary>
        /// Sets up the perspective projection matrix.
        /// </summary>
        private void SetupProjectionMatrix()
        {
            _projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(70f),
                Size.X / (float)Size.Y,
                0.1f,
                100f
            );
        }

        /// <summary>
        /// Called every frame to update input and logic (e.g., movement, item collection, FPS).
        /// </summary>
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            // Exit app when ESC is pressed
            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
            {
                Close();
                return;
            }

            _camera.Update(args, KeyboardState, MouseState, _map, _mapWidth, _mapHeight);

            _time += (float)args.Time;

            _fpsTimeAccumulator += args.Time;
            _frameCount++;

            if (_fpsTimeAccumulator >= 1.0)
            {
                Title = $"3D Maze – FPS: {_frameCount} | Items: {_itemRenderer.CollectedCount}";
                _fpsTimeAccumulator = 0.0;
                _frameCount = 0;
            }
        }

        /// <summary>
        /// Called every frame to render the full scene.
        /// </summary>
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 view = _camera.GetViewMatrix();

            _floorRenderer.Render(view, _projection, _camera.Position, _camera.Front);

            // Render maze walls
            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    if (MapLoader.IsWall(_map[y, x]))
                    {
                        Vector3 position = new Vector3(x * 2f, 1.5f, y * 2f);
                        _cubeRenderer.RenderAt(position, view, _projection, _camera.Position, _camera.Front);
                    }
                }
            }

            // Render collectibles
            _itemRenderer.Render(view, _projection, _camera.Position, _camera.Front, _time);

            // Render minimap
            _miniMap.Render(_map, _mapWidth, _mapHeight, _camera.Position, _camera.Yaw);

            SwapBuffers();
        }

        /// <summary>
        /// Disposes of renderers and resources when the application closes.
        /// </summary>
        protected override void OnUnload()
        {
            base.OnUnload();

            _cubeRenderer.Dispose();
            _floorRenderer.Dispose();
            _itemRenderer.Dispose(); // Ensure IDisposable is implemented
            // _miniMap.Dispose(); // Uncomment if MiniMapRenderer implements IDisposable
        }
    }
}
