namespace ZeDNA
{
    partial class AddCreneau
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
            this.comboZones = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.timeStart = new System.Windows.Forms.DateTimePicker();
            this.timeEnd = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.numericCreneau = new System.Windows.Forms.NumericUpDown();
            this.labelCreneau = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericCreneau)).BeginInit();
            this.SuspendLayout();
            // 
            // comboZones
            // 
            this.comboZones.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboZones.FormattingEnabled = true;
            this.comboZones.Location = new System.Drawing.Point(55, 12);
            this.comboZones.Name = "comboZones";
            this.comboZones.Size = new System.Drawing.Size(155, 21);
            this.comboZones.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Zone:";
            // 
            // timeStart
            // 
            this.timeStart.CustomFormat = "HH:mm";
            this.timeStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.timeStart.Location = new System.Drawing.Point(17, 64);
            this.timeStart.Name = "timeStart";
            this.timeStart.ShowUpDown = true;
            this.timeStart.Size = new System.Drawing.Size(53, 20);
            this.timeStart.TabIndex = 2;
            // 
            // timeEnd
            // 
            this.timeEnd.CustomFormat = "HH:mm";
            this.timeEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.timeEnd.Location = new System.Drawing.Point(157, 64);
            this.timeEnd.Name = "timeEnd";
            this.timeEnd.ShowUpDown = true;
            this.timeEnd.Size = new System.Drawing.Size(53, 20);
            this.timeEnd.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Début:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(186, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(24, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Fin:";
            // 
            // buttonAdd
            // 
            this.buttonAdd.Location = new System.Drawing.Point(17, 96);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(82, 35);
            this.buttonAdd.TabIndex = 6;
            this.buttonAdd.Text = "Ajouter";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(128, 96);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(82, 35);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "Annuler";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // numericCreneau
            // 
            this.numericCreneau.Location = new System.Drawing.Point(100, 64);
            this.numericCreneau.Name = "numericCreneau";
            this.numericCreneau.ReadOnly = true;
            this.numericCreneau.Size = new System.Drawing.Size(28, 20);
            this.numericCreneau.TabIndex = 8;
            this.numericCreneau.ValueChanged += new System.EventHandler(this.numericCreneau_ValueChanged);
            // 
            // labelCreneau
            // 
            this.labelCreneau.AutoSize = true;
            this.labelCreneau.Location = new System.Drawing.Point(91, 48);
            this.labelCreneau.Name = "labelCreneau";
            this.labelCreneau.Size = new System.Drawing.Size(47, 13);
            this.labelCreneau.TabIndex = 9;
            this.labelCreneau.Text = "Créneau";
            // 
            // AddCreneau
            // 
            this.AcceptButton = this.buttonAdd;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(224, 143);
            this.Controls.Add(this.labelCreneau);
            this.Controls.Add(this.numericCreneau);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.timeEnd);
            this.Controls.Add(this.timeStart);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboZones);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddCreneau";
            this.Text = "Ajouter/Supprimer des créneaux";
            ((System.ComponentModel.ISupportInitialize)(this.numericCreneau)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboZones;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.DateTimePicker timeStart;
        public System.Windows.Forms.DateTimePicker timeEnd;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonCancel;
        public System.Windows.Forms.NumericUpDown numericCreneau;
        public System.Windows.Forms.Label labelCreneau;
    }
}