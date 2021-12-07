using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;

namespace CS453_Term_Project
{
    class Game : GameWindow
    {
        private float[] vertices =
        {
             0.5f,  0.5f, 0.0f, // top right
             0.5f, -0.5f, 0.0f, // bottom right
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
            base.OnResize(e);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
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

            this.UpdateGeometry(Vert, Idx);

            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            this.shader.Use();

            GL.BindVertexArray(VertexArrayObject);

            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

            Context.SwapBuffers();
        }
    }
}
