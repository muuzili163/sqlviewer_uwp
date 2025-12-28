namespace SQLViewer
{
    partial class TableViewer
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            button6 = new Button();
            button4 = new Button();
            button3 = new Button();
            button2 = new Button();
            textBox1 = new TextBox();
            panel1 = new Panel();
            dataGridView1 = new DataGridView();
            richTextBox1 = new RichTextBox();
            listBoxSuggest = new ListBox();
            label4 = new Label();
            button5 = new Button();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            label5 = new Label();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // button6
            // 
            button6.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button6.Location = new Point(992, 14);
            button6.Name = "button6";
            button6.Size = new Size(94, 35);
            button6.TabIndex = 12;
            button6.Text = "复制SQL";
            button6.UseVisualStyleBackColor = true;
            button6.Click += button6_Click;
            // 
            // button4
            // 
            button4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button4.Location = new Point(892, 14);
            button4.Name = "button4";
            button4.Size = new Size(94, 35);
            button4.TabIndex = 11;
            button4.Text = "清空排序";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // button3
            // 
            button3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button3.Location = new Point(792, 13);
            button3.Name = "button3";
            button3.Size = new Size(94, 36);
            button3.TabIndex = 10;
            button3.Text = "查询";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // button2
            // 
            button2.Location = new Point(8, 13);
            button2.Name = "button2";
            button2.Size = new Size(94, 35);
            button2.TabIndex = 9;
            button2.Text = "表结构";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // textBox1
            // 
            textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBox1.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            textBox1.Location = new Point(108, 14);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(678, 33);
            textBox1.TabIndex = 8;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.Controls.Add(dataGridView1);
            panel1.Controls.Add(richTextBox1);
            panel1.Location = new Point(8, 64);
            panel1.Name = "panel1";
            panel1.Size = new Size(1091, 717);
            panel1.TabIndex = 13;
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Location = new Point(0, 497);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.RowTemplate.Height = 29;
            dataGridView1.Size = new Size(1091, 220);
            dataGridView1.TabIndex = 5;
            dataGridView1.ColumnHeaderMouseClick += dataGridView1_ColumnHeaderMouseClick;
            dataGridView1.DataBindingComplete += dataGridView1_DataBindingComplete;
            // 
            // richTextBox1
            // 
            richTextBox1.Dock = DockStyle.Top;
            richTextBox1.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            richTextBox1.Location = new Point(0, 0);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(1091, 497);
            richTextBox1.TabIndex = 4;
            richTextBox1.Text = "";
            richTextBox1.Visible = false;
            // 
            // listBoxSuggest
            // 
            listBoxSuggest.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            listBoxSuggest.FormattingEnabled = true;
            listBoxSuggest.ItemHeight = 27;
            listBoxSuggest.Location = new Point(108, 50);
            listBoxSuggest.Name = "listBoxSuggest";
            listBoxSuggest.Size = new Size(236, 193);
            listBoxSuggest.TabIndex = 20;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            label4.AutoSize = true;
            label4.Location = new Point(669, 796);
            label4.Name = "label4";
            label4.Size = new Size(39, 20);
            label4.TabIndex = 18;
            label4.Text = "总数";
            // 
            // button5
            // 
            button5.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button5.Location = new Point(569, 792);
            button5.Name = "button5";
            button5.Size = new Size(94, 29);
            button5.TabIndex = 17;
            button5.Text = "计算总数";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            label3.AutoSize = true;
            label3.Location = new Point(790, 795);
            label3.Name = "label3";
            label3.Size = new Size(39, 20);
            label3.TabIndex = 16;
            label3.Text = "条数";
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(935, 795);
            label2.Name = "label2";
            label2.Size = new Size(69, 20);
            label2.TabIndex = 15;
            label2.Text = "查询时间";
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(11, 794);
            label1.Name = "label1";
            label1.Size = new Size(39, 20);
            label1.TabIndex = 14;
            label1.Text = "查询";
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            label5.AutoSize = true;
            label5.Location = new Point(418, 796);
            label5.Name = "label5";
            label5.Size = new Size(53, 20);
            label5.TabIndex = 19;
            label5.Text = "label5";
            // 
            // UserControl1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(button5);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(listBoxSuggest);
            Controls.Add(panel1);
            Controls.Add(button6);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(textBox1);
            Name = "UserControl1";
            Size = new Size(1102, 831);
            Load += UserControl1_Load;
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button6;
        private Button button4;
        private Button button3;
        private Button button2;
        private TextBox textBox1;
        private Panel panel1;
        private RichTextBox richTextBox1;
        private DataGridView dataGridView1;
        private Label label4;
        private Button button5;
        private Label label3;
        private Label label2;
        private Label label1;
        private Label label5;
        private ListBox listBoxSuggest;
    }
}
