using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using InventorySystem.Helpers;
using InventorySystem.Services;

namespace InventorySystem.Forms
{
    /// <summary>Read-only list of raw milk received, with supplier and how much is left of each delivery.</summary>
    public class MilkIntakesDialog : BaseModalForm
    {
        public MilkIntakesDialog()
        {
            Func<string, string, string> L = LocalizationManager.GetString;
            this.ClientSize = new Size(900, 560);
            this.TitleText = L("Milk_ReceivedBySupplier", "Milk received (by supplier)");

            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = ThemeConfig.SurfaceColor,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Size = new Size(860, 420)
            };
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = L("Exp_Date", "Date"), DataPropertyName = "expense_date", Width = 140 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = L("Milk_Supplier", "Supplier"), DataPropertyName = "supplier_name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = L("Milk_Type", "Milk type"), DataPropertyName = "milk_type", Width = 100 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = L("Milk_Liters", "Liters"), DataPropertyName = "liters", Width = 90, DefaultCellStyle = { Format = "0.##" } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = L("Milk_Used", "Used"), DataPropertyName = "used", Width = 90, DefaultCellStyle = { Format = "0.##" } });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = L("Milk_Remaining", "Remaining"), DataPropertyName = "remaining", Width = 100, DefaultCellStyle = { Format = "0.##" } });
            ThemeConfig.ApplyGridTheme(dgv);
            dgv.DataSource = new MilkProductionService().GetMilkIntakes(false);

            this.ContentPanel.Controls.Add(dgv);
            LocalizationManager.ApplyRTL(this);

            SetFooterButtons(null, L("AddPart_Cancel", "Close"), null, (s, e) => this.Close());
        }
    }
}
