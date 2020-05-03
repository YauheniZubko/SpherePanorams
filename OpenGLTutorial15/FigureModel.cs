using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;

namespace OpenGLTutorial5
{
    public class FigureModel:IDisposable
    {
        public FigureModel(VBO<Vector3> coordinates, VBO<uint> polygons, VBO<Vector3> colors=null, bool generateColorsRandomly = true)
        {
            Coordinates = coordinates;

            if (generateColorsRandomly)
            {
                Colors = ColorManager.GetRandomColors(coordinates.Count);
            }
            else if(colors!=null)
            {
                Colors = colors;
            }
            else
            {
                throw new ArgumentException("Colors of figure are not initialized");
            }

            Polygons = polygons;
        }

        public VBO<Vector3> Coordinates { get; }
        public VBO<Vector3> Colors { get; }
        public VBO<uint> Polygons { get; }

        public VBO<Vector2> Textures { get; set; }

        public void Dispose()
        {
            Coordinates?.Dispose();
            Colors?.Dispose();
            Polygons?.Dispose();
            Textures?.Dispose();
        }

        public void BindBuffer(ShaderProgram program, BeginMode beginMode)
        {
            
            Gl.BindBufferToShaderAttribute(Coordinates, program, "vertexPosition");
            //Gl.BindBufferToShaderAttribute(Colors, program, "vertexColor");
            Gl.BindBufferToShaderAttribute(Textures, program, "vertexUV");
            Gl.BindBuffer(Polygons);

            Gl.DrawElements(beginMode, Polygons.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }
    }
}
