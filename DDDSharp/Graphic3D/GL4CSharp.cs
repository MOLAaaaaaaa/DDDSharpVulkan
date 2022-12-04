using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCollection;
//using bingding  based on Giawa.OpenGL Package
//support windows,IOS and Linux
using GlmNet;
namespace Graphics3D
{
    class COpenGLModel : CModel
    {
        public float[] points = null;
        public float[] normals = null;
        public float[] colors = null;
        public int[] indices = null;
        public DrawParameter drawParameter = new DrawParameter(true);
        public COpenGLModel()
        {
            Release();
        }
        public override void Release()
        {
            points = null;
            normals = null;
            colors = null;
            indices = null;
        }
        public bool AddIndices(int[] _indices)
        {
            if (_indices.Length < 1) return false;
            indices = new int[_indices.Length];
            if (indices == null) return false;
            Array.Copy(_indices, indices, _indices.Length);
            return true;
        }
        public bool AddIndices(List<int>_indices)
        {
            if (_indices.Count < 1) return false;
            indices = _indices.ToArray();            
            return true;
        }
        public bool AddPoints(Vertex3D[]_points)
        {
            if (_points.Length < 1) return false;
            points = new float[_points.Length*3];            
            if (points == null) return false;
            colors = new float[_points.Length * 4];
            if (colors == null) return false;
            for (int i=0;i<_points.Length;i++)
            {
                points[3 * i] = _points[i].pos.x;
                points[3 * i+1] = _points[i].pos.y;
                points[3 * i+2] = _points[i].pos.z;
                colors[4 * i] = _points[i].color.x;
                colors[4 * i+1] = _points[i].color.y;
                colors[4 * i+2] = _points[i].color.z;
                colors[4 * i+3] = _points[i].color.w;
            }
            return true;
        }
        public bool AddPoints(List<Vertex3D> _points)
        {
            if (_points.Count < 1) return false;
            points = new float[_points.Count * 3];
            if (points == null) return false;
            colors = new float[_points.Count * 4];
            if (colors == null) return false;
            for (int i = 0; i < _points.Count; i++)
            {
                points[3 * i] = _points[i].pos.x;
                points[3 * i + 1] = _points[i].pos.y;
                points[3 * i + 2] = _points[i].pos.z;
                colors[4 * i] = _points[i].color.x;
                colors[4 * i + 1] = _points[i].color.y;
                colors[4 * i + 2] = _points[i].color.z;
                colors[4 * i + 3] = _points[i].color.w;
            }
            return true;
        }
        public override bool UpdateUniformBuffer()
        {
            //scale ->rotate ->translate            
            UniformBufferObject ubo = m_modelMatrix.Copy();
            /*
           // GLES20.PushMatrix();

                ubo.model = glm.scale(ubo.model, m_scale);
                GLES20.Scale(m_scale.x, m_scale.y, m_scale.z);
            
            
                ubo.model = glm.rotate(ubo.model, glm.radians(m_rotate.x), new vec3(1, 0, 0));
                GLES20.Rotate(m_rotate.x,1,0,0);
                GLES20.Rotate(m_rotate.y, 0, 1, 0);
                GLES20.Rotate(m_rotate.z, 0, 0, 1);
            
                ubo.model = glm.translate(ubo.model, m_translate);
                GLES20.Translate(m_translate.x, m_translate.y, m_translate.z);
            */
          //  GLES20.PopMatrix();

            return true;
        }
    }
    class GL4CSharp: CGraphic3D
    {
        //initial window
        private IntPtr windowHandle = IntPtr.Zero;  // = WinForm handle
        
        public override bool Initialize(IntPtr window, string title = "Opengl", int width = 800, int height = 600)
        {
            initialized = false;
            windowWidth = width;
            windowHeight = height;           
            
            engine = gEngine.opengl;
            return initialized;
        } 
       
    }//CGL
}