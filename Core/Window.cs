using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System.Reflection.Metadata;

namespace P2DEngine.Core
{
    public class Window : GameWindow
    {
        float[] viewportVertices =
        {
             1.0f,  1.0f, 0.0f,   1.0f, 1.0f,
             1.0f, -1.0f, 0.0f,   1.0f, 0.0f,
            -1.0f, -1.0f, 0.0f,   0.0f, 0.0f,
            -1.0f,  1.0f, 0.0f,   0.0f, 1.0f
        };

        private uint[] viewportIndices =
        {
            0, 1, 3,
            1, 2, 3
        };

        int vpVao, vpVbo, vpEbo;

        private Shader mainShader;

        private float[] map =
        {
            1, 1, 1, 1, 1, 1, 1, 1,
            1, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 1, 0, 1,
            1, 0, 0, 0, 0, 1, 0, 1,
            1, 0, 0, 0, 0, 1, 1, 1,
            1, 0, 0, 0, 0, 0, 0, 1,
            1, 0, 0, 0, 0, 0, 0, 1,
            1, 1, 1, 1, 1, 1, 1, 1
        };

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.3f, 0.4f, 0.6f, 1.0f);

            // Gen objects
            vpVao = GL.GenVertexArray();
            vpVbo = GL.GenBuffer();
            vpEbo = GL.GenBuffer();

            // VAO
            GL.BindVertexArray(vpVao);

            // VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, vpVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, viewportVertices.Length * sizeof(float), viewportVertices, BufferUsageHint.StaticDraw);

            // EBO
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vpEbo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, viewportIndices.Length * sizeof(uint), viewportIndices, BufferUsageHint.StaticDraw);

            mainShader = new Shader("Assets/Shaders/shader.vert", "Assets/Shaders/shader.frag");
            mainShader.Use();

            // Attribs
            int vertexLocation = mainShader.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            var texCoordLocation = mainShader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            CreateTex();

            // Unbind VBO and VAO
            // GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // GL.BindVertexArray(0);
        }

        int x = 0;

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.ClearColor(0.3f, 0.4f, 0.6f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            mainShader.Use();

            DrawVerticalSlice(x, 600, 800, 600);

            GL.BindVertexArray(vpVao);
            GL.DrawElements(BeginMode.Triangles, viewportIndices.Length, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        float posX = 400, posY = 300;  //x and y start position
        float angle;
        Vector2 dir;
        double planeX = 0, planeY = 0.66; //the 2d raycaster version of camera plane

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            if (input.IsKeyDown(Keys.W))
            {
                posX += dir.X * 100 * (float)e.Time;
                posY += dir.Y * 100 * (float)e.Time;
            }
            if (input.IsKeyDown(Keys.S))
            {
                posX -= dir.X * 100 * (float)e.Time;
                posY -= dir.Y * 100 * (float)e.Time;
            }

            if (input.IsKeyDown(Keys.A))
            {
                angle += 5f * (float)e.Time;
            }
            if (input.IsKeyDown(Keys.D))
            {
                angle -= 5f * (float)e.Time;
            }

            if (angle < 0.0f)
            {
                angle = MathHelper.TwoPi;
            }
            if (angle > MathHelper.TwoPi)
            {
                angle = 0.0f;
            }

            dir = new Vector2((float)MathHelper.Cos(angle), (float)MathHelper.Sin(angle));
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(vpVbo);
            GL.DeleteVertexArray(vpVao);

            GL.DeleteProgram(mainShader.Handle);

            base.OnUnload();
        }

        int textureID;

        void CreateTex()
        {            
            GL.GenTextures(1, out textureID);
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            // Upload the texture data
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, 8, 8, 0, PixelFormat.Red, PixelType.Float, map);

            // Set texture parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // Unbind the texture
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        void DrawVerticalSlice(float x, int wallHeight, int screenWidth, int screenHeight)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            // Draw map
            int textureLocation = GL.GetUniformLocation(mainShader.Handle, "mapTexture");
            GL.Uniform1(textureLocation, 0); // Texture unit 0

            // Draw Player
            mainShader.SetVector2("u_resolution", new Vector2(ClientSize.X, ClientSize.Y));
            mainShader.SetVector2("u_cameraPos", new Vector2(posX, posY));
            //mainShader.SetVector2("u_cameraDir", new Vector2(1, 1));

            mainShader.SetVector2("u_rayStart", new Vector2(posX, posY));
            mainShader.SetVector2("u_rayEnd", 100f * dir + new Vector2(posX, posY)); 
        }
    }
}