using System.ComponentModel;
using System.Windows.Forms;

namespace CSFtoSTR
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
            this.buttonConvert = new System.Windows.Forms.Button();
            this.InputTextBox = new System.Windows.Forms.TextBox();
            this.OutputTextBox = new System.Windows.Forms.TextBox();
            this.labelInput = new System.Windows.Forms.Label();
            this.labelOutput = new System.Windows.Forms.Label();
            this.InputButton = new System.Windows.Forms.Button();
            this.OutputButton = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // buttonConvert
            // 
            this.buttonConvert.Location = new System.Drawing.Point(475, 75);
            this.buttonConvert.Name = "buttonConvert";
            this.buttonConvert.Size = new System.Drawing.Size(125, 24);
            this.buttonConvert.TabIndex = 0;
            this.buttonConvert.Text = "Convert";
            this.buttonConvert.UseVisualStyleBackColor = true;
            this.buttonConvert.Click += new System.EventHandler(this.GoButton_Click);
            // 
            // InputTextBox
            // 
            this.InputTextBox.Location = new System.Drawing.Point(60, 12);
            this.InputTextBox.Name = "InputTextBox";
            this.InputTextBox.ReadOnly = true;
            this.InputTextBox.Size = new System.Drawing.Size(503, 20);
            this.InputTextBox.TabIndex = 1;
            // 
            // OutputTextBox
            // 
            this.OutputTextBox.Location = new System.Drawing.Point(60, 48);
            this.OutputTextBox.Name = "OutputTextBox";
            this.OutputTextBox.ReadOnly = true;
            this.OutputTextBox.Size = new System.Drawing.Size(503, 20);
            this.OutputTextBox.TabIndex = 2;
            // 
            // labelInput
            // 
            this.labelInput.AutoSize = true;
            this.labelInput.Location = new System.Drawing.Point(20, 15);
            this.labelInput.Name = "labelInput";
            this.labelInput.Size = new System.Drawing.Size(34, 13);
            this.labelInput.TabIndex = 3;
            this.labelInput.Text = "Input:";
            // 
            // labelOutput
            // 
            this.labelOutput.AutoSize = true;
            this.labelOutput.Location = new System.Drawing.Point(12, 51);
            this.labelOutput.Name = "labelOutput";
            this.labelOutput.Size = new System.Drawing.Size(42, 13);
            this.labelOutput.TabIndex = 4;
            this.labelOutput.Text = "Output:";
            // 
            // InputButton
            // 
            this.InputButton.Location = new System.Drawing.Point(569, 12);
            this.InputButton.Name = "InputButton";
            this.InputButton.Size = new System.Drawing.Size(31, 23);
            this.InputButton.TabIndex = 5;
            this.InputButton.Text = "...";
            this.InputButton.UseVisualStyleBackColor = true;
            this.InputButton.Click += new System.EventHandler(this.InputButton_Click);
            // 
            // OutputButton
            // 
            this.OutputButton.Location = new System.Drawing.Point(569, 46);
            this.OutputButton.Name = "OutputButton";
            this.OutputButton.Size = new System.Drawing.Size(31, 23);
            this.OutputButton.TabIndex = 6;
            this.OutputButton.Text = "...";
            this.OutputButton.UseVisualStyleBackColor = true;
            this.OutputButton.Click += new System.EventHandler(this.OutputButton_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog1";
            this.openFileDialog.Filter = "CSF files (*.csf)|*.csf";
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.Filter = "STR files (*.str)|*.str";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(612, 108);
            this.Controls.Add(this.OutputButton);
            this.Controls.Add(this.InputButton);
            this.Controls.Add(this.labelOutput);
            this.Controls.Add(this.labelInput);
            this.Controls.Add(this.OutputTextBox);
            this.Controls.Add(this.InputTextBox);
            this.Controls.Add(this.buttonConvert);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CSF to STR Converter (1.0.0)";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button buttonConvert;
        private TextBox InputTextBox;
        private TextBox OutputTextBox;
        private Label labelInput;
        private Label labelOutput;
        private Button InputButton;
        private Button OutputButton;
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;
    }
}