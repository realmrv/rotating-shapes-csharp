using System;
using System.Numerics;
using Xunit;
using RotatingShapes;

namespace RotatingShapes.Tests;

public class GeometryTests
{
    [Fact]
    public void Cube_VertexCount_Is8()
    {
        // Arrange
        var type = typeof(Cube);
        var field = type.GetField("_vertices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var vertices = (Vector3[])field!.GetValue(null)!;
        // Assert
        Assert.Equal(8, vertices.Length);
    }

    [Fact]
    public void Tetrahedron_VertexCount_Is4()
    {
        var type = typeof(Tetrahedron);
        var field = type.GetField("_vertices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var vertices = (Vector3[])field!.GetValue(null)!;
        Assert.Equal(4, vertices.Length);
    }

    [Fact]
    public void Octahedron_VertexCount_Is6()
    {
        var type = typeof(Octahedron);
        var field = type.GetField("_vertices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var vertices = (Vector3[])field!.GetValue(null)!;
        Assert.Equal(6, vertices.Length);
    }

    [Fact]
    public void Icosahedron_VertexCount_Is12()
    {
        var type = typeof(Icosahedron);
        var field = type.GetField("_vertices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var vertices = (Vector3[])field!.GetValue(null)!;
        Assert.Equal(12, vertices.Length);
    }

    [Fact]
    public void Dodecahedron_VertexCount_Is20()
    {
        var type = typeof(Dodecahedron);
        var field = type.GetField("_vertices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var vertices = (Vector3[])field!.GetValue(null)!;
        Assert.Equal(20, vertices.Length);
    }

    [Fact]
    public void Cube_FaceIndices_AreValid()
    {
        var type = typeof(Cube);
        var fieldVertices = type.GetField("_vertices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var fieldIndices = type.GetField("_faceIndices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var vertices = (Vector3[])fieldVertices!.GetValue(null)!;
        var indices = (uint[])fieldIndices!.GetValue(null)!;
        foreach (var idx in indices)
            Assert.InRange<uint>(idx, 0, (uint)(vertices.Length - 1));
    }

    [Fact]
    public void Tetrahedron_FaceIndices_AreValid()
    {
        var type = typeof(Tetrahedron);
        var fieldVertices = type.GetField("_vertices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var fieldIndices = type.GetField("_faceIndices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var vertices = (Vector3[])fieldVertices!.GetValue(null)!;
        var indices = (uint[])fieldIndices!.GetValue(null)!;
        foreach (var idx in indices)
            Assert.InRange<uint>(idx, 0, (uint)(vertices.Length - 1));
    }

    [Fact]
    public void Octahedron_FaceIndices_AreValid()
    {
        var type = typeof(Octahedron);
        var fieldVertices = type.GetField("_vertices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var fieldIndices = type.GetField("_faceIndices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var vertices = (Vector3[])fieldVertices!.GetValue(null)!;
        var indices = (uint[])fieldIndices!.GetValue(null)!;
        foreach (var idx in indices)
            Assert.InRange<uint>(idx, 0, (uint)(vertices.Length - 1));
    }

    [Fact]
    public void Icosahedron_FaceIndices_AreValid()
    {
        var type = typeof(Icosahedron);
        var fieldVertices = type.GetField("_vertices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var fieldIndices = type.GetField("_faceIndices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var vertices = (Vector3[])fieldVertices!.GetValue(null)!;
        var indices = (uint[])fieldIndices!.GetValue(null)!;
        foreach (var idx in indices)
            Assert.InRange<uint>(idx, 0, (uint)(vertices.Length - 1));
    }

    [Fact]
    public void Dodecahedron_FaceIndices_AreValid()
    {
        var type = typeof(Dodecahedron);
        var fieldVertices = type.GetField("_vertices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var fieldIndices = type.GetField("_faceIndices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var vertices = (Vector3[])fieldVertices!.GetValue(null)!;
        var indices = (uint[])fieldIndices!.GetValue(null)!;
        foreach (var idx in indices)
            Assert.InRange<uint>(idx, 0, (uint)(vertices.Length - 1));
    }
}
