# 3D Maze – OpenGL Game

A retro-style 3D maze game inspired by Wolfenstein 3D, built using C#, OpenTK, and modern OpenGL (core profile 3.3).  
This semester project was created for the "Základy počítačové grafiky" (Fundamentals of Computer Graphics) course at the University of West Bohemia.

## 🎮 Features

- First-person camera with smooth mouse and keyboard controls  
- Collision detection with static walls  
- Textured walls and floor using PBR texture maps:  
  - Albedo, Normal, Roughness, Metallic, AO, Height  
- Normal mapping using per-vertex TBN matrix  
- Fake displacement mapping (height-based vertex offset in shader)  
- Animated collectible items (floating and rotating cubes)  
- Dynamic spotlight attached to the player  
- Interactive doors and hidden passages  
- Real-time minimap in the corner with smooth player tracking  

## 🛠️ Built With

- [.NET 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)  
- [OpenTK](https://opentk.net/) – OpenGL bindings for C#  
- [StbImageSharp](https://github.com/StbSharp/StbImageSharp) – Image loading library  

## 🖥️ How to Run

1. Clone the repository:

   ```
   git clone https://github.com/your-username/3D-Maze-OpenGL.git
   ```

2. Open the solution in **Visual Studio 2022** or newer.

3. Restore NuGet packages if needed.

4. Build and run the project.

## ⌨️ Controls

| Key      | Action                    |
|----------|---------------------------|
| `W` / `S`| Move forward / backward   |
| `A` / `D`| Move left / right         |
| Mouse    | Look around               |
| `E`      | Interact with doors       |
| `Esc`    | Exit the game             |

## 📝 Notes

- The OpenGL code follows the **core profile 3.3**, avoiding deprecated fixed-function pipeline.  
- Shaders are written manually for lighting, normal mapping, and fake displacement.  
- The maze map is loaded from a plain text file (`map.txt`).  
- Assets are either free, public domain, or original.

## 📌 TODO (Optional Future Enhancements)

- Sound effects and ambient music  
- Basic enemy AI  
- Level editor or procedural generation  
- In-game menu and HUD  
- Save/load system

## 👨‍💻 Author

Created by **Tomáš Klepač**  
University of West Bohemia (ZČU), Faculty of Applied Sciences  
Course: KIV/ZPG – Fundamentals of Computer Graphics  
Academic Year: 2024/2025

## 📚 License

This project is intended for educational use.  
You may use or modify it freely for learning or non-commercial purposes.
