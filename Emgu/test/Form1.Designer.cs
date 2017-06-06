namespace test
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.OpenFileDlg = new System.Windows.Forms.OpenFileDialog();
            this.TextBox_FilePath = new System.Windows.Forms.TextBox();
            this.ImgBox_Original = new System.Windows.Forms.PictureBox();
            this.ImgBox_Circle = new System.Windows.Forms.PictureBox();
            this.ImgBox_Line = new System.Windows.Forms.PictureBox();
            this.ImgBox_Triangle_Rect = new System.Windows.Forms.PictureBox();
            this.OpenFile = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.ImgBox_Original)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImgBox_Circle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImgBox_Line)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImgBox_Triangle_Rect)).BeginInit();
            this.SuspendLayout();
            // 
            // OpenFileDlg
            // 
            this.OpenFileDlg.FileName = "openFileDialog1";
            // 
            // TextBox_FilePath
            // 
            this.TextBox_FilePath.Location = new System.Drawing.Point(51, 43);
            this.TextBox_FilePath.Name = "TextBox_FilePath";
            this.TextBox_FilePath.Size = new System.Drawing.Size(100, 20);
            this.TextBox_FilePath.TabIndex = 0;
            // 
            // ImgBox_Original
            // 
            this.ImgBox_Original.Location = new System.Drawing.Point(99, 118);
            this.ImgBox_Original.Name = "ImgBox_Original";
            this.ImgBox_Original.Size = new System.Drawing.Size(100, 50);
            this.ImgBox_Original.TabIndex = 1;
            this.ImgBox_Original.TabStop = false;
            // 
            // ImgBox_Circle
            // 
            this.ImgBox_Circle.Location = new System.Drawing.Point(395, 164);
            this.ImgBox_Circle.Name = "ImgBox_Circle";
            this.ImgBox_Circle.Size = new System.Drawing.Size(197, 246);
            this.ImgBox_Circle.TabIndex = 2;
            this.ImgBox_Circle.TabStop = false;
            // 
            // ImgBox_Line
            // 
            this.ImgBox_Line.Location = new System.Drawing.Point(278, 43);
            this.ImgBox_Line.Name = "ImgBox_Line";
            this.ImgBox_Line.Size = new System.Drawing.Size(100, 50);
            this.ImgBox_Line.TabIndex = 3;
            this.ImgBox_Line.TabStop = false;
            // 
            // ImgBox_Triangle_Rect
            // 
            this.ImgBox_Triangle_Rect.Location = new System.Drawing.Point(219, 273);
            this.ImgBox_Triangle_Rect.Name = "ImgBox_Triangle_Rect";
            this.ImgBox_Triangle_Rect.Size = new System.Drawing.Size(100, 50);
            this.ImgBox_Triangle_Rect.TabIndex = 4;
            this.ImgBox_Triangle_Rect.TabStop = false;
            // 
            // OpenFile
            // 
            this.OpenFile.Location = new System.Drawing.Point(174, 43);
            this.OpenFile.Name = "OpenFile";
            this.OpenFile.Size = new System.Drawing.Size(75, 23);
            this.OpenFile.TabIndex = 5;
            this.OpenFile.Text = "button1";
            this.OpenFile.UseVisualStyleBackColor = true;
            this.OpenFile.Click += new System.EventHandler(this.OpenFile_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(646, 463);
            this.Controls.Add(this.OpenFile);
            this.Controls.Add(this.ImgBox_Triangle_Rect);
            this.Controls.Add(this.ImgBox_Line);
            this.Controls.Add(this.ImgBox_Circle);
            this.Controls.Add(this.ImgBox_Original);
            this.Controls.Add(this.TextBox_FilePath);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ImgBox_Original)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImgBox_Circle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImgBox_Line)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImgBox_Triangle_Rect)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog OpenFileDlg;
        private System.Windows.Forms.TextBox TextBox_FilePath;
        private System.Windows.Forms.PictureBox ImgBox_Original;
        private System.Windows.Forms.PictureBox ImgBox_Circle;
        private System.Windows.Forms.PictureBox ImgBox_Line;
        private System.Windows.Forms.PictureBox ImgBox_Triangle_Rect;
        private System.Windows.Forms.Button OpenFile;
    }
}

