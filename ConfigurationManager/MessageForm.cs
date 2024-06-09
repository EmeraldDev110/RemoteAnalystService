using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConfigurationManager {
    public partial class Error : Form {
        Form source;
        public Error(Form form) {
            InitializeComponent();
            source = form;
        }

        private void MessageForm_Load(object sender, EventArgs e) {
            this.ControlBox = false;
        }

        private void btnExitProgram_Click(object sender, EventArgs e) {
            this.Close();
            source.Close();
        }

        private void button1_Click(object sender, EventArgs e) {
            this.Close();
            source.Close();           
        }

        private void label1_Click(object sender, EventArgs e) {

        }
    }
}
