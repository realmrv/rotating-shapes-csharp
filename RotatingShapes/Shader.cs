using System.Numerics;
using Silk.NET.OpenGL;

namespace RotatingShapes;

public class Shader : IDisposable
{
    private readonly GL _gl;

    public Shader(GL gl, string vertexSource, string fragmentSource)
    {
        _gl = gl;

        var vertexShader = CompileShader(ShaderType.VertexShader, vertexSource);
        var fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentSource);

        Handle = _gl.CreateProgram();
        _gl.AttachShader(Handle, vertexShader);
        _gl.AttachShader(Handle, fragmentShader);
        _gl.LinkProgram(Handle);

        _gl.GetProgram(Handle, ProgramPropertyARB.LinkStatus, out var status);
        if (status == 0)
        {
            var infoLog = _gl.GetProgramInfoLog(Handle);
            throw new Exception($"Error linking shader program: {infoLog}");
        }

        // Shaders are linked into the program; no longer needed individually
        _gl.DetachShader(Handle, vertexShader);
        _gl.DetachShader(Handle, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);
    }

    public uint Handle { get; }

    public void Dispose()
    {
        _gl.DeleteProgram(Handle);
    }

    public static Shader LoadFromFile(GL gl, string vertexPath, string fragmentPath)
    {
        var vertexSource = File.ReadAllText(vertexPath);
        var fragmentSource = File.ReadAllText(fragmentPath);
        return new Shader(gl, vertexSource, fragmentSource);
    }

    private uint CompileShader(ShaderType type, string source)
    {
        var shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);

        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out var status);
        if (status == 0)
        {
            var infoLog = _gl.GetShaderInfoLog(shader);
            throw new Exception($"Error compiling {type}: {infoLog}");
        }

        return shader;
    }

    public void Use()
    {
        _gl.UseProgram(Handle);
    }

    public int GetUniformLocation(string name)
    {
        var location = _gl.GetUniformLocation(Handle, name);
        if (location == -1) // -1 means the uniform was not found
            Console.WriteLine($"Warning: Uniform '{name}' not found in shader program {Handle}.");
        return location;
    }

    // Convenience methods for setting uniforms (add more types as needed)
    public void SetUniform(string name, Matrix4x4 value)
    {
        var location = GetUniformLocation(name);
        if (location != -1)
            unsafe
            {
                // OpenGL expects matrices in column-major order, which System.Numerics.Matrix4x4 is already in.
                _gl.UniformMatrix4(location, 1, false, (float*)&value);
            }
    }

    public void SetUniform(string name, Vector4 value)
    {
        var location = GetUniformLocation(name);
        if (location != -1) _gl.Uniform4(location, value.X, value.Y, value.Z, value.W);
    }
}