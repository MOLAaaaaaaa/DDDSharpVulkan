using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCollection
{
    public class PictureAlgorithm2D
    {
		double LimitValue(double val, double minv, double maxv)
		{
			if (val < minv) return minv;
			else if (val > maxv) return maxv;
			else return val;
		}

		public float[] Sobel(float[]grid2d,int width,int height,double minv,double maxv)
        {
			float[] imageKernel = new float[9];
			float[] outgrid = new float[grid2d.Length];

			for (int i = 1; i < height - 1; i++)
			{
				for (int j = 1; j < width - 1; j++)
				{					
					imageKernel[0] = grid2d[(i - 1) * width + j - 1];
					imageKernel[1] = grid2d[(i - 1) * width + j];
					imageKernel[2] = grid2d[(i - 1) * width + j + 1];
					imageKernel[3] = grid2d[(i) * width + j - 1];
					imageKernel[4] = grid2d[(i) * width + j];
					imageKernel[5] = grid2d[(i) * width + j + 1];
					imageKernel[6] = grid2d[(i + 1) * width + j - 1];
					imageKernel[7] = grid2d[(i + 1) * width + j];
					imageKernel[8] = grid2d[(i + 1) * width + j + 1];

					//化简后结果   这里使用了 1,1.414,1 的模板（各向同性Sobel算子），与 1,2,1的模板区别不是很大
					double GX = imageKernel[2] - imageKernel[0] + (imageKernel[5] - imageKernel[3]) * 1.414 + imageKernel[8] - imageKernel[6];
					double GY = imageKernel[0] + imageKernel[2] + (imageKernel[1] - imageKernel[7]) * 1.414 - imageKernel[6] - imageKernel[8];
					outgrid[i * width + j] = (float) LimitValue( Math.Sqrt(GX * GX + GY * GY) + 0.5, minv, maxv );
				}
			}
			imageKernel = null;
			return outgrid;
		}
    }
}
