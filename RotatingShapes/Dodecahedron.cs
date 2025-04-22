using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RotatingShapes;

/// <summary>
///     Implementation of a colored, rotating dodecahedron shape.
/// </summary>
public class Dodecahedron : ShapeBase
{
    // Geometry data for a regular dodecahedron centered at the origin
    private const float DodecaScale = 0.7f;
    private static readonly float Phi = (1.0f + MathF.Sqrt(5.0f)) / 2.0f;
    private static readonly float InvPhi = 1.0f / Phi;
    private static readonly Vector3[] _vertices =
    {
        new Vector3(1, 1, 1) * DodecaScale,
        new Vector3(1, 1, -1) * DodecaScale,
        new Vector3(1, -1, 1) * DodecaScale,
        new Vector3(1, -1, -1) * DodecaScale,
        new Vector3(-1, 1, 1) * DodecaScale,
        new Vector3(-1, 1, -1) * DodecaScale,
        new Vector3(-1, -1, 1) * DodecaScale,
        new Vector3(-1, -1, -1) * DodecaScale,
        new Vector3(0, InvPhi, Phi) * DodecaScale,
        new Vector3(0, InvPhi, -Phi) * DodecaScale,
        new Vector3(0, -InvPhi, Phi) * DodecaScale,
        new Vector3(0, -InvPhi, -Phi) * DodecaScale,
        new Vector3(InvPhi, Phi, 0) * DodecaScale,
        new Vector3(InvPhi, -Phi, 0) * DodecaScale,
        new Vector3(-InvPhi, Phi, 0) * DodecaScale,
        new Vector3(-InvPhi, -Phi, 0) * DodecaScale,
        new Vector3(Phi, 0, InvPhi) * DodecaScale,
        new Vector3(Phi, 0, -InvPhi) * DodecaScale,
        new Vector3(-Phi, 0, InvPhi) * DodecaScale,
        new Vector3(-Phi, 0, -InvPhi) * DodecaScale
    };
    // Vertex colors (HSV to RGB for variety)
    private static readonly Vector4[] _colors = new Vector4[20];
    // Indices for drawing faces (triangulated pentagons)
    private static readonly uint[] _faceIndices;
    // Indices for drawing edges (lines)
    private static readonly uint[] _edgeIndices;
    // Position offset for rendering
    private readonly Vector3 _position = new(-3.6f, -0.5f, 0.0f);

    static Dodecahedron()
    {
        // Generate vertex colors using HSV
        for (var i = 0; i < _colors.Length; i++)
        {
            var hue = 360f * i / _colors.Length % 360f;
            _colors[i] = HsvToRgb(hue, 0.8f, 0.9f, 0.9f);
        }
        // Define faces as a set of triangles
        _faceIndices = new uint[]
        {
            0, 8, 4, 0, 4, 14, 0, 14, 12,
            0, 12, 1, 0, 1, 17, 0, 17, 16,
            0, 16, 2, 0, 2, 10, 0, 10, 8,
            1, 12, 14, 1, 14, 5, 1, 5, 9,
            1, 9, 11, 1, 11, 3, 1, 3, 17,
            2, 13, 15, 2, 15, 6, 2, 6, 10,
            2, 16, 17, 2, 17, 3, 2, 3, 13,
            3, 11, 7, 3, 7, 15, 3, 15, 13,
            4, 8, 10, 4, 10, 6, 4, 6, 18,
            4, 18, 19, 4, 19, 5, 4, 5, 14,
            5, 9, 11, 5, 11, 7, 5, 7, 19,
            6, 15, 7, 6, 7, 19, 6, 19, 18
        };
        // Generate unique edges from face indices
        var edges = new HashSet<Tuple<uint, uint>>();
        for (var i = 0; i < _faceIndices.Length; i += 3)
        {
            AddEdge(edges, _faceIndices[i], _faceIndices[i + 1]);
            AddEdge(edges, _faceIndices[i + 1], _faceIndices[i + 2]);
            AddEdge(edges, _faceIndices[i + 2], _faceIndices[i]);
        }
        _edgeIndices = edges.SelectMany(t => new[] { t.Item1, t.Item2 }).ToArray();
    }

    public unsafe Dodecahedron(GL gl, Shader faceShader, Shader edgeShader)
        : base(gl, faceShader, edgeShader)
    {
        InitFaceBuffers(_vertices, _colors, _faceIndices);
        InitEdgeBuffers(_vertices, _edgeIndices);
    }

    /// <summary>
    ///     Updates the dodecahedron's rotation and model matrix.
    /// </summary>
    public override void Update(double deltaTime)
    {
        Angle += (float)(deltaTime * 40.0f);
        ModelMatrix = Matrix4x4.CreateRotationY(Scalar.DegreesToRadians(Angle)) *
                      Matrix4x4.CreateRotationX(Scalar.DegreesToRadians(Angle * 0.7f)) *
                      Matrix4x4.CreateTranslation(_position);
    }

    /// <summary>
    ///     Renders the dodecahedron (faces and edges).
    /// </summary>
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

    // Adds an edge to the set, ensuring uniqueness (u < v)
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

    // Converts HSV color to RGBA (for vertex coloring)
    private static Vector4 HsvToRgb(float h, float s, float v, float a)
    {
        float r = 0, g = 0, b = 0;
        var i = (int)MathF.Floor(h / 60.0f) % 6;
        var f = h / 60.0f - MathF.Floor(h / 60.0f);
        var p = v * (1 - s);
        var q = v * (1 - f * s);
        var t = v * (1 - (1 - f) * s);
        switch (i)
        {
            case 0:
                r = v;
                g = t;
                b = p;
                break;
            case 1:
                r = q;
                g = v;
                b = p;
                break;
            case 2:
                r = p;
                g = v;
                b = t;
                break;
            case 3:
                r = p;
                g = q;
                b = v;
                break;
            case 4:
                r = t;
                g = p;
                b = v;
                break;
            case 5:
                r = v;
                g = p;
                b = q;
                break;
        }
        return new Vector4(r, g, b, a);
    }
}