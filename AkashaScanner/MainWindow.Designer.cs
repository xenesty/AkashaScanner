namespace AkashaScanner
{
    partial class MainWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.blazorWebView = new Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView();
            this.loading = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // blazorWebView
            // 
            this.blazorWebView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.blazorWebView.Location = new System.Drawing.Point(0, 0);
            this.blazorWebView.Name = "blazorWebView";
            this.blazorWebView.Size = new System.Drawing.Size(1262, 673);
            this.blazorWebView.TabIndex = 0;
            this.blazorWebView.Text = "blazorWebView";
            // 
            // loading
            // 
            this.loading.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.loading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loading.Location = new System.Drawing.Point(0, 0);
            this.loading.Name = "loading";
            this.loading.Size = new System.Drawing.Size(1262, 673);
            this.loading.TabIndex = 1;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1262, 673);
            this.Controls.Add(this.loading);
            this.Controls.Add(this.blazorWebView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(1280, 720);
            this.Name = "MainWindow";
            this.Text = "Akasha Scanner";
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.AspNetCore.Components.WebView.WindowsForms.BlazorWebView blazorWebView;
        private System.Windows.Forms.Panel loading;
    }
}
