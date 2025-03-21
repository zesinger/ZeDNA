namespace ZeDNA
{
    /// <summary>
    /// Classe de la description de la fenêtre principale
    /// </summary>
    partial class MainForm
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.listZones = new System.Windows.Forms.ListView();
            this.columnZone = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnOpen1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnClose1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnOpen2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnClose2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnOpen3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnClose3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnOpen4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnClose4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button1 = new System.Windows.Forms.Button();
            this.checkNotif = new System.Windows.Forms.CheckBox();
            this.textDate = new System.Windows.Forms.TextBox();
            this.checkSound = new System.Windows.Forms.CheckBox();
            this.panelMap = new System.Windows.Forms.Panel();
            this.buttonPrint = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textPlafond = new System.Windows.Forms.TextBox();
            this.textPlancher = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.printDocument1 = new System.Drawing.Printing.PrintDocument();
            this.textZone = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // listZones
            // 
            this.listZones.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnZone,
            this.columnOpen1,
            this.columnClose1,
            this.columnOpen2,
            this.columnClose2,
            this.columnOpen3,
            this.columnClose3,
            this.columnOpen4,
            this.columnClose4});
            this.listZones.FullRowSelect = true;
            this.listZones.GridLines = true;
            this.listZones.HideSelection = false;
            this.listZones.Location = new System.Drawing.Point(17, 39);
            this.listZones.Name = "listZones";
            this.listZones.OwnerDraw = true;
            this.listZones.Size = new System.Drawing.Size(881, 297);
            this.listZones.TabIndex = 0;
            this.listZones.UseCompatibleStateImageBehavior = false;
            this.listZones.View = System.Windows.Forms.View.Details;
            // 
            // columnZone
            // 
            this.columnZone.Text = "Zone";
            this.columnZone.Width = 300;
            // 
            // columnOpen1
            // 
            this.columnOpen1.Text = "Ouverture 1";
            this.columnOpen1.Width = 70;
            // 
            // columnClose1
            // 
            this.columnClose1.Text = "Fermeture1";
            this.columnClose1.Width = 70;
            // 
            // columnOpen2
            // 
            this.columnOpen2.Text = "Ouverture 2";
            this.columnOpen2.Width = 70;
            // 
            // columnClose2
            // 
            this.columnClose2.Text = "Fermeture 2";
            this.columnClose2.Width = 70;
            // 
            // columnOpen3
            // 
            this.columnOpen3.Text = "Ouverture 3";
            this.columnOpen3.Width = 70;
            // 
            // columnClose3
            // 
            this.columnClose3.Text = "Fermeture 3";
            this.columnClose3.Width = 70;
            // 
            // columnOpen4
            // 
            this.columnOpen4.Text = "Ouverture 4";
            this.columnOpen4.Width = 70;
            // 
            // columnClose4
            // 
            this.columnClose4.Text = "Fermeture 4";
            this.columnClose4.Width = 70;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(932, 77);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(229, 44);
            this.button1.TabIndex = 1;
            this.button1.Text = "Mise à jour";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // checkNotif
            // 
            this.checkNotif.AutoSize = true;
            this.checkNotif.Checked = true;
            this.checkNotif.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkNotif.Location = new System.Drawing.Point(943, 230);
            this.checkNotif.Name = "checkNotif";
            this.checkNotif.Size = new System.Drawing.Size(206, 17);
            this.checkNotif.TabIndex = 2;
            this.checkNotif.Text = "Notification activations/désactivations";
            this.checkNotif.UseVisualStyleBackColor = true;
            this.checkNotif.CheckedChanged += new System.EventHandler(this.checkNotif_CheckedChanged);
            // 
            // textDate
            // 
            this.textDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textDate.Location = new System.Drawing.Point(17, 12);
            this.textDate.Name = "textDate";
            this.textDate.ReadOnly = true;
            this.textDate.Size = new System.Drawing.Size(881, 26);
            this.textDate.TabIndex = 3;
            this.textDate.Text = "Pas de données chargées encore...";
            this.textDate.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // checkSound
            // 
            this.checkSound.AutoSize = true;
            this.checkSound.Checked = true;
            this.checkSound.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkSound.Location = new System.Drawing.Point(980, 253);
            this.checkSound.Name = "checkSound";
            this.checkSound.Size = new System.Drawing.Size(123, 17);
            this.checkSound.TabIndex = 4;
            this.checkSound.Text = "+ Notification sonore";
            this.checkSound.UseVisualStyleBackColor = true;
            // 
            // panelMap
            // 
            this.panelMap.BackColor = System.Drawing.Color.Transparent;
            this.panelMap.Location = new System.Drawing.Point(341, 342);
            this.panelMap.Name = "panelMap";
            this.panelMap.Size = new System.Drawing.Size(500, 500);
            this.panelMap.TabIndex = 5;
            // 
            // buttonPrint
            // 
            this.buttonPrint.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPrint.Location = new System.Drawing.Point(932, 151);
            this.buttonPrint.Name = "buttonPrint";
            this.buttonPrint.Size = new System.Drawing.Size(229, 44);
            this.buttonPrint.TabIndex = 6;
            this.buttonPrint.Text = "Imprimer le tableau";
            this.buttonPrint.UseVisualStyleBackColor = true;
            this.buttonPrint.Click += new System.EventHandler(this.buttonPrint_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(145, 580);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Plafond:";
            // 
            // textPlafond
            // 
            this.textPlafond.Location = new System.Drawing.Point(197, 577);
            this.textPlafond.Name = "textPlafond";
            this.textPlafond.ReadOnly = true;
            this.textPlafond.Size = new System.Drawing.Size(76, 20);
            this.textPlafond.TabIndex = 8;
            // 
            // textPlancher
            // 
            this.textPlancher.Location = new System.Drawing.Point(197, 616);
            this.textPlancher.Name = "textPlancher";
            this.textPlancher.ReadOnly = true;
            this.textPlancher.Size = new System.Drawing.Size(76, 20);
            this.textPlancher.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(145, 619);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Plancher:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(194, 600);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "-------------------------";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(145, 536);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Zone:";
            // 
            // textZone
            // 
            this.textZone.Location = new System.Drawing.Point(197, 533);
            this.textZone.Name = "textZone";
            this.textZone.ReadOnly = true;
            this.textZone.Size = new System.Drawing.Size(76, 20);
            this.textZone.TabIndex = 13;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1173, 852);
            this.Controls.Add(this.textZone);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textPlancher);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textPlafond);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonPrint);
            this.Controls.Add(this.panelMap);
            this.Controls.Add(this.checkSound);
            this.Controls.Add(this.textDate);
            this.Controls.Add(this.checkNotif);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.listZones);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainForm";
            this.Text = "Récupération automatique des ouvertures des zones basse altitude";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public System.Windows.Forms.CheckBox checkNotif;
        public System.Windows.Forms.ListView listZones;
        public System.Windows.Forms.Button button1;
        public System.Windows.Forms.ColumnHeader columnZone;
        public System.Windows.Forms.ColumnHeader columnOpen1;
        public System.Windows.Forms.ColumnHeader columnClose1;
        public System.Windows.Forms.ColumnHeader columnOpen2;
        public System.Windows.Forms.ColumnHeader columnClose2;
        public System.Windows.Forms.ColumnHeader columnOpen3;
        public System.Windows.Forms.ColumnHeader columnClose3;
        public System.Windows.Forms.ColumnHeader columnOpen4;
        public System.Windows.Forms.ColumnHeader columnClose4;
        public System.Windows.Forms.TextBox textDate;
        public System.Windows.Forms.CheckBox checkSound;
        public System.Windows.Forms.Panel panelMap;
        public System.Windows.Forms.Button buttonPrint;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox textPlafond;
        public System.Windows.Forms.TextBox textPlancher;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Drawing.Printing.PrintDocument printDocument1;
        public System.Windows.Forms.TextBox textZone;
    }
}

