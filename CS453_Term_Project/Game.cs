using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics;

namespace CS453_Term_Project
{
    class Game : GameWindow
    {
        public Game(GameWindowSettings gws, NativeWindowSettings nws) 
            : base(gws, nws) 
        {
        }
    }
}
