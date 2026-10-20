using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace InventorySystem.Helpers
{
    public static class LocalizationManager
    {
        public static event EventHandler LanguageChanged;

        private static readonly ResourceManager _enManager = new ResourceManager("InventorySystem.Properties.Resources", typeof(LocalizationManager).Assembly);
        private static readonly ResourceManager _arManager = new ResourceManager("InventorySystem.Properties.Resources_ar", typeof(LocalizationManager).Assembly);

        // Track RTL state without stomping on Control.Tag

        public static bool IsArabicBuild
        {
            get
            {
#if ARABIC_VERSION
                return true;
#else
                return false;
#endif
            }
        }

        private static string _currentLanguage = "en";

        public static void SetLanguage(string cultureCode)
        {
            var culture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture; // Guarantee ThreadPool/API threads inherit this!
            _currentLanguage = culture.TwoLetterISOLanguageName;

            // Keep date and number formatting consistent (Invariant) to prevent database/parsing errors
            // but allow UICulture to handle translations.
            var customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            customCulture.NumberFormat.CurrencySymbol = "$";
            Thread.CurrentThread.CurrentCulture = customCulture;
            CultureInfo.DefaultThreadCurrentCulture = customCulture;

            // Persist the selected language choice
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "language.txt");
                File.WriteAllText(configPath, cultureCode);
            }
            catch { }

            ResetTranslationCaches();
            // Invoke handlers individually so one failing form cannot block the rest (sidebar/MainForm).
            var handlers = LanguageChanged;
            if (handlers != null)
            {
                foreach (Delegate d in handlers.GetInvocationList())
                {
                    try { ((EventHandler)d)(null, EventArgs.Empty); }
                    catch { }
                }
            }
        }

        public static string GetString(string key)
        {
            if (string.IsNullOrEmpty(key)) return "";
            try
            {
                if (IsArabic)
                {
                    string res = _arManager.GetString(key);
                    if (!string.IsNullOrEmpty(res)) return res;
                }

                string enRes = _enManager.GetString(key);
                if (!string.IsNullOrEmpty(enRes)) return enRes;
            }
            catch { }

            return key;
        }

        public static string GetString(string key, string fallback)
        {
            if (string.IsNullOrEmpty(key)) return fallback;
            try
            {
                if (IsArabic)
                {
                    string res = _arManager.GetString(key);
                    if (!string.IsNullOrEmpty(res)) return res;
                }

                string enRes = _enManager.GetString(key);
                if (!string.IsNullOrEmpty(enRes)) return enRes;
            }
            catch { }

            return fallback;
        }

        public static string GetStringLang(string key, bool arabic)
        {
            if (string.IsNullOrEmpty(key)) return "";
            string res = LookupResource(key, arabic);
            return string.IsNullOrEmpty(res) ? key : res;
        }

        private class ValueBox<T> { public T Value { get; set; } public ValueBox(T val) { Value = val; } }

        private static ConditionalWeakTable<Control, string> _originalTexts = new ConditionalWeakTable<Control, string>();
        private static ConditionalWeakTable<Control, string> _originalLabelTexts = new ConditionalWeakTable<Control, string>();
        private static ConditionalWeakTable<Control, ValueBox<Point>> _originalLocations = new ConditionalWeakTable<Control, ValueBox<Point>>();
        private static ConditionalWeakTable<FlowLayoutPanel, ValueBox<FlowDirection>> _originalFlowDirections = new ConditionalWeakTable<FlowLayoutPanel, ValueBox<FlowDirection>>();
        private static ConditionalWeakTable<DataGridViewColumn, string> _originalColumnHeaders = new ConditionalWeakTable<DataGridViewColumn, string>();
        private static ConditionalWeakTable<Control, ValueBox<AnchorStyles>> _originalAnchors = new ConditionalWeakTable<Control, ValueBox<AnchorStyles>>();

        private static void ResetTranslationCaches()
        {
            _originalTexts = new ConditionalWeakTable<Control, string>();
            _originalLabelTexts = new ConditionalWeakTable<Control, string>();
            _originalColumnHeaders = new ConditionalWeakTable<DataGridViewColumn, string>();
            // Keep RTL layout weak tables — clearing them desyncs dock/location mirrors from actual control state.
        }

        private static string LookupResource(string key, bool isAr)
        {
            if (string.IsNullOrEmpty(key)) return null;
            try
            {
                if (isAr)
                {
                    string ar = _arManager.GetString(key);
                    if (!string.IsNullOrEmpty(ar)) return ar;
                }
                return _enManager.GetString(key);
            }
            catch { return null; }
        }

        public static void TranslateControl(Control parent)
        {
            if (parent == null) return;
            bool isAr = IsArabic;

            foreach (Control c in parent.Controls)
            {
                if (c.HasChildren) TranslateControl(c);

                string key = !string.IsNullOrEmpty(c.Name) ? c.Name : null;

                // 1. Handle standard controls (Button, Label, CheckBox, RadioButton, GroupBox)
                if (c is Button || c is Label || c is CheckBox || c is RadioButton || c is GroupBox)
                {
                    bool isStandardButton = c is Button && c.Tag != null && c.Tag.ToString().StartsWith("standard_");
                    if (!isStandardButton)
                    {
                        if (!string.IsNullOrEmpty(key))
                        {
                            string translated = LookupResource(key, isAr);
                            if (!string.IsNullOrEmpty(translated))
                                c.Text = translated;
                        }
                        // No resource key: leave text as-is (ApplyLocalization updates those controls).
                    }
                }

                // 2. Handle Custom Modern User Controls with LabelText property (ModernComboBox, ModernTextBox, ModernNumericUpDown)
                var propLabelText = c.GetType().GetProperty("LabelText");
                if (propLabelText != null && propLabelText.CanWrite && propLabelText.CanRead)
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        string translated = LookupResource(key, isAr);
                        if (!string.IsNullOrEmpty(translated))
                            propLabelText.SetValue(c, translated);
                    }
                }

                // 3. Handle TabControl TabPages
                if (c is TabControl tabCtrl)
                {
                    foreach (TabPage page in tabCtrl.TabPages)
                    {
                        if (!string.IsNullOrEmpty(page.Name))
                        {
                            string pageTrans = LookupResource(page.Name, isAr);
                            if (!string.IsNullOrEmpty(pageTrans))
                                page.Text = pageTrans;
                        }
                        TranslateControl(page);
                    }
                }

                // 4. Handle DataGridView Columns
                if (c is DataGridView dgv)
                {
                    foreach (DataGridViewColumn col in dgv.Columns)
                    {
                        if (!string.IsNullOrEmpty(col.Name))
                        {
                            string colTrans = LookupResource(col.Name, isAr);
                            if (!string.IsNullOrEmpty(colTrans))
                                col.HeaderText = colTrans;
                        }
                    }
                }
            }
        }

        public static string CurrentLanguage => _currentLanguage;
        public static bool IsArabic => CurrentLanguage == "ar";

        // Track RTL state without stomping on Control.Tag
        private static ConditionalWeakTable<Control, HashSet<string>> _rtlStates = new ConditionalWeakTable<Control, HashSet<string>>();

        private static bool HasRtlState(Control control, string state)
        {
            if (_rtlStates.TryGetValue(control, out var states)) return states.Contains(state);
            return false;
        }

        private static void MirrorTextAlign(Control control)
        {
            if (control is Label lbl)
            {
                lbl.TextAlign = SwapAlignment(lbl.TextAlign);
            }
            else if (control is Button btn)
            {
                btn.TextAlign = SwapAlignment(btn.TextAlign);
            }
        }

        private static ContentAlignment SwapAlignment(ContentAlignment alignment)
        {
            switch (alignment)
            {
                case ContentAlignment.TopLeft: return ContentAlignment.TopRight;
                case ContentAlignment.TopRight: return ContentAlignment.TopLeft;
                case ContentAlignment.MiddleLeft: return ContentAlignment.MiddleRight;
                case ContentAlignment.MiddleRight: return ContentAlignment.MiddleLeft;
                case ContentAlignment.BottomLeft: return ContentAlignment.BottomRight;
                case ContentAlignment.BottomRight: return ContentAlignment.BottomLeft;
                default: return alignment;
            }
        }

        private static void AddRtlState(Control control, string state)
        {
            if (!_rtlStates.TryGetValue(control, out var states))
            {
                states = new HashSet<string>();
                _rtlStates.Add(control, states);
            }
            states.Add(state);
        }

        private static void RemoveRtlState(Control control, string state)
        {
            if (_rtlStates.TryGetValue(control, out var states)) states.Remove(state);
        }

        /// <summary>
        /// Recursively applies RTL (or LTR) to a control tree.
        /// - Sets RightToLeft on every control (except MS Chart, which throws Win32Exception).
        /// - Mirrors DockStyle.Left <-> DockStyle.Right on Panels.
        /// - Reverses FlowDirection on FlowLayoutPanels.
        /// Per-control failures are swallowed so one bad control cannot abort a language switch.
        /// </summary>
        public static void ApplyRTL(Control control)
        {
            if (control == null || control.IsDisposed) return;
            bool isAr = IsArabic;
            bool isChart = control.GetType().FullName == "System.Windows.Forms.DataVisualization.Charting.Chart";

            try
            {
                // MS Chart crashes with "Error creating window handle" when RightToLeft is set.
                if (!isChart)
                    control.RightToLeft = isAr ? RightToLeft.Yes : RightToLeft.No;

                // Mirror FlowLayoutPanel direction for RTL
                if (control is FlowLayoutPanel flp)
                {
                    if (!_originalFlowDirections.TryGetValue(flp, out ValueBox<FlowDirection> box))
                    {
                        box = new ValueBox<FlowDirection>(flp.FlowDirection);
                        _originalFlowDirections.Add(flp, box);
                    }
                    FlowDirection origFlow = box.Value;
                    if (isAr)
                    {
                        if (origFlow == FlowDirection.LeftToRight) flp.FlowDirection = FlowDirection.RightToLeft;
                        else if (origFlow == FlowDirection.RightToLeft) flp.FlowDirection = FlowDirection.LeftToRight;
                    }
                    else
                    {
                        flp.FlowDirection = origFlow;
                    }
                }

                // Skip manual location/anchor mirroring for internal components of ModernTextBox and StatCard
                // as they handle their own internal layout logic.
                // Also skip manually laid-out header / filter toolbars — they own EN-right / AR-left placement.
                bool isManualToolbarChild = control.Parent != null &&
                    (control.Parent.Name == "pnlGlobalHeaderActions" || control.Parent.Name == "pnlFilterBar");
                bool isInternalHandled = isManualToolbarChild ||
                                         (control.Parent != null && (control.Parent.GetType().Name == "ModernTextBox" || control.Parent.GetType().Name == "StatCard" || control.Parent.GetType().Name == "ModernNumericUpDown" || control.Parent.GetType().Name == "ModernComboBox")) ||
                                         (control.Parent != null && control.Parent.Parent != null && (control.Parent.Parent.GetType().Name == "ModernTextBox" || control.Parent.Parent.GetType().Name == "StatCard" || control.Parent.Parent.GetType().Name == "ModernNumericUpDown" || control.Parent.Parent.GetType().Name == "ModernComboBox"));

                if (!isInternalHandled)
                {
                    // Handle Docking Mirroring for all controls (Labels, Buttons, Panels, etc.)
                    if (control.Dock == DockStyle.Left || control.Dock == DockStyle.Right)
                    {
                        // MainForm owns these docks explicitly on language change.
                        if (control.Name == "rightPanel" || control.Name == "panel2")
                        {
                            // skip dock mirroring
                        }
                        else
                        {
                            bool isSwapped = HasRtlState(control, "rtl_dock_swapped");
                            if (isAr && !isSwapped)
                            {
                                control.Dock = (control.Dock == DockStyle.Left) ? DockStyle.Right : DockStyle.Left;
                                AddRtlState(control, "rtl_dock_swapped");
                            }
                            else if (!isAr && isSwapped)
                            {
                                control.Dock = (control.Dock == DockStyle.Left) ? DockStyle.Right : DockStyle.Left;
                                RemoveRtlState(control, "rtl_dock_swapped");
                            }
                        }
                    }
                }

                // Handle Chart title/legend alignment without touching RightToLeft.
                if (isChart)
                {
                    dynamic chart = control;
                    foreach (var title in chart.Titles)
                        title.Alignment = isAr ? ContentAlignment.TopRight : ContentAlignment.TopLeft;

                    foreach (var legend in chart.Legends)
                    {
                        if (legend.Docking == System.Windows.Forms.DataVisualization.Charting.Docking.Top ||
                            legend.Docking == System.Windows.Forms.DataVisualization.Charting.Docking.Bottom)
                        {
                            legend.Alignment = isAr ? StringAlignment.Far : StringAlignment.Near;
                        }
                    }
                }

                if (!isInternalHandled)
                {
                    // Handle Absolute Location Mirroring for child controls (if parent is not a layout panel)
                    if (control.Parent != null && !(control.Parent is TableLayoutPanel || control.Parent is FlowLayoutPanel))
                    {
                        if (!_originalLocations.TryGetValue(control, out ValueBox<Point> locBox))
                        {
                            locBox = new ValueBox<Point>(control.Location);
                            _originalLocations.Add(control, locBox);
                        }
                        Point origLoc = locBox.Value;

                        if (isAr)
                        {
                            int pWidth = control.Parent.ClientSize.Width;
                            if (pWidth > 0 && !HasRtlState(control, "rtl_loc_swapped"))
                            {
                                control.Location = new Point(pWidth - origLoc.X - control.Width, origLoc.Y);
                                AddRtlState(control, "rtl_loc_swapped");
                            }
                        }
                        else
                        {
                            if (HasRtlState(control, "rtl_loc_swapped"))
                            {
                                control.Location = origLoc;
                                RemoveRtlState(control, "rtl_loc_swapped");
                            }
                        }
                    }

                    // Swap Anchors (Only for controls that are not Docked, as setting Anchor resets Dock to None)
                    bool isAnchorSwapped = HasRtlState(control, "rtl_anchor_swapped");
                    bool isLayoutChild = control.Parent != null && (control.Parent is TableLayoutPanel || control.Parent is FlowLayoutPanel);

                    if (isAr && !isAnchorSwapped && control.Dock == DockStyle.None && !isLayoutChild)
                    {
                        if (!_originalAnchors.TryGetValue(control, out ValueBox<AnchorStyles> anchorBox))
                        {
                            anchorBox = new ValueBox<AnchorStyles>(control.Anchor);
                            _originalAnchors.Add(control, anchorBox);
                        }
                        AnchorStyles a = anchorBox.Value;
                        if ((a & AnchorStyles.Left) == AnchorStyles.Left && (a & AnchorStyles.Right) != AnchorStyles.Right)
                        {
                            control.Anchor = (a & ~AnchorStyles.Left) | AnchorStyles.Right;
                            AddRtlState(control, "rtl_anchor_swapped");
                        }
                        else if ((a & AnchorStyles.Right) == AnchorStyles.Right && (a & AnchorStyles.Left) != AnchorStyles.Left)
                        {
                            control.Anchor = (a & ~AnchorStyles.Right) | AnchorStyles.Left;
                            AddRtlState(control, "rtl_anchor_swapped");
                        }
                    }
                    else if (!isAr && isAnchorSwapped && control.Dock == DockStyle.None && !isLayoutChild)
                    {
                        if (_originalAnchors.TryGetValue(control, out ValueBox<AnchorStyles> anchorBox))
                            control.Anchor = anchorBox.Value;
                        RemoveRtlState(control, "rtl_anchor_swapped");
                    }
                }
            }
            catch
            {
                // One control must never abort the whole language/RTL pass.
            }

            // Snapshot children — Controls collection can change while we recurse.
            Control[] children;
            try { children = control.Controls.Cast<Control>().ToArray(); }
            catch { return; }

            foreach (Control child in children)
                ApplyRTL(child);
        }

        private static void MirrorTableLayout(TableLayoutPanel tlp)
        {
            int maxCol = tlp.ColumnCount - 1;
            var controls = new List<Control>();
            var positions = new List<TableLayoutPanelCellPosition>();

            foreach (Control c in tlp.Controls)
            {
                controls.Add(c);
                positions.Add(tlp.GetPositionFromControl(c));
            }

            for (int i = 0; i < controls.Count; i++)
            {
                tlp.SetColumn(controls[i], maxCol - positions[i].Column);
            }
        }

        /// <summary>
        /// Reverses the FlowDirection of a FlowLayoutPanel for RTL.
        /// </summary>
        public static void ApplyRTLToFlowLayout(FlowLayoutPanel flow)
        {
            if (flow == null) return;
            flow.FlowDirection = IsArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        /// <summary>
        /// Mirrors an X position inside a container for fixed-position controls.
        /// </summary>
        public static int MirrorX(Control control, Control parent)
        {
            return parent.ClientSize.Width - control.Location.X - control.Width;
        }
    }
}

