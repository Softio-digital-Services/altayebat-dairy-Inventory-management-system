using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Text.Json;
using System.IO;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using InventorySystem.Helpers;

namespace InventorySystem
{
    /// <summary>
    /// Centralized configuration for UI Theming and Branding.
    /// Updated to "Horizon UI" inspired Light Theme.
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class ThemeConfig
    {
        // ==========================================
        // BRANDING
        // ==========================================
        public static string CompanyName { get; set; } = "Al-Tayebat";
        public static string AppTitle { get; set; } = "Al-Tayebat Inventory";

        static ThemeConfig()
        {
            try
            {
                string configPath = "appsettings.json";
                if (File.Exists(configPath))
                {
                    string jsonString = File.ReadAllText(configPath);
                    using (JsonDocument doc = JsonDocument.Parse(jsonString))
                    {
                        if (doc.RootElement.TryGetProperty("SystemBranding", out JsonElement branding))
                        {
                            if (branding.TryGetProperty("CompanyName", out JsonElement compName))
                                CompanyName = compName.GetString() ?? CompanyName;
                            
                            if (branding.TryGetProperty("AppName", out JsonElement appName))
                                AppTitle = appName.GetString() ?? AppTitle;
                        }
                    }
                }
            }
            catch { /* Fallback to default generic names */ }
        }

        // ==========================================
        // COLOR PALETTE (Light / Horizon Blue)
        // ==========================================
        
        // Primary Brand Color (Blue)
        public static Color PrimaryColor { get; } = Color.FromArgb(37, 99, 235); // Blue #2563EB
        public static Color PrimaryHoverColor { get; } = Color.FromArgb(29, 78, 216); // Darker Blue #1D4ED8

        // Gradient Colors for Primary Buttons
        public static Color GradientStart { get; } = Color.FromArgb(59, 130, 246); // Lighter Blue #3B82F6
        public static Color GradientEnd { get; } = Color.FromArgb(37, 99, 235);   // Blue #2563EB

        // Secondary / Text Colors
        public static Color SecondaryColor { get; } = Color.FromArgb(100, 116, 139); // Slate Gray
        public static Color SecondaryHoverColor { get; } = Color.FromArgb(210, 210, 220); 
        public static Color TextColorDark { get; } = Color.FromArgb(15, 23, 42);    
        public static Color TextColorLight { get; } = Color.White;
        public static Color TextColorWhite { get; } = Color.White;

        // Backgrounds
        public static Color BackgroundColor { get; } = Color.FromArgb(241, 245, 249); 
        public static Color SidebarColor { get; } = Color.FromArgb(248, 250, 252);     
        public static Color HeaderColor { get; } = Color.FromArgb(37, 99, 235);      
        public static Color ActiveBackColor { get; } = Color.FromArgb(239, 246, 255); 

        
        // Semantic Token Mapping
        public static Color SelectionBackColor { get; } = Color.FromArgb(237, 242, 247); // Light Gray-Blue selection
        public static Color BorderColor { get; } = Color.FromArgb(226, 232, 240); // Standard Border Color (Slate-200)
        public static Color MutedTextColor { get; } = Color.FromArgb(160, 174, 192); // Cool Gray for subtitles/muted text
        public static Color GridHeaderBgColor { get; } = Color.FromArgb(248, 250, 252); // Light Gray #F8FAFC

        // Status Colors
        public static Color SuccessColor { get; } = Color.FromArgb(5, 205, 153); // Green #05CD99
        public static Color SuccessLight { get; } = Color.FromArgb(230, 255, 250); // Light Green

        public static Color DangerColor { get; } = Color.FromArgb(238, 93, 80);  // Red #EE5D50 (Main Theme Red)
        public static Color DangerColorBright { get; } = Color.FromArgb(239, 68, 68); // Red-500 for clear visibility
        public static Color DangerLight { get; } = Color.FromArgb(255, 240, 240); // Light Red

        public static Color WarningColor { get; } = Color.FromArgb(255, 181, 71); // Orange #FFB547
        public static Color WarningLight { get; } = Color.FromArgb(255, 250, 235); // Light Orange
        
        // Status Badge Palette (Horizon UI / Tailwind inspired)
        public static Color SuccessBadgeBg { get; } = Color.FromArgb(240, 253, 244); // Light Green
        public static Color SuccessBadgeText { get; } = Color.FromArgb(21, 128, 61); // Dark Green
        public static Color SuccessBorder { get; } = Color.FromArgb(34, 197, 94);   // Green-500

        // Standard action button colors (outline: text + border only)
        public static Color ActionImportColor { get; } = Color.FromArgb(30, 64, 175);   // Dark Blue #1E40AF
        public static Color ActionExportColor { get; } = Color.FromArgb(251, 146, 60);  // Light Orange #FB923C
        public static Color ActionAddColor { get; } = Color.FromArgb(74, 222, 128);     // Light Green #4ADE80
        public static Color ActionRefreshColor { get; } = Color.FromArgb(4, 120, 87);   // Dark Green #047857

        public static Color DangerBadgeBg { get; } = Color.FromArgb(254, 242, 242);  // Light Red
        public static Color DangerBadgeText { get; } = Color.FromArgb(185, 28, 28);  // Dark Red
        public static Color DangerBorder { get; } = Color.FromArgb(239, 68, 68);    // Red-500

        public static Color WarningBadgeBg { get; } = Color.FromArgb(255, 251, 235); // Light Yellow
        public static Color WarningBadgeText { get; } = Color.FromArgb(180, 83, 9);   // Dark Orange
        public static Color WarningBorder { get; } = Color.FromArgb(245, 158, 11);  // Orange-500

        public static Color InfoBadgeBg { get; } = Color.FromArgb(239, 246, 255);    // Light Blue
        public static Color InfoBadgeText { get; } = Color.FromArgb(29, 78, 216);    // Dark Blue
        public static Color InfoBorder { get; } = Color.FromArgb(59, 130, 246);      // Blue-500
        
        // Neon / Premium Accents
        public static Color NeonBlue { get; } = Color.FromArgb(0, 245, 255); // Vibrant Neon Blue
        public static Color NeonBlueAlpha { get; } = Color.FromArgb(100, 0, 245, 255);

        // Card Surface
        public static Color SurfaceColor { get; } = Color.White;

        // POS-specific tokens
        public static Color POS_SidebarBg { get; } = Color.FromArgb(248, 249, 251);
        public static Color POS_CartItemBg { get; } = Color.FromArgb(248, 250, 252);
        public static Color POS_SeparatorColor { get; } = Color.FromArgb(235, 237, 240);
        public static Color POS_ChipActive { get; } = Color.FromArgb(37, 99, 235);
        public static Color POS_ChipActiveBorder { get; } = Color.FromArgb(29, 78, 216);


        // ==========================================
        // FONTS
        // ==========================================
        private static FontFamily GetAppFontFamily()
        {
            return new FontFamily("Segoe UI");
        }

        public static FontFamily AppFontFamily { get; } = GetAppFontFamily();

        public static Font HeaderFont { get; } = new Font(AppFontFamily, 14F, FontStyle.Bold); 
        public static Font CardTitleFont { get; } = new Font(AppFontFamily, 12F, FontStyle.Bold);
        public static Font SubHeaderFont { get; } = new Font(AppFontFamily, 10F, FontStyle.Bold);
        public static Font StandardFont { get; } = new Font(AppFontFamily, 9F, FontStyle.Regular);
        public static Font SmallFont { get; } = new Font(AppFontFamily, 8F, FontStyle.Regular);
        public static Font ButtonFont { get; } = new Font(AppFontFamily, 9F, FontStyle.Bold);
        public static Font SmallBoldFont { get; } = new Font(AppFontFamily, 9F, FontStyle.Bold);
        public static Font MicroBoldFont { get; } = new Font(AppFontFamily, 8F, FontStyle.Bold);
        public static Font EmojiFont { get; } = new Font("Segoe UI Emoji", 11F);
        public static Font EmojiFontLarge { get; } = new Font("Segoe UI Emoji", 14F);
        public static Font SymbolFont { get; } = new Font("Segoe UI Symbol", 11F);

        // ==========================================
        // HELPER METHODS
        // ==========================================

        public static Label CreateStandardHeader(string text)
        {
            return new Label
            {
                Name = "lblStandardHeader", // Ensures global themer identifies this as a header label
                Text = text,
                Font = HeaderFont,
                ForeColor = PrimaryColor,
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 35,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(0)
            };
        }

        public static TableLayoutPanel CreateGlobalFormHeader(Control titleLabel, Control searchBox = null, Control[] actionButtons = null)
        {
            bool hasActions = searchBox != null || (actionButtons != null && actionButtons.Length > 0);

            // Header auto-sizes: title only = 42px, title + actions row = 42+52 = 94px
            TableLayoutPanel tlpHeader = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = hasActions ? 94 : 42,
                Margin = new Padding(0, 0, 0, 12),
                ColumnCount = 1,
                RowCount = hasActions ? 2 : 1
            };
            tlpHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpHeader.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F)); // Title row
            if (hasActions)
                tlpHeader.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F)); // Actions row

            if (titleLabel != null)
            {
                titleLabel.Dock = DockStyle.Fill;
                titleLabel.Margin = new Padding(0);
                tlpHeader.Controls.Add(titleLabel, 0, 0);
            }

            if (hasActions)
            {
                // Positioned by hand: ApplyRTL must not mirror these children (panel name
                // is the skip key). English = actions on the right / search on the left;
                // Arabic stays the opposite so existing AR chrome is unchanged.
                Panel pnlActions = new Panel
                {
                    Name = "pnlGlobalHeaderActions",
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0),
                    BackColor = Color.Transparent,
                    RightToLeft = RightToLeft.No
                };

                var buttons = new List<Control>();
                if (actionButtons != null)
                {
                    foreach (var btn in actionButtons)
                    {
                        if (btn == null) continue;
                        btn.Margin = new Padding(0);
                        btn.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                        btn.Dock = DockStyle.None;
                        buttons.Add(btn);
                        pnlActions.Controls.Add(btn);
                    }
                }

                if (searchBox != null)
                {
                    searchBox.Margin = new Padding(0);
                    searchBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                    searchBox.Dock = DockStyle.None;
                    pnlActions.Controls.Add(searchBox);
                }

                void LayoutActionsRow()
                {
                    if (pnlActions.IsDisposed) return;
                    pnlActions.RightToLeft = RightToLeft.No;
                    int w = pnlActions.ClientSize.Width;
                    int h = pnlActions.ClientSize.Height;
                    if (w <= 0) return;

                    const int gap = 10;
                    int Top(Control c) => Math.Max(0, (h - c.Height) / 2);

                    int groupW = 0;
                    for (int i = 0; i < buttons.Count; i++)
                        groupW += buttons[i].Width + (i > 0 ? gap : 0);

                    // EN: actions flush right, search left. AR: actions left, search right.
                    bool ar = LocalizationManager.IsArabic;
                    int groupX = ar ? 0 : Math.Max(0, w - groupW);
                    for (int i = 0; i < buttons.Count; i++)
                    {
                        buttons[i].RightToLeft = RightToLeft.No;
                        buttons[i].Location = new Point(groupX, Top(buttons[i]));
                        groupX += buttons[i].Width + gap;
                    }

                    if (searchBox != null)
                    {
                        int searchX = ar ? Math.Max(0, w - searchBox.Width) : 0;
                        searchBox.Location = new Point(searchX, Top(searchBox));
                    }
                }

                // ApplyRTL mirrors child locations, so re-place the row once every
                // localization handler has run.
                void DeferLayout()
                {
                    if (pnlActions.IsDisposed) return;
                    if (pnlActions.IsHandleCreated) pnlActions.BeginInvoke((Action)LayoutActionsRow);
                    else LayoutActionsRow();
                }

                pnlActions.Resize += (s, e) => LayoutActionsRow();
                pnlActions.RightToLeftChanged += (s, e) => DeferLayout();
                pnlActions.HandleCreated += (s, e) => { LayoutActionsRow(); DeferLayout(); };

                EventHandler onLanguageChanged = (s, e) => DeferLayout();
                LocalizationManager.LanguageChanged += onLanguageChanged;
                pnlActions.Disposed += (s, e) => LocalizationManager.LanguageChanged -= onLanguageChanged;

                LayoutActionsRow();
                tlpHeader.Controls.Add(pnlActions, 0, 1);
            }

            return tlpHeader;
        }

        public static void ApplyPrimaryButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = PrimaryColor;
            btn.Height = 35;
            btn.ForeColor = TextColorLight;
            btn.Font = SmallBoldFont;
            btn.Cursor = Cursors.Hand;
            btn.TextAlign = ContentAlignment.MiddleCenter;
            
            btn.MouseEnter += (s, e) => btn.BackColor = PrimaryHoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = PrimaryColor;

        }

        private static System.Runtime.CompilerServices.ConditionalWeakTable<Button, string> _standardButtonKeys = new System.Runtime.CompilerServices.ConditionalWeakTable<Button, string>();

        public static void ApplyStandardAddButton(Button btn, string localizationKey = null)
        {
            if (btn == null) return;
            btn.Tag = "standard_add";
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.Transparent;
            btn.Height = 35;
            btn.ForeColor = Color.Transparent;
            btn.Font = SmallBoldFont;
            btn.Cursor = Cursors.Hand;
            btn.UseVisualStyleBackColor = false;
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;
            
            btn.Text = "";
            if (localizationKey != null)
            {
                _standardButtonKeys.Remove(btn);
                _standardButtonKeys.Add(btn, localizationKey);
            }

            btn.MouseEnter -= StandardOutline_MouseEnter;
            btn.MouseEnter += StandardOutline_MouseEnter;
            btn.MouseLeave -= StandardOutline_MouseLeave;
            btn.MouseLeave += StandardOutline_MouseLeave;

            btn.Paint -= StandardAdd_Paint;
            btn.Paint += StandardAdd_Paint;
        }

        private static void StandardAdd_Paint(object s, PaintEventArgs e)
        {
            if (s is Button btn)
            {
                _standardButtonKeys.TryGetValue(btn, out string key);
                DrawIconButton(btn, e.Graphics, "add", key, ActionAddColor, ActionAddColor, true);
            }
        }

        public static void ApplySuccessAddButton(Button btn, string localizationKey = null)
        {
            if (btn == null) return;
            btn.Tag = "success_add";
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.Transparent;
            btn.Height = 35;
            btn.ForeColor = Color.Transparent;
            btn.Font = SmallBoldFont;
            btn.Cursor = Cursors.Hand;
            btn.UseVisualStyleBackColor = false;
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;
            
            btn.Text = "";
            if (localizationKey != null)
            {
                _standardButtonKeys.Remove(btn);
                _standardButtonKeys.Add(btn, localizationKey);
            }

            btn.MouseEnter -= StandardOutline_MouseEnter;
            btn.MouseEnter += StandardOutline_MouseEnter;
            btn.MouseLeave -= StandardOutline_MouseLeave;
            btn.MouseLeave += StandardOutline_MouseLeave;

            btn.Paint -= SuccessAdd_Paint;
            btn.Paint += SuccessAdd_Paint;
        }

        private static void SuccessAdd_Paint(object s, PaintEventArgs e)
        {
            if (s is Button btn)
            {
                _standardButtonKeys.TryGetValue(btn, out string key);
                DrawIconButton(btn, e.Graphics, "add", key, ActionAddColor, ActionAddColor, true);
            }
        }

        public static void ApplyStandardDeleteButton(Button btn, string localizationKey = null)
        {
            if (btn == null) return;
            btn.Tag = "standard_delete";
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.Transparent;
            btn.Height = 35;
            btn.ForeColor = Color.Transparent;
            btn.Font = SmallBoldFont;
            btn.Cursor = Cursors.Hand;
            btn.UseVisualStyleBackColor = false;
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;
            
            // Suppress native text shadows completely
            btn.Text = "";
            if (localizationKey != null)
            {
                _standardButtonKeys.Remove(btn);
                _standardButtonKeys.Add(btn, localizationKey);
            }

            btn.MouseEnter -= StandardOutline_MouseEnter;
            btn.MouseEnter += StandardOutline_MouseEnter;
            btn.MouseLeave -= StandardOutline_MouseLeave;
            btn.MouseLeave += StandardOutline_MouseLeave;

            btn.Paint -= StandardDelete_Paint;
            btn.Paint += StandardDelete_Paint;
        }

        private static void StandardOutline_MouseEnter(object s, EventArgs e) { if (s is Button b) b.Invalidate(); }
        private static void StandardOutline_MouseLeave(object s, EventArgs e) { if (s is Button b) b.Invalidate(); }

        private static void StandardDelete_Paint(object s, PaintEventArgs e)
        {
            if (s is Button btn)
            {
                _standardButtonKeys.TryGetValue(btn, out string key);
                DrawIconButton(btn, e.Graphics, "delete", key, DangerColor, DangerColor, true);
            }
        }

        public static void ApplyStandardRefreshButton(Button btn, string localizationKey = null)
        {
            if (btn == null) return;
            btn.Tag = "standard_refresh";
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.Transparent;
            btn.Height = 35;
            btn.ForeColor = Color.Transparent;
            btn.Font = SmallBoldFont;
            btn.Cursor = Cursors.Hand;
            btn.UseVisualStyleBackColor = false;
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;
            
            // Suppress native text shadows completely
            btn.Text = "";
            if (localizationKey != null)
            {
                _standardButtonKeys.Remove(btn);
                _standardButtonKeys.Add(btn, localizationKey);
            }

            btn.MouseEnter -= StandardOutline_MouseEnter;
            btn.MouseEnter += StandardOutline_MouseEnter;
            btn.MouseLeave -= StandardOutline_MouseLeave;
            btn.MouseLeave += StandardOutline_MouseLeave;

            btn.Paint -= StandardRefresh_Paint;
            btn.Paint += StandardRefresh_Paint;
        }

        private static void StandardRefresh_Paint(object s, PaintEventArgs e)
        {
            if (s is Button btn)
            {
                _standardButtonKeys.TryGetValue(btn, out string key);
                DrawIconButton(btn, e.Graphics, "refresh", key, ActionRefreshColor, ActionRefreshColor, true);
            }
        }

        public static void ApplyStandardImportButton(Button btn, string localizationKey = null)
        {
            if (btn == null) return;
            btn.Tag = "standard_import";
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.Transparent;
            btn.Height = 35;
            btn.ForeColor = Color.Transparent;
            btn.Font = SmallBoldFont;
            btn.Cursor = Cursors.Hand;
            btn.UseVisualStyleBackColor = false;
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;

            btn.Text = "";
            if (localizationKey != null)
            {
                _standardButtonKeys.Remove(btn);
                _standardButtonKeys.Add(btn, localizationKey);
            }

            btn.MouseEnter -= StandardOutline_MouseEnter;
            btn.MouseEnter += StandardOutline_MouseEnter;
            btn.MouseLeave -= StandardOutline_MouseLeave;
            btn.MouseLeave += StandardOutline_MouseLeave;

            btn.Paint -= StandardImport_Paint;
            btn.Paint += StandardImport_Paint;
        }

        private static void StandardImport_Paint(object s, PaintEventArgs e)
        {
            if (s is Button btn)
            {
                _standardButtonKeys.TryGetValue(btn, out string key);
                DrawIconButton(btn, e.Graphics, "import", key, ActionImportColor, ActionImportColor, true);
            }
        }

        public static void ApplyStandardExportButton(Button btn, string localizationKey = null)
        {
            if (btn == null) return;
            btn.Tag = "standard_export";
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.Transparent;
            btn.Height = 35;
            btn.ForeColor = Color.Transparent;
            btn.Font = SmallBoldFont;
            btn.Cursor = Cursors.Hand;
            btn.UseVisualStyleBackColor = false;
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;

            btn.Text = "";
            if (localizationKey != null)
            {
                _standardButtonKeys.Remove(btn);
                _standardButtonKeys.Add(btn, localizationKey);
            }

            btn.MouseEnter -= StandardOutline_MouseEnter;
            btn.MouseEnter += StandardOutline_MouseEnter;
            btn.MouseLeave -= StandardOutline_MouseLeave;
            btn.MouseLeave += StandardOutline_MouseLeave;

            btn.Paint -= StandardExport_Paint;
            btn.Paint += StandardExport_Paint;
        }

        private static void StandardExport_Paint(object s, PaintEventArgs e)
        {
            if (s is Button btn)
            {
                _standardButtonKeys.TryGetValue(btn, out string key);
                DrawIconButton(btn, e.Graphics, "export", key, ActionExportColor, ActionExportColor, true);
            }
        }

        public static Color GetParentColor(Control ctrl)
        {
            Control p = ctrl.Parent;
            while (p != null)
            {
                if (p.Tag != null && p.Tag.ToString() == "surface") return SurfaceColor;
                if (p.BackColor != Color.Transparent && p.BackColor.A != 0 && p.BackColor != Color.Empty)
                    return p.BackColor;
                p = p.Parent;
            }
            return BackgroundColor;
        }

        public static void DrawIconButton(Button btn, Graphics g, string iconName, string localizationKey, Color textColor, Color accentColor, bool isOutline)
        {
            if (btn == null) return;
            bool isArabic = InventorySystem.Helpers.LocalizationManager.IsArabic;
            string text = InventorySystem.Helpers.LocalizationManager.GetString(localizationKey);
            if (string.IsNullOrEmpty(text) || text == localizationKey)
            {
                if (!string.IsNullOrEmpty(btn.Text)) text = btn.Text;
                else if (!string.IsNullOrEmpty(localizationKey)) text = localizationKey;
            }

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);

            bool isPaletted = btn.Tag != null && btn.Tag.ToString() == "paletted";
            bool isStandardAdd = btn.Tag != null && btn.Tag.ToString() == "standard_add";
            Color baseBgColor = isPaletted ? btn.BackColor : accentColor;
            if (isStandardAdd) baseBgColor = PrimaryColor;

            bool isHovered = btn.ClientRectangle.Contains(btn.PointToClient(System.Windows.Forms.Cursor.Position));

            Color effectiveBg = baseBgColor;
            if (isHovered && !isOutline)
            {
                effectiveBg = isStandardAdd ? PrimaryHoverColor : Color.FromArgb(Math.Max(0, baseBgColor.R - 20), Math.Max(0, baseBgColor.G - 20), Math.Max(0, baseBgColor.B - 20));
            }

            Color effectiveText = isPaletted ? TextColorWhite : textColor;
            bool effectiveOutline = isPaletted ? false : isOutline;

            if (btn.BackColor != Color.Transparent || isPaletted)
            {
                using (var pb = new SolidBrush(GetParentColor(btn)))
                    g.FillRectangle(pb, -1, -1, btn.Width + 2, btn.Height + 2);
            }

            using (var path = GetRoundedPath(r, 12)) 
            {
                if (effectiveOutline)
                {
                    if (isHovered)
                    {
                        using (var hoverBrush = new SolidBrush(Color.FromArgb(20, effectiveBg)))
                            g.FillPath(hoverBrush, path);
                    }
                    using (Pen pen = new Pen(effectiveBg, 1.5f))
                        g.DrawPath(pen, path);
                }
                else
                {
                    using (SolidBrush brush = new SolidBrush(effectiveBg))
                        g.FillPath(brush, path);
                }
            }

            Image img = GetNuricon(iconName);
            int iconSize = 24;
            int margin = 8;
            
            bool hasText = !string.IsNullOrEmpty(text);
            int iconX = hasText ? (isArabic ? (btn.Width - iconSize - margin) : margin) : (btn.Width - iconSize) / 2;
            int iconY = (btn.Height - iconSize) / 2;
            
            if (img != null)
            {
                using (var tinted = TintImage(img, textColor))
                {
                    g.DrawImage(tinted, new Rectangle(iconX, iconY, iconSize, iconSize));
                }
            }

            if (hasText)
            {
                int textX = isArabic ? margin : (iconX + iconSize + 4);
                int textW = btn.Width - iconSize - (margin * 2) - 4;
                Rectangle textRect = new Rectangle(textX, 0, textW, btn.Height);

                TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding;
                if (isArabic) flags |= TextFormatFlags.RightToLeft;

                TextRenderer.DrawText(g, text, btn.Font, textRect, effectiveText, flags);
            }
        }

        public static void ApplyComboBoxStyle(ComboBox cbo)
        {
            cbo.FlatStyle = FlatStyle.Flat;
            cbo.Font = StandardFont;
            cbo.BackColor = SurfaceColor;
            cbo.ForeColor = TextColorDark;
            cbo.Cursor = Cursors.Hand;
            if (Helpers.LocalizationManager.IsArabic) cbo.RightToLeft = RightToLeft.Yes;
            
            // Prevent blue selection highlight on focus
            cbo.Enter += (s, e) => {
                if (cbo.IsHandleCreated) {
                    cbo.BeginInvoke(new Action(() => {
                        cbo.Select(0, 0);
                        cbo.SelectionLength = 0;
                    }));
                }
            };
            
            // Also clear on choice to prevent focus-highlight after selection
            cbo.SelectedIndexChanged += (s, e) => {
                if (cbo.IsHandleCreated) {
                    cbo.BeginInvoke(new Action(() => {
                        cbo.Select(0, 0);
                        cbo.SelectionLength = 0;
                    }));
                }
            };
        }

        public static Panel WrapInStyledInput(Control innerControl, int height = 35, bool isMultiline = false)
        {
            if (!isMultiline) height = 35; // Enforce 35px height for single line inputs

            Panel p = new Panel
            {
                Size      = new Size(200, height),
                BackColor = Color.White
            };

            innerControl.Dock = DockStyle.None;
            void positionControl() {
                if (innerControl == null) return;
                innerControl.Width = p.Width - 20;
                if (!isMultiline)
                    innerControl.Location = new Point(10, (p.Height - innerControl.Height) / 2);
                else {
                    innerControl.Location = new Point(10, 10);
                    innerControl.Height = p.Height - 20;
                }
            }

            p.Resize += (s, e) => {
                positionControl();
                using (var path = GetRoundedPath(new Rectangle(0, 0, p.Width, p.Height), 12))
                {
                    var oldRegion = p.Region;
                    p.Region = new Region(path);
                    if (oldRegion != null) oldRegion.Dispose();
                }
                p.Invalidate();
            };

            positionControl();
            using (var path = GetRoundedPath(new Rectangle(0, 0, p.Width, p.Height), 12))
            {
                var oldRegion = p.Region;
                p.Region = new Region(path);
                if (oldRegion != null) oldRegion.Dispose();
            }

            p.Paint += (s, e) =>
            {
                // Panel is Region-clipped to the same radius, so inset the stroke.
                DrawRoundedBorder(e.Graphics, new Rectangle(0, 0, p.Width, p.Height), 12f, BorderColor, 1.5f);
            };

            p.Controls.Add(innerControl);
            return p;
        }

        public static readonly Color[] ChartPalette = new Color[]
        {
            PrimaryColor, SuccessColor, WarningColor, DangerColor, SecondaryColor,
            Color.FromArgb(99, 102, 241), Color.FromArgb(168, 85, 247), Color.FromArgb(236, 72, 153)
        };

        public static void ApplyChartTheme(Chart chart)
        {
            try
            {
                chart.BeginInit();
                chart.BackColor = SurfaceColor;
                
                // Re-enable high quality rendering (now safe with new library)
                chart.AntiAliasing = AntiAliasingStyles.All; 
                chart.TextAntiAliasingQuality = TextAntiAliasingQuality.High;

                if (chart.ChartAreas.Count == 0)
                {
                    chart.ChartAreas.Add("Default");
                }
                
                var area = chart.ChartAreas[0];
                area.Name = "Default";
                area.BackColor = SurfaceColor;
                
                // Optimized Positioning to prevent clipping
                area.Position.Auto = false;
                area.Position.X = 3;
                area.Position.Y = 10;
                area.Position.Width = 92;
                area.Position.Height = 85;

                area.InnerPlotPosition.Auto = true; // Let it calculate based on labels

                area.AxisX.LabelStyle.Font = StandardFont;
                area.AxisY.LabelStyle.Font = StandardFont;
                area.AxisX.LabelStyle.ForeColor = SecondaryColor;
                area.AxisY.LabelStyle.ForeColor = SecondaryColor;

                area.AxisX.LineColor = Color.FromArgb(230, 230, 230);
                area.AxisY.LineColor = Color.Transparent;

                area.AxisX.MajorGrid.LineColor = Color.FromArgb(245, 245, 245);
                area.AxisY.MajorGrid.LineColor = Color.FromArgb(245, 245, 245);
                area.AxisX.MajorGrid.Enabled = false;
                
                // Restore premium look for series
                foreach (var s in chart.Series)
                {
                    // Smooth curves where appropriate
                    if (s.ChartType == SeriesChartType.Area) s.ChartType = SeriesChartType.SplineArea;
                    if (s.ChartType == SeriesChartType.Line) s.ChartType = SeriesChartType.Spline;
                    if (s.ChartType == SeriesChartType.Pie) s.ChartType = SeriesChartType.Doughnut;

                    if (s.ChartType == SeriesChartType.SplineArea || s.ChartType == SeriesChartType.Column)
                    {
                        if (s.Color.IsEmpty || s.Color.ToArgb() == Color.Blue.ToArgb()) s.Color = PrimaryColor;
                        s.BackGradientStyle = GradientStyle.TopBottom;
                        s.BackSecondaryColor = Color.FromArgb(100, s.Color);
                        if (s.ChartType == SeriesChartType.Column) s["PointWidth"] = "0.25";
                    }
                    
                    if (s.ChartType == SeriesChartType.Doughnut)
                    {
                        s["PieLabelStyle"] = "Outside";
                        s["PieDrawingStyle"] = "SoftEdge";
                    }
                }

                chart.EndInit();
            }
            catch { /* Chart error should not crash the app */ }
        }

        public static void ApplyEmojiButton(Button btn, Color backColor, Color hoverColor, Color textColor)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = backColor;
            btn.ForeColor = textColor;
            btn.Font = SmallBoldFont;
            btn.Cursor = Cursors.Hand;

            btn.MouseEnter += (s, e) => btn.BackColor = hoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;

            btn.Paint -= Btn_PaintEmojiButton;
            btn.Paint += Btn_PaintEmojiButton;
        }

        public static void ApplyHeaderIconStyle(Control c)
        {
            ApplyHeaderIconStyle(c, 0.7f);
        }

        public static void ApplyHeaderIconStyle(Control c, float scaleMultiplier)
        {
            c.Cursor = Cursors.Hand;
            c.BackColor = Color.Transparent;
            
            c.MouseEnter += (s, e) => { c.Tag = true; c.Invalidate(); };
            c.MouseLeave += (s, e) => { c.Tag = false; c.Invalidate(); };
            
            if (c is PictureBox pb)
            {
                Image icon = pb.Image;
                pb.Image = null; 

                pb.Paint += (s, e) => {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                    if (pb.Tag is bool hovered && hovered)
                    {
                        Rectangle r = new Rectangle(0, 0, pb.Width - 1, pb.Height - 1);
                        using (var path = GetRoundedPath(r, 6))
                        using (var b = new SolidBrush(Color.FromArgb(40, 255, 255, 255))) e.Graphics.FillPath(b, path);
                    }
                    
                    if (icon != null)
                    {
                        float ratio = Math.Min((float)pb.Width / icon.Width, (float)pb.Height / icon.Height) * scaleMultiplier;
                        int nw = (int)(icon.Width * ratio), nh = (int)(icon.Height * ratio);
                        e.Graphics.DrawImage(icon, (pb.Width - nw) / 2, (pb.Height - nh) / 2, nw, nh);
                    }
                };
            }
        }

        public static void ApplyWindowControl(Button btn, string type)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.Transparent;
            btn.Text = string.Empty; 
            btn.Cursor = Cursors.Default;
            btn.TabStop = false;

            btn.Paint -= WinCtrl_PaintMinimize;
            btn.Paint -= WinCtrl_PaintMaximize;
            btn.Paint -= WinCtrl_PaintClose;

            bool isDarkHeader = (btn.FindForm() is MainForm);
            Color defaultHover = isDarkHeader ? Color.FromArgb(40, 255, 255, 255) : Color.FromArgb(30, PrimaryColor);
            Color closeHover = DangerColor; 

            if (type == "Close")
            {
                btn.Paint += WinCtrl_PaintClose;
                btn.MouseEnter += (s, e) => { btn.BackColor = closeHover; btn.Invalidate(); };
                btn.MouseLeave += (s, e) => { btn.BackColor = Color.Transparent; btn.Invalidate(); };
            }
            else if (type == "Maximize" || type == "Restore" || type == "Minimize")
            {
                if (type == "Minimize") btn.Paint += WinCtrl_PaintMinimize;
                else btn.Paint += WinCtrl_PaintMaximize;
                btn.MouseEnter += (s, e) => { btn.BackColor = defaultHover; btn.Invalidate(); };
                btn.MouseLeave += (s, e) => { btn.BackColor = Color.Transparent; btn.Invalidate(); };
            }
        }

        private static void WinCtrl_PaintMinimize(object sender, PaintEventArgs e)
        {
            var btn = sender as Button; if (btn == null) return;
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            if (btn.BackColor != Color.Transparent)
            {
                Rectangle r = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                using (var path = GetRoundedPath(r, 6))
                using (var b = new SolidBrush(btn.BackColor)) g.FillPath(b, path);
            }
            int cx = btn.Width / 2, cy = btn.Height / 2;
            int lineW = 6;
            bool isDarkHeader = (btn.FindForm() is MainForm);
            Color iconColor = isDarkHeader ? Color.White : ThemeConfig.TextColorDark;
            using (var p = new Pen(iconColor, 1.5f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round })
                g.DrawLine(p, cx - lineW, cy, cx + lineW, cy);
        }

        private static void WinCtrl_PaintMaximize(object sender, PaintEventArgs e)
        {
            var btn = sender as Button; if (btn == null) return;
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            if (btn.BackColor != Color.Transparent)
            {
                Rectangle r = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                using (var path = GetRoundedPath(r, 6))
                using (var b = new SolidBrush(btn.BackColor)) g.FillPath(b, path);
            }
            int cx = btn.Width / 2, cy = btn.Height / 2;
            bool isRestore = (btn.Tag as string) == "Restore";
            bool isDarkHeader = (btn.FindForm() is MainForm);
            Color iconColor = isDarkHeader ? Color.White : ThemeConfig.TextColorDark;
            using (var p = new Pen(iconColor, 1.5f))
            {
                if (isRestore)
                {
                    g.DrawRectangle(p, cx - 4, cy - 2, 7, 6);
                    g.DrawRectangle(p, cx - 2, cy - 4, 7, 6);
                }
                else
                {
                    g.DrawRectangle(p, cx - 5, cy - 4, 10, 8);
                }
            }
        }

        private static void WinCtrl_PaintClose(object sender, PaintEventArgs e)
        {
            var btn = sender as Button; if (btn == null) return;
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (btn.BackColor != Color.Transparent)
            {
                Rectangle r = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                using (var path = GetRoundedPath(r, 6))
                using (var b = new SolidBrush(btn.BackColor)) 
                    g.FillPath(b, path);
            }

            int cx = btn.Width / 2, cy = btn.Height / 2;
            int s = 4; 
            bool isDarkHeader = (btn.FindForm() is MainForm);
            Color iconColor = isDarkHeader ? Color.White : ThemeConfig.TextColorDark;
            
            if (btn.BackColor == DangerColor) iconColor = Color.White;
            
            using (var p = new Pen(iconColor, 1.5f) { StartCap = System.Drawing.Drawing2D.LineCap.Round, EndCap = System.Drawing.Drawing2D.LineCap.Round })
            {
                g.DrawLine(p, cx - s, cy - s, cx + s, cy + s);
                g.DrawLine(p, cx + s, cy - s, cx - s, cy + s);
            }
        }

        private static void Btn_PaintEmojiButton(object sender, PaintEventArgs e)
        {
            var btn = sender as Button; if (btn == null) return;
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Rectangle r = new Rectangle(0, 0, btn.Width, btn.Height);
            if (btn.Parent != null)
                using (var pb = new SolidBrush(GetParentColor(btn)))
                    g.FillRectangle(pb, r);

            using (var path = GetRoundedPath(r, 8))
            using (var brush = new SolidBrush(btn.BackColor))
                g.FillPath(brush, path);

            string full = btn.Text ?? string.Empty;
            int spaceIdx = full.IndexOf(' ');
            string emojiPart = spaceIdx > 0 ? full.Substring(0, spaceIdx) : full;
            string labelPart = spaceIdx > 0 ? full.Substring(spaceIdx + 1) : string.Empty;

            Size emojiSize = TextRenderer.MeasureText(emojiPart, EmojiFont);
            Size labelSize = TextRenderer.MeasureText(labelPart, SmallBoldFont);
            int totalWidth = emojiSize.Width + labelSize.Width - 8; 
            int startX = (btn.Width - totalWidth) / 2;

            TextRenderer.DrawText(g, emojiPart, EmojiFont,
                new Rectangle(startX, 0, emojiSize.Width, btn.Height),
                btn.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);

            if (!string.IsNullOrEmpty(labelPart))
                TextRenderer.DrawText(g, labelPart, SmallBoldFont,
                    new Rectangle(startX + emojiSize.Width - 6, 0, labelSize.Width + 8, btn.Height),
                    btn.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);
        }

        public static void DrawRoundedButton(Button btn, Graphics g)
        {
            if (btn == null) return;
            
            // Standard icon buttons (e.g., standard_refresh) handle their own painting.
            // By dispatching them to DrawIconButton here, we eliminate the need for base.OnPaint to fire Paint events.
            if (btn.Tag != null)
            {
                string tag = btn.Tag.ToString();
                if (tag.StartsWith("standard_") || tag.StartsWith("success_"))
                {
                    _standardButtonKeys.TryGetValue(btn, out string key);
                    if (tag == "standard_add" || tag == "success_add")
                        DrawIconButton(btn, g, "add", key, Color.White, PrimaryColor, false);
                    else if (tag == "standard_refresh")
                        DrawIconButton(btn, g, "refresh", key, PrimaryColor, PrimaryColor, true);
                    else if (tag == "standard_delete")
                        DrawIconButton(btn, g, "delete", key, DangerColor, DangerColor, true);
                    return;
                }
            }
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
            
            using (var parentBrush = new SolidBrush(GetParentColor(btn)))
                g.FillRectangle(parentBrush, -1, -1, btn.Width + 2, btn.Height + 2);
                
            using (var path = GetRoundedPath(r, 12)) 
            using (var brush = new SolidBrush(btn.BackColor))
            {
                g.FillPath(brush, path);
                using (var glowPen = new Pen(Color.FromArgb(50, Color.White), 1f))
                    g.DrawPath(glowPen, path);
            }    

            // Draw image + text, centered as a group when ImageBeforeText
            if (btn.Image != null && btn.TextImageRelation == TextImageRelation.ImageBeforeText)
            {
                // Measure icon size
                int iconSize = btn.Height - 16;
                if (!string.IsNullOrEmpty(btn.Text)) iconSize = Math.Min(iconSize, 20);
                const int gap = 6;

                // Measure text width
                Size textSize = TextRenderer.MeasureText(btn.Text, btn.Font);
                int totalW = iconSize + gap + textSize.Width;

                // Start X to center the whole block
                int startX = (btn.Width - totalW) / 2;
                int iconY  = (btn.Height - iconSize) / 2;

                // Draw icon
                using (var tinted = TintImage(btn.Image, btn.ForeColor))
                    g.DrawImage(tinted, new Rectangle(startX, iconY, iconSize, iconSize));

                // Draw text right after icon
                int textX = startX + iconSize + gap;
                Rectangle textRect = new Rectangle(textX, 0, textSize.Width + 4, btn.Height);
                TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis;
                if (InventorySystem.Helpers.LocalizationManager.IsArabic)
                    flags |= TextFormatFlags.RightToLeft;
                TextRenderer.DrawText(g, btn.Text, btn.Font, textRect, btn.ForeColor, flags);
            }
            else
            {
                // No image — just draw text centered
                if (btn.Image != null)
                {
                    Rectangle imgRect = GetImageRectangle(btn);
                    using (var tinted = TintImage(btn.Image, btn.ForeColor))
                        g.DrawImage(tinted, imgRect);
                }

                TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding;
                if (InventorySystem.Helpers.LocalizationManager.IsArabic)
                    flags |= TextFormatFlags.RightToLeft;
                TextRenderer.DrawText(g, btn.Text, btn.Font, Rectangle.Round(r), btn.ForeColor, flags);
            }
        }

        private static Rectangle GetImageRectangle(Button btn)
        {
            if (btn.Image == null) return Rectangle.Empty;

            int imgW = btn.Image.Width;
            int imgH = btn.Image.Height;

            // Scale to fit
            // If button has text, make icon smaller (approx 18-20px)
            int targetH = btn.Height - 16;
            if (!string.IsNullOrEmpty(btn.Text)) targetH = Math.Min(targetH, 20);
            int targetW = targetH; // Keep square icons

            float ratio = Math.Min((float)targetW / imgW, (float)targetH / imgH);
            imgW = (int)(imgW * ratio);
            imgH = (int)(imgH * ratio);

            int x = (btn.Width - imgW) / 2;
            int y = (btn.Height - imgH) / 2;

            if (btn.TextImageRelation == TextImageRelation.ImageBeforeText)
            {
                x = 12; // Left aligned with padding
            }
            else
            {
                switch (btn.ImageAlign)
                {
                    case ContentAlignment.TopLeft: x = 8; y = 8; break;
                    case ContentAlignment.TopCenter: y = 8; break;
                    case ContentAlignment.TopRight: x = btn.Width - imgW - 8; y = 8; break;
                    case ContentAlignment.MiddleLeft: x = 8; break;
                    case ContentAlignment.MiddleRight: x = btn.Width - imgW - 8; break;
                    case ContentAlignment.BottomLeft: x = 8; y = btn.Height - imgH - 8; break;
                    case ContentAlignment.BottomCenter: y = btn.Height - imgH - 8; break;
                    case ContentAlignment.BottomRight: x = btn.Width - imgW - 8; y = btn.Height - imgH - 8; break;
                }
            }
            return new Rectangle(x, y, imgW, imgH);
        }

        private static void Btn_PaintRounded(object sender, PaintEventArgs e)
        {
             DrawRoundedButton(sender as Button, e.Graphics);
        }

        public static GraphicsPath GetRoundedPathPublic(Rectangle rect, float radius)
        {
            var path = new GraphicsPath();
            float d = radius * 2;
            RectangleF r = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height); 
            if (d > r.Width) d = r.Width;
            if (d > r.Height) d = r.Height;
            if (d <= 0.1f) d = 1f;

            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static GraphicsPath GetRoundedPath(Rectangle rect, float radius)
        {
            return GetRoundedPathPublic(rect, radius);
        }

        /// <summary>
        /// Rounded path on float bounds, so a stroke can be centred on a half-pixel.
        /// </summary>
        public static GraphicsPath GetRoundedPathF(RectangleF r, float radius)
        {
            var path = new GraphicsPath();
            float d = radius * 2f;
            if (d > r.Width) d = r.Width;
            if (d > r.Height) d = r.Height;
            if (d <= 0.1f) d = 1f;

            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Fills a control's full bounds with a rounded background. Use with
        /// <see cref="DrawRoundedBorder"/> so the fill reaches the edge while the
        /// stroke stays inside it.
        /// </summary>
        public static void FillRoundedBackground(Graphics g, Rectangle bounds, float radius, Color color)
        {
            if (bounds.Width < 2 || bounds.Height < 2) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = GetRoundedPathF(new RectangleF(0, 0, bounds.Width, bounds.Height), radius);
            using var brush = new SolidBrush(color);
            g.FillPath(brush, path);
        }

        /// <summary>
        /// Draws a rounded outline that fits entirely within a control's bounds.
        /// A stroke centred on the bounds loses its outer half to the control edge
        /// (and to any rounded Region, whose clip is hard-edged), which is what makes
        /// borders look thin, broken, or disconnected at the corners. Insetting by
        /// half the pen width plus a half-pixel keeps the whole stroke visible.
        /// </summary>
        public static void DrawRoundedBorder(Graphics g, Rectangle bounds, float radius, Color color,
            float penWidth = 1f, float extraInset = 0f)
        {
            if (bounds.Width < 3 || bounds.Height < 3) return;
            float inset = penWidth / 2f + 0.5f + extraInset;
            var r = new RectangleF(inset, inset, bounds.Width - inset * 2f, bounds.Height - inset * 2f);
            if (r.Width < 1f || r.Height < 1f) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = GetRoundedPathF(r, Math.Max(1f, radius - inset));
            using var pen = new Pen(color, penWidth);
            g.DrawPath(pen, path);
        }

        public static void ApplyPrintPreviewTheme(PrintPreviewDialog preview)
        {
            if (preview == null) return;

            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            preview.FormBorderStyle = FormBorderStyle.None;
            preview.BackColor = Color.White;
            preview.ShowIcon = false;
            preview.WindowState = FormWindowState.Normal;
            
            // A4 Ratio (210:297) - Approx 1:1.41
            int targetHeight = (int)(workingArea.Height * 0.85);
            int targetWidth = (int)(targetHeight / 1.414);
            
            // Limit width if it exceeds screen
            if (targetWidth > workingArea.Width * 0.9)
            {
                targetWidth = (int)(workingArea.Width * 0.9);
                targetHeight = (int)(targetWidth * 1.414);
            }

            preview.Size = new Size(targetWidth, targetHeight);
            preview.StartPosition = FormStartPosition.CenterParent;

            // Internal components
            ToolStrip ts = null;
            PrintPreviewControl ppc = null;
            foreach (Control c in preview.Controls)
            {
                if (c is ToolStrip) ts = (ToolStrip)c;
                if (c is PrintPreviewControl) ppc = (PrintPreviewControl)c;
            }

            // 1. Position the ToolStrip in our "Styled Header" area
            if (ts != null)
            {
                ts.BackColor = Color.White;
                ts.GripStyle = ToolStripGripStyle.Hidden;
                ts.AutoSize = false;
                ts.Height = 35;
                ts.Dock = DockStyle.None;
                ts.CanOverflow = false; // Remove the extra section / overflow arrow
                ts.Location = new Point(20, 45); 
                ts.Width = preview.Width - 40;
                ts.Padding = new Padding(0);
                ts.Renderer = new ModernNotificationRenderer();

                foreach (ToolStripItem item in ts.Items)
                {
                    if (item is ToolStripButton btn)
                    {
                        btn.AutoSize = false;
                        btn.Size = new Size(32, 32);
                        btn.Margin = new Padding(2);
                        btn.DisplayStyle = ToolStripItemDisplayStyle.Image;
                        
                        // Map internal names to themed icons
                        string name = item.Name.ToLower();
                        if (name.Contains("close") || name.Contains("info")) { item.Visible = false; continue; }

                        if (name.Contains("print")) item.Image = GetNuricon("print");
                        else if (name.Contains("zoom") || name.Contains("search")) item.Image = GetNuricon("search");
                        else if (name.Contains("onepage")) item.Image = GetNuricon("one_page");
                        else if (name.Contains("twopage")) item.Image = GetNuricon("two_pages");
                        else if (name.Contains("threepage")) item.Image = GetNuricon("three_pages");
                        else if (name.Contains("fourpage")) item.Image = GetNuricon("four_pages"); 
                        else if (name.Contains("sixpage")) item.Image = GetNuricon("six_pages");
                        else item.Image = GetNuricon("view");
                    }
                    else if (item is ToolStripSeparator)
                    {
                        item.Visible = false; // Hide separators for cleaner look
                    }
                }
            }

            // 2. Styled Close Button & Title
            Label lblTitle = new Label { 
                Text = preview.Text.ToUpper(), 
                Font = new Font("Segoe UI", 9, FontStyle.Bold), 
                ForeColor = ThemeConfig.PrimaryColor,
                AutoSize = true,
                Location = new Point(20, 15)
            };
            preview.Controls.Add(lblTitle);

            Button btnClose = new Button { Size = new Size(32, 32), Location = new Point(preview.Width - 40, 15), Cursor = Cursors.Hand };
            ApplyWindowControl(btnClose, "Close");
            btnClose.Click += (s, e) => preview.Close();
            preview.Controls.Add(btnClose);
            btnClose.BringToFront();

            // 3. PrintPreviewControl
            if (ppc != null)
            {
                ppc.BackColor = BackgroundColor;
                ppc.Dock = DockStyle.None;
                ppc.Location = new Point(8, 92); // Inset from left/right to protect borders
                ppc.Size = new Size(preview.Width - 16, preview.Height - 100);
                ppc.Zoom = 1.0;
                ppc.Columns = 1;
                ppc.AutoZoom = true;
            }

            // 4. Painting (Header & Neon Border)
            preview.Paint += (s, e) => {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.White);

                Rectangle bounds = new Rectangle(0, 0, preview.Width, preview.Height);
                const float radius = 16f;

                // Clip the form to the rounded shape, then keep every stroke inside it.
                using (var clip = GetRoundedPathF(new RectangleF(0, 0, bounds.Width, bounds.Height), radius))
                {
                    var oldRegion = preview.Region;
                    preview.Region = new Region(clip);
                    oldRegion?.Dispose();
                }

                Color neonColor = PrimaryColor;
                DrawRoundedBorder(g, bounds, radius, Color.FromArgb(40, neonColor), 4f);
                DrawRoundedBorder(g, bounds, radius, Color.FromArgb(70, neonColor), 2f);
                DrawRoundedBorder(g, bounds, radius, neonColor, 1.8f);

                // Header Separator line - Darker and more obvious
                using (Pen sep = new Pen(Color.FromArgb(220, 225, 235), 1.5f))
                    g.DrawLine(sep, 3, 90, preview.Width - 3, 90);
            };

            // Support dragging
            preview.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left && e.Y < 90)
                {
                    ReleaseCapture();
                    SendMessage(preview.Handle, 0xA1, 0x2, 0);
                }
            };
        }

        [DllImport("user32.dll")] private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")] private static extern bool ReleaseCapture();

                public static void ApplyPaletteButton(Button btn, Color baseColor)
        {
            btn.Tag = "palette_button";
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = baseColor;
            btn.Height = 35;
            btn.ForeColor = TextColorWhite;
            btn.Font = SmallBoldFont;
            btn.Cursor = Cursors.Hand;
            btn.TextAlign = ContentAlignment.MiddleCenter;
            
            Color hoverColor = ControlPaint.Light(baseColor, 0.2f);
            
            btn.MouseEnter += (s, e) => btn.BackColor = hoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = baseColor;

        }

        public static void ApplyDangerButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = DangerColor;
            btn.Height = 35;
            btn.ForeColor = TextColorLight;
            btn.Font = SmallBoldFont;
            btn.Cursor = Cursors.Hand;
            btn.TextAlign = ContentAlignment.MiddleCenter;
            
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(0, 184, 138); // Slightly darker/brighter green for hover
            btn.MouseLeave += (s, e) => btn.BackColor = SuccessColor;

        }

        public static void ApplySecondaryButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.FromArgb(230, 230, 240);
            btn.Height = 35;
            btn.ForeColor = TextColorDark;
            btn.Font = SmallBoldFont;
            btn.Cursor = Cursors.Hand;
            btn.TextAlign = ContentAlignment.MiddleCenter;
            
            btn.MouseEnter += (s, e) => btn.BackColor = SecondaryHoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(230, 230, 240);

        }

        public static void ApplyGridTheme(DataGridView grid)
        {
            grid.BackgroundColor = SurfaceColor;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.EnableHeadersVisualStyles = false;
            grid.GridColor = Color.FromArgb(230, 230, 230);
            
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = TextColorDark; 
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = SelectionBackColor;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextColorDark; 
            grid.ColumnHeadersDefaultCellStyle.Font = SmallBoldFont; 
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None; 
            grid.ColumnHeadersHeight = 45; 
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            grid.DefaultCellStyle.BackColor = SurfaceColor;
            grid.DefaultCellStyle.ForeColor = TextColorDark;
            grid.DefaultCellStyle.Font = StandardFont; 

            // Center all cell and header text regardless of language direction
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            if (Helpers.LocalizationManager.IsArabic)
            {
                grid.RightToLeft = RightToLeft.Yes;
            }
            else
            {
                grid.RightToLeft = RightToLeft.No;
            }
            
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(237, 242, 247);
            grid.DefaultCellStyle.SelectionForeColor = TextColorDark;
            grid.Padding = new Padding(12, 5, 5, 5); 
            
            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (col is DataGridViewCheckBoxColumn)
                {
                    col.SortMode = DataGridViewColumnSortMode.NotSortable;
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }
            grid.ColumnAdded += (s, e) =>
            {
                if (e.Column is DataGridViewCheckBoxColumn)
                {
                    e.Column.SortMode = DataGridViewColumnSortMode.NotSortable;
                    e.Column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    e.Column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            };

            grid.RowHeadersVisible = false;
            grid.RowTemplate.Height = 60; 
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.White; 
            
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AllowUserToResizeRows = false;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Color.FromArgb(240, 240, 240);

            grid.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (grid.IsCurrentCellDirty && grid.CurrentCell is DataGridViewCheckBoxCell)
                {
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };
        }

        public static CheckBox ApplyHeaderCheckBox(DataGridView grid, string checkBoxColumnName = "colSelect")
        {
            if (grid == null) return null;

            CheckBox headerCheckBox = new CheckBox 
            { 
                Size = new Size(15, 15), 
                BackColor = Color.Transparent, 
                Cursor = Cursors.Hand 
            };

            headerCheckBox.CheckedChanged += (s, e) =>
            {
                grid.EndEdit();
                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (grid.Columns.Contains(checkBoxColumnName))
                        row.Cells[checkBoxColumnName].Value = headerCheckBox.Checked;
                }
            };

            grid.CellPainting += (s, e) =>
            {
                if (e.RowIndex == -1 && grid.Columns.Contains(checkBoxColumnName) && e.ColumnIndex == grid.Columns[checkBoxColumnName].Index)
                {
                    e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                    headerCheckBox.Location = new Point(
                        e.CellBounds.X + (e.CellBounds.Width - headerCheckBox.Width) / 2, 
                        e.CellBounds.Y + (e.CellBounds.Height - headerCheckBox.Height) / 2
                    );
                    e.Handled = true;
                }
            };

            grid.Controls.Add(headerCheckBox);
            return headerCheckBox;
        }

        public static Image TintImage(Image source, Color tintColor)
        {
            if (source == null) return null;
            Bitmap bmp = new Bitmap(source.Width, source.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                var cm = new System.Drawing.Imaging.ColorMatrix(new float[][]
                {
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {tintColor.R/255f, tintColor.G/255f, tintColor.B/255f, 0, 1}
                });
                var attributes = new System.Drawing.Imaging.ImageAttributes();
                attributes.SetColorMatrix(cm);
                g.DrawImage(source, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
            }
            return bmp;
        }

        public static void ApplySidebarButton(Button btn, bool isActive)
        {
             btn.FlatStyle = FlatStyle.Flat;
             btn.FlatAppearance.BorderSize = 0;
             btn.BackColor = isActive ? ActiveBackColor : Color.Transparent; 
             btn.ForeColor = isActive ? PrimaryColor : Color.FromArgb(160, 174, 192); // Gray when inactive
             btn.Font = new Font("Segoe UI", 10F, isActive ? FontStyle.Bold : FontStyle.Regular);
             btn.Cursor = Cursors.Hand;
             btn.TextAlign = LocalizationManager.IsArabic ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
             
             btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(244, 247, 254);
             btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(230, 235, 245);
        }

        public static void ApplySidebarButtonIcon(Button btn, Image icon, bool isActive)
        {
            ApplySidebarButton(btn, isActive);
            
            // Tint the icon to match the text color (Blue if active, Gray if inactive)
            Color tintColor = isActive ? PrimaryColor : Color.FromArgb(31, 41, 55); 
            btn.Image = TintImage(icon, tintColor);
            
            bool isAr = LocalizationManager.IsArabic;
            // Keep RightToLeft.No: Button mirrors TextAlign/TextImageRelation when RTL is Yes,
            // which put Arabic labels on the left even after we asked for MiddleRight.
            btn.RightToLeft = RightToLeft.No;
            btn.TextAlign = isAr ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
            btn.ImageAlign = isAr ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
            btn.TextImageRelation = isAr ? TextImageRelation.TextBeforeImage : TextImageRelation.ImageBeforeText;
            int padLeft = isAr ? 0 : 15;
            int padRight = isAr ? 15 : 0;
            btn.Padding = new Padding(padLeft, 0, padRight, 0);
        }

        public static Image GetNuricon(string name)
        {
            try
            {
                string[] exts = { ".svg", ".png" };
                Image img = null;
                bool isSvg = false;

                foreach (var ext in exts)
                {
                    string filename = $"nuricon_{name}{ext}";
                    string path = Path.Combine(Application.StartupPath, "Assets", filename);
                    
                    if (!File.Exists(path)) {
                        string currentDir = Application.StartupPath;
                        for (int i = 0; i < 4; i++) {
                            string checkPath = Path.Combine(currentDir, "Assets", filename);
                            if (File.Exists(checkPath)) { path = checkPath; break; }
                            var parent = Directory.GetParent(currentDir);
                            if (parent == null) break;
                            currentDir = parent.FullName;
                        }
                    }

                    if (File.Exists(path)) {
                        if (ext == ".svg") {
                            try {
                                // Use the Svg library if available
                                var svgDoc = Svg.SvgDocument.Open(path);
                                img = svgDoc.Draw(64, 64);
                                isSvg = true;
                            } catch { /* Fallback to next extension if SVG fails */ }
                        } else {
                            img = Image.FromFile(path);
                        }
                    }
                    if (img != null) break;
                }

                if (img != null)
                {
                    Bitmap bmp = new Bitmap(img.Width, img.Height);
                    using (Graphics g = Graphics.FromImage(bmp)) {
                        g.Clear(Color.Transparent);
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(img, 0, 0, img.Width, img.Height);
                    }
                    if (!isSvg) img.Dispose();
                    return bmp;
                }
                return GenerateNuriconFallback(name);
            }
            catch { return GenerateNuriconFallback(name); }
        }

        private static Image GenerateNuriconFallback(string name)
        {
            try 
            {
                Bitmap bmp = new Bitmap(64, 64);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);

                    // Default Gradient (Blue to Purple)
                    Color c1 = Color.FromArgb(37, 99, 235); 
                    Color c2 = Color.FromArgb(147, 51, 234); 

                    // Specific colors based on icon type
                    if (name == "refresh") { c1 = Color.FromArgb(5, 205, 153); c2 = Color.FromArgb(37, 99, 235); }
                    else if (name.Contains("import")) { c1 = Color.FromArgb(22, 163, 74); c2 = Color.FromArgb(20, 184, 166); } 
                    else if (name.Contains("export")) { c1 = Color.FromArgb(37, 99, 235); c2 = Color.FromArgb(6, 182, 212); } 
                    else if (name.Contains("filter")) { c1 = Color.FromArgb(249, 115, 22); c2 = Color.FromArgb(236, 72, 153); }
                    else if (name.Contains("search")) { c1 = Color.FromArgb(79, 70, 229); c2 = Color.FromArgb(124, 58, 237); }
                    else if (name.Contains("orders")) { c1 = Color.FromArgb(217, 70, 239); c2 = Color.FromArgb(168, 85, 247); }
                    else if (name.Contains("revenue") || name.Contains("sales")) { c1 = Color.FromArgb(245, 158, 11); c2 = Color.FromArgb(252, 211, 77); }
                    else if (name == "warning") { c1 = Color.FromArgb(245, 158, 11); c2 = Color.FromArgb(251, 191, 36); }
                    else if (name == "info") { c1 = Color.FromArgb(59, 130, 246); c2 = Color.FromArgb(96, 165, 250); }
                    else if (name == "check" || name == "success") { c1 = Color.FromArgb(34, 197, 94); c2 = Color.FromArgb(74, 222, 128); }
                    else if (name == "add" || name == "plus") { c1 = Color.FromArgb(59, 130, 246); c2 = Color.FromArgb(147, 51, 234); }
                    else if (name == "pos") { c1 = Color.FromArgb(5, 205, 153); c2 = Color.FromArgb(20, 184, 166); }
                    else if (name == "inventory") { c1 = Color.FromArgb(99, 102, 241); c2 = Color.FromArgb(168, 85, 247); }
                    else if (name == "customers") { c1 = Color.FromArgb(59, 130, 246); c2 = Color.FromArgb(37, 99, 235); }
                    else if (name == "suppliers") { c1 = Color.FromArgb(20, 184, 166); c2 = Color.FromArgb(13, 148, 136); }
                    else if (name == "reports") { c1 = Color.FromArgb(37, 99, 235); c2 = Color.FromArgb(29, 78, 216); }
                    else if (name == "history" || name == "expenses") { c1 = Color.FromArgb(244, 63, 94); c2 = Color.FromArgb(225, 29, 72); }
                    else if (name == "quotations") { c1 = Color.FromArgb(37, 99, 235); c2 = Color.FromArgb(29, 78, 216); }
                    else if (name == "currencies") { c1 = Color.FromArgb(245, 158, 11); c2 = Color.FromArgb(217, 119, 6); }
                    else if (name == "user") { c1 = Color.FromArgb(79, 70, 229); c2 = Color.FromArgb(67, 56, 202); }
                    else if (name == "barcode") { c1 = Color.FromArgb(59, 130, 246); c2 = Color.FromArgb(139, 92, 246); }
                    else if (name == "engine") { c1 = Color.FromArgb(239, 68, 68); c2 = Color.FromArgb(185, 28, 28); }
                    else if (name == "brakes") { c1 = Color.FromArgb(249, 115, 22); c2 = Color.FromArgb(194, 65, 12); }
                    else if (name == "accessories") { c1 = Color.FromArgb(37, 99, 235); c2 = Color.FromArgb(29, 78, 216); }

                    using (var brush = new LinearGradientBrush(new Rectangle(8, 8, 48, 48), c1, c2, 45f))
                    {
                        if (name == "search")
                        {
                            g.DrawEllipse(new Pen(brush, 6), 12, 12, 28, 28);
                            g.DrawLine(new Pen(brush, 8) { StartCap = LineCap.Round, EndCap = LineCap.Round }, 36, 36, 52, 52);
                        }
                        else if (name == "import")
                        {
                            g.FillPolygon(brush, new Point[] { new Point(32, 50), new Point(14, 28), new Point(50, 28) });
                            g.FillRectangle(brush, 26, 8, 12, 20);
                        }
                        else if (name == "export")
                        {
                            g.FillPolygon(brush, new Point[] { new Point(32, 8), new Point(14, 30), new Point(50, 30) });
                            g.FillRectangle(brush, 26, 30, 12, 20);
                        }
                        else if (name == "filter")
                        {
                            g.FillPolygon(brush, new Point[] { new Point(8, 8), new Point(56, 8), new Point(36, 34), new Point(36, 56), new Point(28, 56), new Point(28, 34) });
                        }
                        else if (name == "orders")
                        {
                            using (var pen = new Pen(brush, 6) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                            using (var sBrush = new SolidBrush(c1))
                            {
                                g.DrawLine(pen, 16, 20, 24, 20);
                                g.DrawLine(pen, 24, 20, 30, 42);
                                g.DrawLine(pen, 30, 42, 48, 42);
                                g.DrawLine(pen, 48, 42, 52, 28);
                                g.DrawLine(pen, 26, 28, 52, 28);
                                g.FillEllipse(sBrush, 32, 46, 6, 6);
                                g.FillEllipse(sBrush, 44, 46, 6, 6);
                            }
                        }
                        else if (name == "barcode")
                        {
                            g.FillRectangle(brush, 14, 20, 4, 24); 
                            g.FillRectangle(brush, 22, 20, 2, 24); 
                            g.FillRectangle(brush, 28, 20, 6, 24); 
                            g.FillRectangle(brush, 38, 20, 2, 24); 
                            g.FillRectangle(brush, 44, 20, 4, 24); 
                        }
                        else if (name == "inventory")
                        {
                            // Draw a simple box symbol
                            g.DrawRectangle(new Pen(brush, 6), 12, 20, 40, 32);
                            g.DrawLine(new Pen(brush, 4), 12, 30, 52, 30);
                        }
                        else if (name == "customers" || name == "user" || name == "users")
                        {
                            using (var pen = new Pen(brush, 4))
                            {
                                g.DrawEllipse(pen, 24, 18, 16, 16); 
                                g.DrawArc(pen, 16, 34, 32, 32, 180, 180); 
                            }
                        }
                        else if (name == "suppliers")
                        {
                            using (var pen = new Pen(brush, 4))
                            {
                                g.DrawRectangle(pen, 16, 26, 24, 16); 
                                g.DrawRectangle(pen, 40, 32, 8, 10);  
                                g.DrawEllipse(pen, 20, 42, 6, 6);     
                                g.DrawEllipse(pen, 36, 42, 6, 6);     
                            }
                        }
                        else if (name == "expenses" || name == "history")
                        {
                            using (var pen = new Pen(brush, 4) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                            {
                                g.DrawArc(pen, 18, 18, 28, 28, 45, 270);
                                g.DrawLine(pen, 32, 22, 32, 32);
                                g.DrawLine(pen, 32, 32, 40, 32);
                            }
                        }
                        else if (name == "engine")
                        {
                            using (var pen = new Pen(brush, 5))
                            {
                                g.DrawRectangle(pen, 16, 24, 32, 20);
                                g.DrawLine(pen, 12, 30, 16, 30);
                                g.DrawLine(pen, 48, 30, 52, 30);
                                g.DrawEllipse(pen, 22, 28, 20, 12);
                            }
                        }
                        else if (name == "brakes")
                        {
                            using (var pen = new Pen(brush, 5))
                            {
                                g.DrawEllipse(pen, 16, 16, 32, 32);
                                g.DrawArc(new Pen(brush, 8), 12, 12, 40, 40, 135, 90);
                                g.DrawArc(new Pen(brush, 8), 12, 12, 40, 40, 315, 90);
                            }
                        }
                        else if (name == "accessories")
                        {
                            using (var pen = new Pen(brush, 5))
                            {
                                g.DrawEllipse(pen, 14, 14, 36, 36);
                                g.DrawLine(pen, 32, 14, 32, 50);
                                g.DrawLine(pen, 14, 32, 50, 32);
                            }
                        }
                        else if (name == "add" || name == "plus")
                        {
                            using (var whitePen = new Pen(Color.White, 8) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                            {
                                g.DrawLine(whitePen, 32, 16, 32, 48);
                                g.DrawLine(whitePen, 16, 32, 48, 32);
                            }
                        }
                        else if (name == "remove" || name == "delete" || name == "minus")
                        {
                            using (var whitePen = new Pen(Color.White, 8) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                            {
                                g.DrawLine(whitePen, 16, 32, 48, 32);
                            }
                        }
                        else if (name == "refresh")
                        {
                            using (var p = new Pen(brush, 6) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                            {
                                g.DrawArc(p, 14, 14, 36, 36, 45, 270);
                                g.FillPolygon(brush, new Point[] { new Point(42, 10), new Point(52, 22), new Point(36, 26) });
                            }
                        }
                        else if (name == "chevron_up")
                        {
                            using (var p = new Pen(brush, 8) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
                                g.DrawLines(p, new Point[] { new Point(12, 44), new Point(32, 20), new Point(52, 44) });
                        }
                        else if (name == "chevron_down")
                        {
                            using (var p = new Pen(brush, 8) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
                                g.DrawLines(p, new Point[] { new Point(12, 20), new Point(32, 44), new Point(52, 20) });
                        }
                        else
                        {
                            g.FillEllipse(brush, 8, 8, 48, 48);
                        }
                    }
                }
                return bmp;
            } catch { return null; }
        }

        public static void ApplyFormIcon(Form form)
        {
            try
            {
                string iconPath = Path.Combine(Application.StartupPath, "Assets", "icon.ico");
                if (File.Exists(iconPath)) form.Icon = new Icon(iconPath);
            }
            catch { }
        }

        public static (Panel panel, ComboBox combo) CreateStyledCurrencySelector(int width = 140, int height = 36)
        {
            ComboBox cbo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = StandardFont, ForeColor = TextColorDark, BackColor = SurfaceColor, FlatStyle = FlatStyle.Flat, Dock = DockStyle.Fill };
            Panel wrapper = new Panel { Size = new Size(width, height), BackColor = SurfaceColor, Padding = new Padding(4, 4, 4, 0) };
            wrapper.Paint += (s, e) => {
                DrawRoundedBorder(e.Graphics, wrapper.ClientRectangle, 8f, BorderColor, 1.5f);
            };
            wrapper.Controls.Add(cbo);
            return (wrapper, cbo);
        }

        public static Panel CreateCardPanel(Control inner)
        {
            Panel p = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            Panel card = new Panel { Dock = DockStyle.Fill, BackColor = SurfaceColor, Padding = new Padding(15) };
            card.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Color parentColor = GetParentColor(card);
                using (var bgBrush = new SolidBrush(parentColor))
                    e.Graphics.FillRectangle(bgBrush, -1, -1, card.Width + 2, card.Height + 2);
                FillRoundedBackground(e.Graphics, card.ClientRectangle, 15f, SurfaceColor);
                DrawRoundedBorder(e.Graphics, card.ClientRectangle, 15f, BorderColor, 1f);
            };
            card.Controls.Add(inner);
            inner.Dock = DockStyle.Fill;
            p.Controls.Add(card);
            return p;
        }

        public static void ApplyModernMenuTheme(ContextMenuStrip menu)
        {
            menu.Renderer = new ModernNotificationRenderer();
            menu.BackColor = SurfaceColor;
        }

        // ==========================================
        // GENERIC BASE STYLING METHODS
        // ==========================================
        
        public static void ApplyFormStyle(Form form)
        {
            form.BackColor = BackgroundColor;
            form.Font = StandardFont;
            form.ForeColor = TextColorDark;
        }

                public static void ApplyUserControlStyle(UserControl uc)
        {
            if (uc.GetType().Name.StartsWith("Modern") || uc.GetType().Name == "StatCard")
            {
                // Modern controls and StatCards manage their own background
            }
            else
            {
                uc.BackColor = BackgroundColor;
            }
            uc.Font = StandardFont;
            uc.ForeColor = TextColorDark;
        }

        public static void ApplyPanelStyle(Panel pnl)
        {
            pnl.BackColor = BackgroundColor;
        }
        
        public static void ApplyCardStyle(Panel pnl)
        {
            pnl.BackColor = SurfaceColor;
            // Rounded corners and borders should be applied via Paint events if needed, 
            // but simply setting the background ensures consistency.
        }

        public static void ApplyTextBoxStyle(TextBox txt)
        {
            txt.BackColor = SurfaceColor;
            txt.ForeColor = TextColorDark;
            txt.Font = StandardFont;
            txt.BorderStyle = BorderStyle.None; // Usually wrapped in WrapInStyledInput
            if (Helpers.LocalizationManager.IsArabic) txt.RightToLeft = RightToLeft.Yes;
        }

        public static void ApplySearchBoxStyle(TextBox txt)
        {
            ApplyTextBoxStyle(txt);
            // Any search specific tweaks can go here.
        }

        public static void ApplyLabelStyle(Label lbl, string type = "Standard")
        {
            lbl.BackColor = Color.Transparent;
            switch (type)
            {
                case "Header":
                    lbl.Font = HeaderFont;
                    lbl.ForeColor = PrimaryColor;
                    break;
                case "Subtitle":
                    lbl.Font = SubHeaderFont;
                    lbl.ForeColor = SecondaryColor;
                    break;
                case "Muted":
                    lbl.Font = SmallFont;
                    lbl.ForeColor = MutedTextColor;
                    break;
                case "Danger":
                    lbl.Font = SmallBoldFont;
                    lbl.ForeColor = DangerColor;
                    break;
                case "Success":
                    lbl.Font = SmallBoldFont;
                    lbl.ForeColor = SuccessColor;
                    break;
                case "Standard":
                default:
                    lbl.Font = StandardFont;
                    lbl.ForeColor = TextColorDark;
                    break;
            }
        }
        // ==========================================
        // DYNAMIC GLOBAL THEMER
        // ==========================================
                public static void ApplyGlobalTheme(Control parent)
        {
            if (parent is Form f)
            {
                ApplyFormStyle(f);
            }
            else if (parent is UserControl uc)
            {
                ApplyUserControlStyle(uc);
            }



            foreach (Control c in parent.Controls)
            {
                // Skip specific styled panels or charts that are manually configured
                if (c is Chart) continue;
                  if (c.GetType().Name == "StatCard") continue; 
                
                // Recursively style children
                if (c.HasChildren) ApplyGlobalTheme(c);

                if (c is Button btn)
                {
                    // Ignore window controls
                    if (btn.Text == string.Empty && btn.Name.StartsWith("btnWindow")) continue;
                    // Ignore emojis (they usually have a specific paint handler)
                    if (btn.Name.StartsWith("btnIcon")) continue;
                    // Ignore custom tabs
                    if (btn.Name.ToLower().StartsWith("btntab")) continue;

                    // Ignore already paletted or standard buttons
                    if (btn.Tag != null && (btn.Tag.ToString() == "palette_button" || btn.Tag.ToString().StartsWith("standard_") || btn.Tag.ToString().StartsWith("success_"))) continue; 

                    string name = btn.Name.ToLower();
                    if (name.Contains("delete") || name.Contains("remove") || name.Contains("clear"))
                        ApplyDangerButton(btn);
                    else if (name.Contains("save") || name.Contains("add") || name.Contains("update") || name.Contains("confirm") || name.Contains("ok") || name.Contains("print") || name.Contains("submit") || name.Contains("scan"))
                        ApplyPrimaryButton(btn);
                    else if (!name.Contains("emoji"))
                        ApplySecondaryButton(btn);
                }

                else if (c is TextBox txt)
                {
                    ApplyTextBoxStyle(txt);
                }
                else if (c is ComboBox cbo)
                {
                    ApplyComboBoxStyle(cbo);
                }
                else if (c is DataGridView dgv)
                {
                    ApplyGridTheme(dgv);
                }
                else if (c is Label lbl)
                {
                    // Do not downgrade labels that were explicitly made bold by the developer
                    if (lbl.Font != null && lbl.Font.Bold)
                    {
                        lbl.BackColor = Color.Transparent;
                        if (lbl.Font.Size >= HeaderFont.Size)
                            lbl.ForeColor = PrimaryColor; // Keep giant headers primary blue
                    }
                    // Try to guess label type by name keyword
                    else if (lbl.Name.ToLower().Contains("title") || lbl.Name.ToLower().Contains("header"))
                        ApplyLabelStyle(lbl, "Header");
                    else if (lbl.Name.ToLower().Contains("sub") || lbl.Name.ToLower().Contains("desc"))
                        ApplyLabelStyle(lbl, "Subtitle");
                    else if (lbl.Name.ToLower().Contains("error") || lbl.Name.ToLower().Contains("warning"))
                        ApplyLabelStyle(lbl, "Danger");
                    else if (lbl.Name.ToLower().Contains("success"))
                        ApplyLabelStyle(lbl, "Success");
                    else
                        ApplyLabelStyle(lbl, "Standard");
                }
                else if (c is Panel pnl)
                {
                    // Do not override complex specific layouts like headers, but ensure base color is standard
                    if (pnl.BackColor == Color.White || pnl.BackColor == Color.Transparent || pnl.BackColor == ThemeConfig.SurfaceColor)
                    {
                        // leave as is if intentionally card-like
                    }
                    else
                    {
                        ApplyPanelStyle(pnl);
                    }
                }
            }
        }
    }

    public class ModernNotificationRenderer : ToolStripProfessionalRenderer
    {
        public ModernNotificationRenderer() : base(new ModernColorTable()) { }
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Selected)
            {
                Rectangle rc = new Rectangle(4, 2, e.Item.Width - 8, e.Item.Height - 4);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using(var path = ThemeConfig.GetRoundedPathPublic(rc, 8))
                using(var brush = new SolidBrush(ThemeConfig.ActiveBackColor))
                    e.Graphics.FillPath(brush, path);
            }
        }
    }

    public class ModernColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected => ThemeConfig.ActiveBackColor;
        public override Color MenuItemBorder => Color.Transparent;
        public override Color ToolStripDropDownBackground => Color.White;
    }

    // ────────────────────────────────────────────────────────
    // POS HELPER UTILITIES
    // ────────────────────────────────────────────────────────
    internal static class POSThemeHelpers
    {
        /// <summary>Creates a rounded card Panel that paints its own border and clears corners.</summary>
        public static Panel CreateRoundedCard(int radius = 12, Color? bg = null, Color? border = null)
        {
            Color bgColor   = bg     ?? ThemeConfig.SurfaceColor;
            Color bdColor   = border ?? ThemeConfig.BorderColor;
            Panel p = new Panel { BackColor = bgColor, BorderStyle = BorderStyle.None, Padding = new Padding(0) };
            p.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Color parentBg = ThemeConfig.GetParentColor(p);
                using (var brush = new System.Drawing.SolidBrush(parentBg))
                    g.FillRectangle(brush, -1, -1, p.Width + 2, p.Height + 2);
                ThemeConfig.FillRoundedBackground(g, p.ClientRectangle, radius, bgColor);
                ThemeConfig.DrawRoundedBorder(g, p.ClientRectangle, radius, bdColor, 1f);
            };
            return p;
        }

        public static System.Drawing.Drawing2D.GraphicsPath RoundedPath(Rectangle r, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            int d = radius * 2;
            path.AddArc(r.X,         r.Y,          d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y,          d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d,   0, 90);
            path.AddArc(r.X,         r.Bottom - d, d, d,  90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
















