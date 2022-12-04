using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCollection
{
    public enum TreeNodeType
    {
        Folder = 0, //目录
        Data = 1,  //数据节点
    }    
    public class TreeStructData
    {
        public string Name = ""; //节点名称
        public TreeNodeType Type = TreeNodeType.Data; //节点类型       
        public C3DObjectBase Item = null;//节点关联的数据
        public int ItemKey = -1;         //节点关联的数据在Dictionary中的Key值
        public TreeStructData Parent = null;
        public List<TreeStructData> Items = new List<TreeStructData>();
        public int Count { get { return Items.Count; } }
        public bool Checked = true;
        public TreeStructData(string name = "", TreeNodeType type = TreeNodeType.Data, C3DObjectBase obj = null)
        {
            Name = name;
            Type = type;
            Item = obj;
        }
        public void AddItem(TreeStructData item)
        {
            Items.Add(item);
        }
        public void Clear()
        {
            Items.Clear();
        }
        static public bool Save(BinaryWriter br, TreeStructData data)
        {
            try
            {
                C3DData.SaveString(br, data.Name);
                br.Write((int)data.Type);
                br.Write(data.ItemKey);
                br.Write(data.Checked);
                br.Write(data.Items.Count);
                foreach (TreeStructData p in data.Items)
                {
                    if (!Save(br, p)) return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        static public TreeStructData Load(BinaryReader br)
        {
            try
            {
                TreeStructData data = new TreeStructData("");
                data.Name = C3DData.LoadString(br);
                data.Type = (TreeNodeType)br.ReadInt32();
                data.ItemKey = br.ReadInt32();
                data.Checked = br.ReadBoolean();
                if (data.ItemKey >= 0) data.Item = C3DData.GetObjectByKey(data.ItemKey);
                int n = br.ReadInt32();
                if (n > 0)
                {
                    for (int i = 0; i < n; i++)
                    {
                        data.AddItem(Load(br));
                    }
                }
                return data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
