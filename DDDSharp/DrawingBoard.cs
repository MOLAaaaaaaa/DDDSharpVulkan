using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDSharp
{
    /// <summary>
    /// 为绘图提供双缓冲[BufferedGraphics]。
    /// <para>提供建立多层画纸的方法并返回用于绘制的画纸[Graphics]。</para>
    /// </summary>
    public class DrawingBoard
    {
        private BufferedGraphicsContext currentGraphics;
        private List<BufferedGraphics> memGraphics; 
        /// <summary>返回缓冲的画纸层数</summary>
        public int Layers { get; private set; }
        private int Current = 0;
        /// <summary>返回Graphics，表示当前预备呈现的画纸</summary>
        public Graphics CurrentGraphics { get { return memGraphics[Current].Graphics; } }
        /// <summary>返回Graphics，表示指定序号的画纸</summary>
        public Graphics GetGraphics(int id) { return memGraphics[id].Graphics; }

        public Rectangle DrawRect { get; set; }
        public int Height { get { return DrawRect.Height; } }
        public int Width { get { return DrawRect.Width; } }

        /// <summary>
        /// 新建画板并关联到指定区域的画纸上
        /// </summary>
        /// <param name="g">关联的Graphic，通常为Form.CreateGraphics</param>
        /// <param name="rect">关联的矩形区域，作为画板的工作区</param>
        /// <param name="层数">画板内含的缓冲区个数，不建议大于4个，默认为2个</param>
        public DrawingBoard(Graphics g, Rectangle rect, int layers = 2)
        {
            memGraphics = new List<BufferedGraphics>();
            currentGraphics = BufferedGraphicsManager.Current;
            DrawRect = rect;
            Layers = layers;
            for (int i = 1; i <= Layers; i++)
            {
                BufferedGraphics tg;
                tg = currentGraphics.Allocate(g, DrawRect);
                memGraphics.Add(tg);
            }
            Current = 0;
        }
        /// <summary>将呈现当前画纸，并使得当前序号递增一位，到尾部时跳回第一张画纸循环</summary>
        public void Display()
        {
            Current++;
            if (Current > Layers - 1) { Current = 0; }
            memGraphics[Current].Render();
        }
    }
}
