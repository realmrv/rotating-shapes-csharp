using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RotatingShapes;

/// <summary>
///     Implementation of a colored, rotating tetrahedron shape.
/// </summary>
public class Tetrahedron : ShapeBase
{
    private const float TetraScale = 0.7f;
    private static readonly Vector3 v0 = Vector3.Normalize(new Vector3(1, 1, 1)) * TetraScale;
    private static readonly Vector3 v1 = Vector3.Normalize(new Vector3(1, -1, -1)) * TetraScale;
    private static readonly Vector3 v2 = Vector3.Normalize(new Vector3(-1, 1, -1)) * TetraScale;
    private static readonly Vector3 v3 = Vector3.Normalize(new Vector3(-1, -1, 1)) * TetraScale;
    private static readonly Vector3[] _vertices = { v0, v1, v2, v3 };
    private static readonly Vector4[] _colors =
    {
        new(0.0f, 1.0f, 0.0f, 0.8f), // v0
        new(1.0f, 0.0f, 1.0f, 0.8f), // v1
        new(0.0f, 1.0f, 0.0f, 0.8f), // v2
        new(1.0f, 0.0f, 1.0f, 0.8f) // v3
    };
    private static readonly uint[] _faceIndices = { 0, 1, 2, 0, 3, 1, 0, 2, 3, 1, 3, 2 };
    private static readonly uint[] _edgeIndices = { 0, 1, 0, 2, 0, 3, 1, 2, 1, 3, 2, 3 };
    private readonly Vector3 _position = new(3.6f, 0.0f, 0.0f);

    public unsafe Tetrahedron(GL gl, Shader faceShader, Shader edgeShader)
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
        EdgeShader.SetUniform("edgeColor", new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
        Gl.BindVertexArray(EdgeVao);
        Gl.DrawElements(PrimitiveType.Lines, (uint)_edgeIndices.Length, DrawElementsType.UnsignedInt, (void*)0);
        Gl.BindVertexArray(0);
    }
}