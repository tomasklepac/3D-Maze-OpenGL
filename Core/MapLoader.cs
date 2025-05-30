namespace MazeProject.Core
{
    /// <summary>
    /// Provides static methods for loading and interpreting the maze map from file.
    /// </summary>
    public static class MapLoader
    {
        /// <summary>
        /// Loads a map from the given file path, extracting its dimensions and player start position.
        /// </summary>
        /// <param name="path">Path to the map file.</param>
        /// <param name="width">Outputs the map width (number of columns).</param>
        /// <param name="height">Outputs the map height (number of rows).</param>
        /// <param name="start">Outputs the starting player tile coordinates.</param>
        /// <returns>2D character array representing the map.</returns>
        /// <exception cref="Exception">Thrown on invalid map format or symbols.</exception>
        public static char[,] LoadMap(string path, out int width, out int height, out (int x, int y) start)
        {
            string[] lines = File.ReadAllLines(path);

            // Parse dimensions from first line
            string[] dimensions = lines[0].Split('x');
            width = int.Parse(dimensions[0]);
            height = int.Parse(dimensions[1]);

            char[,] map = new char[height, width];
            start = (-1, -1);

            for (int y = 0; y < height; y++)
            {
                string line = lines[y + 1];

                if (line.Length < width)
                {
                    throw new Exception($"Line {y + 1} is shorter than expected width {width}.");
                }

                for (int x = 0; x < width; x++)
                {
                    char c = line[x];
                    map[y, x] = c;

                    if (c == '@')
                    {
                        if (start != (-1, -1))
                            throw new Exception("Map contains more than one start position '@'.");

                        start = (x, y);
                    }

                    if (!IsWall(c) && !IsFree(c) && !IsLight(c) && !IsItem(c) && !IsEnemy(c) && c != '@')
                    {
                        throw new Exception($"Unknown character in map: '{c}' at ({x},{y}).");
                    }
                }
            }

            if (start == (-1, -1))
                throw new Exception("Map does not contain a start position '@'.");

            return map;
        }

        /// <summary>
        /// Determines if the given character represents an impassable wall or solid object.
        /// </summary>
        public static bool IsWall(char c)
        {
            return (c >= 'o' && c <= 'z') || // standard walls
                   (c >= 'A' && c <= 'G') || // doors and secret passages
                   (c >= 'H' && c <= 'N');   // fixed objects
        }

        /// <summary>
        /// Determines if the given character represents a light source.
        /// </summary>
        public static bool IsLight(char c)
        {
            return c == '*' || c == '^' || c == '!';
        }

        /// <summary>
        /// Determines if the given character represents a collectible item.
        /// </summary>
        public static bool IsItem(char c)
        {
            return c >= 'T' && c <= 'Z';
        }

        /// <summary>
        /// Determines if the given character represents an enemy.
        /// </summary>
        public static bool IsEnemy(char c)
        {
            return c >= 'O' && c <= 'R';
        }

        /// <summary>
        /// Determines if the given character is a walkable/free space.
        /// </summary>
        public static bool IsFree(char c)
        {
            // Free = empty, letters a–n, start '@', items, enemies, lights, '|'
            return (c == ' ') || (c >= 'a' && c <= 'n') || (c == '@') || (c == '|') ||
                   IsItem(c) || IsLight(c) || IsEnemy(c);
        }

        /// <summary>
        /// Scans the map for collectible items and returns their positions.
        /// </summary>
        public static List<(int x, int y)> FindCollectibles(char[,] map, int width, int height)
        {
            List<(int x, int y)> items = new();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (IsItem(map[y, x]))
                        items.Add((x, y));
                }
            }

            return items;
        }
    }
}
