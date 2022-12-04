using System;
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
    public partial class PlyElementMatchForm : Form
    {
        public PlyFile ply = null;

        public PlyElementMatchForm()
        {
            InitializeComponent();
        }

        private void UpdateList()
        {
            if (ply == null) return;
            VerticXcomboBox.Items.Clear();

            //for(int i=0;i<ply.elements.Count;i++)
        }
        private void PlyElementMatchForm_Load(object sender, EventArgs e)
        {
            UpdateList();
        }
    }
}
