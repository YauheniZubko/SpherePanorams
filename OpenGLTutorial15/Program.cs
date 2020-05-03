using System;
using System.Collections.Generic;
using System.Linq;
using Tao.FreeGlut;
using OpenGL;
using OpenGLTutorial5;

namespace OpenGLTutorial15
{
    class Program
    {
        private static int width = 1280, height = 720;
        private static System.Diagnostics.Stopwatch watch;

        private static ShaderProgram program;
        private static VBO<Vector3> cube;
        private static VBO<Vector2> cubeUV;
        private static VBO<uint> cubeTriangles;
        private static Texture brickDiffuse;
        private static bool fullscreen = false;
        private static bool left, right, up, down, space;

        private static Camera camera;

        private static BMFont font;
        private static ShaderProgram fontProgram;
        private static FontVAO information;

        static void Main(string[] args)
        {
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH | Glut.GLUT_MULTISAMPLE);   // multisampling makes things beautiful!
            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow("OpenGL Tutorial");

            Glut.glutIdleFunc(OnRenderFrame);
            Glut.glutDisplayFunc(OnDisplay);

            Glut.glutKeyboardFunc(OnKeyboardDown);
            Glut.glutKeyboardUpFunc(OnKeyboardUp);

            Glut.glutCloseFunc(OnClose);
            Glut.glutReshapeFunc(OnReshape);

            // add our mouse callbacks for this tutorial
            Glut.glutMouseFunc(OnMouse);
            Glut.glutMotionFunc(OnMove);

            Gl.Enable(EnableCap.DepthTest);
            Gl.Enable(EnableCap.Blend);
            Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // create our shader program
            program = new ShaderProgram(VertexShader, FragmentShader);

            // create our camera
            camera = new Camera(new Vector3(0, 0, 0), Quaternion.Identity);
            camera.SetDirection(new Vector3(0, 0, -1));

            // set up the projection and view matrix
            program.Use();
            program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)width / height, 0.1f, 1000f));
            //program["view_matrix"].SetValue(Matrix4.LookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.Up));

            program["light_direction"].SetValue(new Vector3(0, 0, 1));
           
            program["normalTexture"].SetValue(1);

            brickDiffuse = new Texture("333.jpg");
            

            var sphereCoordinates = CoordinatesManager.GetSphereCoordinates().ToArray();

            var UVcoordinates = new List<Vector2>();
            for (int i = 0; i < sphereCoordinates.Length / 4; i++)
            {
                UVcoordinates.Add(new Vector2(0, 0));
                UVcoordinates.Add(new Vector2(1, 0));
                UVcoordinates.Add(new Vector2(1, 1));
                UVcoordinates.Add(new Vector2(0, 1));
            }
            var sphereUV = new VBO<Vector2>(UVcoordinates.ToArray());


            Vector3[] vertices = new Vector3[] {
                new Vector3(1, 1, -1), new Vector3(-1, 1, -1), new Vector3(-1, 1, 1), new Vector3(1, 1, 1),         // top
                new Vector3(1, -1, 1), new Vector3(-1, -1, 1), new Vector3(-1, -1, -1), new Vector3(1, -1, -1),     // bottom
                new Vector3(1, 1, 1), new Vector3(-1, 1, 1), new Vector3(-1, -1, 1), new Vector3(1, -1, 1),         // front face
                new Vector3(1, -1, -1), new Vector3(-1, -1, -1), new Vector3(-1, 1, -1), new Vector3(1, 1, -1),     // back face
                new Vector3(-1, 1, 1), new Vector3(-1, 1, -1), new Vector3(-1, -1, -1), new Vector3(-1, -1, 1),     // left
                new Vector3(1, 1, -1), new Vector3(1, 1, 1), new Vector3(1, -1, 1), new Vector3(1, -1, -1) };
            cube = new VBO<Vector3>(sphereCoordinates);

            Vector2[] uvs = new Vector2[] {
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
            cubeUV = CoordinatesManager.GetSphereUVCoordinates();

            List<uint> triangles = new List<uint>();
            for (uint i = 0; i < 6; i++)
            {
                triangles.Add(i * 4);
                triangles.Add(i * 4 + 1);
                triangles.Add(i * 4 + 2);
                triangles.Add(i * 4);
                triangles.Add(i * 4 + 2);
                triangles.Add(i * 4 + 3);
            }
            //cubeTriangles = new VBO<uint>(triangles.ToArray(), BufferTarget.ElementArrayBuffer);
            cubeTriangles =
                new VBO<uint>(Enumerable.Range(0, sphereCoordinates.Length * 4).Select(t => (uint) t).ToArray(),
                    BufferTarget.ElementArrayBuffer);

           // Vector3[] normals = Geometry.CalculateNormals(vertices, triangles.ToArray());
           //cubeNormals = new VBO<Vector3>(normals);



            // load the bitmap font for this tutorial
            font = new BMFont("font24.fnt", "font24.png");
            fontProgram = new ShaderProgram(BMFont.FontVertexSource, BMFont.FontFragmentSource);

            fontProgram.Use();
            fontProgram["ortho_matrix"].SetValue(Matrix4.CreateOrthographic(width, height, 0, 1000));
            fontProgram["color"].SetValue(new Vector3(1, 1, 1));

            information = font.CreateString(fontProgram, "OpenGL C# Tutorial 15");

            watch = System.Diagnostics.Stopwatch.StartNew();

            Glut.glutMainLoop();
        }

        

        private static void OnClose()
        {
            cube.Dispose();
            //cubeNormals.Dispose();
            cubeUV.Dispose();
  
            cubeTriangles.Dispose();
            brickDiffuse.Dispose();
           
            program.DisposeChildren = true;
            program.Dispose();
            fontProgram.DisposeChildren = true;
            fontProgram.Dispose();
            font.FontTexture.Dispose();
            information.Dispose();
        }

        private static void OnDisplay()
        {

        }

        private static bool mouseDown = false;
        private static int downX, downY;
        private static int prevX, prevY;

        private static void OnMouse(int button, int state, int x, int y)
        {
            if (button != Glut.GLUT_RIGHT_BUTTON) return;

            // this method gets called whenever a new mouse button event happens
            mouseDown = (state == Glut.GLUT_DOWN);

            // if the mouse has just been clicked then we hide the cursor and store the position
            if (mouseDown)
            {
                Glut.glutSetCursor(Glut.GLUT_CURSOR_NONE);
                prevX = downX = x;
                prevY = downY = y;
            }
            else // unhide the cursor if the mouse has just been released
            {
                Glut.glutSetCursor(Glut.GLUT_CURSOR_LEFT_ARROW);
                Glut.glutWarpPointer(downX, downY);
            }
        }

        private static void OnMove(int x, int y)
        {
            // if the mouse move event is caused by glutWarpPointer then do nothing
            if (x == prevX && y == prevY) return;

            // move the camera when the mouse is down
            if (mouseDown)
            {
                float yaw = (prevX - x) * 0.002f;
                camera.Yaw(yaw);

                float pitch = (prevY - y) * 0.002f;
                camera.Pitch(pitch);

                prevX = x;
                prevY = y;
            }

            if (x < 0) Glut.glutWarpPointer(prevX = width, y);
            else if (x > width) Glut.glutWarpPointer(prevX = 0, y);

            if (y < 0) Glut.glutWarpPointer(x, prevY = height);
            else if (y > height) Glut.glutWarpPointer(x, prevY = 0);
        }

        private static void OnRenderFrame()
        {
            watch.Stop();
            float deltaTime = (float)watch.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency;
            watch.Restart();

            // update our camera by moving it up to 5 units per second in each direction
            if (down) camera.MoveRelative(Vector3.UnitZ * deltaTime * 5);
            if (up) camera.MoveRelative(-Vector3.UnitZ * deltaTime * 5);
            if (left) camera.MoveRelative(-Vector3.UnitX * deltaTime * 5);
            if (right) camera.MoveRelative(Vector3.UnitX * deltaTime * 5);
            if (space) camera.MoveRelative(Vector3.UnitY * deltaTime * 3);

            // set up the viewport and clear the previous depth and color buffers
            Gl.Viewport(0, 0, width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // make sure the shader program and texture are being used
            Gl.UseProgram(program);
           
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(brickDiffuse);

            // apply our camera view matrix to the shader view matrix (this can be used for all objects in the scene)
            program["view_matrix"].SetValue(camera.ViewMatrix);

            // set up the model matrix and draw the cube
            program["model_matrix"].SetValue(Matrix4.Identity);
           
            Gl.BindBufferToShaderAttribute(cube, program, "vertexPosition");

            Gl.BindBufferToShaderAttribute(cubeUV, program, "vertexUV");
            Gl.BindBuffer(cubeTriangles);

            Gl.DrawElements(BeginMode.Quads, cubeTriangles.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            // bind the font program as well as the font texture
            Gl.UseProgram(fontProgram.ProgramID);
            Gl.BindTexture(font.FontTexture);

            // draw the tutorial information, which is static
            information.Draw();

            Glut.glutSwapBuffers();
        }

        private static void OnReshape(int width, int height)
        {
            Program.width = width;
            Program.height = height;

            Gl.UseProgram(program.ProgramID);
            program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)width / height, 0.1f, 1000f));

            Gl.UseProgram(fontProgram.ProgramID);
            fontProgram["ortho_matrix"].SetValue(Matrix4.CreateOrthographic(width, height, 0, 1000));

            information.Position = new Vector2(-width / 2 + 10, height / 2 - font.Height - 10);
        }

        private static void OnKeyboardDown(byte key, int x, int y)
        {
            //if (key == 'w') up = true;
            //else if (key == 's') down = true;
            //else if (key == 'd') right = true;
            //else if (key == 'a') left = true;
            //else 
            if (key == ' ') space = true;
            else if (key == 27) Glut.glutLeaveMainLoop();
        }

        private static void OnKeyboardUp(byte key, int x, int y)
        {
            //if (key == 'w') up = false;
            //else if (key == 's') down = false;
            //else if (key == 'd') right = false;
            //else if (key == 'a') left = false;
            //else
            if (key == ' ') space = false;
            
            else if (key == 'f')
            {
                fullscreen = !fullscreen;
                if (fullscreen) Glut.glutFullScreen();
                else
                {
                    Glut.glutPositionWindow(0, 0);
                    Glut.glutReshapeWindow(1280, 720);
                }
            }
        }


        public static string VertexShader = @"
#version 130

in vec3 vertexPosition;
in vec3 vertexNormal;
in vec3 vertexTangent;
in vec2 vertexUV;

uniform vec3 light_direction;

out vec3 normal;
out vec2 uv;
out vec3 light;

uniform mat4 projection_matrix;
uniform mat4 view_matrix;
uniform mat4 model_matrix;
uniform bool enable_mapping;

void main(void)
{
    normal = normalize((model_matrix * vec4(floor(vertexNormal), 0)).xyz);
    uv = vertexUV;

    mat3 tbnMatrix = mat3(vertexTangent, cross(vertexTangent, normal), normal);
    light = (enable_mapping ? light_direction * tbnMatrix : light_direction);

    gl_Position = projection_matrix * view_matrix * model_matrix * vec4(vertexPosition, 1);
}
";

        public static string FragmentShader = @"
#version 130

uniform sampler2D colorTexture;
uniform sampler2D normalTexture;

uniform bool enable_lighting;
uniform mat4 model_matrix;
uniform bool enable_mapping;

in vec3 normal;
in vec2 uv;
in vec3 light;

out vec4 fragment;

void main(void)
{
    vec3 fragmentNormal = texture2D(normalTexture, uv).xyz * 2 - 1;
    vec3 selectedNormal = (enable_mapping ? fragmentNormal : normal);
    float diffuse = max(dot(selectedNormal, light), 0);
    float ambient = 0.3;
    float lighting = (enable_lighting ? max(diffuse, ambient) : 1);

    fragment = vec4(lighting * texture2D(colorTexture, uv).xyz, 1);
}
";
    }
}
