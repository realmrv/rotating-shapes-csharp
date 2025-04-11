using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.Input;
using System.Numerics;
using System;
using Silk.NET.Maths;
using System.IO; // Needed for loading shaders from files

namespace RotatingShapes
{
    public class Renderer : IDisposable
    {
        private IWindow? _window;
        private GL? _gl;
        private IInputContext? _inputContext;

        // Removed Cube data fields (moved to Cube class)
        // OpenGL object handles are now in Cube class

        // Shaders & Cube Instance
        private Shader? _faceShaderProgram;
        private Shader? _edgeShaderProgram;
        private Cube? _cube;
        private Tetrahedron? _tetrahedron;
        private Octahedron? _octahedron;
        private Icosahedron? _icosahedron;
        private Dodecahedron? _dodecahedron; // Added Dodecahedron instance

        // Matrices (View and Projection are managed by Renderer)
        private Matrix4x4 _viewMatrix;
        private Matrix4x4 _projectionMatrix;
        // ModelMatrix and Angle are now managed by Cube class

        // Removed Shader source code constants

        public Renderer()
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(1280, 720);
            options.Title = "Rotating Shapes";
            options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(3, 3));

            _window = Window.Create(options);

            _window.Load += OnLoad;
            _window.Update += OnUpdate;
            _window.Render += OnRender;
            _window.Closing += OnClose;
            _window.Resize += OnResize;
        }

        public void Run()
        {
            _window?.Run();
        }

        private unsafe void OnLoad()
        {
            _gl = _window!.CreateOpenGL();
            _inputContext = _window!.CreateInput();
            _inputContext!.Keyboards[0].KeyDown += KeyDown;

            _gl!.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            _gl!.Enable(EnableCap.DepthTest);
            _gl!.Enable(EnableCap.Blend);
            _gl!.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            _gl!.LineWidth(2.0f);

            Console.WriteLine("OpenGL Version: " + _gl!.GetStringS(StringName.Version));
            Console.WriteLine("GLSL Version: " + _gl!.GetStringS(StringName.ShadingLanguageVersion));

            // --- Removed Cube Resource Creation (now in Cube class) ---

            // Load Shaders from files
            try
            {
                // Use paths relative to the executable location
                string baseDirectory = AppContext.BaseDirectory;
                string faceVertPath = Path.Combine(baseDirectory, "Shaders", "face.vert");
                string faceFragPath = Path.Combine(baseDirectory, "Shaders", "face.frag");
                string edgeVertPath = Path.Combine(baseDirectory, "Shaders", "edge.vert");
                string edgeFragPath = Path.Combine(baseDirectory, "Shaders", "edge.frag");

                _faceShaderProgram = Shader.LoadFromFile(_gl!, faceVertPath, faceFragPath);
                _edgeShaderProgram = Shader.LoadFromFile(_gl!, edgeVertPath, edgeFragPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading shaders: {ex.Message}");
                // Handle the error appropriately, e.g., close the window or use fallback shaders
                _window?.Close(); // Close if shaders failed to load
                return;
            }

            // Create Cube instance
            _cube = new Cube(_gl!, _faceShaderProgram!, _edgeShaderProgram!);

            // Create Tetrahedron instance
            _tetrahedron = new Tetrahedron(_gl!, _faceShaderProgram!, _edgeShaderProgram!);

            // Create Octahedron instance
            _octahedron = new Octahedron(_gl!, _faceShaderProgram!, _edgeShaderProgram!);

            // Create Icosahedron instance
            _icosahedron = new Icosahedron(_gl!, _faceShaderProgram!, _edgeShaderProgram!);

            // Create Dodecahedron instance
            _dodecahedron = new Dodecahedron(_gl!, _faceShaderProgram!, _edgeShaderProgram!);

            // Setup initial matrices (View and Projection)
            _viewMatrix = Matrix4x4.CreateLookAt(new Vector3(0.0f, 0.0f, 9.0f), new Vector3(0.0f, 0.0f, 0.0f), Vector3.UnitY);
            SetupProjectionMatrix(_window!.Size);
        }

        private void OnUpdate(double deltaTime)
        {
            // Update the cube (handles its own rotation)
            _cube?.Update(deltaTime);
            // Update the tetrahedron
            _tetrahedron?.Update(deltaTime);
            // Update the octahedron
            _octahedron?.Update(deltaTime);
            // Update the icosahedron
            _icosahedron?.Update(deltaTime);
            // Update the dodecahedron
            _dodecahedron?.Update(deltaTime);
            // Removed direct angle and model matrix update
        }

        private unsafe void OnRender(double deltaTime)
        {
            _gl!.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

            // --- Render Transparent Objects ---
            // Depth test is enabled, but disable depth writes for transparency
            _gl.DepthMask(false);

            // Render the cube
            _cube?.Render(_viewMatrix, _projectionMatrix);
            // Render the tetrahedron
            _tetrahedron?.Render(_viewMatrix, _projectionMatrix);
            // Render the octahedron
            _octahedron?.Render(_viewMatrix, _projectionMatrix);
            // Render the icosahedron
            _icosahedron?.Render(_viewMatrix, _projectionMatrix);
            // Render the dodecahedron
            _dodecahedron?.Render(_viewMatrix, _projectionMatrix);

            // Re-enable depth writes for any subsequent opaque rendering (or just good practice)
            _gl.DepthMask(true);

            // Removed direct rendering code
        }

        private void OnResize(Vector2D<int> size)
        {
            _gl!.Viewport(size);
            SetupProjectionMatrix(size);
        }

        private void SetupProjectionMatrix(Vector2D<int> size)
        {
            if (size.Y == 0) return;
            float aspectRatio = (float)size.X / size.Y;
            _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(Scalar.DegreesToRadians(45.0f), aspectRatio, 0.1f, 100.0f);
        }

        private void KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            if (key == Key.Escape)
            {
                _window!.Close();
            }
        }

        private void OnClose()
        {
            // Dispose Cube Resources
            _cube?.Dispose(); // Dispose the cube's resources
            // Dispose Tetrahedron Resources
            _tetrahedron?.Dispose();
            // Dispose Octahedron Resources
            _octahedron?.Dispose();
            // Dispose Icosahedron Resources
            _icosahedron?.Dispose();
            // Dispose Dodecahedron Resources
            _dodecahedron?.Dispose();

            // --- Removed direct disposal of cube VBOs/VAOs/EBOs ---

            // Dispose Shaders and other resources
            _faceShaderProgram?.Dispose();
            _edgeShaderProgram?.Dispose();
            _gl?.Dispose();
            _inputContext?.Dispose();
            // Note: _window is disposed automatically by Silk.NET when Run() exits
        }

        public void Dispose()
        {
            OnClose(); // Ensure resources are released if Dispose is called explicitly
            // _window?.Dispose(); // Let Silk.NET handle window disposal
        }
    }
}
