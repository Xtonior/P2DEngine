using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;

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

        private int[,] map =
        {
            {1, 1, 1, 1},
            {1, 0, 0, 1},
            {1, 0, 0, 1},
            {1, 1, 1, 1},
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

            if (x <= 300)
            {
                DrawVerticalSlice(x, 600, 800, 600);
            }
            if (x > 300)
            {
                DrawVerticalSlice(x, 600, 800, 600);
                mainShader.SetVector3("wallColor", new Vector3(0.9f, 0.7f, 0.7f));
            }

            if (x < 800)
            {
                x++;
            }
            else
            {
                x = 0;
            }

            GL.BindVertexArray(vpVao);
            GL.DrawElements(BeginMode.Triangles, viewportIndices.Length, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }
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

        double posX = 3, posY = 2;  //x and y start position
        double dirX = -1, dirY = 0; //initial direction vector
        double planeX = 0, planeY = 0.66; //the 2d raycaster version of camera plane

        void DrawVerticalSlice(float x, int wallHeight, int screenWidth, int screenHeight)
        {
            int startY = (screenHeight - wallHeight) / 2;
            int endY = startY + wallHeight;

            int start = (startY / endY);
            int end = endY / endY;

            mainShader.SetFloat("sliceStart", 0.0f);
            mainShader.SetFloat("sliceEnd", 0.5f);
            mainShader.SetVector3("wallColor", new Vector3(0.3f, 0.5f, 0.7f));
        }

        void CastRays(float playerX, float playerY, float playerAngle, float fov)
        {
            float rayAngleStart = playerAngle - fov / 2;
            float rayAngleStep = fov / ClientSize.X;

            for (int x = 0; x < ClientSize.X; x++)
            {
                float rayAngle = rayAngleStart + x * rayAngleStep;

                // Cast ray
                float distanceToWall = 0;
                bool hitWall = false;
                while (!hitWall && distanceToWall < 150.0f)
                {
                    distanceToWall += 0.1f; // Increment ray

                    // Calculate test coordinates
                    int testX = (int)(playerX + MathF.Cos(rayAngle) * distanceToWall);
                    int testY = (int)(playerY + MathF.Sin(rayAngle) * distanceToWall);

                    // Check if ray is out of bounds
                    if (testX < 0 || testY < 0 || testX >= map.GetLength(0) || testY >= map.GetLength(1))
                    {
                        hitWall = true;
                        distanceToWall = 150.0f; // Hit wall at maximum depth
                    }
                    else if (map[testX, testY] == 1) // Hit a wall
                    {
                        hitWall = true;
                    }
                }

                // Calculate wall height
                int wallHeight = (int)(ClientSize.Y / distanceToWall);

                // Draw vertical slice of wall
                DrawVerticalSlice(x, wallHeight, ClientSize.X, ClientSize.Y);
            }
        }
    }
}