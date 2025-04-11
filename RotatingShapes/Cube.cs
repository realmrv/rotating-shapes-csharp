using Silk.NET.OpenGL;
using System.Numerics;
using System;
using Silk.NET.Maths; // Required for Scalar.DegreesToRadians

namespace RotatingShapes
{
    public class Cube : IDisposable
    {
        private GL _gl;

        // Geometry Data
        private const float CubeScale = 0.6f; // Adjusted scale
        private static readonly Vector3[] _vertices =
        {
            // Front face
            new Vector3(-0.5f, -0.5f,  0.5f) * CubeScale, new Vector3( 0.5f, -0.5f,  0.5f) * CubeScale,
            new Vector3( 0.5f,  0.5f,  0.5f) * CubeScale, new Vector3(-0.5f,  0.5f,  0.5f) * CubeScale,
            // Back face
            new Vector3(-0.5f, -0.5f, -0.5f) * CubeScale, new Vector3( 0.5f, -0.5f, -0.5f) * CubeScale,
            new Vector3( 0.5f,  0.5f, -0.5f) * CubeScale, new Vector3(-0.5f,  0.5f, -0.5f) * CubeScale
        };
        private static readonly Vector4[] _colors =
        {
            new Vector4(1.0f, 0.0f, 0.0f, 0.7f), new Vector4(1.0f, 1.0f, 0.0f, 0.7f), new Vector4(1.0f, 1.0f, 0.0f, 0.7f), new Vector4(1.0f, 0.0f, 0.0f, 0.7f),
            new Vector4(0.0f, 0.0f, 1.0f, 0.7f), new Vector4(0.0f, 1.0f, 1.0f, 0.7f), new Vector4(0.0f, 1.0f, 1.0f, 0.7f), new Vector4(0.0f, 0.0f, 1.0f, 0.7f)
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

        // OpenGL Handles
        private uint _faceVao;
        private uint _faceVboPosition;
        private uint _faceVboColor;
        private uint _faceEbo;
        private uint _edgeVao;
        private uint _edgeVboPosition;
        private uint _edgeEbo;

        // Shaders (Passed in constructor or loaded here)
        private Shader _faceShaderProgram;
        private Shader _edgeShaderProgram;

        // Transformations
        public Matrix4x4 ModelMatrix { get; set; } = Matrix4x4.Identity;
        private float _angle = 0.0f;
        // Position this one mid-right
        private Vector3 _position = new Vector3(1.8f, 0.0f, 0.0f);

        public unsafe Cube(GL gl, Shader faceShader, Shader edgeShader)
        {
            _gl = gl;
            _faceShaderProgram = faceShader;
            _edgeShaderProgram = edgeShader;

            // Create Face Resources
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

            // Create Edge Resources
            _edgeVao = _gl.GenVertexArray();
            _gl.BindVertexArray(_edgeVao);
            _edgeVboPosition = _gl.GenBuffer(); // Reuse position data buffer handle is okay, but let's create a new one for clarity
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _edgeVboPosition);
            fixed (Vector3* ptr = _vertices) { _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(_vertices.Length * sizeof(Vector3)), ptr, BufferUsageARB.StaticDraw); } // Upload same vertex data
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
            _edgeShaderProgram.SetUniform("edgeColor", new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
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

            // Shaders are typically managed by the Renderer or whoever created them,
            // so we don't dispose them here unless Cube is responsible for loading them.
        }
    }
}
