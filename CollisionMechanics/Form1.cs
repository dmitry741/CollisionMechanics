using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CollisionMechanics
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region members

        Bitmap _bitmap;
        Timer _timer;

        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO:
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
