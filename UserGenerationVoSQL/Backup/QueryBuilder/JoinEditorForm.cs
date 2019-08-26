using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FastReport.FastQueryBuilder
{
    internal partial class JoinEditorForm : Form
    {
        internal Link link;

        public JoinEditorForm()
        {
            InitializeComponent();        
        }

        private void JoinEditorForm_Load(object sender, EventArgs e)
        {
            label1.Text = link.From.Table.Name;
            label2.Text = link.To.Table.Name;

            label6.Text = link.From.Name;
            label5.Text = link.To.Name;

            comboBox2.DataSource = QueryEnums.JoinTypesToStr;
            comboBox2.SelectedIndex = (int)link.Join;
            
            comboBox1.DataSource = QueryEnums.WhereTypesToStr[0];
            comboBox1.SelectedIndex = (int)link.Where;
        }

        private void JoinEditorForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                link.Where = (QueryEnums.WhereTypes)comboBox1.SelectedIndex;
                link.Join = (QueryEnums.JoinTypes)comboBox2.SelectedIndex;
            }
        }
    }
}