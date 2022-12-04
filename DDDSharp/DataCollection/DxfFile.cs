using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IxMilia.Dxf;
using IxMilia.Dxf.Entities;
//added DXF libery 2019.8.10
namespace DataCollection
{
    public class DxfFileObj
    {
        public string errMessage = "";
        public DxfFile dxfFile = null;
        public IList<DxfEntity> dxfEntities = null;
        public List<Polygon2D> polygons = new List<Polygon2D>();
        public List<C3DLine> lines = new List<C3DLine>();
        
        public bool Load(String filename)
        {           
            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    dxfFile = DxfFile.Load(fs);
                    fs.Close();
                    dxfEntities = dxfFile.Entities;
                    if (dxfEntities == null) return false;
                    else return true;
                }
                //// if .NETFrameworkv3.5 or >= NETStandard1.3 you can use:
                //DxfFile dxfFile = DxfFile.Load(filename);                
            }
            catch(Exception e)
            {
                errMessage = e.Message;
                return false;
            }

            
        }        
        public Polygon2D toPolygon2D(DxfPolyline entity)
        {
            Polygon2D poly = new Polygon2D();
            DxfPoint p;
            for(int i=0;i<entity.Vertices.Count;i++)
            {
                p = entity.Vertices[i].Location;
                poly.Add(p.X, p.Y, p.Z);
            }
            poly.IsClosed = entity.IsClosed;
            poly.lineWidth = (float)entity.Thickness;
            if (poly.lineWidth < 1 ) poly.lineWidth = 1;
            poly.UpdateRange();

            return poly;
        }
        public Polygon2D toPolygon2D(DxfLwPolyline entity)
        {
            Polygon2D poly = new Polygon2D();
            foreach ( DxfLwPolylineVertex p in entity.Vertices)
            {
                poly.Add(p.X, p.Y, 0);
            }
            poly.IsClosed = entity.IsClosed;
            poly.lineWidth = (float)entity.Thickness;
            if (poly.lineWidth < 1) poly.lineWidth = 1;
            poly.UpdateRange();

            return poly;
        }
        public int ExtractObjects()
        {
            if (dxfEntities == null) return 0;
            foreach (DxfEntity entity in dxfEntities)
            {
                switch (entity.EntityType)
                {
                    case DxfEntityType.Polyline:
                        polygons.Add(toPolygon2D((DxfPolyline)entity));
                        break;
                    case DxfEntityType.LwPolyline:                        
                        polygons.Add(toPolygon2D((DxfLwPolyline)entity));
                        break;
                    case DxfEntityType.Region:
                        DxfRegion regon = (DxfRegion)entity;                        
                        break;
                    default:                        
                        break;
                }
            }
            return polygons.Count;
        }
        public DxfVertex toDxfVertex(Vector64 p)
        {
            return new DxfVertex( new DxfPoint(p.x,p.y,p.z) );
        }
        public List<DxfVertex> toDxfVertexs(List<Vector64>points)
        {
            List<DxfVertex> vertices = new List<DxfVertex>();
            foreach(Vector64 p in points)
            {
                vertices.Add(toDxfVertex(p));
            }
            return vertices;
        }
        public DxfPolyline CreateDxfPolyline(C3DLine line)
        {
            DxfPolyline dxfPline = new DxfPolyline(toDxfVertexs(line.points));
            dxfPline.IsClosed = line.Closed;
            return dxfPline;
        }
        public DxfPolyline CreateDxfPolyline(Polygon2D poly)
        {
            DxfPolyline dxfPline = new DxfPolyline(toDxfVertexs(poly.points));
            dxfPline.IsClosed = true;            
            return dxfPline;
        }
        public DxfPolyline CreateDxfPolyline(List<Vector64>points,bool closed = false)
        {
            if (points.Count < 1) return null;
            DxfPolyline dxfPline = new DxfPolyline(toDxfVertexs(points));
            dxfPline.IsClosed = closed;
            dxfPline.IsPolyfaceMesh = false;
            dxfPline.IsPolygonMeshClosedInNDirection = false;
            dxfPline.Is3DPolygonMesh = false;
            dxfPline.Is3DPolyline = true;
            return dxfPline;
        }
    }
}
