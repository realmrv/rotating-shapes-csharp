using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RotatingShapes;

/// <summary>
///     Implementation of a colored, rotating icosahedron shape.
/// </summary>
public class Icosahedron : ShapeBase
{
    private const float IcoScale = 0.9f;
    private static readonly float Phi = (1.0f + MathF.Sqrt(5.0f)) / 2.0f;
    private static readonly Vector3[] _vertices =
    {
        new Vector3(-1, Phi, 0) * IcoScale, new Vector3(1, Phi, 0) * IcoScale, new Vector3(-1, -Phi, 0) * IcoScale,
        new Vector3(1, -Phi, 0) * IcoScale,
        new Vector3(0, -1, Phi) * IcoScale, new Vector3(0, 1, Phi) * IcoScale, new Vector3(0, -1, -Phi) * IcoScale,
        new Vector3(0, 1, -Phi) * IcoScale,
        new Vector3(Phi, 0, -1) * IcoScale, new Vector3(Phi, 0, 1) * IcoScale, new Vector3(-Phi, 0, -1) * IcoScale,
        new Vector3(-Phi, 0, 1) * IcoScale
    };
    private static readonly Vector4[] _colors;
    private static readonly uint[] _faceIndices =
    {
        0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11,
        1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
        3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
        4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1
    };
    private static readonly uint[] _edgeIndices;
    private readonly Vector3 _position = new(-1.8f, -0.5f, 0.0f);

    static Icosahedron()
    {
        for (var i = 0; i < _vertices.Length; i++) _vertices[i] = Vector3.Normalize(_vertices[i]) * IcoScale;
        _colors = new[]
        {
            new Vector4(1.0f, 0.0f, 0.0f, 0.85f), new Vector4(0.0f, 1.0f, 0.0f, 0.85f), new Vector4(0.0f, 0.0f, 1.0f, 0.85f),
            new Vector4(1.0f, 1.0f, 0.0f, 0.85f), new Vector4(1.0f, 0.0f, 1.0f, 0.85f), new Vector4(0.0f, 1.0f, 1.0f, 0.85f),
            new Vector4(1.0f, 0.5f, 0.0f, 0.85f), new Vector4(0.5f, 0.0f, 1.0f, 0.85f), new Vector4(0.0f, 0.5f, 1.0f, 0.85f),
            new Vector4(0.5f, 1.0f, 0.0f, 0.85f), new Vector4(1.0f, 0.0f, 0.5f, 0.85f), new Vector4(0.0f, 1.0f, 0.5f, 0.85f)
        };
        var edges = new HashSet<Tuple<uint, uint>>();
        for (var i = 0; i < _faceIndices.Length; i += 3)
        {
            var i1 = _faceIndices[i];
            var i2 = _faceIndices[i + 1];
            var i3 = _faceIndices[i + 2];
            AddEdge(edges, i1, i2);
            AddEdge(edges, i2, i3);
            AddEdge(edges, i3, i1);
        }
        _edgeIndices = edges.SelectMany(t => new[] { t.Item1, t.Item2 }).ToArray();
    }

    public unsafe Icosahedron(GL gl, Shader faceShader, Shader edgeShader)
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

    private static void AddEdge(HashSet<Tuple<uint, uint>> edges, uint u, uint v)
    {
        if (u > v)
        {
            var temp = u;
            u = v;
            v = temp;
        }
        edges.Add(Tuple.Create(u, v));
    }
}