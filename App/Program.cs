using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace MazeProject.App
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main method — sets up the main window and starts the application loop.
        /// </summary>
        /// <param name="args">Command-line arguments (not used).</param>
        public static void Main(string[] args)
        {
            var nativeSettings = new NativeWindowSettings
            {
                ClientSize = new Vector2i(1280, 720),
                Title = "3D Maze"
            };

            using var window = new MainWindow(GameWindowSettings.Default, nativeSettings);
            window.Run();
        }
    }
}
