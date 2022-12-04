using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.IO;
using DataCollection;

namespace Graphics3D
{  
    /// <summary>
    /// Texture load class
    /// </summary>
    public class CTextureCoord : IDisposable
    {
        public float[] pTextCoords;
        public int nCoordNum;
        public CTextureCoord(float[] _Coods, int n)
        {
            pTextCoords = _Coods;
            nCoordNum = n;
        }
        public void Dispose()
        {
            pTextCoords = null;
            nCoordNum = 0;
        }
    }
    public struct TEXTURE_PARAMETER_INT
    {
        public int para1;
        public int para2;
    }
    public struct TEXTURE_PARAMETER_FLOAT
    {
        public int para1;
        public float para2;
    }
    public struct TEXTURE_ENV_INT
    {
        public int para1;
        public int para2;
    }
    public struct TEXTURE_ENV_FLOAT
    {
        public int para1;
        public float para2;
    }
    
    public class CTexture : IDisposable
    {
        public int[] textureID;
        public Bitmap image;
        public string errorString;
        public int Width, Height;

        public int nMaxTexture;
        public int maxTextureSize;
        public bool forcePowerOfTwo;
        private Bitmap[] imageArray;
        private CTextureCoord[] pTexCoordArray;
        private List<TEXTURE_PARAMETER_INT> pTextureParai;
        private List<TEXTURE_PARAMETER_FLOAT> pTextureParaf;
        private List<TEXTURE_ENV_INT> pTextureEnvi;
        private List<TEXTURE_ENV_FLOAT> pTextureEnvf;
        public CTexture()
        {
            nMaxTexture = GetMaxTexture();
            maxTextureSize = GetMaxTextureImgSize();
            textureID = new int[nMaxTexture];
            imageArray = new Bitmap[nMaxTexture];
            pTexCoordArray = new CTextureCoord[nMaxTexture];
            Width = Height = 0;
            for (int i = 0; i < nMaxTexture; i++)
            {
                textureID[i] = -1;
                imageArray[i] = null;
                pTexCoordArray[i] = null;
            }
            forcePowerOfTwo = true;

            pTextureParai = new List<TEXTURE_PARAMETER_INT>();
            pTextureParaf = new List<TEXTURE_PARAMETER_FLOAT>();
            pTextureEnvi = new List<TEXTURE_ENV_INT>();
            pTextureEnvf = new List<TEXTURE_ENV_FLOAT>();
        }
        //return current support max texture image size
        public virtual int GetMaxTextureImgSize()
        {
            /*
            // find out the max texture size this system will hold.
            Gl.glGetIntegerv(Gl.GL_MAX_TEXTURE_SIZE, out maxTextureSize);
            //max texture number            
            */

            return 1024;
        }
        //return supported max Texture,default 8
        public virtual int GetMaxTexture()
        {
            /*
            Gl.glGetIntegerv(Gl.GL_MAX_TEXTURE_UNITS, out nMaxTexture);
            if (nMaxTexture <= 0) nMaxTexture = 8;
            */
            return 8;
        }
        public void SetPowerOfTwo(bool val)
        {
            forcePowerOfTwo = val;
        }
        public void ClearTexParameters()
        {
            pTextureParai.Clear();
            pTextureParaf.Clear();
            pTextureEnvi.Clear();
            pTextureEnvf.Clear();
        }
        public void glTexParameteri(int para1, int para2)
        {
            TEXTURE_PARAMETER_INT para = new TEXTURE_PARAMETER_INT();
            para.para1 = para1;
            para.para2 = para2;
            pTextureParai.Add(para);
        }
        public void glTexParameterf(int para1, int para2)
        {
            TEXTURE_PARAMETER_FLOAT para = new TEXTURE_PARAMETER_FLOAT();
            para.para1 = para1;
            para.para2 = para2;
            pTextureParaf.Add(para);
        }
        public void glTexEnvi(int para1, int para2)
        {
            TEXTURE_ENV_INT para = new TEXTURE_ENV_INT();
            para.para1 = para1;
            para.para2 = para2;
            pTextureEnvi.Add(para);
        }
        public void glTexEnvf(int para1, int para2)
        {
            TEXTURE_ENV_FLOAT para = new TEXTURE_ENV_FLOAT();
            para.para1 = para1;
            para.para2 = para2;
            pTextureEnvf.Add(para);
        }
        public string GetErrorString()
        {
            return errorString;
        }
        public void ClearTexture()
        {
            for (int i = 0; i < nMaxTexture; i++)
            {
                ClearTexture(i);
            }
            ClearTexParameters();
        }
        public virtual void ClearTexture(int id)
        {
            if (textureID[id] > 0)
            {
                var textureHandles = new int[1];
                textureHandles[0] = (int)textureID[id];
                // Gl.glDeleteTextures(1, textureHandles);
                textureID[id] = -1;
            }
            textureID[id] = -1;
            if (imageArray[id] != null) imageArray[id].Dispose();
            imageArray[id] = null;
            if (pTexCoordArray[id] != null) pTexCoordArray[id].Dispose();
            pTexCoordArray[id] = null;
        }
        public void BindTexCoord(float[] _texCoord, int pointNum, int textureid = 0)
        {
            // already added
            if (pTexCoordArray[textureid] != null) return;
            pTexCoordArray[textureid] = new CTextureCoord(_texCoord, pointNum);
        }
        public CTextureCoord GetTexCoordData(int i)
        {
            if (i >= nMaxTexture) return null;
            else return pTexCoordArray[i];
        }
        public int GetActivedTextureNum()
        {
            int n = 0;
            for (int i = 0; i < nMaxTexture; i++)
                if (textureID[i] > 0) n++;
            return n;
        }
        public bool LoadTexture(String fileName, int textureid = 0)
        {
            if (textureid < 0 || textureid >= nMaxTexture)
            {
                errorString = "texture number is between 0 to " + nMaxTexture.ToString();
                return false;
            }
            // already loaded
            if (imageArray[textureid] != null)
                return true;

            FileInfo file = new FileInfo(fileName);
            if (file.Exists == false)
            {
                errorString = "file ' " + fileName + " ' not exist.";
                return false;
            }
            try
            {
                if (file.Extension.ToUpper() == ".TGA")
                {
                    ///http://blog.csdn.net/zgke/article/details/4667499
                    //ImageTGA tga = new ImageTGA(fileName);
                    //image = tga.Image;
                }
                else
                {
                    image = new Bitmap(fileName);
                    if (image != null)
                    {
                        Width = image.Width;
                        Height = image.Height;
                        image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        //image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        imageArray[textureid] = CreateCompitableBitmap(image);
                    }
                }
            }
            catch (System.ArgumentException)
            {
                errorString = "loading file ' " + fileName + " ' failed.";
                return false;
            }
            return true;
        }
        public Bitmap SelectBitmap(int id)
        {
            if (id < 0 || id >= nMaxTexture)
            {
                errorString = "texture number is between 0 to 8.";
                return null;
            }

            if (imageArray[id] == null)
                errorString = "no texture exist.";

            return imageArray[id];
        }
        private int next_p2(int a)
        {
            int rval = 1;

            while (rval < a)
                rval = rval << 1;

            int leftMargin = a - (rval >> 1);

            // very close the previous boundary,then select it
            // if the left margin <= 1/4 width            
            if (leftMargin <= (rval >> 3))
                rval = rval >> 1;

            return rval;
        }
        PixelFormat GetBitmapPixelFormat(Bitmap bit)
        {
            PixelFormat format = bit.PixelFormat;
            /*
            switch (format)
            {
                case PixelFormat.Format24bppRgb:
                    return Gl.GL_RGB8;
                case PixelFormat.Format32bppArgb:
                    return Gl.GL_RGBA;
                case PixelFormat.Format32bppPArgb:
                    return Gl.GL_RGBA;
                default: return Gl.GL_RGB8;
            }
            */
            return format;
        }
        //create Compitable with formate PixelFormat.Format32bppArgb
        public Bitmap CreateCompitableBitmap(Bitmap bit,bool forceoftwo = false)
        {
            int width = bit.Width;
            int height = bit.Height;
            if ( forceoftwo )
            {
                Size size = GetForceOfTwoDimension(bit);
                width = size.Width;
                height = size.Height;
            }
                        
            Bitmap newbmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(newbmp))
            {
                var textsize = new Rectangle(new Point(0, 0), new Size(width, height));
                var imageRect = new Rectangle(new Point(0, 0), bit.Size);
                g.DrawImage(bit, textsize, imageRect, GraphicsUnit.Pixel);
            }
            
            return newbmp;
        }
        /// <summary>
        /// check the image is meet force of two requirment
        /// </summary>
        /// <param name="bit"></param>
        /// <returns>Yes / No </returns>
        public bool IsForceOfTwoImage(Bitmap bit)
        {
            Size size = GetForceOfTwoDimension(bit);
            return size.Width == bit.Width && size.Height == bit.Height;
        }
        public Size GetForceOfTwoDimension(Bitmap bit)
        {
            int newWidth = Math.Min(maxTextureSize, bit.Width);
            int newHeight = Math.Min(maxTextureSize, bit.Height);
            newWidth = next_p2(newWidth);
            newHeight = next_p2(newHeight);
            return new Size(newWidth, newHeight);            
        }
        public void ReleaseTexture(int textureid = 0)
        {
            ClearTexture(textureid);
        }
        public virtual bool BindTexture(int textureid = 0, Bitmap _img = null)
        {
            /*
            if (textureid < 0 || textureid >= nMaxTexture)
            {
                errorString = "texture number is between 0 to 8.";
                return false;
            }
            if (textureID[textureid] > 0)
            {
                Gl.glActiveTexture(Gl.GL_TEXTURE0_ARB + textureid);
                Gl.glBindTexture(Gl.GL_TEXTURE_2D, textureID[textureid]);
                return true;
            }
            if (_img != null) image = CreateCompitableBitmap(_img);
            else image = imageArray[textureid];
            if (image == null)
            {
                errorString = "please load image file first.";
                return false;
            }

            // the first binding
            BitmapData bitmapdata;
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            bitmapdata = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            Gl.glActiveTexture(Gl.GL_TEXTURE0_ARB + textureid);
            Gl.glGenTextures(1, out textureID[textureid]);
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, textureID[textureid]);

            // set teture parameters
            for (int i = 0; i < pTextureParai.Count; i++)
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, pTextureParai[i].para1, pTextureParai[i].para2);

            for (int i = 0; i < pTextureParaf.Count; i++)
                Gl.glTexParameterf(Gl.GL_TEXTURE_2D, pTextureParaf[i].para1, pTextureParaf[i].para2);

            for (int i = 0; i < pTextureEnvi.Count; i++)
                Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, pTextureEnvi[i].para1, pTextureEnvi[i].para2);

            for (int i = 0; i < pTextureEnvf.Count; i++)
                Gl.glTexEnvf(Gl.GL_TEXTURE_ENV, pTextureEnvf[i].para1, pTextureEnvf[i].para2);
                                   
            Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, GetBitmapPixelFormat(image), image.Width, image.Height, 0, Gl.GL_BGR, Gl.GL_UNSIGNED_BYTE, bitmapdata.Scan0);
            image.UnlockBits(bitmapdata);
            int success = Gl.glGetError();
            if (success != 0)
            {
                errorString = Glu.gluErrorString(success);
                return false;
            }
            */
            return true;
        }
        public void FreeImage()
        {
            if (image != null)
            {
                image.Dispose();
            }
            for (int i = 0; i < nMaxTexture; i++)
            {
                if (imageArray[i] != null)
                    imageArray[i].Dispose();
                if (pTexCoordArray[i] != null)
                    pTexCoordArray[i].Dispose();
            }
        }
        public void Dispose()
        {
            FreeImage();
        }
    }
    ////////////////////////////////
}
