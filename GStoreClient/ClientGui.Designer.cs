namespace GStoreClient
{
    partial class ClientGui
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.registerBt = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // registerBt
            // 
            this.registerBt.AccessibleName = "";
            this.registerBt.Location = new System.Drawing.Point(74, 61);
            this.registerBt.Name = "registerBt";
            this.registerBt.Size = new System.Drawing.Size(86, 50);
            this.registerBt.TabIndex = 0;
            this.registerBt.Text = "Register";
            this.registerBt.UseVisualStyleBackColor = true;
            this.registerBt.Click += new System.EventHandler(this.register_on_click);
            // 
            // ClientGui
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.registerBt);
            this.Name = "ClientGui";
            this.Text = "Welcome";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button registerBt;
    }
}

