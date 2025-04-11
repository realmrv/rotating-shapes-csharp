using System;
using System.Numerics;

namespace RotatingShapes
{
    /// <summary>
    /// Interface for a renderable and updatable shape.
    /// </summary>
    public interface IShape : IDisposable
    {
        /// <summary>
        /// The current Model transformation matrix for the shape.
        /// </summary>
        Matrix4x4 ModelMatrix { get; }

        /// <summary>
        /// Updates the shape's state (e.g., rotation).
        /// </summary>
        /// <param name="deltaTime">Time elapsed since the last frame.</param>
        void Update(double deltaTime);

        /// <summary>
        /// Renders the shape.
        /// </summary>
        /// <param name="viewMatrix">The camera view matrix.</param>
        /// <param name="projectionMatrix">The camera projection matrix.</param>
        void Render(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix);

        // Dispose method is inherited from IDisposable
    }
}
