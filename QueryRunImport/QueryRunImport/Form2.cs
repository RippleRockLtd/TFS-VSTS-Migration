using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ASOS_MIG
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        public void ShowMyDialogBox(Exception ex,string s)
        {
            Form2 testDialog = new Form2();
            testDialog.linkLabel1.Text = s;
            if (ex != null)
            {
                testDialog.textBox1.Text = ex.Message;
            }
            System.Diagnostics.Process.Start(s);
            // Show testDialog as a modal dialog and determine if DialogResult = OK.
            if (testDialog.ShowDialog(this) == DialogResult.OK)
            {

                // Read the contents of testDialog's TextBox.
              //  this.txtResult.Text = testDialog.textBox1.Text;
            }
            else
            {
//this.txtResult.Text = "Cancelled";
            }
            testDialog.Dispose();
        }
    }
}
