﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CodeGenBusinessLayer;
using GenerateDataAccessLayerLibrary;

namespace MyCodeGenerator
{
    public partial class Form1 : Form
    {
        private string _tableName = string.Empty;
        private List<List<clsColumnInfoForDataAccess>>
            _columnsInfoForDataAccess = new List<List<clsColumnInfoForDataAccess>>();

        public Form1()
        {
            InitializeComponent();
            FillDatabaseNames();
        }

        private void FillDatabaseNames()
        {
            DataTable dtDatabase = clsSQL.GetAllDatabaseNames();
            foreach (DataRow row in dtDatabase.Rows)
            {
                comboDatabaseName.Items.Add(row["DatabaseName"]);
            }
        }

        private void comboDatabaseName_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable data = clsSQL.GetTablesNameByDB(comboDatabaseName.Text);
            DGVTablesName.DataSource = data;

            if (data != null && data.Rows.Count > 0)
            {
                lblNumberOfTablesRecords.Text = data.Rows.Count.ToString();
            }
            else
            {
                MessageBox.Show("No tables found in the selected database");
            }
        }

        private void DGVTablesName_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                string tableName = DGVTablesName.Rows[e.RowIndex].Cells[0].Value.ToString();
                string dbName = comboDatabaseName.SelectedItem.ToString();

                DataTable data = clsSQL.GetTableInfo(tableName, dbName);
                DGVTableInfo.DataSource = data;

                listviewColumnsInfo.Columns.Clear();

                foreach (DataColumn column in data.Columns)
                {
                    listviewColumnsInfo.Columns.Add(column.ColumnName);
                }

                listviewColumnsInfo.Items.Clear();
                foreach (DataRow row in data.Rows)
                {
                    ListViewItem item = new ListViewItem(row.ItemArray.Select(x => x.ToString()).ToArray());
                    listviewColumnsInfo.Items.Add(item);
                }

                lblNumberOfColumnsRecords.Text = data.Rows.Count.ToString();
            }
        }
      
        private void listviewColumnsInfo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DGVTablesName.SelectedRows.Count > 0)
            {
                txtGenText.Clear();
                _tableName = DGVTablesName.SelectedRows[0].Cells[0].Value.ToString();
                _FillColumnInfoForDataAccessObjectFromListView();
            }
        }

        private void _FillColumnInfoForDataAccessObjectFromListView()
        {
            _columnsInfoForDataAccess.Clear();

            for (int i = 0; i < listviewColumnsInfo.Items.Count; i++)
            {
                ListViewItem firstItem = listviewColumnsInfo.Items[i];

                if (firstItem.SubItems.Count > 0)
                {
                    var columnInfo = new List<clsColumnInfoForDataAccess>
                    {
                        new clsColumnInfoForDataAccess
                        {
                            ColumnName = firstItem.SubItems[0].Text,
                            DataType = firstItem.SubItems[1].Text,
                            IsNullable = firstItem.SubItems[2].Text.ToLower() == "yes"
                        }
                    };

                    _columnsInfoForDataAccess.Add(columnInfo);
                }
            }
        }

        private void _Reset()
        {
            comboDatabaseName.SelectedIndex = -1;
            listviewColumnsInfo.Items.Clear();
            DGVTablesName.DataSource = null;
            txtGenText.Clear();
            lblNumberOfColumnsRecords.Text = "0";
            lblNumberOfTablesRecords.Text = "0";
        }

        private void guna2PictureBox4_Click_1(object sender, EventArgs e)
        {
            _Reset();
        }

        private void btnCopyGenText_Click_1(object sender, EventArgs e)
        {
            if (txtGenText.Text == null)
                return;
            else

            Clipboard.SetText(txtGenText.Text);
            MessageBox.Show("Text copied to clipboard.");
        }

        private void guna2PictureBox2_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnGenerateDataAccessSettings_Click(object sender, EventArgs e)
        {
            if (int.TryParse(lblNumberOfColumnsRecords.Text, out int numberOfColumns) && numberOfColumns > 0)
            {
               
                txtGenText.Text = clsSQL.GenerateDataAccessSettings
                    (comboDatabaseName.Text);
            }
            else
            {
                MessageBox.Show("You have to select a column at least!", "Miss Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGenerateDateAccessLayer_Click(object sender, EventArgs e)
        {
            if (int.Parse(lblNumberOfColumnsRecords.Text) <= 0)
            {
                MessageBox.Show("You have to select a column at least!", "Miss Data",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                //to test;
                String FirstValue = _columnsInfoForDataAccess[0][0].ColumnName;
                MessageBox.Show("the Tabel Name =" + FirstValue.Remove(FirstValue.Length - 2));

                txtGenText.Text =
                 clsSQL.
                    GenerateDataLayer(comboDatabaseName.Text, _columnsInfoForDataAccess);
            }

        }
    }
}