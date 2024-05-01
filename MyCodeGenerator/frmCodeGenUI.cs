using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CodeGenBusinessLayer;
using CodeGenDataLayer;
using GenerateDataAccessLayerLibrary;

namespace MyCodeGenerator
{
    public partial class frmCodeGenUI : Form
    {
        private string _tableName = string.Empty;
        private List<List<clsColumnInfoForDataAccess>>
        _columnsInfoForDataAccess = new List<List<clsColumnInfoForDataAccess>>();

        public void DummyData()
        {
            List<Tuple<string, string, bool>> columnInfoList = new List<Tuple<string, string, bool>>();

            // Adding dummy data for 12 records
            for (int i = 1; i <= 12; i++)
            {
                columnInfoList.Add(new Tuple<string, string, bool>($"Column{i}", $"DataType{i}", i % 2 == 0));
            }

            // Accessing the values
            foreach (var columnInfo in columnInfoList)
            {
                string columnName = columnInfo.Item1;
                string dataType = columnInfo.Item2;
                bool isNullable = columnInfo.Item3;

                //    Console.WriteLine($"Column Name: {columnName}, Data Type: {dataType}, Is Nullable: {isNullable}");
            }
        }

        public frmCodeGenUI()
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

                //listviewColumnsInfo.Columns.Clear();

                foreach (DataColumn column in data.Columns)
                {
                   // listviewColumnsInfo.Columns.Add(column.ColumnName);
                }

               // listviewColumnsInfo.Items.Clear();
                foreach (DataRow row in data.Rows)
                {
                    ListViewItem item = new ListViewItem(row.ItemArray.Select(x => x.ToString()).ToArray());
                   // listviewColumnsInfo.Items.Add(item);
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
              //  _FillColumnInfoForDataAccessObjectFromListView();
            }
        }

        //private void _FillColumnInfoForDataAccessObjectFromListView()
        //{
        //    _columnsInfoForDataAccess.Clear();

        //    for (int i = 0; i < listviewColumnsInfo.Items.Count; i++)
        //    {
        //        ListViewItem firstItem = listviewColumnsInfo.Items[i];

        //        if (firstItem.SubItems.Count > 0)
        //        {
        //            var columnInfo = new List<clsColumnInfoForDataAccess>
        //            {
        //                new clsColumnInfoForDataAccess
        //                {
        //                    ColumnName = firstItem.SubItems[0].Text,
        //                    DataType = firstItem.SubItems[1].Text,
        //                    IsNullable = firstItem.SubItems[2].Text.ToLower() == "yes"
        //                }
        //            };

        //            _columnsInfoForDataAccess.Add(columnInfo);
        //        }
        //    }
        //}

        private void _Reset()
        {
            comboDatabaseName.SelectedIndex = -1;
            // listviewColumnsInfo.Items.Clear();
            DGVTableInfo.DataSource = null;
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
                //String FirstValue = _columnsInfoForDataAccess[0][0].ColumnName;
                //MessageBox.Show("the Tabel Name =" + FirstValue.Remove(FirstValue.Length - 2));

                //to get test ;
                List<List<clsColumnInfoForDataAccess>> _columnsInfoForDataAccess = new List<List<clsColumnInfoForDataAccess>>();
                for (int i = 1; i <= 6; i++)
                {
                    List<clsColumnInfoForDataAccess> columnInfoList = new List<clsColumnInfoForDataAccess>();
                    columnInfoList.Add(new clsColumnInfoForDataAccess { ColumnName = $"Column{i}", DataType = $"int", IsNullable = i % 2 == 0 });
                    _columnsInfoForDataAccess.Add(columnInfoList);
                }


                txtGenText.Text =
                 clsSQL.
                    GenerateDataLayer(comboDatabaseName.Text, _columnsInfoForDataAccess);
            }

        }

        private void btnGenerateBusinessLayer_Click(object sender, EventArgs e)
        {
            if (int.Parse(lblNumberOfColumnsRecords.Text) <= 0)
            {
                MessageBox.Show("You have to select a column at least!", "Miss Data",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                //to test;
                //String FirstValue = _columnsInfoForDataAccess[0][0].ColumnName;
                //MessageBox.Show("the Tabel Name =" + FirstValue.Remove(FirstValue.Length - 2));

                //to get test ;
                List<List<clsColumnInfoForDataAccess>> _columnsInfoForDataAccess = new List<List<clsColumnInfoForDataAccess>>();
                for (int i = 1; i <= 6; i++)
                {
                    List<clsColumnInfoForDataAccess> columnInfoList = new List<clsColumnInfoForDataAccess>();
                    columnInfoList.Add(new clsColumnInfoForDataAccess { ColumnName = $"Column{i}", DataType = $"int", IsNullable = i % 2 == 0 });
                    _columnsInfoForDataAccess.Add(columnInfoList);
                }


                txtGenText.Text =
                 clsSQL.
                    GenerateBusinessLayer(comboDatabaseName.Text, _columnsInfoForDataAccess);

               // guna2TextBox1.Text =
               //clsSQL.
               //   GenerateBusinessLayer(comboDatabaseName.Text, _columnsInfoForDataAccess);
            }
        }

        private void btnGenerateStoredProcedure_Click(object sender, EventArgs e)
        {

            if (int.Parse(lblNumberOfColumnsRecords.Text) <= 0)
            {
                MessageBox.Show("You have to select a column at least!", "Miss Data",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                //to test;
                //String FirstValue = _columnsInfoForDataAccess[0][0].ColumnName;
                //MessageBox.Show("the Tabel Name =" + FirstValue.Remove(FirstValue.Length - 2));

                //to get test ;
                List<List<clsColumnInfoForDataAccess>> _columnsInfoForDataAccess = new List<List<clsColumnInfoForDataAccess>>();
                for (int i = 1; i <= 6; i++)
                {
                    List<clsColumnInfoForDataAccess> columnInfoList = new List<clsColumnInfoForDataAccess>();
                    columnInfoList.Add(new clsColumnInfoForDataAccess { ColumnName = $"Column{i}", DataType = $"int", IsNullable = i % 2 == 0 });
                    _columnsInfoForDataAccess.Add(columnInfoList);
                }


                txtGenText.Text =
                 clsSQL.
                    GenerateStoredProcedure(comboDatabaseName.Text, _columnsInfoForDataAccess);

            }
        }

        private void btnGenerateErrorLogger_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(comboDatabaseName.SelectedItem?.ToString()))
            {
                MessageBox.Show("You should choose a database.", "Information", MessageBoxButtons.OK, 
                    MessageBoxIcon.Information);
                return;
            }

           txtGenText.Text=
                clsSQL.GenerateErrorLogger(comboDatabaseName.SelectedItem.ToString());
        }

        private void GenerateLogHandler_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(comboDatabaseName.SelectedItem?.ToString()))
            {
                MessageBox.Show("You should choose a database.", "Information", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            txtGenText.Text =

           clsSQL.GenerateLogHandler(comboDatabaseName.SelectedItem.ToString());
        }

        private void btnGenerateDataAccessHelper_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(comboDatabaseName.SelectedItem?.ToString()))
            {
                MessageBox.Show("You should choose a database.", "Information", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }
            
            txtGenText.Text =

           clsSQL.GenerateDelegateHelperMethods(comboDatabaseName.SelectedItem.ToString());
        }
    }
}
