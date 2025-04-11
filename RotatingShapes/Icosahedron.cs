using Silk.NET.OpenGL;
using System.Numerics;
using System;
using Silk.NET.Maths;

namespace RotatingShapes
{
    public class Icosahedron : IShape
    {
        private GL _gl;

        // Geometry Data (Regular Icosahedron centered at origin)
        private const float IcoScale = 0.6f; // Adjusted scale
        private static readonly float Phi = (1.0f + MathF.Sqrt(5.0f)) / 2.0f; // Golden ratio

        private static readonly Vector3[] _vertices =
        {
            new Vector3(-1,  Phi,  0) * IcoScale, new Vector3( 1,  Phi,  0) * IcoScale, new Vector3(-1, -Phi,  0) * IcoScale, new Vector3( 1, -Phi,  0) * IcoScale,
            new Vector3( 0, -1,  Phi) * IcoScale, new Vector3( 0,  1,  Phi) * IcoScale, new Vector3( 0, -1, -Phi) * IcoScale, new Vector3( 0,  1, -Phi) * IcoScale,
            new Vector3( Phi,  0, -1) * IcoScale, new Vector3( Phi,  0,  1) * IcoScale, new Vector3(-Phi,  0, -1) * IcoScale, new Vector3(-Phi,  0,  1) * IcoScale
        };

        // Different colors (e.g., White/Gray gradient, semi-transparent)
        private static readonly Vector4[] _colors;

        // Indices for drawing the faces (20 triangles)
        private static readonly uint[] _faceIndices =
        {
            0, 11, 5,  0, 5, 1,   0, 1, 7,   0, 7, 10,  0, 10, 11, // 5 faces around point 0
            1, 5, 9,   5, 11, 4,  11, 10, 2,  10, 7, 6,  7, 1, 8,  // 5 adjacent faces
            3, 9, 4,   3, 4, 2,   3, 2, 6,   3, 6, 8,   3, 8, 9,  // 5 faces around point 3
            4, 9, 5,   2, 4, 11,  6, 2, 10,  8, 6, 7,   9, 8, 1   // 5 adjacent faces
        };

        // Indices for drawing the edges (30 lines)
        private static readonly uint[] _edgeIndices;

        // Combined static constructor
        static Icosahedron()
        {
            // 1. Normalize vertices
            for (int i = 0; i < _vertices.Length; i++)
            {
                _vertices[i] = Vector3.Normalize(_vertices[i]) * IcoScale;
            }

            // 2. Assign distinct colors to each vertex
            _colors = new Vector4[]
            {
                new Vector4(1.0f, 0.0f, 0.0f, 0.85f), // Red
                new Vector4(0.0f, 1.0f, 0.0f, 0.85f), // Green
                new Vector4(0.0f, 0.0f, 1.0f, 0.85f), // Blue
                new Vector4(1.0f, 1.0f, 0.0f, 0.85f), // Yellow
                new Vector4(1.0f, 0.0f, 1.0f, 0.85f), // Magenta
                new Vector4(0.0f, 1.0f, 1.0f, 0.85f), // Cyan
                new Vector4(1.0f, 0.5f, 0.0f, 0.85f), // Orange
                new Vector4(0.5f, 0.0f, 1.0f, 0.85f), // Purple
                new Vector4(0.0f, 0.5f, 1.0f, 0.85f), // Sky Blue
                new Vector4(0.5f, 1.0f, 0.0f, 0.85f), // Lime Green
                new Vector4(1.0f, 0.0f, 0.5f, 0.85f), // Pink
                new Vector4(0.0f, 1.0f, 0.5f, 0.85f)  // Teal
            };

            // 3. Generate edges from faces (avoid duplicates)
            var edges = new System.Collections.Generic.HashSet<Tuple<uint, uint>>();
            for (int i = 0; i < _faceIndices.Length; i += 3)
            {
                uint i1 = _faceIndices[i];
                uint i2 = _faceIndices[i + 1];
                uint i3 = _faceIndices[i + 2];
                AddEdge(edges, i1, i2);
                AddEdge(edges, i2, i3);
                AddEdge(edges, i3, i1);
            }
            _edgeIndices = edges.SelectMany(t => new[] { t.Item1, t.Item2 }).ToArray();
        }

        private static void AddEdge(System.Collections.Generic.HashSet<Tuple<uint, uint>> edges, uint u, uint v)
        {
            // Store edge with smaller index first to ensure uniqueness
            if (u > v) { var temp = u; u = v; v = temp; }
            edges.Add(Tuple.Create(u, v));
        }

        // OpenGL Handles
        private uint _faceVao;
        private uint _faceVboPosition;
        private uint _faceVboColor;
        private uint _faceEbo;
        private uint _edgeVao;
        private uint _edgeVboPosition;
        private uint _edgeEbo;

        // Shaders
        private Shader _faceShaderProgram;
        private Shader _edgeShaderProgram;

        // Transformations
        public Matrix4x4 ModelMatrix { get; private set; } = Matrix4x4.Identity;
        private float _angle = 0.0f;
        // Position this one mid-left
        private Vector3 _position = new Vector3(-1.8f, 0.0f, 0.0f);

        public unsafe Icosahedron(GL gl, Shader faceShader, Shader edgeShader)
        {
            _gl = gl;
            _faceShaderProgram = faceShader;
            _edgeShaderProgram = edgeShader;

            // --- Create Face Resources ---
            _faceVao = _gl.GenVertexArray();
            _gl.BindVertexArray(_faceVao);
            _faceVboPosition = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _faceVboPosition);
            fixed (Vector3* ptr = _vertices) { _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(_vertices.Length * sizeof(Vector3)), ptr, BufferUsageARB.StaticDraw); }
            _gl.VertexAttribPointer(0, 3, GLEnum.Float, false, (uint)sizeof(Vector3), (void*)0);
            _gl.EnableVertexAttribArray(0);
            _faceVboColor = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _faceVboColor);
            fixed (Vector4* ptr = _colors) { _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(_colors.Length * sizeof(Vector4)), ptr, BufferUsageARB.StaticDraw); }
            _gl.VertexAttribPointer(1, 4, GLEnum.Float, false, (uint)sizeof(Vector4), (void*)0);
            _gl.EnableVertexAttribArray(1);
            _faceEbo = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _faceEbo);
            fixed (uint* ptr = _faceIndices) { _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)(_faceIndices.Length * sizeof(uint)), ptr, BufferUsageARB.StaticDraw); }

            // --- Create Edge Resources ---
            _edgeVao = _gl.GenVertexArray();
            _gl.BindVertexArray(_edgeVao);
            _edgeVboPosition = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _edgeVboPosition);
            fixed (Vector3* ptr = _vertices) { _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(_vertices.Length * sizeof(Vector3)), ptr, BufferUsageARB.StaticDraw); }
            _gl.VertexAttribPointer(0, 3, GLEnum.Float, false, (uint)sizeof(Vector3), (void*)0);
            _gl.EnableVertexAttribArray(0);
            _edgeEbo = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _edgeEbo);
            fixed (uint* ptr = _edgeIndices) { _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)(_edgeIndices.Length * sizeof(uint)), ptr, BufferUsageARB.StaticDraw); }

            // Unbind all
            _gl.BindVertexArray(0);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        }

        public void Update(double deltaTime)
        {
            // Different rotation
            _angle += (float)(deltaTime * 40.0f); // Standard speed
            // Standard rotation
            ModelMatrix = Matrix4x4.CreateRotationY(Scalar.DegreesToRadians(_angle)) *
                          Matrix4x4.CreateRotationX(Scalar.DegreesToRadians(_angle * 0.7f)) *
                          Matrix4x4.CreateTranslation(_position);
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
            _edgeShaderProgram.SetUniform("edgeColor", new Vector4(1.0f, 1.0f, 1.0f, 1.0f)); // Changed edge color to white
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
}
