using System;
using OpenTK;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace CS453_Term_Project
{
    class Program
    {
        static void Main(string[] args)
        {
            using( Game game = new Game(GameWindowSettings.Default, NativeWindowSettings.Default))
            {
                game.Run();
            }
        }
    }
}
