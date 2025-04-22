using System.Numerics;
using Silk.NET.OpenGL;

namespace RotatingShapes;

/// <summary>
///     Базовый абстрактный класс для фигур с OpenGL-ресурсами (VAO/VBO/EBO), чтобы избежать дублирования кода.
/// </summary>
public abstract class ShapeBase : IShape
{
    protected readonly GL Gl;
    protected readonly Shader FaceShader;
    protected readonly Shader EdgeShader;
    protected uint FaceVao, FaceVboPosition, FaceVboColor, FaceEbo;
    protected uint EdgeVao, EdgeVboPosition, EdgeEbo;
    protected float Angle;
    public Matrix4x4 ModelMatrix { get; protected set; } = Matrix4x4.Identity;

    protected ShapeBase(GL gl, Shader faceShader, Shader edgeShader)
    {
        Gl = gl;
        FaceShader = faceShader;
        EdgeShader = edgeShader;
    }

    /// <summary>
    ///     Инициализация OpenGL-ресурсов для вершин, цветов и индексов.
    /// </summary>
    protected unsafe void InitFaceBuffers(Vector3[] vertices, Vector4[] colors, uint[] indices)
    {
        FaceVao = Gl.GenVertexArray();
        Gl.BindVertexArray(FaceVao);
        FaceVboPosition = Gl.GenBuffer();
        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, FaceVboPosition);
        fixed (Vector3* ptr = vertices)
            Gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(vertices.Length * sizeof(Vector3)), ptr, BufferUsageARB.StaticDraw);
        Gl.VertexAttribPointer(0, 3, GLEnum.Float, false, (uint)sizeof(Vector3), (void*)0);
        Gl.EnableVertexAttribArray(0);
        FaceVboColor = Gl.GenBuffer();
        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, FaceVboColor);
        fixed (Vector4* ptr = colors)
            Gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(colors.Length * sizeof(Vector4)), ptr, BufferUsageARB.StaticDraw);
        Gl.VertexAttribPointer(1, 4, GLEnum.Float, false, (uint)sizeof(Vector4), (void*)0);
        Gl.EnableVertexAttribArray(1);
        FaceEbo = Gl.GenBuffer();
        Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, FaceEbo);
        fixed (uint* ptr = indices)
            Gl.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)(indices.Length * sizeof(uint)), ptr, BufferUsageARB.StaticDraw);
    }

    protected unsafe void InitEdgeBuffers(Vector3[] vertices, uint[] indices)
    {
        EdgeVao = Gl.GenVertexArray();
        Gl.BindVertexArray(EdgeVao);
        EdgeVboPosition = Gl.GenBuffer();
        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, EdgeVboPosition);
        fixed (Vector3* ptr = vertices)
            Gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(vertices.Length * sizeof(Vector3)), ptr, BufferUsageARB.StaticDraw);
        Gl.VertexAttribPointer(0, 3, GLEnum.Float, false, (uint)sizeof(Vector3), (void*)0);
        Gl.EnableVertexAttribArray(0);
        EdgeEbo = Gl.GenBuffer();
        Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, EdgeEbo);
        fixed (uint* ptr = indices)
            Gl.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)(indices.Length * sizeof(uint)), ptr, BufferUsageARB.StaticDraw);
        Gl.BindVertexArray(0);
        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
    }

    public abstract void Update(double deltaTime);
    public abstract void Render(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix);
    public virtual void Dispose()
    {
        Gl.DeleteBuffer(FaceVboPosition);
        Gl.DeleteBuffer(FaceVboColor);
        Gl.DeleteBuffer(FaceEbo);
        Gl.DeleteVertexArray(FaceVao);
        Gl.DeleteBuffer(EdgeVboPosition);
        Gl.DeleteBuffer(EdgeEbo);
        Gl.DeleteVertexArray(EdgeVao);
    }
}
