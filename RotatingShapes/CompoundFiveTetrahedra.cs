using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace RotatingShapes;

/// <summary>
///     Реализация соединения пяти тетраэдров (Five Tetrahedra Compound).
/// </summary>
public class CompoundFiveTetrahedra : ShapeBase
{
    private const float Scale = 0.9f;
    private static readonly Vector3[] BaseTetra =
    {
        Vector3.Normalize(new Vector3(1, 1, 1)),
        Vector3.Normalize(new Vector3(1, -1, -1)),
        Vector3.Normalize(new Vector3(-1, 1, -1)),
        Vector3.Normalize(new Vector3(-1, -1, 1))
    };
    private static readonly uint[] FaceIndices = { 0, 1, 2, 0, 3, 1, 0, 2, 3, 1, 3, 2 };
    private static readonly uint[] EdgeIndices = { 0, 1, 0, 2, 0, 3, 1, 2, 1, 3, 2, 3 };

    // Helper methods for static initialization
    private static (Vector3[] vertices, Vector4[] colors, uint[] faces, uint[] edges) CreateUniqueVerticesAndColors()
    {
        var rotations = GetFiveTetrahedraRotations();
        var uniqueVerts = new List<Vector3>();
        var uniqueColors = new List<Vector4>();
        var vertMap = new Dictionary<Vector3, int>(new Vector3EqualityComparer());
        var faces = new List<uint>();
        var edges = new List<uint>();
        // Assign colors based on position for smooth gradient
        Func<Vector3, Vector4> colorFunc = v =>
        {
            // Use spherical coordinates for color mapping
            var n = Vector3.Normalize(v);
            float hue = (float)((Math.Atan2(n.Y, n.X) + Math.PI) / (2 * Math.PI));
            float sat = 0.8f;
            float val = 0.9f;
            return HsvToRgb(hue * 360f, sat, val, 0.9f);
        };
        for (int t = 0; t < 5; t++)
        {
            var rot = rotations[t];
            for (int i = 0; i < 4; i++)
            {
                var v = Vector3.Transform(BaseTetra[i] * Scale, rot);
                if (!vertMap.TryGetValue(v, out int idx))
                {
                    idx = uniqueVerts.Count;
                    uniqueVerts.Add(v);
                    uniqueColors.Add(colorFunc(v));
                    vertMap[v] = idx;
                }
            }
        }
        // Now build faces and edges using unique vertex indices
        for (int t = 0; t < 5; t++)
        {
            var rot = rotations[t];
            var indices = new int[4];
            for (int i = 0; i < 4; i++)
            {
                var v = Vector3.Transform(BaseTetra[i] * Scale, rot);
                indices[i] = vertMap[v];
            }
            for (int i = 0; i < FaceIndices.Length; i++)
                faces.Add((uint)indices[FaceIndices[i]]);
            for (int i = 0; i < EdgeIndices.Length; i++)
                edges.Add((uint)indices[EdgeIndices[i]]);
        }
        return (uniqueVerts.ToArray(), uniqueColors.ToArray(), faces.ToArray(), edges.ToArray());
    }

    // Custom comparer for Vector3 to allow dictionary keying
    private sealed class Vector3EqualityComparer : IEqualityComparer<Vector3>
    {
        public bool Equals(Vector3 a, Vector3 b) => Vector3.DistanceSquared(a, b) < 1e-6f;
        public int GetHashCode(Vector3 v) => v.GetHashCode();
    }

    private static readonly Vector3[] Vertices;
    private static readonly Vector4[] VertexColors;
    private static readonly uint[] Faces;
    private static readonly uint[] Edges;

    static CompoundFiveTetrahedra()
    {
        (Vertices, VertexColors, Faces, Edges) = CreateUniqueVerticesAndColors();
    }

    private readonly Vector3 _position = new(0.0f, 2.8f, 0.0f);

    public unsafe CompoundFiveTetrahedra(GL gl, Shader faceShader, Shader edgeShader)
        : base(gl, faceShader, edgeShader)
    {
        InitFaceBuffers(Vertices, VertexColors, Faces);
        InitEdgeBuffers(Vertices, Edges);
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
        Gl.DrawElements(PrimitiveType.Triangles, (uint)Faces.Length, DrawElementsType.UnsignedInt, (void*)0);

        EdgeShader.Use();
        EdgeShader.SetUniform("model", ModelMatrix);
        EdgeShader.SetUniform("view", viewMatrix);
        EdgeShader.SetUniform("projection", projectionMatrix);
        EdgeShader.SetUniform("edgeColor", new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
        Gl.BindVertexArray(EdgeVao);
        Gl.DrawElements(PrimitiveType.Lines, (uint)Edges.Length, DrawElementsType.UnsignedInt, (void*)0);
        Gl.BindVertexArray(0);
    }

    // Returns five quaternion rotations for the five tetrahedra
    private static List<Quaternion> GetFiveTetrahedraRotations()
    {
        var list = new List<Quaternion>();
        list.Add(Quaternion.Identity);
        // The following are approximate, but sufficient for visual compound
        list.Add(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 2 * MathF.PI / 5));
        list.Add(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 4 * MathF.PI / 5));
        list.Add(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 6 * MathF.PI / 5));
        list.Add(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 8 * MathF.PI / 5));
        return list;
    }

    // HSV -> RGBA (как у других фигур)
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
            case 0: r = v; g = t; b = p; break;
            case 1: r = q; g = v; b = p; break;
            case 2: r = p; g = v; b = t; break;
            case 3: r = p; g = q; b = v; break;
            case 4: r = t; g = p; b = v; break;
            case 5: r = v; g = p; b = q; break;
        }
        return new Vector4(r, g, b, a);
    }
}
