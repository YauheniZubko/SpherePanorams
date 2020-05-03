using OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGLTutorial5
{
    public struct SphericVector
    {
        public float R { get; set; }
        public float Fi { get; set; }
        public float Teta { get; set; }

        public SphericVector(float r, float fi, float teta)
        {
            R = r;
            Fi = fi;
            Teta = teta;
        }

    }

    public struct VcoordVector
    {
        public float U { get; set; }
        public float V { get; set; }

        public VcoordVector(float u, float v)
        {
            U = u;
            V = v;
        }
    }
    static class CoordinatesManager
    {
        public static VBO<Vector3> GetPyramidCoordinates()
        {
            return new VBO<Vector3>(new Vector3[] {
                new Vector3(0, 1, 0), new Vector3(-1, -1, 1), new Vector3(1, -1, 1),        // front face
                new Vector3(0, 1, 0), new Vector3(1, -1, 1), new Vector3(1, -1, -1),        // right face
                new Vector3(0, 1, 0), new Vector3(1, -1, -1), new Vector3(-1, -1, -1),      // back face
                new Vector3(0, 1, 0), new Vector3(-1, -1, -1), new Vector3(-1, -1, 1) });   // left face
        }

        public static VBO<Vector3> GetCubeCoordinates()
        {
            return new VBO<Vector3>(new Vector3[] {
                new Vector3(1, 1, -1), new Vector3(-1, 1, -1), new Vector3(-1, 1, 1), new Vector3(1, 1, 1),
                new Vector3(1, -1, 1), new Vector3(-1, -1, 1), new Vector3(-1, -1, -1), new Vector3(1, -1, -1),
                new Vector3(1, 1, 1), new Vector3(-1, 1, 1), new Vector3(-1, -1, 1), new Vector3(1, -1, 1),
                new Vector3(1, -1, -1), new Vector3(-1, -1, -1), new Vector3(-1, 1, -1), new Vector3(1, 1, -1),
                new Vector3(-1, 1, 1), new Vector3(-1, 1, -1), new Vector3(-1, -1, -1), new Vector3(-1, -1, 1),
                new Vector3(1, 1, -1), new Vector3(1, 1, 1), new Vector3(1, -1, 1), new Vector3(1, -1, -1) });
        }

        
        public static VBO<Vector2> GetSphereUVCoordinates()
        {

            var xyz = GetSphereCoordinates().ToArray();

            var res = xyz.Select(t =>
                new Vector3(
                    t.X / radius, t.Y / radius, t.Z / radius
                )).ToArray();

            var xyzForFixUv = invalid.Select(t =>
                new Vector3(
                    t.X / radius, t.Y / radius, t.Z / radius
                )).ToArray();


            var fixedUCoord = xyzForFixUv.Select(t => Math.Atan2(t.X, t.Z) / (2 * Math.PI) + 0.5).ToArray();
            
            for (int i = 0; i < fixedUCoord.Length; i += 4)
            {
                if (Math.Abs(fixedUCoord[i] - fixedUCoord[i + 1]) > 0.4)
                {
                    fixedUCoord[i] = fixedUCoord[i] < 0.5 ? 1 + fixedUCoord[i] : fixedUCoord[i];
                    fixedUCoord[i + 1] = fixedUCoord[i + 1] < 0.5 ? 1 + fixedUCoord[i + 1] : fixedUCoord[i + 1];
                    fixedUCoord[i + 2] = fixedUCoord[i + 2] < 0.5 ? 1 + fixedUCoord[i + 2] : fixedUCoord[i + 2];
                    fixedUCoord[i + 3] = fixedUCoord[i + 3] < 0.5 ? 1 + fixedUCoord[i + 3] : fixedUCoord[i + 3];
                }
            }

            var Ucoord = res.Select(t => Math.Atan2(t.X, t.Z) / (2 * Math.PI) + 0.5).ToArray();
            var Vcoord = res.Select(t => t.Y * 0.5 + 0.5).ToArray();

            for (int i = 0; i < fixedIndexes.Count; i++)
            {
                Ucoord[fixedIndexes[i]] = fixedUCoord[i];
            }

            for (int i = 0; i < Ucoord.Length; i++)
            {
                if (i >= sectorCount * stackCount * 4 - 8 + (fixCenterLeftBorder + 1) * 4 && i <= sectorCount * stackCount * 4 - 5 + (fixCenterRightBorder - 1) * 4)//тут 9 и 23 особые числа, 
                {
                    if (Ucoord[i] > 0.8)
                    {
                        Ucoord[i] = 1 - Ucoord[i];
                    }
                }
            }

            Vector2[] v = new Vector2[res.Length];
            for (int i = 0; i < Ucoord.Length; i++)
            {
                v[i] = new Vector2(Ucoord[i], Vcoord[i]);
            }

            return new VBO<Vector2>(v);
        }
        static List<int> fixedIndexes = new List<int>();
        static List<Vector3> invalid = new List<Vector3>();
        static float radius = 5.0f;
        static int qualityCount = 30;
        static int sectorCount = qualityCount;// по вертикали x кусков будет
        static int stackCount = qualityCount;// по горизонтали x кусков будет

        static int fixCenterLeftBorder = qualityCount / 4 + 1;
        static int fixCenterRightBorder = qualityCount - fixCenterLeftBorder + 1;

        public static IEnumerable<Vector3> GetSphereCoordinates()
        {
            var points = new List<Vector3>();

            float x, y, z, xy;                              // vertex position
            float sectorStep = (float)(2 * Math.PI / sectorCount);
            float stackStep = (float)Math.PI / stackCount;
            float sectorAngle, stackAngle;


            for (int i = 0; i <= stackCount; ++i)
            {
                stackAngle = (float)Math.PI / 2 - i * stackStep;        // starting from pi/2 to -pi/2
                xy = radius * (float)Math.Cos(stackAngle);             // r * cos(u)
                z = radius * (float)Math.Sin(stackAngle);              // r * sin(u)

                // add (sectorCount+1) vertices per stack
                // the first and last vertices have same position and normal, but different tex coords
                for (int j = 0; j <= sectorCount; ++j)
                {
                    sectorAngle = j * sectorStep;           // starting from 0 to 2pi

                    // vertex position (x, y, z)
                    x = xy * (float)Math.Cos(sectorAngle);             // r * cos(u) * cos(v)
                    y = xy * (float)Math.Sin(sectorAngle);             // r * cos(u) * sin(v)
                    if (Math.Abs(z) < 0.000001)
                    {
                        z = 0;
                    }
                    if (Math.Abs(x) < 0.000001)
                    {
                        x = 0;
                    }
                    if (Math.Abs(y) < 0.000001)
                    {
                        y = 0;
                    }
                    points.Add(new Vector3(x, y, z));
                    points = points.Distinct().ToList();
                }
            }

            var points1 = new List<Vector3>();
           
            for (int i = 0; i < stackCount; ++i)
            {
                if (i == 0)
                {

                    points1.AddRange(new[] { points[0], points[0], points[sectorCount], points[1] });
                    for (int j = 1; j < sectorCount; j++)
                    {
                        if (j == fixCenterRightBorder || j == fixCenterLeftBorder)
                        {
                            fixedIndexes.AddRange(new[] { points1.Count, points1.Count + 1, points1.Count + 2, points1.Count + 3 });
                            invalid.AddRange(new[] { points[0], points[0], points[j], points[j + 1] });
                        }
                        points1.AddRange(new[] { points[0], points[0], points[j], points[j + 1] });
                    }

                }
                else if (i == stackCount - 1)
                {


                    points1.AddRange(new[] { points[(i) * sectorCount], points[(i - 1) * sectorCount + 1], points[points.Count - 1], points[points.Count - 1] });
                    for (int j = 1; j < sectorCount; ++j)
                    {
                        if (j == fixCenterRightBorder || j == fixCenterLeftBorder)
                        {

                            fixedIndexes.AddRange(new[] { points1.Count, points1.Count + 1, points1.Count + 2, points1.Count + 3 });
                            invalid.AddRange(new[] { points[(i - 1) * sectorCount + j], points[(i - 1) * sectorCount + j + 1], points[points.Count - 1], points[points.Count - 1] });
                        }
                        points1.AddRange(new[] { points[(i - 1) * sectorCount + j], points[(i - 1) * sectorCount + j + 1], points[points.Count - 1], points[points.Count - 1] });

                    }
                }
                else
                {
                    points1.AddRange(new[] { points[(i) * sectorCount], points[(i - 1) * sectorCount + 1], points[(i) * sectorCount + 1], points[(i + 1) * sectorCount] });
                    for (int j = 1; j < sectorCount; ++j)
                    {
                        if (j == fixCenterRightBorder || j == fixCenterLeftBorder)
                        {

                            fixedIndexes.AddRange(new[] { points1.Count, points1.Count + 1, points1.Count + 2, points1.Count + 3 });
                            invalid.AddRange(new[] { points[(i - 1) * sectorCount + j], points[(i - 1) * sectorCount + j + 1], points[(i) * sectorCount + j + 1], points[(i) * sectorCount + j] });
                        }
                        points1.AddRange(new[] { points[(i - 1) * sectorCount + j], points[(i - 1) * sectorCount + j + 1], points[(i) * sectorCount + j + 1], points[(i) * sectorCount + j] });
                    }
                    points1.AddRange(new[] { points[(i) * sectorCount], points[(i - 1) * sectorCount + 1], points[(i) * sectorCount + 1], points[(i + 1) * sectorCount] });
                }
            }
            return points1;
        }
    }
}
