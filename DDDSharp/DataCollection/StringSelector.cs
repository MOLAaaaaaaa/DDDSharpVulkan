using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.ComponentModel;
using System.Globalization;
namespace DataCollection
{   
    public class StringSelector
    {
        public string SelectedItem = "";
        public int SelectedItemIndex = -1;
        public List<string> Items = new List<string>();
        public StringSelector()
        {            
        }
        public StringSelector(List<string>values)
        {
            Items.AddRange(values);
        }
        public void Add(string text) { Items.Add(text); }
        public void Clear() { Items.Clear(); }
    }    

    public class StringSelectorConverter : StringConverter
    {
        public override string ToString()
        {
            return "";
        }

        //true enable,false disable
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
       
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var obj = context.Instance as C3DObjectBase;
            if( obj.type == ShapeEnum.Borehole )
            {
                CBorehole bh = (CBorehole)obj;
                return new StandardValuesCollection(bh.Curves.lasData.CurveInformation);
            }
            if (obj.type == ShapeEnum.LasData)
            {
                LasFileData las = (LasFileData)obj;
                return new StandardValuesCollection(las.GetPropertiesString());
            }
            return new StandardValuesCollection(new List<string>());
        }

        //true: disable text editting.    false: enable text editting;
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    } 

}
