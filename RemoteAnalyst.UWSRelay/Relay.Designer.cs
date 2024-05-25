namespace RemoteAnalyst.UWSRelay {
    partial class Relay {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tbSQS2 = new System.Windows.Forms.TextBox();
            this.tbSQS1 = new System.Windows.Forms.TextBox();
            this.tbS3Bucket2 = new System.Windows.Forms.TextBox();
            this.tbS3Bucket1 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(281, 205);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 13);
            this.label4.TabIndex = 30;
            this.label4.Text = "SQS Prod";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 205);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 13);
            this.label3.TabIndex = 29;
            this.label3.Text = "SQS Dev";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(279, 166);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 28;
            this.label2.Text = "S3 Prod";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 166);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 27;
            this.label1.Text = "S3 Dev";
            // 
            // tbSQS2
            // 
            this.tbSQS2.Location = new System.Drawing.Point(282, 219);
            this.tbSQS2.Name = "tbSQS2";
            this.tbSQS2.Size = new System.Drawing.Size(160, 20);
            this.tbSQS2.TabIndex = 26;
            this.tbSQS2.Text = "sqs-prod-loadQ";
            // 
            // tbSQS1
            // 
            this.tbSQS1.Location = new System.Drawing.Point(19, 219);
            this.tbSQS1.Name = "tbSQS1";
            this.tbSQS1.Size = new System.Drawing.Size(160, 20);
            this.tbSQS1.TabIndex = 25;
            this.tbSQS1.Text = "LoadQ";
            // 
            // tbS3Bucket2
            // 
            this.tbS3Bucket2.Location = new System.Drawing.Point(282, 182);
            this.tbS3Bucket2.Name = "tbS3Bucket2";
            this.tbS3Bucket2.Size = new System.Drawing.Size(160, 20);
            this.tbS3Bucket2.TabIndex = 24;
            this.tbS3Bucket2.Text = "s3-prod-remoteanalyst-ftp";
            // 
            // tbS3Bucket1
            // 
            this.tbS3Bucket1.Location = new System.Drawing.Point(19, 182);
            this.tbS3Bucket1.Name = "tbS3Bucket1";
            this.tbS3Bucket1.Size = new System.Drawing.Size(160, 20);
            this.tbS3Bucket1.TabIndex = 23;
            this.tbS3Bucket1.Text = "ra-production-ftp";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(282, 12);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(160, 20);
            this.textBox3.TabIndex = 22;
            // 
            // listBox2
            // 
            this.listBox2.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.listBox2.FormattingEnabled = true;
            this.listBox2.Location = new System.Drawing.Point(282, 90);
            this.listBox2.Name = "listBox2";
            this.listBox2.Size = new System.Drawing.Size(160, 69);
            this.listBox2.TabIndex = 21;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(325, 49);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 20;
            this.button3.Text = "Add";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(349, 245);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 19;
            this.button2.Text = "Run";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(59, 49);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 18;
            this.button1.Text = "Add";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // listBox1
            // 
            this.listBox1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(19, 90);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(160, 69);
            this.listBox1.TabIndex = 17;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(19, 12);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(160, 20);
            this.textBox1.TabIndex = 16;
            // 
            // Relay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 289);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbSQS2);
            this.Controls.Add(this.tbSQS1);
            this.Controls.Add(this.tbS3Bucket2);
            this.Controls.Add(this.tbS3Bucket1);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.listBox2);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.textBox1);
            this.Name = "Relay";
            this.Text = "Relay";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbSQS2;
        private System.Windows.Forms.TextBox tbSQS1;
        private System.Windows.Forms.TextBox tbS3Bucket2;
        private System.Windows.Forms.TextBox tbS3Bucket1;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.TextBox textBox1;
    }
}