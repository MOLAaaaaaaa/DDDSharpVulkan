using System;
using System.Collections.Generic;
using System.Linq;

namespace DataCollection
{
    /// <summary>
    /// 多边形平滑与等分工具类
    /// </summary>
    public static class PolygonProcessor
    {   
        /// <summary>
        /// 对多边形进行平滑（二次贝塞尔曲线插值）
        /// </summary>
        public static List<Vector64> SmoothPolygon(List<Vector64> originalPoints, int smoothSegments)
        {
            var smoothed = new List<Vector64>();
            int pointCount = originalPoints.Count;

            for (int i = 0; i < pointCount; i++)
            {
                // 获取当前点、前一个点、后一个点（闭合处理）
                Vector64 prev = originalPoints[(i - 1 + pointCount) % pointCount];
                Vector64 curr = originalPoints[i];
                Vector64 next = originalPoints[(i + 1) % pointCount];

                // 计算贝塞尔控制点（基于前后点的中点）
                Vector64 control1 = new Vector64((prev.X + curr.X) / 2, (prev.Y + curr.Y) / 2,0);
                Vector64 control2 = new Vector64((curr.X + next.X) / 2, (curr.Y + next.Y) / 2,0);

                // 插值生成平滑段
                for (int j = 0; j <= smoothSegments; j++)
                {
                    double t = j / (double)smoothSegments;
                    // 二次贝塞尔曲线公式
                    double x = (1 - t) * (1 - t) * control1.X + 2 * (1 - t) * t * curr.X + t * t * control2.X;
                    double y = (1 - t) * (1 - t) * control1.Y + 2 * (1 - t) * t * curr.Y + t * t * control2.Y;
                    smoothed.Add(new Vector64(x, y,0));
                }
            }

            return smoothed;
        }

        /// <summary>
        /// 计算轮廓总长度
        /// </summary>
        private static double CalculateContourLength(List<Vector64> contour)
        {
            double total = 0;
            for (int i = 0; i < contour.Count - 1; i++)
            {
                total += contour[i].Distance(contour[i + 1]);
            }
            return total;
        }

        /// <summary>
        /// 按步长采样轮廓点
        /// </summary>
        private static List<Vector64> SampleContourByStep(List<Vector64> contour, double step, int divisions)
        {
            var sampled = new List<Vector64>();
            double currentDistance = 0;
            double targetDistance = 0;

            sampled.Add(contour[0]); // 第一个点
            targetDistance += step;

            for (int i = 0; i < contour.Count - 1; i++)
            {
                Vector64 p1 = contour[i];
                Vector64 p2 = contour[i + 1];
                double segmentLength = p1.Distance(p2);

                // 当前段长度不足目标距离，累加后跳过
                if (currentDistance + segmentLength < targetDistance)
                {
                    currentDistance += segmentLength;
                    continue;
                }

                // 计算在当前段的插值位置
                double remaining = targetDistance - currentDistance;
                double t = remaining / segmentLength;
                double x = p1.X + t * (p2.X - p1.X);
                double y = p1.Y + t * (p2.Y - p1.Y);

                sampled.Add(new Vector64(x, y,0));
                targetDistance += step;
                currentDistance += remaining;

                // 达到等分数量后停止
                if (sampled.Count >= divisions)
                    break;
            }

            // 确保最后一个点闭合（补充到原始终点）
            if (sampled.Count < divisions)
            {
                sampled.Add(contour.Last());
            }

            return sampled;
        }
    }
}