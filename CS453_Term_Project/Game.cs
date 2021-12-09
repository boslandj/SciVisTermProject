using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CS453_Term_Project
{
    struct Vertex
    {
        public float x;
        public float y;
        public float z;
        public bool empty;


        public Vertex(float X, float Y, float Z, bool Empty)
        {
            this.x = X;
            this.y = Y;
            this.z = Z;
            this.empty = Empty;
        }

        public static Vertex operator *(Vertex a, Vertex b) => new Vertex(a.x * b.x, a.y * b.y, a.z * b.z, a.empty);
        public static Vertex operator +(Vertex a, Vertex b) => new Vertex(a.x + b.x, a.y + b.y, a.z + b.z, a.empty);
        public static Vertex operator -(Vertex a, Vertex b) => new Vertex(a.x - b.x, a.y - b.y, a.z - b.z, a.empty);
        public static Vertex operator *(float f, Vertex a) => new Vertex(a.x * f, a.y * f, a.z * f, a.empty);

        public Vector3 ToVector3()
        {
            return new Vector3(this.x, this.y, this.z);
        }
    };

    class Surface
    {
        List<List<Vertex>> surf = new List<List<Vertex>>();
        float step;
        Vertex start1;
        Vertex start2;

        float erot = MathHelper.DegreesToRadians(3);

        public Surface(float x1, float y1, float z1, float x2, float y2, float z2, float Step)
        {
            start1 = new Vertex(x1, y1, z1, false);
            start2 = new Vertex(x2, y2, z2, false);
            step = Step;

            float dist = MathF.Sqrt(MathF.Pow(x1 - x2, 2.0f) + MathF.Pow(y1 - y2, 2.0f) + MathF.Pow(z1 - z2, 2.0f));
            int stepCnt = (int)(dist / step);
            stepCnt *= 2;

            surf.Add(new List<Vertex>());
            surf.Add(new List<Vertex>());
            surf[0].Add(start1);
            surf[1].Add(start2);

            for(int i = 1; i < stepCnt; i++)
            {
                Vertex delta = start2 - start1;
                delta = ((float)i / (float)stepCnt) * delta;

                surf.Insert(i, new List<Vertex>());

                surf[i].Add(start1 + delta);

            }
            Advance_Front();
        }

        Vertex Vec_Fun(Vertex start)
        {
            return new Vertex(
                start.y * start.z,
                start.x * start.z,
                start.x * start.y,
                false);
        }

        float Distance(Vertex A, Vertex B)
        {
            Vector3 v1 = A.ToVector3();
            Vector3 v2 = B.ToVector3();

            return (v1 - v2).Length;
        }

        float Angle(Vertex S, Vertex A, Vertex B)
        {
            Vector3 v1 = (S - A).ToVector3();
            Vector3 v2 = (S - B).ToVector3();

            return MathF.Acos(Vector3.Dot(v1, v2) / (v1.Length * v2.Length));
        }

        private void Div()
        {
            int i1 = 0;
            int i2 = 1;

            while(i1 < surf.Count - 1)
            {
                for(; i2 < surf.Count; i2++)
                {
                    if (!surf[i2][surf.Count - 2].empty)
                        break;
                }
                if (i2 >= surf.Count)
                    break;

                float A1 = Angle(surf[i1][surf[i1].Count - 2], surf[i2][surf[i2].Count - 2], surf[i1][surf[i1].Count - 1]);
                float A2 = Angle(surf[i2][surf[i2].Count - 2], surf[i1][surf[i1].Count - 2], surf[i2][surf[i2].Count - 1]);
                float ln = Distance(surf[i1][surf[i1].Count - 2], surf[i2][surf[i2].Count - 2]);

                if(
                    A1 > MathHelper.DegreesToRadians(90) &&
                    A2 > MathHelper.DegreesToRadians(90) &&
                    ln > (step / 2)
                    )
                {
                    if((i1 + 1) < i2)
                    {
                        Vertex v1 = surf[i1][surf[i1].Count - 1];
                        Vertex v2 = surf[i2][surf[i2].Count - 1];

                        Vertex v3 = v1 + (0.5f * (v2 - v1));
                        v3.empty = false;
                        surf[i1 + 1][surf.Count - 1] = v3;


                        v1 = surf[i1][surf[i1].Count - 2];
                        v2 = surf[i2][surf[i2].Count - 2];

                        v3 = v1 + (0.5f * (v2 - v1));
                        v3.empty = false;
                        surf[i1 + 1][surf[i1 + 1].Count - 2] = v3;
                    }
                    else
                    {
                        List<Vertex> sl = new List<Vertex>();

                        for(int i = 0; i < surf[0].Count; i++)
                        {
                            sl.Add(new Vertex(0.0f, 0.0f, 0.0f, true));
                        }

                        surf.Insert(i1 + 1, sl);

                        i2++;

                        Vertex v1 = surf[i1][surf[i1].Count - 1];
                        Vertex v2 = surf[i2][surf[i2].Count - 1];

                        Vertex v3 = v1 + (0.5f * (v2 - v1));
                        v3.empty = false;
                        surf[i1 + 1][surf[i1 + 1].Count - 1] = v3;


                        v1 = surf[i1][surf[i1].Count - 2];
                        v2 = surf[i2][surf[i2].Count - 2];

                        v3 = v1 + (0.5f * (v2 - v1));
                        v3.empty = false;
                        surf[i1 + 1][surf[i1 + 1].Count - 2] = v3;
                    }
                }
                //need to progress counters
                i1 = i2;
                i2++;
            }
        }

        private void Con()
        {
            int i1 = 0;
            int i2 = 0;
            int i3 = 0;

            while(i1 < surf.Count - 1)
            {
                for (; i3 < surf.Count; i3++)
                {
                    if (!surf[i3][surf.Count - 2].empty)
                        break;
                }
                if (i3 >= surf.Count)
                    break;
                i2 = i3;
                for (; i3 < surf.Count; i3++)
                {
                    if (!surf[i3][surf.Count - 2].empty)
                        break;
                }
                if (i3 >= surf.Count)
                    break;

                float A1 = Angle(surf[i1][surf[i1].Count - 2], surf[i2][surf[i2].Count - 2], surf[i1][surf[i1].Count - 1]);
                float A2 = Angle(surf[i3][surf[i3].Count - 2], surf[i2][surf[i2].Count - 2], surf[i3][surf[i3].Count - 1]);
                float ln = Distance(surf[i1][surf[i1].Count - 2], surf[i2][surf[i2].Count - 2]) + Distance(surf[i2][surf[i2].Count - 2], surf[i3][surf[i3].Count - 2]);

                if (
                    A1 < MathHelper.DegreesToRadians(90) &&
                    A2 < MathHelper.DegreesToRadians(90) &&
                    ln < (step / 2)
                    )
                {

                    Vertex v1 = surf[i2][surf[i2].Count - 1];
                    Vertex v2 = surf[i2][surf[i2].Count - 2];

                    v1.empty = true;
                    v2.empty = true;
                    surf[i2][surf[i2].Count - 1] = v1;
                    surf[i2][surf[i2].Count - 2] = v2;
                }
                //need to progress counters
                i1 = i2;
                i2 = i3;
                i3++;
            }
        }

        private void Cur()
        {

        }


        public void Advance_Front()
        {
            for(int i = 0; i < surf.Count; i++)
            {

                surf[i].Add(streamline_step(surf[i][surf[i].Count - 1]));

            }

        }

        public Vertex streamline_step(Vertex cpos)
        {

            //setting up the vector components according to the field equation x = yz, y = xz, z = xy
            Vertex vect = Vec_Fun(cpos);

            //going forwards on the stream line
            vect = 0.5f * step * vect;

            //returning the new vector

            return cpos + vect;
        }

        public void build_streamline(float x, float y, float z, ref List<List<Vertex>> arr)
        {

            arr.Add(new List<Vertex>());
            int index = arr.Count - 1;

            Vertex cpos_f = new Vertex();
            Vertex cpos_b = new Vertex();

            cpos_f.empty = false;
            cpos_b.empty = false;

            cpos_f.x = x;
            cpos_b.x = x;
            cpos_f.y = y;
            cpos_b.y = y;
            cpos_f.z = z;
            cpos_b.z = z;

            bool forward = true;
            bool backward = true;
            uint stepmax = 100;

            arr[index].Add(cpos_f);
            while((forward || backward) && (stepmax > 0))
            {
                stepmax--;

                if (forward)
                {
                    Vertex npos_f = new Vertex();
                    streamline_step(cpos_f);
                    arr[index].Insert(0, npos_f);
                    cpos_f = npos_f;

                }
                else if(backward){

                    Vertex npos_b = new Vertex();
                    streamline_step(cpos_b);
                    arr[index].Add(npos_b);
                    cpos_b = npos_b;

                }

            }


        }
    }

    class Game : GameWindow
    {
        private float[] vertices =
        {
             0.5f,  0.5f, 2.0f, // top right
             0.5f, -0.5f, 2.0f, // bottom right
            -0.5f, -0.5f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, // top left
        };

        uint[] indices =
        {
            // Note that indices start at 0!
            0, 1, 3, // The first triangle will be the bottom-right half of the triangle
            1, 2, 3  // Then the second will be the top-right half of the triangle
        };

        private int VertexBufferObject;
        private int VertexArrayObject;
        private int elementBufferObject;
        private Shader shader;
        private Matrix4 Pers = Matrix4.Identity;
        private Matrix4 Tran = Matrix4.Identity * Matrix4.CreateTranslation(0, 0, -10.0f);
        private Matrix4 RotX = Matrix4.Identity;
        private Matrix4 RotY = Matrix4.Identity;
        private Matrix4 CameraT = Matrix4.Identity;

        private bool drawStream = true;
        

        public Game(GameWindowSettings gws, NativeWindowSettings nws)
            : base(gws, nws)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            this.VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, this.vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            
            this.VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            this.elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, this.indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            this.shader = new Shader("..\\..\\..\\shader.Vert", "..\\..\\..\\shader.frag");
            this.shader.Use();
        }

        public void UpdateGeometry(float[] Vert, uint[] Idx)
        {
            GL.BufferData(BufferTarget.ArrayBuffer, Vert.Length * sizeof(float), Vert, BufferUsageHint.StaticDraw);

            GL.BufferData(BufferTarget.ElementArrayBuffer, Idx.Length * sizeof(uint), Idx, BufferUsageHint.StaticDraw);
        }

        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(VertexBufferObject);
            GL.DeleteVertexArray(VertexArrayObject);

            shader.Dispose();

            base.OnUnload();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            Pers = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90), (float)e.Width / (float)e.Height, 0.0001f, 10000.0f);
            base.OnResize(e);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            if(drawStream)
            {
                List<List<Vertex>> surface_arr = new List<List<Vertex>>();

                Surface surface = new Surface();

                surface.build_streamline(1.0f, 1.0f, 10.0f, ref surface_arr);
                surface.build_streamline(1.0f, 1.0f, 20.0f, ref surface_arr);

                List<uint> indxs = new List<uint>();
                List<float> verts = new List<float>();

                for(uint i = 0; i < surface_arr[0].Count; i++)
                {
                    uint offset = (uint)surface_arr[0].Count - 1;

                    indxs.Add(i);
                    indxs.Add(i + 1);
                    indxs.Add(i + offset);

                    indxs.Add(i + offset);
                    indxs.Add(i + offset + 1);
                    indxs.Add(i + 1);
                }

                for(int i = 0; i < surface_arr[0].Count; i++)
                {
                    verts.Add(surface_arr[0][i].x);
                    verts.Add(surface_arr[0][i].y);
                    verts.Add(surface_arr[0][i].z);
                }
                for (int i = 0; i < surface_arr[1].Count; i++)
                {
                    verts.Add(surface_arr[1][i].x);
                    verts.Add(surface_arr[1][i].y);
                    verts.Add(surface_arr[1][i].z);
                }

                vertices = verts.ToArray();
                indices = indxs.ToArray();

                this.UpdateGeometry(vertices, indices);

                Console.WriteLine(vertices.Length);
                Console.WriteLine(indices.Length);

                for (int i = 0; i < vertices.Length; i += 3)
                {
                    System.Console.WriteLine(vertices[i].ToString() + ' ' + vertices[i + 1].ToString() + ' ' + vertices[i + 2].ToString());
                }

                drawStream = false;
            }

            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            this.shader.Use();

            this.shader.SetMatrix4("transform", CameraT);

            GL.BindVertexArray(VertexArrayObject);

            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

            Context.SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            var mov_input = KeyboardState;

            Vector3 Translate = new Vector3();
            Matrix4 RotationX = Matrix4.Identity;
            Matrix4 RotationY = Matrix4.Identity;

            float mov_int = 0.0025f;
            float rot_mul = 0.01f;

            if (mov_input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftShift))
            {
                mov_int = 0.005f;
            }

            if (mov_input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W))
            {
                Translate.Z += mov_int;
            }

            if (mov_input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S))
            {
                Translate.Z -= mov_int;
            }

            if (mov_input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A))
            {
                Translate.X += mov_int;
            }

            if (mov_input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D))
            {
                Translate.X -= mov_int;
            }

            if (mov_input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Space))
            {
                Translate.Y -= mov_int;
            }

            if (mov_input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftAlt))
            {
                Translate.Y += mov_int;
            }

            if (MouseState.IsButtonDown(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Button1))
            {
                RotationX *= Matrix4.CreateRotationY(MouseState.Delta.X * rot_mul);
                RotationY *= Matrix4.CreateRotationX(MouseState.Delta.Y * rot_mul);
            }

            RotX *= RotationX;
            RotY *= RotationY;

            var TranRot = RotX * RotY;

            Translate = Vector3.Transform(Translate, TranRot.ExtractRotation().Inverted());

            Tran *= Matrix4.CreateTranslation(Translate.X, Translate.Y, Translate.Z);

            CameraT = Matrix4.Identity;
            CameraT *= Tran;
            CameraT *= RotX;
            CameraT *= RotY;
            CameraT *= Pers;
        }
    }
}
