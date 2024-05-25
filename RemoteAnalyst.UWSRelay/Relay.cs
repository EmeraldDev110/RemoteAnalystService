using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using RemoteAnalyst.BusinessLogic.Util;
using RemoteAnalyst.UWSRelay.BLL;

namespace RemoteAnalyst.UWSRelay {
    public partial class Relay : Form {
        private readonly List<string> _systemList1;
        private readonly List<string> _systemList2;
        private string _s3BucketDev = string.Empty;
        private string _s3BucketProd = string.Empty;
        private string _sqsQueueDev = string.Empty;
        private string _sqsQueueProd = string.Empty;

        public Relay() {
            InitializeComponent();
            _systemList1 = new List<string>();
            _systemList2 = new List<string>();
        }

        private void button1_Click(object sender, EventArgs e) {
            try {
                string systemSerial = textBox1.Text;
                if (systemSerial.Length == 0) {
                    return;
                }
                //Check for duplicates.
                if (listBox1.Items.Cast<object>().Any(li => li.ToString() == systemSerial)) {
                    MessageBox.Show("Duplicate entry!");
                    textBox1.Text = "";
                    return;
                }
                //Check entry on Production list.
                if (listBox2.Items.Cast<object>().Any(li => li.ToString() == systemSerial)) {
                    MessageBox.Show("Duplicate entry on Production System List!");
                    textBox1.Text = "";
                    return;
                }

                _systemList1.Add(systemSerial);
                listBox1.DataSource = null;
                listBox1.DataSource = _systemList1;

                textBox1.Text = "";
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e) {
            try {
                string systemSerial = textBox3.Text;
                if (systemSerial.Length == 0) {
                    return;
                }
                //Check for duplicates.
                if (listBox2.Items.Cast<object>().Any(li => li.ToString() == systemSerial)) {
                    MessageBox.Show("Duplicate entry!");
                    textBox3.Text = "";
                    return;
                }
                //Check for duplicates from dev..
                if (listBox1.Items.Cast<object>().Any(li => li.ToString() == systemSerial)) {
                    MessageBox.Show("Duplicate entry on Dev System List!");
                    textBox3.Text = "";
                    return;
                }
                _systemList2.Add(systemSerial);
                listBox2.DataSource = null;
                listBox2.DataSource = _systemList2;

                textBox3.Text = "";
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e) {
            button2.Enabled = false;

            _s3BucketDev = tbS3Bucket1.Text;
            _s3BucketProd = tbS3Bucket2.Text;
            _sqsQueueDev = tbSQS1.Text;
            _sqsQueueProd = tbSQS2.Text;

            ReadXML.ImportDataFromXML();
            var jobRelay = new JobRelay();
            jobRelay.StartJobWatch(_systemList1, _systemList2, _s3BucketDev, _sqsQueueDev, _s3BucketProd, _sqsQueueProd);
        }
    }
}