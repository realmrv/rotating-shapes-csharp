using Silk.NET.OpenGL;
using System.Numerics;
using System;
using Silk.NET.Maths;

namespace RotatingShapes
{
    public class Tetrahedron : IShape, IDisposable
    {
        private GL _gl;

        // Geometry Data (Regular Tetrahedron centered at origin, scaled)
        private const float TetraScale = 0.7f; // Adjusted scale
        private static readonly Vector3 v0 = Vector3.Normalize(new Vector3(1, 1, 1)) * TetraScale;
        private static readonly Vector3 v1 = Vector3.Normalize(new Vector3(1, -1, -1)) * TetraScale;
        private static readonly Vector3 v2 = Vector3.Normalize(new Vector3(-1, 1, -1)) * TetraScale;
        private static readonly Vector3 v3 = Vector3.Normalize(new Vector3(-1, -1, 1)) * TetraScale;

        private static readonly Vector3[] _vertices =
        {
            v0, v1, v2, v3
            // Old vertices:
            // new Vector3( 0.0f,  0.5f,  0.0f),  // Top vertex (0)
            // new Vector3(-0.5f, -0.5f,  0.5f),  // Base front-left (1)
            // new Vector3( 0.5f, -0.5f,  0.5f),  // Base front-right (2)
            // new Vector3( 0.0f, -0.5f, -0.5f)   // Base back (3)
        };

        // Colors remain the same (Green/Magenta gradient)
        private static readonly Vector4[] _colors =
        {
            new Vector4(0.0f, 1.0f, 0.0f, 0.8f), // v0
            new Vector4(1.0f, 0.0f, 1.0f, 0.8f), // v1
            new Vector4(0.0f, 1.0f, 0.0f, 0.8f), // v2
            new Vector4(1.0f, 0.0f, 1.0f, 0.8f)  // v3
        };

        // Indices for drawing the faces (4 triangles)
        private static readonly uint[] _faceIndices =
        {
            0, 1, 2, // Face 1
            0, 3, 1, // Face 2
            0, 2, 3, // Face 3
            1, 3, 2  // Face 4
            // Old indices:
            // 0, 1, 2,  // Front face
            // 0, 2, 3,  // Right face
            // 0, 3, 1,  // Left face
            // 1, 3, 2   // Bottom face
        };

        // Indices for drawing the edges (6 lines)
        private static readonly uint[] _edgeIndices =
        {
            0, 1, 0, 2, 0, 3, // Edges from v0
            1, 2, 1, 3,       // Edges from v1
            2, 3              // Edge from v2
            // Old indices:
            // 0, 1, 0, 2, 0, 3, // Top edges
            // 1, 2, 2, 3, 3, 1  // Base edges
        };

        // OpenGL Handles
        private uint _faceVao;
        private uint _faceVboPosition;
        private uint _faceVboColor;
        private uint _faceEbo;
        private uint _edgeVao;
        private uint _edgeVboPosition;
        private uint _edgeEbo;

        // Shaders (Reused from Cube/Renderer)
        private Shader _faceShaderProgram;
        private Shader _edgeShaderProgram;

        // Transformations
        public Matrix4x4 ModelMatrix { get; private set; } = Matrix4x4.Identity;
        private float _angle = 0.0f;
        private Vector3 _position = new Vector3(3.6f, 0.0f, 0.0f);

        public unsafe Tetrahedron(GL gl, Shader faceShader, Shader edgeShader)
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
            // Different rotation speed and axis
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
            _edgeShaderProgram.SetUniform("edgeColor", new Vector4(0.8f, 0.8f, 0.8f, 1.0f)); // Light gray edges
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
