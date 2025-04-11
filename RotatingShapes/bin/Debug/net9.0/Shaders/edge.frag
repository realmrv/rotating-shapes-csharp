#version 330 core
out vec4 FragColor;

uniform vec4 edgeColor; // Uniform for edge color

void main()
{
    FragColor = edgeColor; // Output the solid edge color
} 
