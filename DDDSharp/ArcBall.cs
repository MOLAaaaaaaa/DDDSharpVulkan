using System;
using System.Drawing;
using System.Windows.Forms;
namespace ArcTrackball
{
    /*
     * This code is a part of NeHe tutorials and was converted from C++ into C# by Jian. 
     * Matrix/Vector manipulations have been updated.
     * last modified: 2017.4.25
     */
    public class Trackball
    {
        public bool horDragInverse { set; get; }
        public bool verDragInverse { set; get; }
        private const float Epsilon = 1.0e-5f;

        private Vector3f StVec; //Saved click vector
        private Vector3f EnVec; //Saved drag vector
        private float adjustWidth; //Mouse bounds width
        private float adjustHeight; //Mouse bounds height

        public Trackball(float NewWidth, float NewHeight)
        {
            horDragInverse = false;
            verDragInverse = false;
            StVec = new Vector3f();
            EnVec = new Vector3f();
            setBounds(NewWidth, NewHeight);
        }

        private void mapToSphere(Point point, Vector3f vector)
        {
            Point2f tempPoint = new Point2f(point.X, point.Y);

            //Adjust point coords and scale down to range of [-1 ... 1]
            if(horDragInverse) tempPoint.x = 1.0f - (tempPoint.x * this.adjustWidth);
            else tempPoint.x = (tempPoint.x * this.adjustWidth) - 1.0f;

            if (verDragInverse) tempPoint.y = 1.0f - (tempPoint.y * this.adjustHeight);
            else tempPoint.y = (tempPoint.y * this.adjustHeight) - 1.0f;

            //Compute square of the length of the vector from this point to the center
            float length = (tempPoint.x * tempPoint.x) + (tempPoint.y * tempPoint.y);

            //If the point is mapped outside the sphere... (length > radius squared)
            if (length > 1.0f)
            {
                //Compute a normalizing factor (radius / sqrt(length))
                float norm = (float)(1.0 / Math.Sqrt(length));

                //Return the "normalized" vector, a point on the sphere
                vector.x = tempPoint.x * norm;
                vector.y = tempPoint.y * norm;
                vector.z = 0.0f;
            }
            //Else it's inside
            else
            {
                //Return a vector to a point mapped inside the sphere sqrt(radius squared - length)
                vector.x = tempPoint.x;
                vector.y = tempPoint.y;
                vector.z = (float)System.Math.Sqrt(1.0f - length);
            }
        }

        public void setBounds(float NewWidth, float NewHeight)
        {
            //Set adjustment factor for width/height
            adjustWidth = 1.0f / ((NewWidth - 1.0f) * 0.5f);
            adjustHeight = 1.0f / ((NewHeight - 1.0f) * 0.5f);
        }

        //Mouse down
        public virtual void click(Point NewPt)
        {
            mapToSphere(NewPt, this.StVec);
        }

        //Mouse drag, calculate rotation
        public void drag(Point NewPt, Quat4f NewRot)
        {
            //Map the point to the sphere
            this.mapToSphere(NewPt, EnVec);

            //Return the quaternion equivalent to the rotation
            if (NewRot != null)
            {
                Vector3f Perp = new Vector3f();

                //Compute the vector perpendicular to the begin and end vectors
                Vector3f.cross(Perp, StVec, EnVec);

                //Compute the length of the perpendicular vector
                if (Perp.length() > Epsilon)
                //if its non-zero
                {
                    //We're ok, so return the perpendicular vector as the transform after all
                    NewRot.x = Perp.x;
                    NewRot.y = Perp.y;
                    NewRot.z = Perp.z;
                    //In the quaternion values, w is cosine (theta / 2), where theta is the rotation angle
                    NewRot.w = Vector3f.dot(StVec, EnVec);
                }
                //if it is zero
                else
                {
                    //The begin and end vectors coincide, so return an identity transform
                    NewRot.x = NewRot.y = NewRot.z = NewRot.w = 0.0f;
                }
            }
        }
    }

    public class Matrix4f
    {
        public float[,] M;
        public float scl = 1.0f;
        public Vector3f pan = new Vector3f();

        public Matrix4f()
        {
            SetIdentity();
        }
        public void SetIdentity()
        {
            this.M = new float[4, 4]; // set to zero
            for (int i = 0; i <= 3; i++) this.M[i, i] = 1.0f;
        }
        public override int GetHashCode()
        {
            int code = (int)scl ^ 2 + (int)pan.x ^ 2 + (int)pan.y ^ 2 + (int)pan.z ^ 2;
            if(M!=null)
            {
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                    {
                        code += (int)M[i, j] ^ 2;
                    }
            }
            return code;
        }
        public Matrix4f Copy()
        {
            Matrix4f ret = new Matrix4f();
            ret.scl = scl;
            ret.pan = new Vector3f(pan.x, pan.y, pan.z);
            if (M != null)
            {
                ret.M = new float[4, 4];
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                    {
                        ret.M[i, j] = M[i, j];
                    }
            }
            return ret;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is Matrix4f))
            {
                return false;
            }
            Matrix4f p = (Matrix4f)obj;
            for(int i=0;i<4;i++)
                for (int j = 0; j < 4; j++)
                {
                    if (M[i, j] != p.M[i, j])
                        return false;
                }
            if (scl != p.scl) return false;
            return ((pan.x == p.pan.x) &&
                    (pan.y == p.pan.y) &&
                    (pan.z == p.pan.z) );

        }
        public static bool operator ==(Matrix4f p1, Matrix4f p2)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(p1, p2))
            {
                return true;
            }
            // If one is null, but not both, return false.
            if (((object)p1 == null) || ((object)p2 == null))
            {
                return false;
            }
            return p1.Equals(p2);
        }
        public static bool operator !=(Matrix4f p1, Matrix4f p2)
        {
            return (!(p1 == p2));
        }

        public void get_Renamed(float[] dest)
        {
            int k = 0;
            for (int i = 0; i <= 3; i++)
                for (int j = 0; j <= 3; j++)
                {
                    dest[k] = this.M[j, i];
                    k++;
                }
        }
        public void set_Renamed(Matrix4f m1)
        {
            M = m1.M;            
        }

        public void MatrixMultiply(Matrix4f m1, Matrix4f m2)
        {
            float[] MulMat = new float[16];
            float elMat = 0.0f;
            int k = 0;

            for (int i = 0; i <= 3; i++)
                for (int j = 0; j <= 3; j++)
                {
                    for (int l = 0; l <= 3; l++)
                        elMat += m1.M[i, l] * m2.M[l, j];

                    MulMat[k] = elMat;
                    elMat = 0.0f;
                    k++;
                }

            k = 0;
            for (int i = 0; i <= 3; i++)
                for (int j = 0; j <= 3; j++)
                {
                    m1.M[i, j] = MulMat[k];
                    k++;
                }
        }

        public Quat4f Rotation
        {
            set
            {
                float n, s;
                float xs, ys, zs;
                float wx, wy, wz;
                float xx, xy, xz;
                float yy, yz, zz;

                M = new float[4, 4];

                n = (value.x * value.x) + (value.y * value.y) + (value.z * value.z) + (value.w * value.w);
                s = (n > 0.0f) ? 2.0f / n : 0.0f;

                xs = value.x * s;
                ys = value.y * s;
                zs = value.z * s;
                wx = value.w * xs;
                wy = value.w * ys;
                wz = value.w * zs;
                xx = value.x * xs;
                xy = value.x * ys;
                xz = value.x * zs;
                yy = value.y * ys;
                yz = value.y * zs;
                zz = value.z * zs;

                // rotation
                M[0, 0] = 1.0f - (yy + zz);
                M[0, 1] = xy - wz;
                M[0, 2] = xz + wy;

                M[1, 0] = xy + wz;
                M[1, 1] = 1.0f - (xx + zz);
                M[1, 2] = yz - wx;

                M[2, 0] = xz - wy;
                M[2, 1] = yz + wx;
                M[2, 2] = 1.0f - (xx + yy);

                M[3, 3] = 1.0f;

                // translation (pan)
                M[0, 3] = pan.x;
                M[1, 3] = pan.y;

                // scale (zoom)
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        M[i, j] *= scl;
            }
        }

        public float Scale
        {
            set { scl = value; }
        }

        public Vector3f Pan
        {
            set { pan = value; }
        }

    }

    public class Point2f
    {
        public float x, y;

        public Point2f(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class Quat4f
    {
        public float x, y, z, w;
        public Quat4f()
        {
            x = y = z = w = 0;
        }
        public Quat4f(float _x, float _y, float _z, float _w)
        {
            x = _x;
            y = _y;
            z = _z;
            w = _w;
        }
        public Quat4f Copy(Quat4f p)
        {
            return new Quat4f(p.x, p.y, p.z, p.w);
        }
    }

    public class Vector3f
    {
        public float x, y, z;

        /// <summary>
        /// Constructor
        /// </summary>
        public Vector3f()
        { }

        /// <summary>
        /// Constructor - overload 1
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        public Vector3f(float X, float Y, float Z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        /// <summary>
        /// Cross Product of Two Vectors.
        /// </summary>
        /// <param name="Result">Resultant Vector</param>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        public static void cross(Vector3f Result, Vector3f v1, Vector3f v2)
        {
            Result.x = (v1.y * v2.z) - (v1.z * v2.y);
            Result.y = (v1.z * v2.x) - (v1.x * v2.z);
            Result.z = (v1.x * v2.y) - (v1.y * v2.x);
        }

        /// <summary>
        /// Dot Product of Two Vectors.
        /// </summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns></returns>
        public static float dot(Vector3f v1, Vector3f v2)
        {
            return (v1.x * v2.x) + (v1.y * v2.y) + (v1.z + v2.z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public float length()
        {
            return (float)System.Math.Sqrt(x * x + y * y + z * z);
        }
    }
}
