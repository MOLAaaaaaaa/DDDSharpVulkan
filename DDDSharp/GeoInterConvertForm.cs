using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataCollection;
namespace DDDSharp
{
    public partial class GeoInterConvertForm : Form
    {
        public SgyData3D sgyData = new SgyData3D();
        public CGeoFile geoData = new CGeoFile();

        double minx, maxx, miny, maxy;
        int InLineNum = 0;
        int CrossLineNum = 0;
        int SampledNum = 0;
        int nx0 = 0;
        int ny0 = 0;
        int nz0 = 0;
        int nx, ny, nz;
        double rotateAngle = 0;
        double TimeAxisScale = 1.0;
        //the 4 corners of the area
        Vector32 cp1, cp2, cp3, cp4;
        
        public GeoInterConvertForm()
        {
            InitializeComponent();
        }

        private void label11_Click(object sender, EventArgs e)
        {

        }
        void LPtoDP(double x,double y,out double ox,out double oy)
        {   
            ox = oy = 0;
            double x1 = minx;
            double x2 = maxx;
            double y1 = miny;
            double y2 = maxy;

            double dx = x2 - x1;
            double dy = y2 - y1;
            int margine = 10;
            int width = CoordPicBox.Width - margine;
            int height = CoordPicBox.Height - margine;
            double s = 1.0;
            if ( dx > dy )
            {
                s = dy / dx;
                
                ox = margine / 2 + width * (x - x1) / (x2 - x1);
                oy = height - s*height * (y - y1) / (y2 - y1) + margine/2 + height * (1 - s) / 2;
            }
            else
            {
                s = dx / dy;
                ox = margine / 2 + s * width * (x - x1) / (x2 - x1);
                oy = height - height * (y - y1) / (y2 - y1) + margine / 2;
            }            
            
        }
        void DPtoLP(double x, double y, out double ox, out double oy)
        {
            ox = oy = 0;
            double x1 = minx;
            double x2 = maxx;
            double y1 = miny;
            double y2 = maxy;
            int width = CoordPicBox.Width-10;
            int height = CoordPicBox.Height-10;
            ox = (x2 - x1)*(x - 0)/width;
            oy = y2 - (y2 - y1) * (y - 0) / height;
        }

        double CalculateRotateAngle()
        {
            Vector32 p1 = cp1;
            Vector32 p2 = cp2;
            
            Vector32 p1p2 = p1 - p2;
            Vector32 p3p4 = new Vector32(1,0,0);

            double angle = Vector32.VectorAngle(p1p2, p3p4);
            RotateAngle_textBox.Text = angle.ToString();

            return angle;
        }
       
        void DrawCoord()
        {
            if (geoData.pCoords.Count < 1) return;

            int interval = InLineNum / 20;
            if (interval < 1) interval = 1;

            Graphics e = CoordPicBox.CreateGraphics();

            double x1, y1, x2, y2,x3,y3,x4,y4;
            Pen pen1 = new Pen(Color.FromKnownColor(KnownColor.Red));
            Pen pen = new Pen(Color.FromKnownColor(KnownColor.Blue));

            Vector32 p1 = cp1;
            Vector32 p2 = cp2;
            Vector32 p3 = cp3;
            Vector32 p4 = cp4;

            LPtoDP(p1.x, p1.y, out x1, out y1);
            LPtoDP(p2.x, p2.y, out x2, out y2);
            LPtoDP(p3.x, p3.y, out x3, out y3);
            LPtoDP(p4.x, p4.y, out x4, out y4);
            e.DrawLine(pen, (float)x1, (float)y1, (float)x2, (float)y2);
            e.DrawLine(pen, (float)x3, (float)y3, (float)x4, (float)y4);
            e.DrawLine(pen, (float)x1, (float)y1, (float)x3, (float)y3);
            e.DrawLine(pen, (float)x2, (float)y2, (float)x4, (float)y4);

            double dx1 = x3 - x1;
            double dy1 = y3 - y1;
            double dx2 = x4 - x2;
            double dy2 = y4 - y2;

            double px1, py1,px2,py2;
            for ( int i = 1; i <= 19; i++ )
            {
                px1 = x1 + i * dx1 / 20;
                py1 = y1 + i * dy1 / 20;
                px2 = x2 + i * dx2 / 20;
                py2 = y2 + i * dy2 / 20;

                e.DrawLine(pen, (float)px1, (float)py1, (float)px2, (float)py2);
                //draw small arrow
                e.DrawEllipse(pen1, (float)px2, (float)py2, 2, 2);
            }
        }
        //
        private void DoExport_Click(object sender, EventArgs e)
        {
            nx = ConvertData.StringToInt(Export_X_textBox.Text);
            ny = ConvertData.StringToInt(Export_Y_textBox.Text);
            nz = ConvertData.StringToInt(Export_Z_textBox.Text);
            if(nx<2||ny<2||nz<2)
            {
                MessageBox.Show("Export Geometry Grids not correct.");
                return;
            }

            Cursor = Cursors.WaitCursor;

            C3DGridData data = new C3DGridData();
            data.Create(nx0, ny0, nz0, minx, miny, sgyData.minz, maxx, maxy, sgyData.maxz, sgyData.minv, sgyData.maxv);
            data.pGridData = new float[nx0*ny0*nz0];
            int count = 0;
            for (int k =0 ; k<nz0; k++)//0 - NZ-1
            {               
                for (int j = 0; j < ny0; j++)//0 - NY-1
                  for (int i = 0; i < nx0; i++)//0 - NX-1                    
                {
                    count = k * nx0*ny0 + j * nx0 + i;
                    //data.pGridData[count++] = sgyData.pDao[j * CrossLineNum + i].pData[k];
                    data.pGridData[count] = sgyData.pDao[j * nx0 + i].pData[nz0-1-k];
                }
            }
            if (nx == nx0 && ny == ny0 && nz == nz0)
            {
                data.UpdateRange();
                data.SaveAs(export_grid_textBox.Text);
                data.Clear();
            }
            else
            {
                double x, y, z;
                C3DGridData data1 = new C3DGridData();
                data1.Create(nx, ny, nz, minx, miny, sgyData.minz, maxx, maxy, sgyData.maxz, sgyData.minv, sgyData.maxv);
                data1.pGridData = new float[nx * ny * nz];
                count = 0;
                double xstep = (maxx - minx) / (nx - 1);
                double ystep = (maxy - miny) / (ny - 1);
                double zstep = (sgyData.maxz - sgyData.minz) / (nz - 1);
                for (int k = 0; k < nz; k++)
                {
                    z = sgyData.minz + k * zstep;
                    for (int j = 0; j < ny; j++)
                    {
                        y = miny + ystep * j;
                        for (int i = 0; i < nx; i++)
                        {
                            x = minx + xstep * i;
                            data1.pGridData[count++] = (float)data.GetGridValue(x, y, z, true, false);
                        }
                    }
                }
                data1.UpdateDataRange();
                if (data1.SaveAs(export_grid_textBox.Text))
                {
                    MessageBox.Show("3DGrid data exported!\n" + export_grid_textBox.Text);
                }
                data1.Clear();
            }
            //export information
            ExportConvertInfo();
            Cursor = Cursors.Default;
        }
        void ExportConvertInfo()
        {
            string infoFile = export_grid_textBox.Text + "_info.txt";
            try
            {
                FileStream fs = new FileStream(infoFile, FileMode.Create);
                StreamWriter wr = new StreamWriter(fs);
                string str = "#Informations of SGEY data transfer to 3DGrid data#";wr.WriteLine(str);
                str = "[rotate]"; wr.WriteLine(str);
                str = "angle(anticlockwise) = " + rotateAngle; wr.WriteLine(str);
                str = "center = " + rotateAngle; wr.WriteLine(str);
                str = "[corners before transfered]"; wr.WriteLine(str);
                str = "x1 = " + geoData.cp1.x; wr.WriteLine(str);
                str = "y1 = " + geoData.cp1.y; wr.WriteLine(str);
                str = "x2 = " + geoData.cp2.x; wr.WriteLine(str);
                str = "y2 = " + geoData.cp2.y; wr.WriteLine(str);
                str = "x3 = " + geoData.cp3.x; wr.WriteLine(str);
                str = "y3 = " + geoData.cp3.y; wr.WriteLine(str);
                str = "x4 = " + geoData.cp4.x; wr.WriteLine(str);
                str = "y4 = " + geoData.cp4.y; wr.WriteLine(str);
                str = "[corners after transfered]"; wr.WriteLine(str);
                str = "x1 = " + cp1.x; wr.WriteLine(str); 
                str = "y1 = " + cp1.y; wr.WriteLine(str); 
                str = "x2 = " + cp2.x; wr.WriteLine(str); 
                str = "y2 = " + cp2.y; wr.WriteLine(str); 
                str = "x3 = " + cp3.x; wr.WriteLine(str); 
                str = "y3 = " + cp3.y; wr.WriteLine(str); 
                str = "x4 = " + cp4.x; wr.WriteLine(str); 
                str = "y4 = " + cp4.y; wr.WriteLine(str); 
                str = "[data ranges]"; wr.WriteLine(str);
                str = "minimum to maximum(x) = " + minx + " to " + maxx; wr.WriteLine(str);
                str = "minimum to maximum(y) = " + miny + " to " + maxy; wr.WriteLine(str);
                str = "minimum to maximum(z) = " + sgyData.minz + " to " + sgyData.maxz; wr.WriteLine(str);
                str = "z scale = " + TimeAxisScale; wr.WriteLine(str);
                wr.Close();
                fs.Close();
            }
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
            catch(Exception e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
            {
                MessageBox.Show("write infomation file failed.");
            }
        }
        private void textBox11_TextChanged(object sender, EventArgs e)
        {

        }

        bool LoadGeoFile(string filename)
        {
            geoData.Clear();
            geoData.LoadFrom(filename);

            cp1 = geoData.cp1;
            cp2 = geoData.cp2;
            cp3 = geoData.cp3;
            cp4 = geoData.cp4;
            UpdateDataRange();
            
            InLineNum = geoData.InLineNum;
            CrossLineNum = geoData.CrossLineNum;
            TimeAxisScale = geoData.TimeAxisScale;

            nx0 = CrossLineNum;
            ny0 = InLineNum;            

            CrossLine_textBox.Text = CrossLineNum.ToString();
            Inline_textBox.Text = InLineNum.ToString();

            int len = filename.Length;
            string sgyfile = filename.Substring(0, len - 4) + ".sgy";
            SgyFile_textBox.Text = sgyfile;
            GeoFile_textBox.Text = filename;
            export_grid_textBox.Text = filename.Substring(0, len - 4) + ".3DGrid";
            
            sgyData.Clear();

            if ( sgyData.LoadFrom(sgyfile) )
            {
                SampledNum = sgyData.nSampleNum;
                nz0 = SampledNum;
                SampledNum_textBox.Text = SampledNum.ToString();

                TotalTrack_textBox.Text = sgyData.Length.ToString();
                SampledTimeStep_textBox.Text = sgyData.nSampleTimeSpace.ToString();

                Export_X_textBox.Text = nx0.ToString();
                Export_Y_textBox.Text = ny0.ToString();
                Export_Z_textBox.Text = nz0.ToString();

                CalculateRotateAngle();
            }
            else
            {
                MessageBox.Show(sgyData.ErrMsg);
                return false;
            }
            return true;
        }
        private void BrowseGeoButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var dlg = new OpenFileDialog())
                {
                    dlg.Filter = "Geo file(*.geo)|*.geo|all files(*.*)|*.*";

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        this.Cursor = Cursors.WaitCursor;
                        
                        if (LoadGeoFile(dlg.FileName))
                        {
                            
                        }
                        else MessageBox.Show("Read from geo file error.");

                        this.Cursor = Cursors.Default;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void GeoInterConvertForm_Paint(object sender, PaintEventArgs e)
        {
            DrawCoord();
        }

        private void CoordPicBox_Paint(object sender, PaintEventArgs e)
        {
           // DrawCoord();
        }
       
        void UpdateDataRange()
        {
            minx = maxx = cp1.x;
            miny = maxy = cp1.y;

            if (cp2.x < minx) minx = cp2.x;
            if (cp2.y < miny) miny = cp2.y;
            if (cp3.x < minx) minx = cp3.x;
            if (cp3.y < miny) miny = cp3.y;
            if (cp4.x < minx) minx = cp4.x;
            if (cp4.y < miny) miny = cp4.y;

            if (cp2.x > maxx) maxx = cp2.x;
            if (cp2.y > maxy) maxy = cp2.y;
            if (cp3.x > maxx) maxx = cp3.x;
            if (cp3.y > maxy) maxy = cp3.y;
            if (cp4.x > maxx) maxx = cp4.x;
            if (cp4.y > maxy) maxy = cp4.y;

            minx_textBox1.Text = minx.ToString();
            miny_textBox.Text = miny.ToString();
            maxx_textBox.Text = maxx.ToString();
            maxy_textBox.Text = maxy.ToString();
        }

        
        void RotateCoords(double angle)
        {
            cp1 = geoData.cp1;
            cp2 = geoData.cp2;
            cp3 = geoData.cp3;
            cp4 = geoData.cp4;

            cp1.z = cp2.z = cp3.z = cp4.z = 0;

            UpdateDataRange();

            if (angle == 0) return;

            Vector32 p0 = new Vector32((float)(minx + maxx) / 2.0f, (float)(miny + maxy) / 2.0f, 0);
            cp1 = (cp1 - p0).RotateOnAngle(angle, 2);
            cp2 = (cp2 - p0).RotateOnAngle(angle, 2);
            cp3 = (cp3 - p0).RotateOnAngle(angle, 2);
            cp4 = (cp4 - p0).RotateOnAngle(angle, 2);

            UpdateDataRange();

        }
        private void UpdateCoordButton_Click(object sender, EventArgs e)
        {
            double angle  = ConvertData.StringToDouble(RotateAngle_textBox.Text);

            if(angle != rotateAngle)
            {
                RotateCoords(angle);
                rotateAngle = angle;

                Invalidate(true);
                //CoordPicBox.Invalidate(true);
            }

        }
    }
}
