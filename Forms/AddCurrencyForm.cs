using System;
using System.Windows.Forms;
using InventorySystem.Helpers;
using InventorySystem.Services;

namespace InventorySystem.Forms
{
    public partial class AddCurrencyForm : BaseModalForm
    {
        public AddCurrencyForm()
        {
            InitializeComponent();
            
            this.TitleText = LocalizationManager.GetString("Curr_AddBtn");
            ApplyLocalization();
            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();

            btnFetch.Click += btnFetch_Click;
        }

        private async void btnFetch_Click(object sender, EventArgs e)
        {
            string code = txtCode.Text.Trim().ToUpper();
            if (string.IsNullOrEmpty(code))
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("AddCurr_EnterCodeFirst", "Please enter currency code first"));
                return;
            }

            btnFetch.Enabled = false;
            btnFetch.Text = "...";

            try
            {
                var rate = await CurrencyService.FetchRateAsync(code);
                if (rate.HasValue)
                {
                    numRate.Value = rate.Value;
                }
                else
                {
                    MessageHelper.ShowWarning(LocalizationManager.GetString("AddCurr_FetchRateError", "Could not fetch rate. Please enter manually."));
                }
            }
            finally
            {
                btnFetch.Enabled = true;
                btnFetch.Text = LocalizationManager.GetString("AddCurr_Fetch", "Fetch");
            }
        }

        private void ApplyLocalization()
        {
            bool isArabic = LocalizationManager.IsArabic;
            this.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

            txtCode.LabelText = LocalizationManager.GetString("AddCurr_CodeLabel", "Currency Code (e.g. EUR)");
            txtName.LabelText = LocalizationManager.GetString("AddCurr_NameLabel", "Currency Name (e.g. Euro)");
            txtSymbol.LabelText = LocalizationManager.GetString("AddCurr_SymbolLabel", "Symbol (e.g. €)");
            numRate.LabelText = LocalizationManager.GetString("AddCurr_RateLabel", "Exchange Rate (1 USD = ?)");
            btnFetch.Text = LocalizationManager.GetString("AddCurr_Fetch", "Fetch");
            btnFetch.Image = ThemeConfig.GetNuricon("sync");
            btnFetch.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnFetch.ImageAlign = ContentAlignment.MiddleLeft;
            btnFetch.Padding = new Padding(10, 0, 0, 0);
            
            numRate.Value = 1.0000m;

            this.TitleText = LocalizationManager.GetString("Curr_AddBtn");
            SetFooterButtons(
                LocalizationManager.GetString("AddPart_Save"),
                LocalizationManager.GetString("Popup_Cancel"),
                btnSave_Click,
                (s, e) => this.Close()
            );
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidationHelper.ValidateRequiredFields(txtCode, txtName)) return;

            string code = txtCode.Text.Trim().ToUpper();
            string name = txtName.Text.Trim();
            string symbol = txtSymbol.Text.Trim();
            decimal rate = numRate.Value;

            if (code.Length > 10)
            {
                MessageHelper.ShowWarning(LocalizationManager.GetString("AddCurr_CodeTooLong", "Code too long"));
                return;
            }

            try
            {
                // Check if exists
                bool exists = DatabaseHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM currency_rates WHERE code = @code", 
                    new Microsoft.Data.Sqlite.SqliteParameter("@code", code)) > 0;

                if (exists) {
                    MessageHelper.ShowWarning(LocalizationManager.GetString("Curr_MsgExists"));
                    return;
                }

                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO currency_rates (code, name, symbol, rate_vs_usd) VALUES (@code, @name, @symbol, @rate)",
                    new Microsoft.Data.Sqlite.SqliteParameter("@code", code),
                    new Microsoft.Data.Sqlite.SqliteParameter("@name", name),
                    new Microsoft.Data.Sqlite.SqliteParameter("@symbol", symbol),
                    new Microsoft.Data.Sqlite.SqliteParameter("@rate", rate)
                );

                CurrencyService.LoadRatesFromDb();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError("Error adding currency: " + ex.Message);
            }
        }
    }
}
