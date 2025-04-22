using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RotatingShapes;

/// <summary>
///     Implementation of a colored, rotating octahedron shape.
/// </summary>
public class Octahedron : ShapeBase
{
    private const float OctaScale = 0.9f;
    private readonly Vector3 _position = new(0.0f, -0.5f, 0.0f);

    private static readonly Vector3[] _vertices =
    {
        new(0.0f, OctaScale, 0.0f), // Top (0)
        new(-OctaScale, 0.0f, 0.0f), // Left (1)
        new(OctaScale, 0.0f, 0.0f), // Right (2)
        new(0.0f, 0.0f, OctaScale), // Front (3)
        new(0.0f, 0.0f, -OctaScale), // Back (4)
        new(0.0f, -OctaScale, 0.0f) // Bottom (5)
    };
    private static readonly Vector4[] _colors =
    {
        new(1.0f, 0.0f, 0.0f, 0.75f), // Red (Top 0)
        new(0.0f, 1.0f, 0.0f, 0.75f), // Green (Left 1)
        new(0.0f, 0.0f, 1.0f, 0.75f), // Blue (Right 2)
        new(1.0f, 1.0f, 0.0f, 0.75f), // Yellow (Front 3)
        new(1.0f, 0.0f, 1.0f, 0.75f), // Magenta (Back 4)
        new(0.0f, 1.0f, 1.0f, 0.75f) // Cyan (Bottom 5)
    };
    private static readonly uint[] _faceIndices =
    {
        0, 1, 3, 0, 3, 2, 0, 2, 4, 0, 4, 1, 5, 3, 1, 5, 2, 3, 5, 4, 2, 5, 1, 4
    };
    private static readonly uint[] _edgeIndices =
    {
        0, 1, 0, 2, 0, 3, 0, 4, 1, 3, 3, 2, 2, 4, 4, 1, 5, 1, 5, 2, 5, 3, 5, 4
    };

    public unsafe Octahedron(GL gl, Shader faceShader, Shader edgeShader)
        : base(gl, faceShader, edgeShader)
    {
        InitFaceBuffers(_vertices, _colors, _faceIndices);
        InitEdgeBuffers(_vertices, _edgeIndices);
    }

    public override void Update(double deltaTime)
    {
        Angle += (float)(deltaTime * 40.0f);
        ModelMatrix = Matrix4x4.CreateRotationY(Scalar.DegreesToRadians(Angle)) *
                      Matrix4x4.CreateRotationX(Scalar.DegreesToRadians(Angle * 0.7f)) *
                      Matrix4x4.CreateTranslation(_position);
    }

    public override unsafe void Render(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
    {
        FaceShader.Use();
        FaceShader.SetUniform("model", ModelMatrix);
        FaceShader.SetUniform("view", viewMatrix);
        FaceShader.SetUniform("projection", projectionMatrix);
        Gl.BindVertexArray(FaceVao);
        Gl.DrawElements(PrimitiveType.Triangles, (uint)_faceIndices.Length, DrawElementsType.UnsignedInt, (void*)0);

        EdgeShader.Use();
        EdgeShader.SetUniform("model", ModelMatrix);
        EdgeShader.SetUniform("view", viewMatrix);
        EdgeShader.SetUniform("projection", projectionMatrix);
        EdgeShader.SetUniform("edgeColor", new Vector4(0.9f, 0.9f, 0.9f, 1.0f));
        Gl.BindVertexArray(EdgeVao);
        Gl.DrawElements(PrimitiveType.Lines, (uint)_edgeIndices.Length, DrawElementsType.UnsignedInt, (void*)0);
        Gl.BindVertexArray(0);
    }
}