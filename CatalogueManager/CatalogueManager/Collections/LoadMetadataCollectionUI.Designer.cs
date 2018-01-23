﻿using BrightIdeasSoftware;
using CatalogueManager.Refreshing;

namespace CatalogueManager.Collections
{
    partial class LoadMetadataCollectionUI : ILifetimeSubscriber
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
            this.components = new System.ComponentModel.Container();
            this.tlvLoadMetadata = new BrightIdeasSoftware.TreeListView();
            this.olvName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            ((System.ComponentModel.ISupportInitialize)(this.tlvLoadMetadata)).BeginInit();
            this.SuspendLayout();
            // 
            // tlvLoadMetadata
            // 
            this.tlvLoadMetadata.AllColumns.Add(this.olvName);
            this.tlvLoadMetadata.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClick;
            this.tlvLoadMetadata.CellEditUseWholeCell = false;
            this.tlvLoadMetadata.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvName});
            this.tlvLoadMetadata.Cursor = System.Windows.Forms.Cursors.Default;
            this.tlvLoadMetadata.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlvLoadMetadata.Location = new System.Drawing.Point(0, 0);
            this.tlvLoadMetadata.Name = "tlvLoadMetadata";
            this.tlvLoadMetadata.ShowGroups = false;
            this.tlvLoadMetadata.Size = new System.Drawing.Size(500, 600);
            this.tlvLoadMetadata.TabIndex = 0;
            this.tlvLoadMetadata.UseCompatibleStateImageBehavior = false;
            this.tlvLoadMetadata.View = System.Windows.Forms.View.Details;
            this.tlvLoadMetadata.VirtualMode = true;
            this.tlvLoadMetadata.ItemActivate += new System.EventHandler(this.otvLoadMetadata_ItemActivate);
            this.tlvLoadMetadata.KeyUp += new System.Windows.Forms.KeyEventHandler(this.otvLoadMetadata_KeyUp);
            // 
            // olvName
            // 
            this.olvName.AspectName = "ToString";
            this.olvName.FillsFreeSpace = true;
            this.olvName.Text = "Load Metadata";
            // 
            // LoadMetadataCollectionUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tlvLoadMetadata);
            this.Name = "LoadMetadataCollectionUI";
            this.Size = new System.Drawing.Size(500, 600);
            ((System.ComponentModel.ISupportInitialize)(this.tlvLoadMetadata)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private TreeListView tlvLoadMetadata;
        private OLVColumn olvName;
    }
}
