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
        Shader shader;
        Matrix4 Pers = Matrix4.Identity;
        Matrix4 Tran = Matrix4.Identity * Matrix4.CreateTranslation(0, 0, -10.0f);
        Matrix4 RotX = Matrix4.Identity;
        Matrix4 RotY = Matrix4.Identity;
        Matrix4 CameraT = Matrix4.Identity;
        

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
            float[] Vert =
            {
                    1.5f, 1.5f, 0.0f,
                    1.5f, 0.1f, 0.0f,
                    0.1f, 0.1f, 0.0f,
                    0.1f, 1.5f, 0.0f,

                    0.1f, -0.1f, 0.0f,
                    1.5f, -0.1f, 0.0f,
                    1.5f, -1.5f, 0.0f,
                    0.1f, -1.5f, 0.0f,

                    -0.1f, -0.1f, 0.0f,
                    -1.5f, -0.1f, 0.0f,
                    -1.5f, -1.5f, 0.0f,
                    -0.1f, -1.5f, 0.0f,

                    -1.5f, 1.5f, 0.0f,
                    -0.1f, 1.5f, 0.0f,
                    -0.1f, 0.1f, 0.0f,
                    -1.5f, 0.1f, 0.0f
            };

            uint[] Idx =
            {
                    0, 1, 2,
                    0, 2, 3,

                    4, 5, 6,
                    4, 6, 7,

                    8, 9, 10,
                    8, 10, 11,

                    12, 13, 14,
                    12, 14, 15
            };

            vertices = Vert;
            indices = Idx;

            this.UpdateGeometry(vertices, indices);

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
