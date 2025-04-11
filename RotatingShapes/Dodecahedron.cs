using Silk.NET.OpenGL;
using System.Numerics;
using System;
using Silk.NET.Maths;
using System.Collections.Generic;
using System.Linq;

namespace RotatingShapes
{
    public class Dodecahedron : IDisposable
    {
        private GL _gl;

        // Geometry Data (Regular Dodecahedron centered at origin)
        private const float DodecaScale = 0.5f; // Adjusted scale
        private static readonly float Phi = (1.0f + MathF.Sqrt(5.0f)) / 2.0f; // Golden ratio
        private static readonly float InvPhi = 1.0f / Phi; // 1 / Phi = Phi - 1

        private static readonly Vector3[] _vertices = // 20 Vertices
        {
            // Cube corners
            new Vector3( 1,  1,  1) * DodecaScale,
            new Vector3( 1,  1, -1) * DodecaScale,
            new Vector3( 1, -1,  1) * DodecaScale,
            new Vector3( 1, -1, -1) * DodecaScale,
            new Vector3(-1,  1,  1) * DodecaScale,
            new Vector3(-1,  1, -1) * DodecaScale,
            new Vector3(-1, -1,  1) * DodecaScale,
            new Vector3(-1, -1, -1) * DodecaScale,
            // Golden ratio points on axes planes
            new Vector3( 0,  InvPhi,  Phi) * DodecaScale,
            new Vector3( 0,  InvPhi, -Phi) * DodecaScale,
            new Vector3( 0, -InvPhi,  Phi) * DodecaScale,
            new Vector3( 0, -InvPhi, -Phi) * DodecaScale,
            new Vector3( InvPhi,  Phi,  0) * DodecaScale,
            new Vector3( InvPhi, -Phi,  0) * DodecaScale,
            new Vector3(-InvPhi,  Phi,  0) * DodecaScale,
            new Vector3(-InvPhi, -Phi,  0) * DodecaScale,
            new Vector3( Phi,  0,  InvPhi) * DodecaScale,
            new Vector3( Phi,  0, -InvPhi) * DodecaScale,
            new Vector3(-Phi,  0,  InvPhi) * DodecaScale,
            new Vector3(-Phi,  0, -InvPhi) * DodecaScale
        };

        // Distinct colors for vertices
        private static readonly Vector4[] _colors = new Vector4[20];

        // Indices for drawing the faces (12 pentagons -> 36 triangles)
        private static readonly uint[] _faceIndices;

        // Indices for drawing the edges (30 lines)
        private static readonly uint[] _edgeIndices;

        // Static constructor to initialize geometry
        static Dodecahedron()
        {
            // Initialize Colors (Generate programmatically)
            for (int i = 0; i < _colors.Length; i++)
            {
                float hue = (360f * i / _colors.Length) % 360f;
                _colors[i] = HsvToRgb(hue, 0.8f, 0.9f, 0.9f);
            }

            // Define the 12 pentagonal faces using vertex indices
            _faceIndices = new uint[] {
                  0,  8,  4,    0,  4, 14,    0, 14, 12, // Face 1
                  0, 12,  1,    0,  1, 17,    0, 17, 16, // Face 2
                  0, 16,  2,    0,  2, 10,    0, 10,  8, // Face 3
                  1, 12, 14,    1, 14,  5,    1,  5,  9, // Face 4
                  1,  9, 11,    1, 11,  3,    1,  3, 17, // Face 5
                  2, 13, 15,    2, 15,  6,    2,  6, 10, // Face 6
                  2, 16, 17,    2, 17,  3,    2,  3, 13, // Face 7
                  3, 11,  7,    3,  7, 15,    3, 15, 13, // Face 8
                  4,  8, 10,    4, 10,  6,    4,  6, 18, // Face 9
                  4, 18, 19,    4, 19,  5,    4,  5, 14, // Face 10
                  5,  9, 11,    5, 11,  7,    5,  7, 19, // Face 11
                  6, 15,  7,    6,  7, 19,    6, 19, 18  // Face 12
            };

            // Generate edge list from the pre-triangulated faces
            var edges = new HashSet<Tuple<uint, uint>>();
            for (int i = 0; i < _faceIndices.Length; i += 3)
            {
                // Add edges from the triangle definition
                AddEdge(edges, _faceIndices[i], _faceIndices[i + 1]);
                AddEdge(edges, _faceIndices[i + 1], _faceIndices[i + 2]);
                AddEdge(edges, _faceIndices[i + 2], _faceIndices[i]);
            }
            _edgeIndices = edges.SelectMany(t => new[] { t.Item1, t.Item2 }).ToArray();
        }

        private static void AddEdge(HashSet<Tuple<uint, uint>> edges, uint u, uint v)
        {
            if (u > v) { var temp = u; u = v; v = temp; }
            edges.Add(Tuple.Create(u, v));
        }

        // Simple HSV to RGB conversion (for color variety)
        private static Vector4 HsvToRgb(float h, float s, float v, float a)
        {
            float r = 0, g = 0, b = 0;
            int i = (int)MathF.Floor(h / 60.0f) % 6;
            float f = h / 60.0f - MathF.Floor(h / 60.0f);
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);
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
        // Position this one far-left
        private Vector3 _position = new Vector3(-3.6f, 0.0f, 0.0f);

        public unsafe Dodecahedron(GL gl, Shader faceShader, Shader edgeShader)
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
            _edgeShaderProgram.SetUniform("edgeColor", new Vector4(1.0f, 1.0f, 1.0f, 1.0f)); // Changed to White edges
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
