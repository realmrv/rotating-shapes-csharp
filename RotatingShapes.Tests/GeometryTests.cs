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

    [Fact]
    public void Dodecahedron_HsvToRgb_KnownValues()
    {
        var type = typeof(Dodecahedron);
        var method = type.GetMethod("HsvToRgb", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        // Red
        var red = (Vector4)method!.Invoke(null, new object[] { 0f, 1f, 1f, 1f })!;
        Assert.True(Math.Abs(red.X - 1f) < 0.01 && Math.Abs(red.Y) < 0.01 && Math.Abs(red.Z) < 0.01);
        // Green
        var green = (Vector4)method!.Invoke(null, new object[] { 120f, 1f, 1f, 1f })!;
        Assert.True(Math.Abs(green.X) < 0.01 && Math.Abs(green.Y - 1f) < 0.01 && Math.Abs(green.Z) < 0.01);
        // Blue
        var blue = (Vector4)method!.Invoke(null, new object[] { 240f, 1f, 1f, 1f })!;
        Assert.True(Math.Abs(blue.X) < 0.01 && Math.Abs(blue.Y) < 0.01 && Math.Abs(blue.Z - 1f) < 0.01);
    }

    [Fact]
    public void Dodecahedron_AddEdge_AddsUniqueEdges()
    {
        var type = typeof(Dodecahedron);
        var method = type.GetMethod("AddEdge", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var edges = new HashSet<Tuple<uint, uint>>();
        method!.Invoke(null, new object[] { edges, 2u, 5u });
        method!.Invoke(null, new object[] { edges, 5u, 2u }); // Should not add duplicate
        Assert.Single(edges);
        Assert.Contains(Tuple.Create(2u, 5u), edges);
    }

    // [Fact]
    // public void Shape_Update_ChangesModelMatrix()
    // {
    //     // Этот тест требует рефакторинга для мокирования OpenGL зависимостей
    // }
}
