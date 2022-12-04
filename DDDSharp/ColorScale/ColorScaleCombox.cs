using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace DataCollection
{
    public class ColorScaleCombox : ComboBox
    {
        //初始化数据项
        private List<string> originalList = new List<string>();
        public ColorScaleCombox()
        {
            this.TextUpdate += new EventHandler(CmbTextUpdate);
        }
        /// <summary>
        /// 初始化控件的数据
        /// </summary>
        /// <param name="list">数据集合</param>
        public void Init(List<string> list)
        {
            this.originalList = new List<string>();
            foreach (string str in list)
            {
                this.originalList.Add(str);
            }
            this.Items.Clear();
            this.Items.AddRange(this.originalList.ToArray());
        }

        private void CmbTextUpdate(object sender, EventArgs e)
        {
            this.Items.Clear();
            List<string> list = new List<string>();
            list.Add("");
            foreach (var item in originalList)
            {
                if (item.Contains(this.Text))
                {
                    list.Add(item);
                }
            }
            this.Items.AddRange(list.ToArray());
            //设置光标位置，否则光标位置始终保持在第一列，造成输入关键词的倒序排列
            this.SelectionStart = this.Text.Length;
            //保持鼠标指针原来状态，有时候鼠标指针会被下拉框覆盖，所以要进行一次设置。
            Cursor = Cursors.Default;
            this.DroppedDown = true;
        }
    }
}
