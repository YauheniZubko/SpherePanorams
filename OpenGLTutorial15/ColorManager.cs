using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGLTutorial5
{
    public static class ColorManager
    {
        public static VBO<Vector3> GetRandomColors(int count)
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            return new VBO<Vector3>(Enumerable.Range(1, count).Select(r => new Vector3(rnd.Next(0, 100) / 100.0, rnd.Next(0, 100) / 100.0, rnd.Next(0, 100) / 100.0)).ToArray());
        }
    }
}
