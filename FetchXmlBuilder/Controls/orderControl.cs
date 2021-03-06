﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cinteros.Xrm.FetchXmlBuilder.AppCode;
using Cinteros.Xrm.XmlEditorUtils;
using Microsoft.Xrm.Sdk.Metadata;

namespace Cinteros.Xrm.FetchXmlBuilder.Controls
{
    public partial class orderControl : UserControl, IDefinitionSavable
    {
        private readonly Dictionary<string, string> collec;
        private string controlsCheckSum = "";
        private bool friendly;
        FetchXmlBuilder form;

        #region Delegates

        public delegate void SaveEventHandler(object sender, SaveEventArgs e);

        #endregion

        #region Event Handlers

        public event SaveEventHandler Saved;

        #endregion

        public orderControl()
        {
            InitializeComponent();
            collec = new Dictionary<string, string>();
        }

        public orderControl(TreeNode Node, AttributeMetadata[] attributes, FetchXmlBuilder fetchXmlBuilder)
            : this()
        {
            form = fetchXmlBuilder;
            friendly = fetchXmlBuilder.currentSettings.useFriendlyNames;
            collec = (Dictionary<string, string>)Node.Tag;
            if (collec == null)
            {
                collec = new Dictionary<string, string>();
            }

            PopulateControls(Node, attributes);
            ControlUtils.FillControls(collec, this.Controls);
            controlsCheckSum = ControlUtils.ControlsChecksum(this.Controls);
            Saved += fetchXmlBuilder.CtrlSaved;
        }

        private void PopulateControls(TreeNode node, AttributeMetadata[] attributes)
        {
            var aggregate = FetchXmlBuilder.IsFetchAggregate(node);
            if (!aggregate)
            {
                cmbAttribute.Items.Clear();
                if (attributes != null)
                {
                    foreach (var attribute in attributes)
                    {
                        AttributeItem.AddAttributeToComboBox(cmbAttribute, attribute, false, friendly);
                    }
                }
            }
            else
            {
                cmbAlias.Items.Clear();
                cmbAlias.Items.Add("");
                cmbAlias.Items.AddRange(GetAliases(form.tvFetch.Nodes[0]).ToArray());
            }
            cmbAttribute.Enabled = !aggregate;
            cmbAlias.Enabled = aggregate;
        }

        private List<string> GetAliases(TreeNode node)
        {
            var result = new List<string>();
            if (node.Name == "entity" || node.Name == "link-entity")
            {
                foreach (TreeNode child in node.Nodes)
                {
                    if (child.Name == "attribute")
                    {
                        var alias = TreeNodeHelper.GetAttributeFromNode(child, "alias");
                        if (!string.IsNullOrEmpty(alias))
                        {
                            result.Add(alias);
                        }
                    }
                }
            }
            foreach (TreeNode child in node.Nodes)
            {
                result.AddRange(GetAliases(child));
            }
            return result;
        }

        public void Save()
        {
            try
            {
                Dictionary<string, string> collection = ControlUtils.GetAttributesCollection(this.Controls, true);
                SendSaveMessage(collection);
            }
            catch (ArgumentNullException ex)
            {
                MessageBox.Show(ex.Message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            controlsCheckSum = ControlUtils.ControlsChecksum(this.Controls);
        }

        /// <summary>
        /// Sends a connection success message 
        /// </summary>
        /// <param name="service">IOrganizationService generated</param>
        /// <param name="parameters">Lsit of parameter</param>
        private void SendSaveMessage(Dictionary<string, string> collection)
        {
            SaveEventArgs sea = new SaveEventArgs { AttributeCollection = collection };

            if (Saved != null)
            {
                Saved(this, sea);
            }
        }

        private void Control_Leave(object sender, EventArgs e)
        {
            if (controlsCheckSum != ControlUtils.ControlsChecksum(this.Controls))
            {
                Save();
            }
        }
    }
}
