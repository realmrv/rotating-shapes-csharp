using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
// Needed for loading shaders from files

// Added for List<T>

namespace RotatingShapes;

/// <summary>
///     Manages the main window, OpenGL context, input, shaders, and the main render loop.
/// </summary>
public class Renderer : IDisposable
{
    private Shader? _edgeShaderProgram;

    // Removed Cube data fields (moved to Cube class)
    // OpenGL object handles are now in Cube class

    // Shaders & Shape List
    private Shader? _faceShaderProgram;
    private GL? _gl;
    private IInputContext? _inputContext;

    private Matrix4x4 _projectionMatrix;

    // Use a list of shapes instead of individual fields
    private readonly List<IShape> _shapes = new();
    // Removed individual shape fields (_cube, _tetrahedron, etc.)

    // Matrices (View and Projection are managed by Renderer)
    private Matrix4x4 _viewMatrix;

    private readonly IWindow? _window;
    // ModelMatrix and Angle are now managed by Cube class

    // Removed Shader source code constants

    private bool _disposed = false;

    public Renderer()
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(1280, 720);
        options.Title = "Rotating Shapes";
        options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible,
            new APIVersion(3, 3));

        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Closing += OnClose;
        _window.Resize += OnResize;
    }

    ~Renderer()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Dispose managed resources
            OnClose(); // Ensure resources are released if Dispose is called explicitly
            // _window?.Dispose(); // Let Silk.NET handle window disposal
        }

        // Free unmanaged resources (if any)

        _disposed = true;
    }

    /// <summary>
    ///     Main entry point to run the render loop.
    /// </summary>
    public void Run()
    {
        _window?.Run();
    }

    private void OnLoad()
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
            var baseDirectory = AppContext.BaseDirectory;
            const string ShadersDir = "Shaders";
            var faceVertPath = Path.Combine(baseDirectory, ShadersDir, "face.vert");
            var faceFragPath = Path.Combine(baseDirectory, ShadersDir, "face.frag");
            var edgeVertPath = Path.Combine(baseDirectory, ShadersDir, "edge.vert");
            var edgeFragPath = Path.Combine(baseDirectory, ShadersDir, "edge.frag");

            _faceShaderProgram = Shader.LoadFromFile(_gl!, faceVertPath, faceFragPath);
            _edgeShaderProgram = Shader.LoadFromFile(_gl!, edgeVertPath, edgeFragPath);
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Shader file not found: {ex.Message}");
            _window?.Close();
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading shaders: {ex.Message}");
            _window?.Close();
            return;
        }

        // Create Shape instances and add them to the list
        _shapes.Add(new Dodecahedron(_gl!, _faceShaderProgram!, _edgeShaderProgram!)); // Far-left
        _shapes.Add(new Icosahedron(_gl!, _faceShaderProgram!, _edgeShaderProgram!)); // Mid-left
        _shapes.Add(new Octahedron(_gl!, _faceShaderProgram!, _edgeShaderProgram!)); // Center
        _shapes.Add(new Cube(_gl!, _faceShaderProgram!, _edgeShaderProgram!)); // Mid-right
        _shapes.Add(new Tetrahedron(_gl!, _faceShaderProgram!, _edgeShaderProgram!)); // Far-right
        _shapes.Add(new CompoundFiveTetrahedra(_gl!, _faceShaderProgram!, _edgeShaderProgram!)); // Five Tetrahedra Compound

        // Setup initial matrices (View and Projection)
        _viewMatrix =
            Matrix4x4.CreateLookAt(new Vector3(0.0f, 0.0f, 10.0f), new Vector3(0.0f, 0.0f, 0.0f), Vector3.UnitY);
        SetupProjectionMatrix(_window!.Size);
    }

    private void OnUpdate(double deltaTime)
    {
        // Update all shapes in the list
        foreach (var shape in _shapes) shape.Update(deltaTime);
        // Removed individual update calls
    }

    private void OnRender(double deltaTime)
    {
        _gl!.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        // --- Render Transparent Objects ---
        // Depth test is enabled, but disable depth writes for transparency
        _gl.DepthMask(false);

        // Render all shapes in the list
        foreach (var shape in _shapes) shape.Render(_viewMatrix, _projectionMatrix);

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
        var aspectRatio = (float)size.X / size.Y;
        _projectionMatrix =
            Matrix4x4.CreatePerspectiveFieldOfView(Scalar.DegreesToRadians(45.0f), aspectRatio, 0.1f, 100.0f);
    }

    private void KeyDown(IKeyboard keyboard, Key key, int arg3)
    {
        if (key == Key.Escape) _window!.Close();
    }

    private void OnClose()
    {
        // Dispose all shapes in the list
        foreach (var shape in _shapes) shape.Dispose();
        _shapes.Clear(); // Clear the list
        // Removed individual dispose calls

        // Dispose Shaders and other resources
        _faceShaderProgram?.Dispose();
        _edgeShaderProgram?.Dispose();
        _gl?.Dispose();
        _inputContext?.Dispose();
        // Note: _window is disposed automatically by Silk.NET when Run() exits
    }
}