# Rotating Shapes

A .NET 9 C# project demonstrating basic 3D graphics rendering using Silk.NET (OpenGL wrapper).
It displays five rotating Platonic solids (Cube, Tetrahedron, Octahedron, Icosahedron, Dodecahedron) arranged horizontally.

## Features

* **Multiple Shapes:** Renders Cube, Tetrahedron, Octahedron, Icosahedron, and Dodecahedron.
* **OpenGL Rendering:** Uses Silk.NET for cross-platform OpenGL 3.3 Core Profile rendering.
* **Independent Rotation:** Each shape rotates independently with the same speed and axis logic.
* **Vertex Colors & Gradient:** Each shape has distinct vertex colors, creating interpolated gradients on the faces.
* **Transparency:** Shapes are rendered with alpha blending enabled, allowing visibility through faces.
* **Edge Highlighting:** Edges of the shapes are drawn in white for better definition.
* **Refactored Structure:**
  * Uses an `IShape` interface for easy addition of new shapes.
  * Separate classes for each shape (`Cube`, `Tetrahedron`, etc.) encapsulating geometry, OpenGL resources (VAO/VBO/EBO), update, and rendering logic.
  * A `Renderer` class manages the window, OpenGL context, input, shader loading, and the main render loop.
  * A `Shader` helper class loads GLSL shaders from external files.
* **External Shader Files:** GLSL vertex and fragment shaders are stored in the `RotatingShapes/Shaders/` directory and copied to the output directory on build.

## Requirements

* .NET 9 SDK or later.
* A graphics card supporting OpenGL 3.3.

## How to Run

1. **Clone the repository:**

    ```bash
    git clone <repository-url>
    cd rotating-shapes
    ```

2. **Restore dependencies:**

    ```bash
    dotnet restore
    ```

3. **Build the project:**

    ```bash
    dotnet build
    ```

4. **Run the application:**

    ```bash
    dotnet run --project RotatingShapes/RotatingShapes.csproj
    ```

    Alternatively, you can run directly from the solution file (if using an IDE like Visual Studio or Rider) or navigate to the build output directory (`RotatingShapes/bin/Debug/net9.0/`) and execute the `RotatingShapes` application.

## Controls

* **ESC:** Close the application window.

## Project Structure

```
rotating-shapes/
├── .gitignore
├── rotating-shapes.sln
└── RotatingShapes/
    ├── RotatingShapes.csproj
    ├── Program.cs           # Main entry point
    ├── Renderer.cs          # Manages window, OpenGL, main loop
    ├── IShape.cs            # Interface for all shapes
    ├── Cube.cs              # Cube implementation
    ├── Tetrahedron.cs       # Tetrahedron implementation
    ├── Octahedron.cs        # Octahedron implementation
    ├── Icosahedron.cs       # Icosahedron implementation
    ├── Dodecahedron.cs      # Dodecahedron implementation
    ├── Shader.cs            # Helper for loading/managing shaders
    └── Shaders/             # Directory for GLSL shader files
        ├── edge.frag
        ├── edge.vert
        ├── face.frag
        └── face.vert
```
