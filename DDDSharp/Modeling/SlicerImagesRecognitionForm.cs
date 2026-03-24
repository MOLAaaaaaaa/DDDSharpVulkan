using DataCollection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition.Primitives;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Khronos.Platform;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace DDDSharp.Modeling
{
    public partial class SlicerImagesRecognitionForm : Form
    {
        List<PolygonSlicer> loadedSlicers = new List<PolygonSlicer>();
        List<PolygonSlicer> Slicers = new List<PolygonSlicer>();
        int SampleNX = 400, SampleNY = 400;
        double minx, miny, minz, minv;
        double maxx, maxy, maxz, maxv;
        double SlicerX1, SlicerY1, SlicerX2, SlicerY2;
        int UpdownSelection = -1;
        List<Vector32> Points = new List<Vector32>();
        void UpdateSlicerImageRangeInfo()
        {
            try
            {
                int sel = listBox1.SelectedIndex;
                if (sel < 0) return;
                PolygonSlicer slicer = Slicers[sel];
                ImageStruct im = slicer.backImages[0];
                double width = im.rect.Width;
                double height = im.rect.Height;
                double xstep = double.Parse(ColorSampleStepXText.Text);
                double ystep = double.Parse(ColorSampleStepYText.Text);
                int nx = (int)(width / xstep);
                int ny = (int)(height / ystep);
                SlicerImageRangeLable.Text = "Width = " + Math.Round(width,2) + " Height = " + Math.Round(height,2);
                SlicerImageRangeLable.Text += Environment.NewLine + "Estimated Points:" + nx * ny;
            }
            catch
            {

            }
        }
        
        int[,] SampleSlicer(PolygonSlicer slicer,double xstep,double ystep)
        {
            ImageStruct im = slicer.backImages[0];
            //图像RECT范围
            double x1 = im.rect.X1;
            double x2 = im.rect.X2;
            double y1 = im.rect.Y1;
            double y2 = im.rect.Y2;            
            int nx = (int)((x2 - x1) / xstep + 0.1);
            int ny = (int)((y2 - y1) / ystep + 0.1);                        
            return C3DData.Stratums.SamplingFromImageByColor(im.bmp, nx, ny);
        }
        void ResamplePoints(PolygonSlicer slicer,
                            int[,] RecognizedLayers,
                            bool[,] exports,
                            int layerid = -1, //全部地层
                            bool boder = false,        //是否采样边界点
                            bool boderouter = false,   //是否采样边界外围点                          
                            bool gridsample = false, //背景地层均匀采样
                            double xstepbk = 0,            //背景地层采样网格
                            double ystepbk = 0,            //背景地层采样网格
                            int backvalue = -1)
        {
            if (RecognizedLayers == null) return;

            int col = RecognizedLayers.GetLength(0);
            int row = RecognizedLayers.GetLength(1);
            
            int id;
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    exports[j, i] = false;
                    id = RecognizedLayers[j, i];
                    if (id >= 0 && layerid == id) exports[j, i] = true;
                    if (id >= 0 && layerid < 0) exports[j, i] = true;
                }
            }


            //查找数据的边界范围
            int startX = 0, endX = col-1;
            int startY = 0, endY = row-1;
            if (layerid >= 0)
            {
                startX = col;endX = -1;
                startY = row; endY = -1;
                for (int i = 0; i < row; i++)
                {
                    for (int j = 0; j < col; j++)
                    {
                        if (!exports[j, i]) continue;
                        if (RecognizedLayers[j, i] != layerid) continue;

                        if (j < startX) startX = j;
                        if (j > endX) endX = j;                        
                        if (i < startY) startY = i;
                        if (i > endY) endY = i;
                    }
                }
                for (int i = 0; i < row; i++)
                {
                    for (int j = 0; j < col; j++)
                    {
                        if (!exports[j, i]) continue;
                        if (j < startX) exports[j, i] = false;
                        if (j > endX) exports[j, i] = false;
                        if (i < startY) exports[j, i] = false;
                        if (i > endY) exports[j, i] = false;
                    }
                }                
            }
            
            if (boder)//只保留边界点
            {
                //边界点
                for (int i = startY; i <= endY; i++)
                {
                    for (int j = startX; j <= endX; j++)
                    {
                        if (RecognizedLayers[j, i] >= 0 && exports[j, i])
                        {
                            if (!IsBorderLayer(RecognizedLayers,i, j, row, col, RecognizedLayers[j, i]))
                            {
                                exports[j, i] = false;
                            }
                        }
                    }
                }
            }
            
            if (boderouter)//添加边界外围点
            {
                for (int i = startY; i <= endY; i++)
                {
                    for (int j = startX; j <= endX; j++)
                    {
                        //if (RecognizedLayers[j, i] < 0 || !exports[j, i]) continue;
                        //if (layerid >= 0 && layerid != RecognizedLayers[j, i]) continue;
                        //if (!IsBorderLayer(RecognizedLayers,i, j, row, col, RecognizedLayers[j, i])) continue;
                        if(exports[j, i])
                        for (int iy = i-1;iy <= i+1; iy++)//查找边界外点
                        {
                            for (int ix = j - 1; ix <= j + 1; ix++)
                            {
                                if (ix < 0 || ix >= col) continue;
                                if (iy < 0 || iy >= row) continue;
                                if (ix == j && iy == i) continue;
                                if( RecognizedLayers[ix, iy] != RecognizedLayers[j, i])
                                    exports[ix,iy] = true;
                            }
                        }
                    }
                }
            }
            
            if (gridsample) //是否进行均匀网格采样
            {
                ImageStruct im = slicer.backImages[0];
                //图像RECT范围
                double x1 = im.rect.X1;
                double x2 = im.rect.X2;
                double y1 = im.rect.Y1;
                double y2 = im.rect.Y2;
                int nx = (int)((x2 - x1) / xstepbk + 0.1);
                int ny = (int)((y2 - y1) / xstepbk + 0.1);
                int sx = col / nx;
                int sy = row / ny;
                if (sx < 1) sx = 1;
                if (sy < 1) sy = 1;
                for (int i = startY-4*sy; i <= endY+4*sy; i += sy)
                {
                    for (int j = startX-4*sx; j <= endX+4*sx; j += sx)
                    {
                        if (j >= 0 && j < col && i >= 0 && i < row)
                            exports[j, i] = true;
                    }
                }
            }

            //边界内部采样
            int xx = 2, yy = 2;
            for (int i = startY; i <= endY; i += yy)
            {
                int n1 = -1, n2 = -1;
                int j = 0;
                while (j < col - 1)
                {
                    n1 = ScanLayerStartOnHor(n2 + 1, i, col, RecognizedLayers, layerid);
                    if (n1 < 0 || n1 >= col - 1) break;
                    n2 = ScanLayerEndOnHor(n1 + 1, i, col, RecognizedLayers, layerid);
                    if (n2 - n1 > xx)
                    {
                        for (j = n1; j < n2; j += xx)
                            exports[j, i] = true;
                    }
                    if (j >= col - 1 || n2 >= col - 1) break;
                }
            }

            //边界点二次采样
            for (int i = startY; i <= endY; i++)
            {
                for (int j = startX; j <= endX; j++)
                {
                    if (exports[j, i])
                    {
                        id = RecognizedLayers[j, i];
                        for (int y = i; y <= i + 1; y++)
                        {
                            for (int x = j; x <= j + 1; x++)
                            {
                                if (x == j && y == i) continue;
                                if (x < 0 || x >= col) continue;
                                if (y < 0 || y >= row) continue;
                                if (RecognizedLayers[x, y] != id) continue;
                                if (exports[x, y]) exports[x, y] = false;
                            }
                        }
                    }
                }
            }

            //上下边界采样-需要改进
            if (UpdownSelection > 0)//只采样上边界或下边界
            {
                for (int i = 0; i < row; i++)
                    for (int j = 0; j < col; j++) exports[j, i] = false;
                
                if (UpdownSelection == 1)
                {
                    for (int j = 0; j < col; j++)
                    {
                        int up = -1;
                        for (int i = row-1; i >= 0; i--)
                        {
                            if (RecognizedLayers[j, i] >= 0 )
                            {
                                up = i;                 
                                exports[j, i] = true; break;                            
                            }
                        }
                        //是否唯一点，是则忽略该点
                        if (up >= 0)
                        {
                            int count = 0;
                            for (int i = 0; i < up; i++) { if (RecognizedLayers[j, i] >= 0) count++; }
                            if (count == 0) exports[j, up] = false;
                        }
                    }

                }
                if (UpdownSelection == 2) //下边界
                {
                    for (int j = 0; j < col; j++)
                    {
                        int down = -1;
                        for (int i = 0; i <row; i++)
                        {
                            if (RecognizedLayers[j, i] >= 0)
                            {
                                down = i;
                                exports[j, i] = true;
                                break;
                            }
                        }
                        //是否唯一点，是则忽略该点
                        if (down >= 0)
                        {
                            int count = 0;
                            for (int i = down + 1; i < row; i++) { if (RecognizedLayers[j, i] >= 0) count++; }
                            if (count == 0) exports[j, down] = false;
                        }
                    }
                }
                
            }

        }        
      


        //查找地层第一个边界
        int ScanLayerStartOnHor(int start, int irow, int col, int[,] RecognizedLayers,int layerid)
        {
            for (int j = start; j < col; j++)
            {
                int id = RecognizedLayers[j, irow];
                if (id >= 0 && id == layerid) return j;
            }
            return -1;
        }
        //查找地层结束边界
        int ScanLayerEndOnHor(int start, int irow, int col, int[,] RecognizedLayers, int layerid)
        {
            for (int j = start; j < col; j++)
            {
                int id = RecognizedLayers[j, irow];
                if (id != layerid) return j;
            }
            return -1;
        }
        //是否边界点
        bool IsBorderLayer(int [,] RecognizedLayers, int irow, int jcol, int row, int col, int stratumid)
        {
            for (int i = irow - 1; i <= irow + 1; i++)
            {
                for (int j = jcol - 1; j <= jcol + 1; j++)
                {
                    if (i < 0 || i >= row || j < 0 || j >= col) continue;
                    if (RecognizedLayers[j, i] != stratumid) return true;
                }
            }
            return false;
        }

        List<string>PointsFromLayers = new List<string>();
        int AddToTracedPoints(List<Vector32> points, PolygonSlicer slicer, int[,] RecognizedLayers, bool[,]exports,int layerid,
            bool resetback,
            int backvalue,
            bool resetlayer,
            int resetlayervalue)
        {
            int col = RecognizedLayers.GetLength(0);
            int row = RecognizedLayers.GetLength(1);
            ImageStruct im = slicer.backImages[0];
            double imx1 = im.rect.X1;
            double imy1 = im.rect.Y1;
            double imx2 = im.rect.X2;
            double imy2 = im.rect.Y2;
            double dx = (imx2 - imx1) / col;
            double dy = (imy2 - imy1) / row;
            double x, y; 
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    if (exports[j, i] )
                    {
                        x = imx1 + j * dx + dx / 2;
                        y = imy1 + i * dy + dy / 2;
                        Vector64 p = slicer.toTracedPoint(x, y, 0, 0);
                        int id = RecognizedLayers[j, i];
                        if (resetlayer && layerid >= 0) id = resetlayervalue;
                        if ( resetback && layerid >= 0 )
                        {
                            if (RecognizedLayers[j, i] != layerid)
                                id = backvalue;
                        }
                        p.V = id;
                        points.Add(p);
                        PointsFromLayers.Add(slicer.Name);
                    }
                }
            }
            return points.Count;
        }
        List<Vector32> toTracedPoints(PolygonSlicer slicer,int[,] RecognizedLayers, List<Point>indices, int layerid,
                              bool resetback,
                              int backvalue,
                              bool resetlayer,
                              int resetlayervalue)
        {
            List<Vector32> _points = new List<Vector32>();
            int col = RecognizedLayers.GetLength(0);
            int row = RecognizedLayers.GetLength(1);
            ImageStruct im = slicer.backImages[0];
            double imx1 = im.rect.X1;
            double imy1 = im.rect.Y1;
            double imx2 = im.rect.X2;
            double imy2 = im.rect.Y2;
            double dx = (imx2 - imx1) / col;
            double dy = (imy2 - imy1) / row;
            double x, y;
            for(int i=0;i<indices.Count;i++)
            {               
                int ix = indices[i].X;
                int iy = indices[i].Y;
                x = imx1 + ix * dx + dx / 2;
                y = imy1 + iy * dy + dy / 2;
                Vector64 p = slicer.toTracedPoint(x, y, 0, 0);
                int id = RecognizedLayers[ix, iy];
                if (resetlayer && layerid >= 0) id = resetlayervalue;
                if (resetback && layerid >= 0)
                {
                    if (RecognizedLayers[ix, iy] != layerid)
                        id = backvalue;
                }
                p.V = id;
                _points.Add(p);
                PointsFromLayers.Add(slicer.Name);
            }            
            return _points;
        }
        private void ExportButton_Click(object sender, EventArgs e)
        {
            if(Points.Count < 1) 
            {
                MessageBox.Show("无有效采样点数据！");
                return;
            }            
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = Resource1.XYZVFormatLineFilter;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Cursor = Cursors.WaitCursor;
                    
                    StreamWriter wr = new StreamWriter(dlg.FileName,true);
                    if( wr.BaseStream.Length < 10 ) wr.WriteLine("X,Y,Z,VALUE,Profile");

                    for(int i=0;i<Points.Count;i++)                     
                    {
                        Vector32 p  = Points[i];
                        wr.WriteLine(p.toString(3) + "," +  Math.Round(p.V,0) + "," + PointsFromLayers[i] );
                    }
                    wr.Close();
                    
                    Cursor = Cursors.Default;
                    
                    MessageBox.Show("数据输出成功！" + Environment.NewLine + dlg.FileName);
                }
            }            
        }

        bool Stop = false;
        int nSlicer = 0;
        int curSlicer = 0;
        int curLayer = 0;
        int startLayer = 0;
        delegate void UpdateUI();
        void UpdateProgress()
        {
            if (progressBar1.InvokeRequired) 
            {
                UpdateUI ui = new UpdateUI(UpdateProgress);                
                this.BeginInvoke(ui);
                return;
            }
            else 
            {
                string layerName = "All Stratums";
                if (curLayer >= 0) layerName = C3DData.Stratums[curLayer].Name;
                double percent = 100 * curSlicer / (double)nSlicer;
                progressBar1.Value = (int)percent;
                PercentLabel.Text = layerName + ":" + Math.Round(percent, 2) + "%";
                if (curSlicer == nSlicer) progressBar1.Visible = false;
            }
        }
        void UpdateUIThread(Object obj)
        {            
            while (!Stop) 
            {
                UpdateProgress();
                Thread.Sleep(100);
            }
        }


        int GetZMedium(int id, List<Point>lists)
        {
            List<int>A = new List<int>();
            for(int i = id - 2; i <= id + 2; i++)
            {
                A.Add(lists[i].Y);
            }
            A.Sort();
            return A[2];
        }
        //对Z坐标进行滤波，过滤Z突变点
        List<Point> SortBorderIndices(bool[,]exports,int row,int col)
        {
            List<Point> indices = new List<Point>();
            for(int j=0;j<col;j++)
            {                
                for (int i = 0; i < row; i++)
                {
                    if (exports[j, i]) { indices.Add(new Point(j,i)); break; }
                }
            }
            
            bool[] bads = new bool[indices.Count];
            for (int i = 0; i < bads.Length; i++) bads[i] = false;
            for (int i = 2; i < indices.Count - 2; i++)
            {
                int mad = GetZMedium(i, indices);
                if ( Math.Abs(indices[i].Y - mad) > 2) bads[i] = true;
            }
            
            for(int i=indices.Count-1; i >= 0;i--)
                if(bads[i])indices.RemoveAt(i);
            bads = null;
            return indices;
        }

        void SampleAllThread(Object obj)
        {            
            SampleParaStruct para = (SampleParaStruct)obj;
            int colorDevition = para.colorDevition;
            double xstep = para.xstep;
            double ystep = para.ystep;
            double xbkstep = para.xbkstep;
            double ybkstep = para.ybkstep;
            int resetlayervalue = para.resetlayerValue;
            int bkvalue = para.bkvalue;
            bool useFilter = para.useFilter;
            bool border = para.border;
            bool boderOuter = para.boderOuter;
            bool backgridSample = para.backgridSample;
            bool resetbackvalue = para.resetbackvalue;
            bool resetLayer = para.resetLayer;
            for (int layerid = startLayer; layerid < C3DData.Stratums.Count; layerid++)
            {
                curLayer = layerid;                               
                Points.Clear();
                PointsFromLayers.Clear();   //所属勘探剖面编号                
                curSlicer = 0;
                foreach (PolygonSlicer slicer in Slicers)
                {
                    int[,] RecognizedLayers = SampleSlicer(slicer, xstep, ystep);
                    if (RecognizedLayers == null) return;

                    int col = RecognizedLayers.GetLength(0);
                    int row = RecognizedLayers.GetLength(1);
                    bool[,] exports = new bool[col, row];

                    for (int i = 0; i < row; i++)
                        for (int j = 0; j < col; j++)
                            exports[j, i] = true;

                    if (useFilter) ResamplePoints(slicer, RecognizedLayers, exports, layerid, border, boderOuter, backgridSample, xbkstep, ybkstep, bkvalue);
                    List<Point> indices = SortBorderIndices(exports,row,col);
                    if (UpdownSelection > 0) 
                    { 
                        indices = SortBorderIndices(exports, row, col);
                        Points.AddRange(toTracedPoints(slicer, RecognizedLayers, indices, layerid, resetbackvalue, bkvalue, resetLayer, resetlayervalue));
                        indices.Clear();
                    }
                    else AddToTracedPoints(Points, slicer, RecognizedLayers, exports, layerid, resetbackvalue, bkvalue,resetLayer, resetlayervalue);

                    RecognizedLayers = null;
                    exports = null;
                    curSlicer++;
                }

               
                //export to file
                string layername = C3DData.Stratums[layerid].Name;
                string filename = exportPath + "\\";
                filename += layername + "(" + Points.Count + ").csv";
                StreamWriter wr = new StreamWriter(filename, true);
                if (wr.BaseStream.Length < 10) wr.WriteLine("X,Y,Z,VALUE,Profile");
                for (int i = 0; i < Points.Count; i++)
                {
                    Vector32 p = Points[i];
                    wr.WriteLine(p.toString(3) + "," + Math.Round(p.V, 0) + "," + PointsFromLayers[i]);
                }
                wr.Close();
                Points.Clear();
                PointsFromLayers.Clear();   //所属勘探剖面编号
            }

            
            MessageBox.Show("采样完成，采样点数" + Points.Count);
            Stop = true;
        }
        void SampleThread(Object obj)
        {
            curSlicer = 0;
            SampleParaStruct para = (SampleParaStruct)obj;
            int layerid = para.layerid;
            curLayer = layerid;
            nSlicer = Slicers.Count;
            int colorDevition = para.colorDevition;
            double xstep = para.xstep ;
            double ystep = para.ystep ;
            double xbkstep = para.xbkstep ;
            double ybkstep = para.ybkstep ;
            int resetlayervalue = para.resetlayerValue;
            int bkvalue = para.bkvalue;
            bool useFilter = para.useFilter;
            bool border = para.border;
            bool boderOuter = para.boderOuter;
            bool backgridSample = para.backgridSample;
            bool resetbackvalue = para.resetbackvalue;
            bool resetLayer = para.resetLayer;
            foreach (PolygonSlicer slicer in Slicers)
            {
                int[,] RecognizedLayers = SampleSlicer(slicer, xstep, ystep);
                if (RecognizedLayers == null) return;

                int col = RecognizedLayers.GetLength(0);
                int row = RecognizedLayers.GetLength(1);
                bool[,] exports = new bool[col, row];

                for (int i = 0; i < row; i++)
                    for (int j = 0; j < col; j++)
                        exports[j, i] = true;

                if (useFilter) ResamplePoints(slicer, RecognizedLayers, exports, layerid, border, boderOuter, backgridSample, xbkstep, ybkstep, bkvalue);

                List<Point> indices = SortBorderIndices(exports, row, col);
                if (UpdownSelection > 0)
                {
                    indices = SortBorderIndices(exports, row, col);
                    Points.AddRange(toTracedPoints(slicer, RecognizedLayers, indices, layerid, resetbackvalue, bkvalue, resetLayer, resetlayervalue));
                    indices.Clear();
                }                
                else AddToTracedPoints(Points, slicer, RecognizedLayers, exports, layerid, resetbackvalue, bkvalue,
                    resetLayer,
                    resetlayervalue);
                RecognizedLayers = null;
                exports = null;
                curSlicer++;
            }
            MessageBox.Show("采样完成，采样点数" + Points.Count);
            Stop = true;
        }

        
        struct SampleParaStruct
        {
           public int layerid;
            public int colorDevition;
            public double xstep;
            public double ystep;
            public double xbkstep;
            public double ybkstep;
            public int bkvalue;    //地层背景值
            public int resetlayerValue; //地层值
            public bool useFilter;
            public bool border;
            public bool boderOuter;
            public bool backgridSample;
            public bool resetbackvalue;
            public bool resetLayer;
        }
        string exportPath = "";
        private void CreateAllButton_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "请选择一个目录作为数据存储路径：";
            dialog.ShowNewFolderButton = true;
            dialog.RootFolder = Environment.SpecialFolder.MyComputer;
            if (dialog.ShowDialog() != DialogResult.OK) return;
            exportPath = dialog.SelectedPath;

            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Step = 1;
            progressBar1.Visible = true;
            progressBar1.Value = 0;
            nSlicer = Slicers.Count;
            curSlicer = 0;
            Stop = false;            
            SampleParaStruct para = new SampleParaStruct();
            startLayer = comboBox1.SelectedIndex - 1;
            if (startLayer < 0) startLayer = 0;
            para.layerid = startLayer; //start from 1
            para.colorDevition = int.Parse(ColorDeviationText.Text);
            para.xstep = double.Parse(ColorSampleStepXText.Text);
            para.ystep = double.Parse(ColorSampleStepYText.Text);
            para.xbkstep = double.Parse(BackSampleStepXText.Text);
            para.ybkstep = double.Parse(BackSampleStepYText.Text);
            para.resetlayerValue = int.Parse(LayerValueTextBox.Text);
            para.bkvalue = int.Parse(BackgroundValueText.Text);
            para.useFilter = FilterCheckBox1.Checked;
            para.border = SampleBoudaryCheckBox.Checked;
            para.boderOuter = OuterBoderCheck.Checked;
            para.backgridSample = BackgroundSampleCheck.Checked;
            para.resetbackvalue = ResetBackgroundValueCheck.Checked;
            para.resetLayer = ResetLayerValueCheckBox.Checked;
            UpdownSelection = UpDownBorderComboBox.SelectedIndex;
            Thread thread1 = new Thread(SampleAllThread);
            thread1.Start(para);
            Thread thread2 = new Thread(UpdateUIThread);
            thread2.Start();            
        }

        private void OK_Click(object sender, EventArgs e)
        {
            
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Step = 1;
            progressBar1.Visible = true;
            nSlicer = Slicers.Count;
            curSlicer = 0;
            Stop = false;
            Points.Clear();
            PointsFromLayers.Clear();   //所属勘探剖面编号
            SampleParaStruct para = new SampleParaStruct();
            para.layerid = comboBox1.SelectedIndex - 1;
            curLayer = para.layerid;
            para.colorDevition = int.Parse(ColorDeviationText.Text);
            para.xstep = double.Parse(ColorSampleStepXText.Text);
            para.ystep = double.Parse(ColorSampleStepYText.Text);
            para.xbkstep = double.Parse(BackSampleStepXText.Text);
            para.ybkstep = double.Parse(BackSampleStepYText.Text);
            para.resetlayerValue = int.Parse(LayerValueTextBox.Text);
            para.bkvalue = int.Parse(BackgroundValueText.Text);
            para.useFilter = FilterCheckBox1.Checked;
            para.border = SampleBoudaryCheckBox.Checked;
            para.boderOuter = OuterBoderCheck.Checked;
            para.backgridSample = BackgroundSampleCheck.Checked;
            para.resetbackvalue = ResetBackgroundValueCheck.Checked;
            para.resetLayer = ResetLayerValueCheckBox.Checked;
            UpdownSelection = UpDownBorderComboBox.SelectedIndex;
            Thread thread1 = new Thread(SampleThread);
            thread1.Start(para);
            Thread thread2 = new Thread(UpdateUIThread);
            thread2.Start();
            //MessageBox.Show("采样完成，采样点数:" + Points.Count);

        }

        private void OnSampleStepChanged(object sender, EventArgs e)
        {
            UpdateSlicerImageRangeInfo();
        }        
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int sel = listBox1.SelectedIndex;
            if (sel > -1) 
            {
                PolygonSlicer slicer = Slicers[sel];
                PropertyGrid1.SelectedObject = slicer;
                UpdateSlicerImageRangeInfo();
            }
        }

        int NX = 101, NY = 101, NZ = 101;
        double stepx, stepy, stepz;
        public SlicerImagesRecognitionForm()
        {
            InitializeComponent();
            HideCheckBox.Checked = true;
            ColorDeviationText.Text = "5";
        }
        void UpdateDataRange()
        {
            minx = miny = minz = minv = 0;
            maxx = maxy = maxz = maxv = 0;
            SlicerX1 = SlicerY1 = 1.0E30;
            SlicerX2 = SlicerY2 = -1.0E30;
            for (int i = 0; i < Slicers.Count; i++)
            {
                PolygonSlicer s = Slicers[i];
                ImageStruct im = s.backImages[0];
                if (i == 0)
                {
                    minx = s.Minx;
                    miny = s.Miny;
                    minz = s.Minz;
                    minv = s.Minv;
                    maxx = s.Maxx;
                    maxy = s.Maxy;
                    maxz = s.Maxz;
                    maxv = s.Maxv;
                }
                else
                {
                    if (s.Minx < minx) minx = s.Minx;
                    if (s.Miny < miny) miny = s.Miny;
                    if (s.Minz < minz) minz = s.Minz;
                    if (s.Minv < minv) minv = s.Minv;
                    if (s.Maxx > maxx) maxx = s.Maxx;
                    if (s.Maxy > maxy) maxy = s.Maxy;
                    if (s.Maxz > maxz) maxz = s.Maxz;
                    if (s.Maxv > maxv) maxv = s.Maxv;
                }
                
                if (im.rect.X1 < SlicerX1) SlicerX1 = im.rect.X1;
                if (im.rect.Y1 < SlicerY1) SlicerY1 = im.rect.Y1;
                if (im.rect.X2 > SlicerX2) SlicerX2 = im.rect.X2;
                if (im.rect.Y2 > SlicerY2) SlicerY2 = im.rect.Y2;
            }
        }
        private void UpdateList1()
        {
            listBox1.Items.Clear();
            for (int i = 0; i < Slicers.Count; i++)
            {
                listBox1.Items.Add(Slicers[i].Name);
            }
            listBox1.SelectedIndex = -1;
        }
        private void UpdateCombox()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.Add("All Stratums");
            for (int i = 0; i < C3DData.Stratums.Count; i++)
            {
                comboBox1.Items.Add(C3DData.Stratums[i].Name);
            }
            comboBox1.SelectedIndex = 0;
        }


        private void SlicerImagesRecognitionForm_Load(object sender, EventArgs e)
        {
            List<C3DObjectBase> objects = C3DData.GetObjects();
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].type == ShapeEnum.PolygonSlicer)
                {
                    PolygonSlicer slicer = objects[i] as PolygonSlicer;
                    loadedSlicers.Add(slicer);
                    if (HideCheckBox.Checked) { if (slicer.Visible) Slicers.Add(slicer); }
                    else Slicers.Add(slicer);
                }
            }
            UpdateDataRange();
            UpdateList1();
            UpdateCombox();
            double xstep = (SlicerX2 - SlicerX1) / SampleNX;
            double ystep = (SlicerY2 - SlicerY1) / SampleNY;
            ColorSampleStepXText.Text = Math.Round(xstep, 2).ToString(); 
            ColorSampleStepYText.Text = Math.Round(ystep, 2).ToString();
            BackSampleStepXText.Text = Math.Round(xstep * 10, 2).ToString();
            BackSampleStepYText.Text = Math.Round(ystep * 10, 2).ToString();
            LayerValueTextBox.Text = "1";
            BackgroundValueText.Text = "0";
            UpDownBorderComboBox.Items.Add("All");
            UpDownBorderComboBox.Items.Add("Up Border");
            UpDownBorderComboBox.Items.Add("Down Border");
            UpDownBorderComboBox.SelectedIndex = 0;

        }
    }
}
