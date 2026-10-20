using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using InventorySystem.Helpers;
using InventorySystem.Controls;

namespace InventorySystem.Forms
{
    public class AboutUsForm : BaseModalForm
    {
        private const int LogoTileWidth = 148;
        private const int LogoTileHeight = 148;
        private const int CardWidth = 520;
        private const int CardHeight = 660;

        private Panel _pnlLogo;
        private Image _logo;
        private Label _lblName;
        private Label _lblProduct;
        private Label _lblVer;
        private Label _lblDesc;
        private Label _lblDev;
        private Button _btnContact;
        private Label _lblCopyright;

        public AboutUsForm()
        {
            InitializeAboutLayout();
            _languageHandler = (s, e) =>
            {
                if (IsDisposed) return;
                if (!IsHandleCreated || !Visible) return;
                ApplyLocalization();
            };
            LocalizationManager.LanguageChanged += _languageHandler;
            ApplyLocalization();
            this.Size = new Size(CardWidth, CardHeight);
        }

        private EventHandler _languageHandler;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // The base form fits itself to its content; this card is a fixed poster,
            // so restore the intended proportions and re-centre afterwards.
            this.Size = new Size(CardWidth, CardHeight);
            Rectangle wa = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(
                wa.Left + (wa.Width - this.Width) / 2,
                wa.Top + (wa.Height - this.Height) / 2);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_languageHandler != null)
                LocalizationManager.LanguageChanged -= _languageHandler;
            base.OnFormClosed(e);
            _logo?.Dispose();
            _logo = null;
        }

        private void ApplyLocalization()
        {
            if (IsDisposed) return;
            LocalizationManager.ApplyRTL(this);
            this.TitleText = LocalizationManager.GetString("Nav_AboutUs");
            if (_lblName != null)
            {
                _lblName.Text = LocalizationManager.GetString("AboutUs_Company", "Softio");
                PinCentered(_lblName);
            }
            if (_lblProduct != null)
            {
                _lblProduct.Text = LocalizationManager.GetString("AboutUs_AppName");
                PinCentered(_lblProduct);
            }
            if (_lblVer != null)
            {
                _lblVer.Text = LocalizationManager.GetString("AboutUs_Version");
                PinCentered(_lblVer);
            }
            if (_lblDesc != null)
            {
                _lblDesc.Text = LocalizationManager.GetString("AboutUs_Desc");
                PinCentered(_lblDesc);
            }
            if (_lblDev != null)
            {
                _lblDev.Text = LocalizationManager.GetString("AboutUs_DevInfo");
                PinCentered(_lblDev);
            }
            if (_btnContact != null)
            {
                _btnContact.Text = LocalizationManager.GetString("AboutUs_ContactSupport");
                // ApplyRTL would mirror the icon to the far side; keep it leading the label.
                _btnContact.RightToLeft = RightToLeft.No;
                _btnContact.ImageAlign = ContentAlignment.MiddleLeft;
            }
            if (_lblCopyright != null)
            {
                _lblCopyright.Text = LocalizationManager.GetString("AboutUs_Copyright");
                PinCentered(_lblCopyright);
            }
        }

        private static void PinCentered(Label lbl)
        {
            lbl.RightToLeft = RightToLeft.No;
            lbl.TextAlign = ContentAlignment.MiddleCenter;
        }

        private void InitializeAboutLayout()
        {
            // One column of stacked rows: every row centres itself, so nothing depends on
            // hand-picked coordinates that RTL mirroring would shift off-centre.
            TableLayoutPanel stack = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 10,
                Margin = new Padding(0),
                Padding = new Padding(0, 6, 0, 0),
                BackColor = Color.Transparent
            };
            stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            _pnlLogo = new Panel
            {
                Size = new Size(LogoTileWidth, LogoTileHeight),
                Anchor = AnchorStyles.None,
                Margin = new Padding(0, 0, 0, 18),
                BackColor = Color.Transparent
            };
            _pnlLogo.Paint += PaintLogoTile;
            LoadLogo();

            // AutoSize + Anchor.None so each label measures its own glyphs. Fixed
            // Absolute rows were shorter than the font (especially with bottom margins),
            // which clipped descenders and made Arabic look "glitched".
            _lblName = MakeCenteredLabel(new Font(ThemeConfig.AppFontFamily, 16F, FontStyle.Bold),
                ThemeConfig.PrimaryColor, new Padding(10, 0, 10, 2));

            _lblProduct = MakeCenteredLabel(new Font(ThemeConfig.AppFontFamily, 10.5F, FontStyle.Bold),
                ThemeConfig.TextColorDark, new Padding(10, 0, 10, 2));

            _lblVer = MakeCenteredLabel(new Font(ThemeConfig.AppFontFamily, 10F, FontStyle.Regular),
                ThemeConfig.MutedTextColor, new Padding(10, 0, 10, 10));

            _lblDesc = MakeCenteredLabel(new Font(ThemeConfig.AppFontFamily, 10.5F, FontStyle.Regular),
                ThemeConfig.TextColorDark, new Padding(24, 0, 24, 14), wrap: true, maxWidth: 420);

            Panel divider = new Panel
            {
                Size = new Size(300, 1),
                Anchor = AnchorStyles.None,
                BackColor = Color.FromArgb(232, 234, 238),
                Margin = new Padding(0, 4, 0, 14)
            };

            _lblDev = MakeCenteredLabel(new Font(ThemeConfig.AppFontFamily, 9.5F, FontStyle.Bold),
                ThemeConfig.TextColorDark, new Padding(10, 0, 10, 14));

            _btnContact = new ModernButton
            {
                Size = new Size(210, 44),
                Anchor = AnchorStyles.None,
                Margin = new Padding(0, 0, 0, 14),
                Cursor = Cursors.Hand,
                Image = ThemeConfig.TintImage(ThemeConfig.GetNuricon("contact_us"), Color.White),
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleCenter,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Padding = new Padding(14, 0, 14, 0),
                RightToLeft = RightToLeft.No
            };
            ThemeConfig.ApplyPrimaryButton(_btnContact);
            _btnContact.Font = new Font(ThemeConfig.AppFontFamily, 10F, FontStyle.Bold);
            _btnContact.Click += (s, e) =>
            {
                try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("mailto:softioservices@gmail.com") { UseShellExecute = true }); }
                catch { MessageHelper.ShowInfo(LocalizationManager.GetString("Msg_ContactSupport")); }
            };

            _lblCopyright = MakeCenteredLabel(new Font(ThemeConfig.AppFontFamily, 8.5F, FontStyle.Regular),
                ThemeConfig.MutedTextColor, new Padding(10, 0, 10, 4));

            for (int i = 0; i < 9; i++)
                stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            stack.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // filler

            stack.Controls.Add(_pnlLogo, 0, 0);
            stack.Controls.Add(_lblName, 0, 1);
            stack.Controls.Add(_lblProduct, 0, 2);
            stack.Controls.Add(_lblVer, 0, 3);
            stack.Controls.Add(_lblDesc, 0, 4);
            stack.Controls.Add(divider, 0, 5);
            stack.Controls.Add(_lblDev, 0, 6);
            stack.Controls.Add(_btnContact, 0, 7);
            stack.Controls.Add(_lblCopyright, 0, 8);

            this.ContentPanel.Controls.Add(stack);
        }

        private static Label MakeCenteredLabel(Font font, Color color, Padding margin,
            bool wrap = false, int maxWidth = 0)
        {
            var lbl = new Label
            {
                Font = font,
                ForeColor = color,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = true,
                Anchor = AnchorStyles.None,
                Margin = margin,
                // Mixed Arabic + Latin must not flip; TextAlign already centres the run.
                RightToLeft = RightToLeft.No,
                UseCompatibleTextRendering = true
            };
            if (wrap && maxWidth > 0)
            {
                lbl.MaximumSize = new Size(maxWidth, 0);
                lbl.AutoSize = true;
            }
            return lbl;
        }

        private void LoadLogo()
        {
            _logo = null;
            try
            {
                string path = FindAsset("softio_arrow_logo.png");
                if (string.IsNullOrEmpty(path)) return;
                using (var img = Image.FromFile(path))
                    _logo = new Bitmap(img);
            }
            catch
            {
                _logo?.Dispose();
                _logo = null;
            }
        }

        private static string FindAsset(string filename)
        {
            string currentDir = Application.StartupPath;
            for (int i = 0; i < 5; i++)
            {
                string checkPath = Path.Combine(currentDir, "Assets", filename);
                if (File.Exists(checkPath)) return checkPath;
                var parent = Directory.GetParent(currentDir);
                if (parent == null) break;
                currentDir = parent.FullName;
            }
            return null;
        }

        private void PaintLogoTile(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (var parent = new SolidBrush(ThemeConfig.GetParentColor(_pnlLogo)))
                g.FillRectangle(parent, -1, -1, _pnlLogo.Width + 2, _pnlLogo.Height + 2);

            var bounds = _pnlLogo.ClientRectangle;
            const float radius = 26f;
            using (var path = ThemeConfig.GetRoundedPathF(new RectangleF(0, 0, bounds.Width, bounds.Height), radius))
            using (var fill = new SolidBrush(Color.FromArgb(10, 22, 40)))
            {
                g.FillPath(fill, path);
                g.SetClip(path);
            }

            if (_logo != null)
            {
                float scale = Math.Max((float)bounds.Width / _logo.Width, (float)bounds.Height / _logo.Height);
                int w = Math.Max(1, (int)(_logo.Width * scale));
                int h = Math.Max(1, (int)(_logo.Height * scale));
                var dest = new Rectangle(
                    bounds.X + (bounds.Width - w) / 2,
                    bounds.Y + (bounds.Height - h) / 2,
                    w, h);
                g.DrawImage(_logo, dest);
            }
            else
            {
                TextRenderer.DrawText(g, "Softio", new Font("Segoe UI", 16F, FontStyle.Bold),
                    bounds, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
    }
}
