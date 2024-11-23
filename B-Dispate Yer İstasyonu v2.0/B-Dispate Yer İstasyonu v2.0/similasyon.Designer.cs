namespace B_Dispate_Yer_İstasyonu_v2._0
{
    partial class similasyon
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.glControl1 = new OpenTK.GLControl();
            this.SuspendLayout();
            // 
            // glControl1
            // 
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Location = new System.Drawing.Point(15, 15);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(1800, 780);
            this.glControl1.TabIndex = 0;
            this.glControl1.VSync = false;
            // 
            // similasyon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.glControl1);
            this.Name = "similasyon";
            this.Size = new System.Drawing.Size(1830, 805);
            this.ResumeLayout(false);

        }

        #endregion

        public OpenTK.GLControl glControl1;
    }
}
