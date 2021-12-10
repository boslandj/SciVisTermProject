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

        // Operator Overlaods
        public static Vertex operator *(Vertex a, Vertex b) => new Vertex(a.x * b.x, a.y * b.y, a.z * b.z, a.empty);
        public static Vertex operator +(Vertex a, Vertex b) => new Vertex(a.x + b.x, a.y + b.y, a.z + b.z, a.empty);
        public static Vertex operator -(Vertex a, Vertex b) => new Vertex(a.x - b.x, a.y - b.y, a.z - b.z, a.empty);
        public static Vertex operator *(float f, Vertex a) => new Vertex(a.x * f, a.y * f, a.z * f, a.empty);
        public static Vertex operator /(Vertex a, float f) => new Vertex(a.x / f, a.y / f, a.z / f, a.empty);

        // Converts to Vector3
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

        private Color4 cv1 = new Color4(0.0f, 0.0f, 1.0f, 1.0f);
        private Color4 cv2 = new Color4(0.8f, 0.8f, 1.0f, 1.0f);
        private Color4 cv3 = new Color4(0.8f, 0.8f, 1.0f, 1.0f);
        private Color4 cv4 = new Color4(0.0f, 0.0f, 1.0f, 1.0f);

        float erot = MathHelper.DegreesToRadians(3);

        public Surface(float x1, float y1, float z1, float x2, float y2, float z2, float Step)
        {
            start1 = new Vertex(x1, y1, z1, false);
            start2 = new Vertex(x2, y2, z2, false);
            step = Step;

            // calculate the number of verts imbetween the inital seed points
            float dist = MathF.Sqrt(MathF.Pow(x1 - x2, 2.0f) + MathF.Pow(y1 - y2, 2.0f) + MathF.Pow(z1 - z2, 2.0f));
            int stepCnt = (int)(dist / step);
            stepCnt *= 2;

            // add inital seed points
            surf.Add(new List<Vertex>());
            surf.Add(new List<Vertex>());
            surf[0].Add(start1);
            surf[1].Add(start2);

            // add seed points imbetween inital points
            for(int i = 1; i < stepCnt; i++)
            {
                Vertex delta = start2 - start1;
                delta = ((float)i / (float)stepCnt) * delta;

                surf.Insert(i, new List<Vertex>());

                surf[i].Add(start1 + delta);

            }
            Advance_Front(); // advance one so there will be atleas one full columb of quads
        }

        // the vector field equation
        Vertex Vec_Fun(Vertex start)
        {
            Vector3 v;
            if(start.z > 0)
            {
                v = new Vector3(-start.y/(MathF.Pow(start.x, 2) + MathF.Pow(start.y, 2)), start.x / (MathF.Pow(start.x, 2) + MathF.Pow(start.y, 2)), 0.0f);
            }
            else
            {
                v = new Vector3(-start.y / (MathF.Pow(start.x, 2) + MathF.Pow(start.y, 2)), start.x / (MathF.Pow(start.x, 2) + MathF.Pow(start.y, 2)), 0.0f);
            }
            v.Normalize();
            return new Vertex(v.X, v.Y, v.Z, false);
        }

        // calculates the distance between two verticies
        float Distance(Vertex A, Vertex B)
        {
            Vector3 v1 = A.ToVector3();
            Vector3 v2 = B.ToVector3();

            return (v1 - v2).Length;
        }

        // angle between vertecies starting at S and between A and B
        float Angle(Vertex S, Vertex A, Vertex B)
        {
            Vector3 v1 = (S - A).ToVector3();
            Vector3 v2 = (S - B).ToVector3();

            return MathF.Acos(Vector3.Dot(v1, v2) / (v1.Length * v2.Length));
        }

        // check for divergance and add more geometry if needed
        private void Div()
        {
            int i1 = 0; // index of the top of the quad, index 0 always point to the top of the first quad in the columb
            int i2 = 1; // index of the bottom bottom of the quad

            while(i1 < surf.Count - 1)
            {
                // find bottom of the quad
                for(; i2 < surf.Count; i2++)
                {
                    if (!surf[i2][surf[i2].Count - 2].empty)
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
                    if((i1 + 1) < i2) // if there is an empty stream line between i1 and i2 use the empty for the extra geometry
                    {
                        // add vertex at the midpoint between i1 and i2
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
                    else // if there is not an empty stream line between i1 and i2 add a new stream line between i1 and i2
                    {
                        // add new streamline
                        List<Vertex> sl = new List<Vertex>();

                        for(int i = 0; i < surf[0].Count; i++)
                        {
                            sl.Add(new Vertex(0.0f, 0.0f, 0.0f, true));
                        }

                        surf.Insert(i1 + 1, sl);

                        i2++;

                        // add vertex at the midpoint between i1 and i2
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

        // check for convergence and set streamline to empty if convergence present
        private void Con()
        {
            int i1 = 0; // index of the top of the first quad, index 0 always point to the top of the first quad in the columb
            int i2 = 0; // index of the bottom of the first quad, index of the top of the second quad
            int i3 = 0; // index of the bottom of the second quad

            while (i1 < surf.Count - 1)
            {
                // find the index i2
                for (; i2 < surf.Count; i2++)
                {
                    if (!surf[i2][surf[i2].Count - 2].empty)
                        break;
                }
                if (i2 >= surf.Count)
                    break;
                // find the index i3
                i3 = i2;
                i3++;
                for (; i3 < surf.Count; i3++)
                {
                    if (!surf[i3][surf[i3].Count - 2].empty)
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
                    // set the 2 vertecies bettween i1 and 13
                    Vertex v1 = surf[i2][surf[i2].Count - 1];
                    Vertex v2 = surf[i2][surf[i2].Count - 2];

                    v1.empty = true;
                    v2.empty = true;
                    surf[i2][surf[i2].Count - 1] = v1;
                    surf[i2][surf[i2].Count - 2] = v2;

                    // move the stream line point at i2 from the previous quads to the mid point betwen i1 and i3 of the previous quads to remove holes
                    if (surf[i2].Count >= 3)
                    {
                        Vertex v3 = surf[i1][surf[i1].Count - 3];
                        Vertex v4 = surf[i3][surf[i3].Count - 3];

                        surf[i2][surf[i2].Count - 3] = (v3 + v4) / 2.0f;
                    }

                }
                //need to progress counters
                i1 = i2;
                i2 = i3;
                i3++;
            }
        }

        // check and corect for right curvature
        private void Cur_Right()
        {
            int i1 = 0; // index of the top of the quad, index 0 always point to the top of the first quad in the columb
            int i2 = 1; // index of the bottom bottom of the quad
            bool rot = false; // if any quads have need to be corrected for rotation
            float prevlen = 0; // the length of the last quad that was shortened

            while (i1 < surf.Count - 1)
            {
                // find i2
                for (; i2 < surf.Count; i2++)
                {
                    if (!surf[i2][surf[i2].Count - 2].empty)
                        break;
                }
                if (i2 >= surf.Count)
                    break;

                float A1 = Angle(surf[i1][surf[i1].Count - 2], surf[i2][surf[i2].Count - 2], surf[i1][surf[i1].Count - 1]);
                float A2 = Angle(surf[i2][surf[i2].Count - 2], surf[i1][surf[i1].Count - 2], surf[i2][surf[i2].Count - 1]);

                if (
                    A1 < MathHelper.DegreesToRadians(90) - erot &&
                    A2 > MathHelper.DegreesToRadians(90) + erot
                    )
                {
                    rot = true;

                    Vector3 vl = (surf[i1][surf[i1].Count - 1] - surf[i1][surf[i1].Count - 2]).ToVector3();
                    Vector3 vr = (surf[i2][surf[i2].Count - 1] - surf[i2][surf[i2].Count - 2]).ToVector3();

                    // shorten vr to correct for rotation
                    vr.Normalize();
                    prevlen = A1 / (MathHelper.DegreesToRadians(90) + erot) * vl.Length;
                    vr =  prevlen * vr;

                    Vertex vertexR = new Vertex(vr.X, vr.Y, vr.Z, false);
                    vertexR = surf[i2][surf[i2].Count - 2] + vertexR;

                    surf[i2][surf[i2].Count - 1] = vertexR;
                }
                // if right rotation occurred and left rotation will not occur
                else if(
                    rot &&
                    !(A1 > MathHelper.DegreesToRadians(90) + erot &&
                    A2 < MathHelper.DegreesToRadians(90) - erot))
                {
                    Vector3 vr = (surf[i2][surf[i2].Count - 1] - surf[i2][surf[i2].Count - 2]).ToVector3();
                    vr.Normalize();
                    vr = prevlen * vr;
                    Vertex vertexR = new Vertex(vr.X, vr.Y, vr.Z, false);
                    vertexR = surf[i2][surf[i2].Count - 2] + vertexR;

                    surf[i2][surf[i2].Count - 1] = vertexR;
                }
                // need to progress counters
                i1 = i2;
                i2++;
            }
        }

        // check and corect for left curvature
        private void Cur_Left()
        {
            int i1 = surf.Count - 1; // index of the top of the quad, index 0 always point to the top of the first quad in the columb
            int i2 = surf.Count - 2; // index of the bottom bottom of the quad
            float prevlen = 0; // if any quads have need to be corrected for rotation
            bool rot = false; // the length of the last quad that was shortened

            while (i1 >= 0)
            {
                // find i2
                for (; i2 >= 0; i2--)
                {
                    if (!surf[i2][surf[i2].Count - 2].empty)
                        break;
                }
                if (i2 < 0)
                    break;

                float A1 = Angle(surf[i1][surf[i1].Count - 2], surf[i2][surf[i2].Count - 2], surf[i1][surf[i1].Count - 1]);
                float A2 = Angle(surf[i2][surf[i2].Count - 2], surf[i1][surf[i1].Count - 2], surf[i2][surf[i2].Count - 1]);

                if (
                    A1 < MathHelper.DegreesToRadians(90) - erot &&
                    A2 > MathHelper.DegreesToRadians(90) + erot 
                    )
                {
                    rot = true;

                    Vector3 vl = (surf[i1][surf[i1].Count - 1] - surf[i1][surf[i1].Count - 2]).ToVector3();
                    Vector3 vr = (surf[i2][surf[i2].Count - 1] - surf[i2][surf[i2].Count - 2]).ToVector3();

                    // shorten vr to correct for rotation
                    vr.Normalize();
                    prevlen = A1 / (MathHelper.DegreesToRadians(90) + erot) * vl.Length;
                    vr = prevlen * vr;

                    Vertex vertexR = new Vertex(vr.X, vr.Y, vr.Z, false);
                    vertexR = surf[i2][surf[i2].Count - 2] + vertexR;

                    surf[i2][surf[i2].Count - 1] = vertexR;
                }
                // if right rotation occurred and left rotation will not occur
                else if (
                    rot &&
                    !(A1 > MathHelper.DegreesToRadians(90) + erot &&
                    A2 < MathHelper.DegreesToRadians(90) - erot))
                {
                    Vector3 vr = (surf[i2][surf[i2].Count - 1] - surf[i2][surf[i2].Count - 2]).ToVector3();
                    vr.Normalize();
                    vr = prevlen * vr;
                    Vertex vertexR = new Vertex(vr.X, vr.Y, vr.Z, false);
                    vertexR = surf[i2][surf[i2].Count - 2] + vertexR;

                    surf[i2][surf[i2].Count - 1] = vertexR;
                }
                //need to progress counters
                i1 = i2;
                i2--;
            }
        }

        // check and correct for high curvature
        private void Cur()
        {
            Cur_Right();
            Cur_Left();
        }

        // advance surface by one columb
        public void Advance_Front()
        {
            for(int i = 0; i < surf.Count; i++)
            {

                surf[i].Add(streamline_step(surf[i][surf[i].Count - 1]));

            }

        }

        // add a new vertex to one streamline
        public Vertex streamline_step(Vertex cpos)
        {

            //setting up the vector components according to the field equation x = yz, y = xz, z = xy
            Vertex vect = Vec_Fun(cpos);

            //going forwards on the stream line
            vect = 0.5f * step * vect;

            Vector3 vect1 = vect.ToVector3();
            //Check to see if velocity is zero
            if(vect1.Length == 0)
            {
                //return an empty vertex as the streamline will no longer progress
                return new Vertex(cpos.x, cpos.y, cpos.z, true);

            }

            //returning the new vector

            return cpos + vect;
        }

        // consturct the integral surface
        public void build_surface(int iter)
        {
            for(int i = 0; i < iter; i++)
            {
                Advance_Front();

                Div();
                Con();
                Cur();
            }
        }

        // add a opengl vertex to a list
        private void add_vert_to_list(int idxs, int idxc, ref List<float> ves)
        {
            ves.Add(surf[idxs][idxc].x);
            ves.Add(surf[idxs][idxc].y);
            ves.Add(surf[idxs][idxc].z);
        }
        // add a opengl color to a list
        private void add_color_to_list(float r, float g, float b, ref List<float> ves)
        {
            ves.Add(r);
            ves.Add(g);
            ves.Add(b);
        }

        // convert the integral surface to opengl arrays
        public void get_geo(out float[] verts, out uint[] indxs)
        {
            List<float> ves = new List<float>();
            List<uint> ins = new List<uint>();

            for (int i = 0; i < surf[0].Count - 1; i++)
            {
                int j1 = 0; // index of the top of the quad, index 0 always point to the top of the first quad in the columb
                int j2 = 1; // index of the bottom bottom of the quad

                for (; j2 < surf.Count;)
                {
                    // find bottom of the quad
                    while (j2 < surf.Count)
                    {
                        if (!(surf[j2][i].empty || surf[j2][i + 1].empty))
                        {
                            break;
                        }
                        j2++;
                    }
                    if (j2 >= surf.Count)
                        break;

                    uint ins_start = (uint)(ves.Count / 6);

                    // add indicies of the opengl tiriangles to ins
                    ins.Add(ins_start);
                    ins.Add(ins_start + 1);
                    ins.Add(ins_start + 2);
                    ins.Add(ins_start);
                    ins.Add(ins_start + 2);
                    ins.Add(ins_start + 3);

                    // add verts and color of the quad to ves
                    add_vert_to_list(j1, i, ref ves);
                    add_color_to_list(cv1.R, cv1.G, cv1.B, ref ves);
                    add_vert_to_list(j1, i + 1, ref ves);
                    add_color_to_list(cv2.R, cv2.B, cv2.G, ref ves);
                    add_vert_to_list(j2, i + 1, ref ves);
                    add_color_to_list(cv3.R, cv3.G, cv3.B, ref ves);
                    add_vert_to_list(j2, i, ref ves);
                    add_color_to_list(cv4.R, cv4.G, cv4.B, ref ves);

                    // swap j1 with j2 and inc j2
                    j1 = j2;
                    j2++;
                }
            }

            // convert ves and ins to opengl arrays and store them to verts and indxs
            verts = ves.ToArray();
            indxs = ins.ToArray();
        }
    }

    class Game : GameWindow
    {
        private float[] vertices;
        uint[] indices;
        private float StepSize = 1.0f;
        private int Advancements = 200;

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

            // enable depth test
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.DepthMask(true);
            GL.DepthRange(0.0f, 1.0f);

            // set back groun color
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            // setup the vertex buffer
            this.VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.VertexBufferObject);
            //GL.BufferData(BufferTarget.ArrayBuffer, this.vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            
            // setup the vertex array for verts and colors
            this.VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // setup the element array
            this.elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.elementBufferObject);
            //GL.BufferData(BufferTarget.ElementArrayBuffer, this.indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            // enable the shader
            this.shader = new Shader("..\\..\\..\\shader.Vert", "..\\..\\..\\shader.frag");
            this.shader.Use();
        }

        // upate the vertex and index array
        public void UpdateGeometry(float[] Vert, uint[] Idx)
        {
            GL.BufferData(BufferTarget.ArrayBuffer, Vert.Length * sizeof(float), Vert, BufferUsageHint.StaticDraw);

            GL.BufferData(BufferTarget.ElementArrayBuffer, Idx.Length * sizeof(uint), Idx, BufferUsageHint.StaticDraw);
        }

        protected override void OnUnload()
        {
            // unbind buffers and arrays
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            // unbind shader prog
            GL.UseProgram(0);

            // delete buffers and arrays
            GL.DeleteBuffer(VertexBufferObject);
            GL.DeleteVertexArray(VertexArrayObject);

            // remove shader
            shader.Dispose();

            base.OnUnload();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            // update the view port and perspective transform when resized
            GL.Viewport(0, 0, e.Width, e.Height);
            Pers = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90), (float)e.Width / (float)e.Height, 0.0001f, 10000.0f);
            base.OnResize(e);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            // only draw the steam surface once
            if(drawStream)
            {
                Surface surface = new Surface(1.0f, 1.0f, 1.0f, 10.0f , 2.0f, 2.0f, StepSize);

                surface.build_surface(Advancements);
                surface.get_geo(out vertices, out indices);

                this.UpdateGeometry(vertices, indices);

                drawStream = false;
            }

            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // setup and use the shader
            this.shader.Use();
            this.shader.SetMatrix4("transform", CameraT);

            GL.BindVertexArray(VertexArrayObject);

            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

            Context.SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);


            // camera controles
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
