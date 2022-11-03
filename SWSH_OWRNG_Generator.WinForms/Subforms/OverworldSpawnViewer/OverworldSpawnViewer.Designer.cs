namespace SWSH_OWRNG_Generator.WinForms
{
    partial class OverworldSpawnViewer
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
            this.ReadSpawns = new System.Windows.Forms.Button();
            this.OverworldSpawnResults = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.OverworldSpawnResults)).BeginInit();
            this.SuspendLayout();
            // 
            // ReadSpawns
            // 
            this.ReadSpawns.Location = new System.Drawing.Point(12, 12);
            this.ReadSpawns.Name = "ReadSpawns";
            this.ReadSpawns.Size = new System.Drawing.Size(75, 23);
            this.ReadSpawns.TabIndex = 0;
            this.ReadSpawns.Text = "Read Spawns";
            this.ReadSpawns.UseVisualStyleBackColor = true;
            this.ReadSpawns.Click += new System.EventHandler(this.ReadSpawns_Click);
            // 
            // OverworldSpawnResults
            // 
            this.OverworldSpawnResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.OverworldSpawnResults.Location = new System.Drawing.Point(12, 54);
            this.OverworldSpawnResults.Name = "OverworldSpawnResults";
            this.OverworldSpawnResults.ReadOnly = true;
            this.OverworldSpawnResults.RowTemplate.Height = 25;
            this.OverworldSpawnResults.Size = new System.Drawing.Size(776, 384);
            this.OverworldSpawnResults.TabIndex = 1;
            // 
            // OverworldSpawnViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.OverworldSpawnResults);
            this.Controls.Add(this.ReadSpawns);
            this.Name = "OverworldSpawnViewer";
            this.Text = "OverworldSpawnViewer";
            ((System.ComponentModel.ISupportInitialize)(this.OverworldSpawnResults)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ReadSpawns;
        private System.Windows.Forms.DataGridView OverworldSpawnResults;
    }
}