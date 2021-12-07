using System;
using OpenTK;
using OpenTK.Windowing.Desktop;

namespace CS453_Term_Project
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Game game = new Game(GameWindowSettings.Default, NativeWindowSettings.Default))
            {
                game.Run();

                float[] Vert =
                {
                    0.0f, 0.5f, 0.0f,
                    0.5f, 0.0f, 0.0f,
                    0.0f, -0.5f, 0.0f,
                    -0.5f, 0.0f, 0.0f
                };

                uint[] Idx =
                {
                    0, 1, 2,
                    0, 2, 3
                };

                game.UpdateGeometry(Vert, Idx);
            }


        }
    }
}
