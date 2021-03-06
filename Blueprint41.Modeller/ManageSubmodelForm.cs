﻿using Blueprint41.Modeller.Schemas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Model = Blueprint41.Modeller.Schemas.Modeller;

namespace Blueprint41.Modeller
{
    public partial class ManageSubmodelForm : Form
    {
        public Submodel Submodel { get; private set; }

        public ManageSubmodelForm(Model modeller) : this(new Submodel(modeller))
        {
            Text = "Add Submodel";
        }

        public ManageSubmodelForm(Submodel submodel)
        {
            InitializeComponent();
            Submodel = submodel;
            AssignToUi();
            Text = "Edit Submodel";
        }

        private void AssignToUi()
        {
            txtName.Text = Submodel.Name;
            numericChapter.Value = Submodel.Chapter ?? 0;
            chkIsDraft.Checked = Submodel.IsDraft;
            chkIsLaboratory.Checked = Submodel.IsLaboratory;

            if (!string.IsNullOrEmpty(Submodel.Explaination))
                hteExplaination.BodyHtml = ToNormalString(Submodel.Explaination);

            btnRemove.Enabled = btnAdd.Enabled = false;

            lbAvailable.Items.Clear();

            foreach (Entity entity in Submodel.Model.Entities.Entity.Where(item => !Submodel.Entities.Any(entity => entity.Label == item.Label)).OrderBy(item => item.Label))
            {
                lbAvailable.Items.Add(entity.Label);
            }

            lbExisting.Items.Clear();

            foreach (Entity entity in Submodel.Entities.OrderBy(item => item.Label))
            {
                lbExisting.Items.Add(entity.Label);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Submodel.Name = txtName.Text;
            Submodel.Chapter = numericChapter.Value == 0 ? null : (int?)numericChapter.Value;
            Submodel.IsDraft = chkIsDraft.Checked;
            Submodel.IsLaboratory = chkIsLaboratory.Checked;
            Submodel.Explaination = ToXmlEscape(hteExplaination.GetHTMLAsText());

            foreach (Submodel.NodeLocalType node in Submodel.Node.Where(item => !lbExisting.Items.Contains(item.Label)).ToList())
            {
                Submodel.Node.Remove(node);
            }

            foreach (string item in lbExisting.Items.Cast<string>().Where(item => !Submodel.Node.Any(node=> node.Label == item)).ToList())
            {
                Submodel.AddEntities(Submodel.Model.Entities.Entity.Where(entity => entity.Label == item).ToList(), 0, 0);
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        public static string ToXmlEscape(string self)
        {
            if (string.IsNullOrWhiteSpace(self))
                return self;

            return self.Replace("&", "&amp;")
                .Replace("\"", "&quot;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("'", "&apos;");
                
        }

        public static string ToNormalString(string self)
        {
            if (string.IsNullOrWhiteSpace(self))
                return self;

            return self.Replace("&amp;", "&")
                .Replace("&quot;", "\"")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&apos;", "'");
        }

        private void lbExisting_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRemove.Enabled = lbExisting.SelectedItems.Count > 0;
        }

        private void lbAvailable_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnAdd.Enabled = lbAvailable.SelectedItems.Count > 0;
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            foreach (string item in lbExisting.SelectedItems.Cast<string>().ToList())
            {
                lbAvailable.Items.Add(item);
                lbExisting.Items.Remove(item);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            foreach (string item in lbAvailable.SelectedItems.Cast<string>().ToList())
            {
                lbExisting.Items.Add(item);
                lbAvailable.Items.Remove(item);
            }
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = !string.IsNullOrEmpty(txtName.Text) && !Submodel.Model.Submodels.Submodel.Where(item => item.Name != Submodel.Name).Any(item => item.Name == txtName.Text);
        }

        private void buttonRemoveAll_Click(object sender, EventArgs e)
        {
            foreach (string item in lbExisting.Items.Cast<string>().ToList())
            {
                lbAvailable.Items.Add(item);
                lbExisting.Items.Remove(item);
            }
        }

        private void buttonAddAll_Click(object sender, EventArgs e)
        {
            foreach (string item in lbAvailable.Items.Cast<string>().ToList())
            {
                lbExisting.Items.Add(item);
                lbAvailable.Items.Remove(item);
            }
        }

    }
}
