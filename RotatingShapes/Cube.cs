using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

// Required for Scalar.DegreesToRadians

namespace RotatingShapes;

/// <summary>
///     Implementation of a colored, rotating cube shape.
/// </summary>
public class Cube : ShapeBase
{
    private const float CubeScale = 0.9f; // Adjusted scale
    private static readonly Vector3[] _vertices =
    {
        // Front face
        new Vector3(-0.5f, -0.5f, 0.5f) * CubeScale, new Vector3(0.5f, -0.5f, 0.5f) * CubeScale,
        new Vector3(0.5f, 0.5f, 0.5f) * CubeScale, new Vector3(-0.5f, 0.5f, 0.5f) * CubeScale,
        // Back face
        new Vector3(-0.5f, -0.5f, -0.5f) * CubeScale, new Vector3(0.5f, -0.5f, -0.5f) * CubeScale,
        new Vector3(0.5f, 0.5f, -0.5f) * CubeScale, new Vector3(-0.5f, 0.5f, -0.5f) * CubeScale
    };
    private static readonly Vector4[] _colors =
    {
        new(1.0f, 0.0f, 0.0f, 0.7f), new(1.0f, 1.0f, 0.0f, 0.7f), new(1.0f, 1.0f, 0.0f, 0.7f),
        new(1.0f, 0.0f, 0.0f, 0.7f),
        new(0.0f, 0.0f, 1.0f, 0.7f), new(0.0f, 1.0f, 1.0f, 0.7f), new(0.0f, 1.0f, 1.0f, 0.7f),
        new(0.0f, 0.0f, 1.0f, 0.7f)
    };
    private static readonly uint[] _faceIndices =
    {
        0, 1, 2, 2, 3, 0, 4, 5, 6, 6, 7, 4, 4, 7, 3, 3, 0, 4,
        1, 5, 6, 6, 2, 1, 3, 2, 6, 6, 7, 3, 4, 0, 1, 1, 5, 4
    };
    private static readonly uint[] _edgeIndices =
    {
        0, 1, 1, 2, 2, 3, 3, 0, 4, 5, 5, 6, 6, 7, 7, 4,
        0, 4, 1, 5, 2, 6, 3, 7
    };
    private readonly Vector3 _position = new(1.8f, -0.5f, 0.0f);

    public unsafe Cube(GL gl, Shader faceShader, Shader edgeShader)
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
        EdgeShader.SetUniform("edgeColor", new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
        Gl.BindVertexArray(EdgeVao);
        Gl.DrawElements(PrimitiveType.Lines, (uint)_edgeIndices.Length, DrawElementsType.UnsignedInt, (void*)0);
        Gl.BindVertexArray(0);
    }
}