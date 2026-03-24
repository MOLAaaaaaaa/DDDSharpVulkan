using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CLInterpolation;
namespace DDDSharp
{
    public partial class IDWSet : Form
    {
        public InterpolatorBase ip = null;
        public IDWSet()
        {
            InitializeComponent();
        }

        private void OKbutton1_Click(object sender, EventArgs e)
        {            
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void IDWSet_Load(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = ip;
        }

        private void Cancelbutton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
