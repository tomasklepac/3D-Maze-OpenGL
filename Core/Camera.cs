using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MazeProject.Core
{
    /// <summary>
    /// First-person camera controller with physics-based movement and bobbing effect.
    /// </summary>
    public class Camera
    {
        /// <summary>
        /// Current camera world position.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Forward-facing direction.
        /// </summary>
        public Vector3 Front = -Vector3.UnitZ;

        /// <summary>
        /// Global up direction.
        /// </summary>
        public Vector3 Up = Vector3.UnitY;

        private float _pitch;
        private float _yaw = -90f;

        /// <summary>
        /// Current yaw angle (horizontal view).
        /// </summary>
        public float Yaw => _yaw;

        private float _speed = 1.4f;
        private float _sensitivity = 0.2f;

        private float _walkTime = 0f;
        private bool _isMoving = false;

        private Vector3 _velocity = Vector3.Zero;
        private float _acceleration = 10f;
        private float _damping = 0.1f;

        private const float EyeHeight = 1.7f;

        /// <summary>
        /// Creates a new camera at the given world position.
        /// </summary>
        public Camera(Vector3 startPosition)
        {
            Position = startPosition;
        }

        /// <summary>
        /// Returns the current view matrix for rendering.
        /// </summary>
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + Front, Up);
        }

        /// <summary>
        /// Updates the camera's position and direction based on player input and world map collisions.
        /// </summary>
        public void Update(FrameEventArgs args, KeyboardState keyboard, MouseState mouse, char[,] map, int mapWidth, int mapHeight)
        {
            float deltaTime = (float)args.Time;

            // Determine movement direction based on key input
            Vector3 right = Vector3.Normalize(Vector3.Cross(Front, Up));
            Vector3 forward = Vector3.Normalize(Vector3.Cross(Up, right));
            Vector3 moveDir = Vector3.Zero;

            if (keyboard.IsKeyDown(Keys.W)) moveDir += forward;
            if (keyboard.IsKeyDown(Keys.S)) moveDir -= forward;
            if (keyboard.IsKeyDown(Keys.A)) moveDir -= right;
            if (keyboard.IsKeyDown(Keys.D)) moveDir += right;

            _isMoving = moveDir.LengthSquared > 0;

            // Smooth velocity transition with acceleration
            Vector3 targetVelocity = _isMoving ? Vector3.Normalize(moveDir) * _speed : Vector3.Zero;
            _velocity = Vector3.Lerp(_velocity, targetVelocity, _acceleration * deltaTime);

            // Apply damping when movement stops
            if (!_isMoving)
            {
                float drop = _damping * deltaTime;
                if (_velocity.Length > drop)
                    _velocity -= Vector3.Normalize(_velocity) * drop;
                else
                    _velocity = Vector3.Zero;
            }

            // Attempt to move camera and resolve collisions
            Position = TryMove(_velocity, deltaTime, map, mapWidth, mapHeight);

            // Bobbing effect based on time and motion
            float bobbingOffset = _isMoving ? MathF.Sin(_walkTime) * 0.05f : 0f;
            _walkTime = _isMoving ? _walkTime + deltaTime * 12.5f : 0f;
            Position.Y = EyeHeight + bobbingOffset;

            // Apply mouse input to rotate view
            _yaw += mouse.Delta.X * _sensitivity;
            _pitch -= mouse.Delta.Y * _sensitivity;
            _pitch = MathHelper.Clamp(_pitch, -89f, 89f); // Prevent flipping

            // Recalculate front vector based on yaw and pitch
            Vector3 front;
            front.X = MathF.Cos(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
            front.Y = MathF.Sin(MathHelper.DegreesToRadians(_pitch));
            front.Z = MathF.Sin(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
            Front = Vector3.Normalize(front);
        }

        /// <summary>
        /// Attempts to move the camera in the given direction, resolving collisions with walls.
        /// </summary>
        private Vector3 TryMove(Vector3 direction, float deltaTime, char[,] map, int mapWidth, int mapHeight)
        {
            Vector3 newPos = Position;

            // Check X movement
            Vector3 nextX = Position + new Vector3(direction.X, 0, 0) * deltaTime;
            if (!IsWallAt(nextX, map, mapWidth, mapHeight))
                newPos.X = nextX.X;

            // Check Z movement
            Vector3 nextZ = Position + new Vector3(0, 0, direction.Z) * deltaTime;
            if (!IsWallAt(nextZ, map, mapWidth, mapHeight))
                newPos.Z = nextZ.Z;

            return newPos;
        }

        /// <summary>
        /// Checks whether the given position is colliding with a wall on the map.
        /// </summary>
        private bool IsWallAt(Vector3 position, char[,] map, int mapWidth, int mapHeight)
        {
            const float radius = 0.3f;

            float minX = position.X - radius;
            float maxX = position.X + radius;
            float minZ = position.Z - radius;
            float maxZ = position.Z + radius;

            int startX = (int)MathF.Floor((minX + 1f) / 2f);
            int endX = (int)MathF.Floor((maxX + 1f) / 2f);
            int startY = (int)MathF.Floor((minZ + 1f) / 2f);
            int endY = (int)MathF.Floor((maxZ + 1f) / 2f);

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    // Out-of-bounds is treated as wall
                    if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                        return true;

                    if (MapLoader.IsWall(map[y, x]))
                        return true;
                }
            }

            return false;
        }
    }
}
