using System;

namespace RotatingShapes
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create and run the renderer
            using (var renderer = new Renderer())
            {
                renderer.Run();
            }
            Console.WriteLine("Application closed.");
        }
    }
}
