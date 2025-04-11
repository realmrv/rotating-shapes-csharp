using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RotatingShapes;

public class Octahedron : IShape, IDisposable
{
    // Geometry Data (Regular Octahedron centered at origin)
    private const float OctaScale = 0.6f; // Kept scale factor

    private static readonly Vector3[] _vertices =
    {
        new(0.0f, OctaScale, 0.0f), // Top (0)
        new(-OctaScale, 0.0f, 0.0f), // Left (1)
        new(OctaScale, 0.0f, 0.0f), // Right (2)
        new(0.0f, 0.0f, OctaScale), // Front (3)
        new(0.0f, 0.0f, -OctaScale), // Back (4)
        new(0.0f, -OctaScale, 0.0f) // Bottom (5)
    };

    // Assign more distinct colors to vertices for a clearer gradient
    private static readonly Vector4[] _colors =
    {
        new(1.0f, 0.0f, 0.0f, 0.75f), // Red (Top 0)
        new(0.0f, 1.0f, 0.0f, 0.75f), // Green (Left 1)
        new(0.0f, 0.0f, 1.0f, 0.75f), // Blue (Right 2)
        new(1.0f, 1.0f, 0.0f, 0.75f), // Yellow (Front 3)
        new(1.0f, 0.0f, 1.0f, 0.75f), // Magenta (Back 4)
        new(0.0f, 1.0f, 1.0f, 0.75f) // Cyan (Bottom 5)
        /* Old colors:
        new Vector4(0.0f, 0.0f, 1.0f, 0.75f), // Blue (Top)
        new Vector4(0.0f, 1.0f, 1.0f, 0.75f), // Cyan (Left)
        new Vector4(0.0f, 0.0f, 1.0f, 0.75f), // Blue (Right)
        new Vector4(0.0f, 1.0f, 1.0f, 0.75f), // Cyan (Front)
        new Vector4(0.0f, 0.0f, 1.0f, 0.75f), // Blue (Back)
        new Vector4(0.0f, 1.0f, 1.0f, 0.75f)  // Cyan (Bottom)
        */
    };

    // Indices for drawing the faces (8 triangles)
    private static readonly uint[] _faceIndices =
    {
        // Top pyramid
        0, 1, 3, // Top-Left-Front
        0, 3, 2, // Top-Front-Right
        0, 2, 4, // Top-Right-Back
        0, 4, 1, // Top-Back-Left
        // Bottom pyramid
        5, 3, 1, // Bottom-Front-Left
        5, 2, 3, // Bottom-Right-Front
        5, 4, 2, // Bottom-Back-Right
        5, 1, 4 // Bottom-Left-Back
    };

    // Indices for drawing the edges (12 lines)
    private static readonly uint[] _edgeIndices =
    {
        0, 1, 0, 2, 0, 3, 0, 4, // Top edges
        1, 3, 3, 2, 2, 4, 4, 1, // Middle edges
        5, 1, 5, 2, 5, 3, 5, 4 // Bottom edges
    };

    private float _angle;
    private readonly uint _edgeEbo;
    private readonly Shader _edgeShaderProgram;
    private readonly uint _edgeVao;
    private readonly uint _edgeVboPosition;
    private readonly uint _faceEbo;

    // Shaders
    private readonly Shader _faceShaderProgram;

    // OpenGL Handles
    private readonly uint _faceVao;
    private readonly uint _faceVboColor;
    private readonly uint _faceVboPosition;

    private readonly GL _gl;

    // Position this one center-center (origin)
    private readonly Vector3 _position = new(0.0f, 0.0f, 0.0f);

    public unsafe Octahedron(GL gl, Shader faceShader, Shader edgeShader)
    {
        _gl = gl;
        _faceShaderProgram = faceShader;
        _edgeShaderProgram = edgeShader;

        // --- Create Face Resources ---
        _faceVao = _gl.GenVertexArray();
        _gl.BindVertexArray(_faceVao);

        // Bind and set up Position Buffer (Attribute 0)
        _faceVboPosition = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _faceVboPosition);
        fixed (Vector3* ptr = _vertices)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(_vertices.Length * sizeof(Vector3)), ptr,
                BufferUsageARB.StaticDraw);
        }

        _gl.VertexAttribPointer(0, 3, GLEnum.Float, false, (uint)sizeof(Vector3), (void*)0);
        _gl.EnableVertexAttribArray(0);

        // Bind and set up Color Buffer (Attribute 1)
        _faceVboColor = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _faceVboColor);
        fixed (Vector4* ptr = _colors)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(_colors.Length * sizeof(Vector4)), ptr,
                BufferUsageARB.StaticDraw);
        }

        _gl.VertexAttribPointer(1, 4, GLEnum.Float, false, (uint)sizeof(Vector4), (void*)0);
        _gl.EnableVertexAttribArray(1);

        // Bind Element Buffer
        _faceEbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _faceEbo);
        fixed (uint* ptr = _faceIndices)
        {
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)(_faceIndices.Length * sizeof(uint)), ptr,
                BufferUsageARB.StaticDraw);
        }

        // --- Create Edge Resources ---
        _edgeVao = _gl.GenVertexArray();
        _gl.BindVertexArray(_edgeVao);
        _edgeVboPosition = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _edgeVboPosition);
        fixed (Vector3* ptr = _vertices)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(_vertices.Length * sizeof(Vector3)), ptr,
                BufferUsageARB.StaticDraw);
        }

        _gl.VertexAttribPointer(0, 3, GLEnum.Float, false, (uint)sizeof(Vector3), (void*)0);
        _gl.EnableVertexAttribArray(0);
        _edgeEbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _edgeEbo);
        fixed (uint* ptr = _edgeIndices)
        {
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)(_edgeIndices.Length * sizeof(uint)), ptr,
                BufferUsageARB.StaticDraw);
        }

        // Unbind all
        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
    }

    // Transformations
    public Matrix4x4 ModelMatrix { get; private set; } = Matrix4x4.Identity;

    public void Update(double deltaTime)
    {
        // Yet another rotation style
        _angle += (float)(deltaTime * 40.0f); // Standard speed
        // Standard rotation
        ModelMatrix = Matrix4x4.CreateRotationY(Scalar.DegreesToRadians(_angle)) *
                      Matrix4x4.CreateRotationX(Scalar.DegreesToRadians(_angle * 0.7f)) *
                      Matrix4x4.CreateTranslation(_position); // Apply position offset
    }

    public unsafe void Render(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
    {
        // Render Faces
        _faceShaderProgram.Use();
        _faceShaderProgram.SetUniform("model", ModelMatrix);
        _faceShaderProgram.SetUniform("view", viewMatrix);
        _faceShaderProgram.SetUniform("projection", projectionMatrix);
        _gl.BindVertexArray(_faceVao);
        _gl.DrawElements(PrimitiveType.Triangles, (uint)_faceIndices.Length, DrawElementsType.UnsignedInt, (void*)0);

        // Render Edges
        _edgeShaderProgram.Use();
        _edgeShaderProgram.SetUniform("model", ModelMatrix);
        _edgeShaderProgram.SetUniform("view", viewMatrix);
        _edgeShaderProgram.SetUniform("projection", projectionMatrix);
        _edgeShaderProgram.SetUniform("edgeColor", new Vector4(0.9f, 0.9f, 0.9f, 1.0f)); // Slightly off-white edges
        _gl.BindVertexArray(_edgeVao);
        _gl.DrawElements(PrimitiveType.Lines, (uint)_edgeIndices.Length, DrawElementsType.UnsignedInt, (void*)0);

        _gl.BindVertexArray(0); // Unbind VAO
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(_faceVboPosition);
        _gl.DeleteBuffer(_faceVboColor);
        _gl.DeleteBuffer(_faceEbo);
        _gl.DeleteVertexArray(_faceVao);

        _gl.DeleteBuffer(_edgeVboPosition);
        _gl.DeleteBuffer(_edgeEbo);
        _gl.DeleteVertexArray(_edgeVao);
    }
}