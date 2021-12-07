using System;
using OpenTK;
using OpenTK.Windowing.Desktop;

namespace CS453_Term_Project
{
    class Program
    {
        struct Vertex
        {
                public float x;
                public float y;
                public float z;
                public bool empty;
        };

        bool streamline_step(Vertex cpos, ref Vertex npos, bool forward, bool end)
        {
            //assigning that this vertex is not empty
            npos.empty = false;

            //setting up the vector components according to the field equation x = yz, y = xz, z = xy
            float vectx = cpos.y * cpos.z;
            float vecty = cpos.x * cpos.z;
            float vectz = cpos.x * cpos.y;

            //going forwards on the stream line
            if (forward)
            {

                npos.x = cpos.x + ((float)0.1 * vectx);
                npos.y = cpos.y + ((float)0.1 * vecty);
                npos.z = cpos.z + ((float)0.1 * vectz);

            }
            //going backwards on the streamline
            else
            {

                vectx *= -1;
                vecty *= -1;
                vectz *= -1;

                npos.x = cpos.x + ((float)0.1 * vectx);
                npos.y = cpos.y + ((float)0.1 * vecty);
                npos.z = cpos.z + ((float)0.1 * vectz);

            }
            //testing the bounds for terminating the stream line
            if ((npos.x < -500 || npos.x > 500) || (npos.y < -500 || npos.y > 500) || (npos.z < -500 || npos.z > 500))
            {
                //return end code
                return false;

            }
            else
            {
                //return continue code
                return true;

            }

            
        }

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
