using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

using LiveSplit.Model;
using LiveSplit.TimeFormatters;

namespace LiveSplit.UI.Components;

public enum AlternativeTimerBackgroundCorners
{
    All,
    Top,
    Bottom
}

public enum AlternativeTimerSegmentNumberPosition
{
    Above,
    Below
}

public partial class AlternativeTimerSettings : UserControl
{
    private static string T(string source) => source;

    public new float Height { get; set; }
    public new float Width { get; set; }
    public float SegmentTimerSizeRatio { get; set; }
    public LiveSplitState CurrentState { get; set; }

    public bool TimerShowGradient { get; set; }
    public bool OverrideTimerColors { get; set; }
    public bool SegmentTimerShowGradient { get; set; }
    public bool ShowSplitName { get; set; }

    public float IconSize { get; set; }
    public bool DisplayIcon { get; set; }

    public float DecimalsSize { get; set; }
    public float SegmentTimerDecimalsSize { get; set; }

    public Color TimerColor { get; set; }
    public Color SegmentTimerColor { get; set; }
    public Color SegmentLabelsColor { get; set; }
    public Color SegmentTimesColor { get; set; }
    public Color SplitNameColor { get; set; }

    public bool BackgroundEnabled { get; set; }
    public Color BackgroundColor { get; set; }
    public Color BackgroundColor2 { get; set; }
    public Color BackgroundColor3 { get; set; }
    public int BackgroundColorCount { get; set; }
    public int BackgroundCornerRadius { get; set; }
    public AlternativeTimerBackgroundCorners BackgroundCorners { get; set; }

    public DeltasGradientType BackgroundGradient { get; set; }
    public string GradientString
    {
        get => TimerSettings.GetBackgroundTypeString(BackgroundGradient);
        set
        {
            string clean = (value ?? DeltasGradientType.Plain.ToString()).Replace(" ", "");
            if (!Enum.TryParse(clean, out DeltasGradientType gradient) ||
                (gradient != DeltasGradientType.Plain &&
                 gradient != DeltasGradientType.Vertical &&
                 gradient != DeltasGradientType.Horizontal))
            {
                gradient = DeltasGradientType.Plain;
            }

            BackgroundGradient = gradient;
        }
    }
    private string timerFormat
    {
        get => DigitsFormat + Accuracy;
        set
        {
            int decimalIndex = value.IndexOf('.');
            if (decimalIndex < 0)
            {
                DigitsFormat = value;
                Accuracy = "";
            }
            else
            {
                DigitsFormat = value.Substring(0, decimalIndex);
                Accuracy = value.Substring(decimalIndex);
            }
        }
    }
    public string DigitsFormat { get; set; }
    public string Accuracy { get; set; }
    private string segmentTimerFormat
    {
        get => SegmentDigitsFormat + SegmentAccuracy;
        set
        {
            int decimalIndex = value.IndexOf('.');
            if (decimalIndex < 0)
            {
                SegmentDigitsFormat = value;
                SegmentAccuracy = "";
            }
            else
            {
                SegmentDigitsFormat = value.Substring(0, decimalIndex);
                SegmentAccuracy = value.Substring(decimalIndex);
            }
        }
    }
    public string SegmentDigitsFormat { get; set; }
    public string SegmentAccuracy { get; set; }
    public TimeAccuracy SegmentTimesAccuracy { get; set; }
    public GeneralTimeFormatter SegmentTimesFormatter { get; set; } = new GeneralTimeFormatter()
    {
        NullFormat = NullFormat.Dash,
        Accuracy = TimeAccuracy.Hundredths
    };
    public string SegmentLabelsFontString => SettingsHelper.FormatFont(SegmentLabelsFont);
    public Font SegmentLabelsFont { get; set; }
    public string SegmentTimesFontString => SettingsHelper.FormatFont(SegmentTimesFont);
    public Font SegmentTimesFont { get; set; }
    public string SplitNameFontString => SettingsHelper.FormatFont(SplitNameFont);
    public Font SplitNameFont { get; set; }
    public bool ShowSegmentNumber { get; set; }
    public AlternativeTimerSegmentNumberPosition SegmentNumberPosition { get; set; }
    public Color SegmentNumberColor { get; set; }
    public bool SegmentNumberBold { get; set; }
    public float SegmentNumberLeftPadding { get; set; }
    public float SegmentNumberGap { get; set; }
    public float SegmentTextVerticalOffset { get; set; }
    public bool PrioritizeSegmentNumberWhenTight { get; set; }

    public string Comparison { get; set; }
    public string Comparison2 { get; set; }
    public bool HideComparison { get; set; }
    public string TimingMethod { get; set; }

    public LayoutMode Mode { get; set; }

    private CheckBox chkBackgroundEnabled;
    private Button btnColor3;
    private ComboBox cmbBackgroundColorCount;
    private NumericUpDown numBackgroundCornerRadius;
    private ComboBox cmbBackgroundCorners;
    private CheckBox chkShowSegmentNumber;
    private ComboBox cmbSegmentNumberPosition;
    private Button btnSegmentNumberColor;
    private CheckBox chkSegmentNumberBold;
    private NumericUpDown numSegmentNumberLeftPadding;
    private NumericUpDown numSegmentNumberGap;
    private NumericUpDown numSegmentTextVerticalOffset;
    private CheckBox chkPrioritizeSegmentNumberWhenTight;

    public AlternativeTimerSettings()
    {
        InitializeComponent();

        Height = 75;
        Width = 200;
        SegmentTimerSizeRatio = 40;

        TimerShowGradient = true;
        OverrideTimerColors = false;
        SegmentTimerShowGradient = true;
        ShowSplitName = true;

        TimerColor = Color.FromArgb(170, 170, 170);
        SegmentTimerColor = Color.FromArgb(170, 170, 170);
        SegmentLabelsColor = Color.FromArgb(255, 255, 255);
        SegmentTimesColor = Color.FromArgb(255, 255, 255);
        SplitNameColor = Color.FromArgb(0, 255, 255);

        DigitsFormat = "1";
        Accuracy = ".23";
        SegmentDigitsFormat = "1";
        SegmentAccuracy = ".23";
        SegmentTimesAccuracy = TimeAccuracy.Hundredths;

        SegmentLabelsFont = new Font("Segoe UI", 13, FontStyle.Regular, GraphicsUnit.Pixel);
        SegmentTimesFont = new Font("Segoe UI", 13, FontStyle.Bold, GraphicsUnit.Pixel);
        SplitNameFont = new Font("Segoe UI", 15, FontStyle.Bold, GraphicsUnit.Pixel);

        BackgroundEnabled = false;
        BackgroundColor = Color.Transparent;
        BackgroundColor2 = Color.Transparent;
        BackgroundColor3 = Color.Transparent;
        BackgroundColorCount = 2;
        BackgroundCornerRadius = 0;
        BackgroundCorners = AlternativeTimerBackgroundCorners.All;
        BackgroundGradient = DeltasGradientType.Plain;

        ShowSegmentNumber = true;
        SegmentNumberPosition = AlternativeTimerSegmentNumberPosition.Above;
        SegmentNumberColor = Color.FromArgb(0, 255, 255);
        SegmentNumberBold = true;
        SegmentNumberLeftPadding = 7f;
        SegmentNumberGap = 2f;
        SegmentTextVerticalOffset = 0f;
        PrioritizeSegmentNumberWhenTight = true;

        IconSize = 40f;
        DisplayIcon = false;

        DecimalsSize = 35f;
        SegmentTimerDecimalsSize = 35f;

        Comparison = "Current Comparison";
        Comparison2 = "Best Segments";
        HideComparison = false;
        TimingMethod = "Current Timing Method";

        chkShowGradientSegmentTimer.DataBindings.Add("Checked", this, "SegmentTimerShowGradient", false, DataSourceUpdateMode.OnPropertyChanged);
        chkShowGradientTimer.DataBindings.Add("Checked", this, "TimerShowGradient", false, DataSourceUpdateMode.OnPropertyChanged);
        chkOverrideTimerColors.DataBindings.Add("Checked", this, "OverrideTimerColors", false, DataSourceUpdateMode.OnPropertyChanged);
        chkSplitName.DataBindings.Add("Checked", this, "ShowSplitName", false, DataSourceUpdateMode.OnPropertyChanged);
        btnTimerColor.DataBindings.Add("BackColor", this, "TimerColor", false, DataSourceUpdateMode.OnPropertyChanged);
        btnSegmentTimerColor.DataBindings.Add("BackColor", this, "SegmentTimerColor", false, DataSourceUpdateMode.OnPropertyChanged);
        btnSegmentLabelsColor.DataBindings.Add("BackColor", this, "SegmentLabelsColor", false, DataSourceUpdateMode.OnPropertyChanged);
        btnSegmentTimesColor.DataBindings.Add("BackColor", this, "SegmentTimesColor", false, DataSourceUpdateMode.OnPropertyChanged);
        btnSplitNameColor.DataBindings.Add("BackColor", this, "SplitNameColor", false, DataSourceUpdateMode.OnPropertyChanged);
        trkSegmentTimerRatio.DataBindings.Add("Value", this, "SegmentTimerSizeRatio", false, DataSourceUpdateMode.OnPropertyChanged);
        lblSegmentLabelsFont.DataBindings.Add("Text", this, "SegmentLabelsFontString", false, DataSourceUpdateMode.OnPropertyChanged);
        lblSegmentTimesFont.DataBindings.Add("Text", this, "SegmentTimesFontString", false, DataSourceUpdateMode.OnPropertyChanged);
        lblSplitNameFont.DataBindings.Add("Text", this, "SplitNameFontString", false, DataSourceUpdateMode.OnPropertyChanged);
        chkDisplayIcon.DataBindings.Add("Checked", this, "DisplayIcon", false, DataSourceUpdateMode.OnPropertyChanged);
        trkIconSize.DataBindings.Add("Value", this, "IconSize", false, DataSourceUpdateMode.OnPropertyChanged);
        cmbGradientType.DataBindings.Add("SelectedItem", this, "GradientString", false, DataSourceUpdateMode.OnPropertyChanged);
        btnColor1.DataBindings.Add("Enabled", this, "BackgroundEnabled", false, DataSourceUpdateMode.OnPropertyChanged);
        btnColor1.DataBindings.Add("BackColor", this, "BackgroundColor", false, DataSourceUpdateMode.OnPropertyChanged);
        btnColor2.DataBindings.Add("BackColor", this, "BackgroundColor2", false, DataSourceUpdateMode.OnPropertyChanged);
        cmbComparison.DataBindings.Add("SelectedItem", this, "Comparison", false, DataSourceUpdateMode.OnPropertyChanged);
        cmbComparison2.DataBindings.Add("SelectedItem", this, "Comparison2", false, DataSourceUpdateMode.OnPropertyChanged);
        chkHideComparison.DataBindings.Add("Checked", this, "HideComparison", false, DataSourceUpdateMode.OnPropertyChanged);
        trkDecimalsSize.DataBindings.Add("Value", this, "DecimalsSize", false, DataSourceUpdateMode.OnPropertyChanged);
        trkSegmentDecimalsSize.DataBindings.Add("Value", this, "SegmentTimerDecimalsSize", false, DataSourceUpdateMode.OnPropertyChanged);
        cmbDigitsFormat.DataBindings.Add("SelectedItem", this, "DigitsFormat", false, DataSourceUpdateMode.OnPropertyChanged);
        cmbAccuracy.DataBindings.Add("SelectedItem", this, "Accuracy", false, DataSourceUpdateMode.OnPropertyChanged);
        cmbSegmentDigitsFormat.DataBindings.Add("SelectedItem", this, "SegmentDigitsFormat", false, DataSourceUpdateMode.OnPropertyChanged);
        cmbSegmentAccuracy.DataBindings.Add("SelectedItem", this, "SegmentAccuracy", false, DataSourceUpdateMode.OnPropertyChanged);
        cmbTimingMethod.DataBindings.Add("SelectedItem", this, "TimingMethod", false, DataSourceUpdateMode.OnPropertyChanged);

        CollapseRemovedSegmentTimerSettings();
        ConfigureAlternativeTimerSettings();
    }

    private void CollapseRemovedSegmentTimerSettings()
    {
        HideSettingsRow(2, groupBox11);
        HideSettingsRow(4, label5, trkSegmentTimerRatio);
        HideSettingsRow(6, groupBox3);
        HideSettingsRow(7, groupBox7);
        HideSettingsRow(8, groupBox8);
        Size = new Size(459, 552);
    }

    private void HideSettingsRow(int rowIndex, params Control[] controls)
    {
        foreach (Control control in controls)
        {
            control.Visible = false;
            tableLayoutPanel1.Controls.Remove(control);
        }

        tableLayoutPanel1.RowStyles[rowIndex].SizeType = SizeType.Absolute;
        tableLayoutPanel1.RowStyles[rowIndex].Height = 0;
    }

    private void ConfigureAlternativeTimerSettings()
    {
        AutoScroll = true;
        ConfigureColorButton(btnColor1);
        ConfigureColorButton(btnColor2);
        ConfigureColorButton(btnSplitNameColor);

        cmbGradientType.Items.Clear();
        cmbGradientType.Items.AddRange(new object[] { "Plain", "Vertical", "Horizontal" });
        cmbGradientType.SelectedItem = GradientString;
        if (cmbGradientType.SelectedItem == null)
        {
            cmbGradientType.SelectedItem = "Plain";
        }

        AddBackgroundOptionsRow();
        AddSegmentNumberSettingsRow();

        Size = new Size(459, 720);
        UpdateBackgroundControlStates();
        UpdateSegmentNumberControlStates();
    }

    private void AddBackgroundOptionsRow()
    {
        var groupBox = new GroupBox
        {
            Text = "Background Options",
            Dock = DockStyle.Fill,
        };

        var panel = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Dock = DockStyle.Fill,
            Padding = new Padding(4, 5, 4, 4),
        };

        chkBackgroundEnabled = new CheckBox
        {
            Text = "Enable",
            AutoSize = true,
            Checked = BackgroundEnabled,
            Margin = new Padding(0, 3, 8, 0),
        };
        chkBackgroundEnabled.CheckedChanged += (s, e) =>
        {
            BackgroundEnabled = chkBackgroundEnabled.Checked;
            UpdateBackgroundControlStates();
        };
        panel.Controls.Add(chkBackgroundEnabled);

        panel.Controls.Add(MakeInlineLabel("Colors:"));
        cmbBackgroundColorCount = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 76,
            Margin = new Padding(0, 0, 8, 0),
        };
        cmbBackgroundColorCount.Items.AddRange(new object[] { "2 colors", "3 colors" });
        cmbBackgroundColorCount.SelectedIndex = BackgroundColorCount == 3 ? 1 : 0;
        cmbBackgroundColorCount.SelectedIndexChanged += (s, e) =>
        {
            BackgroundColorCount = cmbBackgroundColorCount.SelectedIndex == 1 ? 3 : 2;
            UpdateBackgroundControlStates();
        };
        panel.Controls.Add(cmbBackgroundColorCount);

        panel.Controls.Add(MakeInlineLabel("3:"));
        btnColor3 = MakeColorButton();
        btnColor3.DataBindings.Add("BackColor", this, "BackgroundColor3", false, DataSourceUpdateMode.OnPropertyChanged);
        panel.Controls.Add(btnColor3);

        panel.Controls.Add(MakeInlineLabel("Radius:"));
        numBackgroundCornerRadius = MakeNumber(0, 200, BackgroundCornerRadius);
        numBackgroundCornerRadius.Width = 54;
        numBackgroundCornerRadius.ValueChanged += (s, e) =>
            BackgroundCornerRadius = (int)numBackgroundCornerRadius.Value;
        panel.Controls.Add(numBackgroundCornerRadius);

        cmbBackgroundCorners = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 108,
        };
        cmbBackgroundCorners.Items.AddRange(new object[] { "All corners", "Top corners", "Bottom corners" });
        cmbBackgroundCorners.SelectedIndex = (int)BackgroundCorners;
        cmbBackgroundCorners.SelectedIndexChanged += (s, e) =>
            BackgroundCorners = (AlternativeTimerBackgroundCorners)cmbBackgroundCorners.SelectedIndex;
        panel.Controls.Add(cmbBackgroundCorners);

        groupBox.Controls.Add(panel);
        tableLayoutPanel1.RowStyles[2].SizeType = SizeType.Absolute;
        tableLayoutPanel1.RowStyles[2].Height = 64;
        tableLayoutPanel1.Controls.Add(groupBox, 0, 2);
        tableLayoutPanel1.SetColumnSpan(groupBox, 4);
    }

    private void AddSegmentNumberSettingsRow()
    {
        var groupBox = new GroupBox
        {
            Text = "Segment Number",
            Dock = DockStyle.Fill,
        };

        var panel = new TableLayoutPanel
        {
            ColumnCount = 4,
            RowCount = 5,
            Dock = DockStyle.Fill,
            Padding = new Padding(4, 2, 4, 4),
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145F));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
        for (int i = 0; i < 5; i++)
        {
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        }

        chkShowSegmentNumber = new CheckBox
        {
            Text = "Show 1 of 5",
            AutoSize = true,
            Checked = ShowSegmentNumber,
            Margin = new Padding(7, 4, 3, 3),
        };
        chkShowSegmentNumber.CheckedChanged += (s, e) =>
        {
            ShowSegmentNumber = chkShowSegmentNumber.Checked;
            UpdateSegmentNumberControlStates();
        };
        panel.Controls.Add(chkShowSegmentNumber, 0, 0);

        cmbSegmentNumberPosition = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 120,
        };
        cmbSegmentNumberPosition.Items.AddRange(new object[] { "Above Name", "Below Name" });
        cmbSegmentNumberPosition.SelectedIndex = (int)SegmentNumberPosition;
        cmbSegmentNumberPosition.SelectedIndexChanged += (s, e) =>
            SegmentNumberPosition = (AlternativeTimerSegmentNumberPosition)cmbSegmentNumberPosition.SelectedIndex;
        panel.Controls.Add(cmbSegmentNumberPosition, 1, 0);

        chkSegmentNumberBold = new CheckBox
        {
            Text = "Bold",
            AutoSize = true,
            Checked = SegmentNumberBold,
            Margin = new Padding(0, 4, 3, 3),
        };
        chkSegmentNumberBold.CheckedChanged += (s, e) =>
            SegmentNumberBold = chkSegmentNumberBold.Checked;
        panel.Controls.Add(chkSegmentNumberBold, 2, 0);

        panel.Controls.Add(MakeSettingsLabel("Color:"), 0, 1);
        btnSegmentNumberColor = MakeColorButton();
        btnSegmentNumberColor.DataBindings.Add("BackColor", this, "SegmentNumberColor", false, DataSourceUpdateMode.OnPropertyChanged);
        panel.Controls.Add(btnSegmentNumberColor, 1, 1);

        panel.Controls.Add(MakeSettingsLabel("Left Padding:"), 0, 2);
        numSegmentNumberLeftPadding = MakeNumber(0, 120, SegmentNumberLeftPadding);
        numSegmentNumberLeftPadding.ValueChanged += (s, e) =>
            SegmentNumberLeftPadding = (float)numSegmentNumberLeftPadding.Value;
        panel.Controls.Add(numSegmentNumberLeftPadding, 1, 2);

        panel.Controls.Add(MakeSettingsLabel("Name Gap:"), 2, 2);
        numSegmentNumberGap = MakeNumber(-40, 60, SegmentNumberGap);
        numSegmentNumberGap.ValueChanged += (s, e) =>
            SegmentNumberGap = (float)numSegmentNumberGap.Value;
        panel.Controls.Add(numSegmentNumberGap, 3, 2);

        panel.Controls.Add(MakeSettingsLabel("Vertical Move:"), 0, 3);
        numSegmentTextVerticalOffset = MakeNumber(-80, 80, SegmentTextVerticalOffset);
        numSegmentTextVerticalOffset.ValueChanged += (s, e) =>
            SegmentTextVerticalOffset = (float)numSegmentTextVerticalOffset.Value;
        panel.Controls.Add(numSegmentTextVerticalOffset, 1, 3);

        chkPrioritizeSegmentNumberWhenTight = new CheckBox
        {
            Text = "Keep number if tight",
            AutoSize = true,
            Checked = PrioritizeSegmentNumberWhenTight,
            Margin = new Padding(0, 4, 3, 3),
        };
        chkPrioritizeSegmentNumberWhenTight.CheckedChanged += (s, e) =>
            PrioritizeSegmentNumberWhenTight = chkPrioritizeSegmentNumberWhenTight.Checked;
        panel.SetColumnSpan(chkPrioritizeSegmentNumberWhenTight, 2);
        panel.Controls.Add(chkPrioritizeSegmentNumberWhenTight, 2, 3);

        groupBox.Controls.Add(panel);

        int row = tableLayoutPanel1.RowCount;
        tableLayoutPanel1.RowCount += 1;
        tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 158F));
        tableLayoutPanel1.Controls.Add(groupBox, 0, row);
        tableLayoutPanel1.SetColumnSpan(groupBox, 4);
    }

    private static Label MakeSettingsLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
        };
    }

    private static Label MakeInlineLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Margin = new Padding(0, 5, 3, 0),
        };
    }

    private Button MakeColorButton()
    {
        var button = new Button
        {
            FlatStyle = FlatStyle.Popup,
            UseVisualStyleBackColor = false,
        };
        ConfigureColorButton(button);
        button.Click += ColorButtonClick;
        return button;
    }

    private static void ConfigureColorButton(Button button)
    {
        button.Width = 23;
        button.Height = 23;
        button.MinimumSize = new Size(23, 23);
        button.MaximumSize = new Size(23, 23);
        button.Anchor = AnchorStyles.Left;
        button.Margin = new Padding(3);
    }

    private static NumericUpDown MakeNumber(decimal minimum, decimal maximum, float value)
    {
        var number = new NumericUpDown
        {
            Minimum = minimum,
            Maximum = maximum,
            DecimalPlaces = 0,
            Increment = 1,
            Width = 56,
            Anchor = AnchorStyles.Left,
        };
        number.Value = Math.Min(maximum, Math.Max(minimum, (decimal)value));
        return number;
    }

    private void UpdateBackgroundControlStates()
    {
        bool enabled = BackgroundEnabled;
        bool gradientMode = BackgroundGradient != DeltasGradientType.Plain;
        bool useThreeColors = BackgroundColorCount == 3;

        if (chkBackgroundEnabled != null)
        {
            chkBackgroundEnabled.Checked = BackgroundEnabled;
        }

        cmbGradientType.Enabled = enabled;
        btnColor1.Enabled = enabled;
        btnColor2.Enabled = enabled && gradientMode;
        if (btnColor3 != null)
        {
            btnColor3.Enabled = enabled && gradientMode && useThreeColors;
        }

        if (cmbBackgroundColorCount != null)
        {
            cmbBackgroundColorCount.Enabled = enabled && gradientMode;
            cmbBackgroundColorCount.SelectedIndex = useThreeColors ? 1 : 0;
        }

        if (numBackgroundCornerRadius != null)
        {
            numBackgroundCornerRadius.Enabled = enabled;
            numBackgroundCornerRadius.Value = Math.Min(numBackgroundCornerRadius.Maximum, Math.Max(numBackgroundCornerRadius.Minimum, BackgroundCornerRadius));
        }

        if (cmbBackgroundCorners != null)
        {
            cmbBackgroundCorners.Enabled = enabled;
            cmbBackgroundCorners.SelectedIndex = (int)BackgroundCorners;
        }
    }

    private void UpdateSegmentNumberControlStates()
    {
        bool enabled = ShowSegmentNumber;

        if (chkShowSegmentNumber != null)
        {
            chkShowSegmentNumber.Checked = ShowSegmentNumber;
        }

        if (cmbSegmentNumberPosition != null)
        {
            cmbSegmentNumberPosition.Enabled = enabled;
            cmbSegmentNumberPosition.SelectedIndex = (int)SegmentNumberPosition;
        }

        if (btnSegmentNumberColor != null)
        {
            btnSegmentNumberColor.Enabled = enabled;
        }

        if (chkSegmentNumberBold != null)
        {
            chkSegmentNumberBold.Enabled = enabled;
            chkSegmentNumberBold.Checked = SegmentNumberBold;
        }

        if (numSegmentNumberLeftPadding != null)
        {
            numSegmentNumberLeftPadding.Enabled = enabled;
            numSegmentNumberLeftPadding.Value = Math.Min(numSegmentNumberLeftPadding.Maximum, Math.Max(numSegmentNumberLeftPadding.Minimum, (decimal)SegmentNumberLeftPadding));
        }

        if (numSegmentNumberGap != null)
        {
            numSegmentNumberGap.Enabled = enabled;
            numSegmentNumberGap.Value = Math.Min(numSegmentNumberGap.Maximum, Math.Max(numSegmentNumberGap.Minimum, (decimal)SegmentNumberGap));
        }

        if (numSegmentTextVerticalOffset != null)
        {
            numSegmentTextVerticalOffset.Enabled = enabled;
            numSegmentTextVerticalOffset.Value = Math.Min(numSegmentTextVerticalOffset.Maximum, Math.Max(numSegmentTextVerticalOffset.Minimum, (decimal)SegmentTextVerticalOffset));
        }

        if (chkPrioritizeSegmentNumberWhenTight != null)
        {
            chkPrioritizeSegmentNumberWhenTight.Enabled = enabled;
            chkPrioritizeSegmentNumberWhenTight.Checked = PrioritizeSegmentNumberWhenTight;
        }
    }

    private void cmbTimingMethod_SelectedIndexChanged(object sender, EventArgs e)
    {
        TimingMethod = cmbTimingMethod.SelectedItem.ToString();
    }

    private void cmbSegmentDigitsFormat_SelectedIndexChanged(object sender, EventArgs e)
    {
        SegmentDigitsFormat = cmbSegmentDigitsFormat.SelectedItem.ToString();
    }

    private void cmbSegmentAccuracy_SelectedIndexChanged(object sender, EventArgs e)
    {
        SegmentAccuracy = cmbSegmentAccuracy.SelectedItem.ToString();
    }

    private void cmbDigitsFormat_SelectedIndexChanged(object sender, EventArgs e)
    {
        DigitsFormat = cmbDigitsFormat.SelectedItem.ToString();
    }

    private void cmbAccuracy_SelectedIndexChanged(object sender, EventArgs e)
    {
        Accuracy = cmbAccuracy.SelectedItem.ToString();
    }

    private void chkSplitName_CheckedChanged(object sender, EventArgs e)
    {
        label9.Enabled = label10.Enabled = lblSplitNameFont.Enabled = btnSplitNameColor.Enabled
            = btnSplitNameFont.Enabled = chkSplitName.Checked;

    }

    private void chkDisplayIcon_CheckedChanged(object sender, EventArgs e)
    {
        label7.Enabled = trkIconSize.Enabled = chkDisplayIcon.Checked;
    }

    private void chkOverrideTimerColors_CheckedChanged(object sender, EventArgs e)
    {
        label1.Enabled = btnTimerColor.Enabled = chkOverrideTimerColors.Checked;
    }

    private void chkHideComparison_CheckedChanged(object sender, EventArgs e)
    {
        cmbComparison2.Enabled = label13.Enabled = !chkHideComparison.Checked;
    }

    private void cmbComparison2_SelectedIndexChanged(object sender, EventArgs e)
    {
        Comparison2 = cmbComparison2.SelectedItem.ToString();
    }

    private void cmbComparison_SelectedIndexChanged(object sender, EventArgs e)
    {
        Comparison = cmbComparison.SelectedItem.ToString();
    }

    private void cmbGradientType_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cmbGradientType.SelectedItem != null)
        {
            GradientString = cmbGradientType.SelectedItem.ToString();
        }

        btnColor1.Visible = true;
        btnColor2.Visible = true;
        UpdateBackgroundControlStates();
    }

    private void rdoSegmentTimesHundredths_CheckedChanged(object sender, EventArgs e)
    {
        UpdateAccuracySegmentTimes();
    }

    private void rdoSegmentTimesTenths_CheckedChanged(object sender, EventArgs e)
    {
        UpdateAccuracySegmentTimes();
    }

    private void rdoSegmentTimesSeconds_CheckedChanged(object sender, EventArgs e)
    {
        UpdateAccuracySegmentTimes();
    }

    private void UpdateAccuracySegmentTimes()
    {
        if (rdoSegmentTimesSeconds.Checked)
        {
            SegmentTimesAccuracy = TimeAccuracy.Seconds;
        }
        else if (rdoSegmentTimesTenths.Checked)
        {
            SegmentTimesAccuracy = TimeAccuracy.Tenths;
        }
        else if (rdoSegmentTimesHundredths.Checked)
        {
            SegmentTimesAccuracy = TimeAccuracy.Hundredths;
        }
        else
        {
            SegmentTimesAccuracy = TimeAccuracy.Milliseconds;
        }

        SegmentTimesFormatter.Accuracy = SegmentTimesAccuracy;
    }

    public void SetSettings(XmlNode node)
    {
        var element = (XmlElement)node;
        Version version = SettingsHelper.ParseVersion(element["Version"]);

        Height = SettingsHelper.ParseFloat(element["Height"]);
        Width = SettingsHelper.ParseFloat(element["Width"]);
        SegmentTimerSizeRatio = SettingsHelper.ParseFloat(element["SegmentTimerSizeRatio"]);
        TimerShowGradient = SettingsHelper.ParseBool(element["TimerShowGradient"]);
        SegmentTimerShowGradient = SettingsHelper.ParseBool(element["SegmentTimerShowGradient"]);
        TimerColor = SettingsHelper.ParseColor(element["TimerColor"]);
        SegmentTimerColor = SettingsHelper.ParseColor(element["SegmentTimerColor"]);
        SegmentLabelsColor = SettingsHelper.ParseColor(element["SegmentLabelsColor"]);
        SegmentTimesColor = SettingsHelper.ParseColor(element["SegmentTimesColor"]);
        TimingMethod = SettingsHelper.ParseString(element["TimingMethod"], "Current Timing Method");
        DecimalsSize = SettingsHelper.ParseFloat(element["DecimalsSize"], 35f);
        SegmentTimerDecimalsSize = SettingsHelper.ParseFloat(element["SegmentTimerDecimalsSize"], 35f);
        DisplayIcon = SettingsHelper.ParseBool(element["DisplayIcon"], false);
        IconSize = SettingsHelper.ParseFloat(element["IconSize"], 40f);
        ShowSplitName = SettingsHelper.ParseBool(element["ShowSplitName"], true);
        SplitNameColor = SettingsHelper.ParseColor(element["SplitNameColor"], Color.FromArgb(0, 255, 255));
        BackgroundEnabled = SettingsHelper.ParseBool(element["BackgroundEnabled"], false);
        BackgroundColor = SettingsHelper.ParseColor(element["BackgroundColor"], Color.Transparent);
        BackgroundColor2 = SettingsHelper.ParseColor(element["BackgroundColor2"], Color.Transparent);
        BackgroundColor3 = SettingsHelper.ParseColor(element["BackgroundColor3"], Color.Transparent);
        BackgroundColorCount = SettingsHelper.ParseInt(element["BackgroundColorCount"], 2) == 3 ? 3 : 2;
        BackgroundCornerRadius = Math.Max(0, SettingsHelper.ParseInt(element["BackgroundCornerRadius"], 0));
        BackgroundCorners = ParseEnum(element["BackgroundCorners"], AlternativeTimerBackgroundCorners.All);
        GradientString = SettingsHelper.ParseString(element["BackgroundGradient"], DeltasGradientType.Plain.ToString());
        if (element["BackgroundEnabled"] == null)
        {
            BackgroundEnabled = HasVisibleBackground();
        }

        ShowSegmentNumber = SettingsHelper.ParseBool(element["ShowSegmentNumber"], true);
        SegmentNumberPosition = ParseEnum(element["SegmentNumberPosition"], AlternativeTimerSegmentNumberPosition.Above);
        SegmentNumberColor = SettingsHelper.ParseColor(element["SegmentNumberColor"], SplitNameColor);
        SegmentNumberBold = SettingsHelper.ParseBool(element["SegmentNumberBold"], true);
        SegmentNumberLeftPadding = SettingsHelper.ParseFloat(element["SegmentNumberLeftPadding"], 7f);
        SegmentNumberGap = SettingsHelper.ParseFloat(element["SegmentNumberGap"], 2f);
        SegmentTextVerticalOffset = SettingsHelper.ParseFloat(element["SegmentTextVerticalOffset"], 0f);
        PrioritizeSegmentNumberWhenTight = SettingsHelper.ParseBool(element["PrioritizeSegmentNumberWhenTight"], true);
        Comparison = SettingsHelper.ParseString(element["Comparison"], "Current Comparison");
        Comparison2 = SettingsHelper.ParseString(element["Comparison2"], "Best Segments");
        HideComparison = SettingsHelper.ParseBool(element["HideComparison"], false);

        SegmentTimesAccuracy = SettingsHelper.ParseEnum<TimeAccuracy>(element["SegmentTimesAccuracy"]);
        SegmentTimesFormatter.Accuracy = SegmentTimesAccuracy;

        if (version >= new Version(1, 3))
        {
            OverrideTimerColors = SettingsHelper.ParseBool(element["OverrideTimerColors"]);
            SegmentLabelsFont = SettingsHelper.GetFontFromElement(element["SegmentLabelsFont"]);
            SegmentTimesFont = SettingsHelper.GetFontFromElement(element["SegmentTimesFont"]);
            SplitNameFont = SettingsHelper.GetFontFromElement(element["SplitNameFont"]);
        }
        else
        {
            OverrideTimerColors = !SettingsHelper.ParseBool(element["TimerUseSplitColors"]);
            SegmentLabelsFont = new Font("Segoe UI", 13, FontStyle.Regular, GraphicsUnit.Pixel);
            SegmentTimesFont = new Font("Segoe UI", 13, FontStyle.Bold, GraphicsUnit.Pixel);
            SplitNameFont = new Font("Segoe UI", 15, FontStyle.Bold, GraphicsUnit.Pixel);
        }

        if (version >= new Version(1, 5))
        {
            timerFormat = element["TimerFormat"].InnerText;
            segmentTimerFormat = element["SegmentTimerFormat"].InnerText;
        }
        else
        {
            DigitsFormat = "1";
            SegmentDigitsFormat = "1";
            TimeAccuracy timerAccuracy = SettingsHelper.ParseEnum<TimeAccuracy>(element["TimerAccuracy"]);
            Accuracy = timerAccuracy switch
            {
                TimeAccuracy.Seconds => "",
                TimeAccuracy.Tenths => ".2",
                TimeAccuracy.Hundredths => ".23",
                TimeAccuracy.Milliseconds => ".234",
                _ => ".23",
            };
            TimeAccuracy segmentTimerAccuracy = SettingsHelper.ParseEnum<TimeAccuracy>(element["SegmentTimerAccuracy"]);
            SegmentAccuracy = segmentTimerAccuracy switch
            {
                TimeAccuracy.Seconds => "",
                TimeAccuracy.Tenths => ".2",
                TimeAccuracy.Hundredths => ".23",
                TimeAccuracy.Milliseconds => ".234",
                _ => ".23",
            };
        }

        UpdateBackgroundControlStates();
        UpdateSegmentNumberControlStates();
    }

    public XmlNode GetSettings(XmlDocument document)
    {
        XmlElement parent = document.CreateElement("Settings");
        CreateSettingsNode(document, parent);
        return parent;
    }

    public int GetSettingsHashCode()
    {
        return CreateSettingsNode(null, null);
    }

    private int CreateSettingsNode(XmlDocument document, XmlElement parent)
    {
        return SettingsHelper.CreateSetting(document, parent, "Version", "2.0") ^
        SettingsHelper.CreateSetting(document, parent, "Height", Height) ^
        SettingsHelper.CreateSetting(document, parent, "Width", Width) ^
        SettingsHelper.CreateSetting(document, parent, "SegmentTimerSizeRatio", SegmentTimerSizeRatio) ^
        SettingsHelper.CreateSetting(document, parent, "TimerShowGradient", TimerShowGradient) ^
        SettingsHelper.CreateSetting(document, parent, "OverrideTimerColors", OverrideTimerColors) ^
        SettingsHelper.CreateSetting(document, parent, "SegmentTimerShowGradient", SegmentTimerShowGradient) ^
        SettingsHelper.CreateSetting(document, parent, "TimerFormat", timerFormat) ^
        SettingsHelper.CreateSetting(document, parent, "SegmentTimerFormat", segmentTimerFormat) ^
        SettingsHelper.CreateSetting(document, parent, "SegmentTimesAccuracy", SegmentTimesAccuracy) ^
        SettingsHelper.CreateSetting(document, parent, "TimerColor", TimerColor) ^
        SettingsHelper.CreateSetting(document, parent, "SegmentTimerColor", SegmentTimerColor) ^
        SettingsHelper.CreateSetting(document, parent, "SegmentLabelsColor", SegmentLabelsColor) ^
        SettingsHelper.CreateSetting(document, parent, "SegmentTimesColor", SegmentTimesColor) ^
        SettingsHelper.CreateSetting(document, parent, "SegmentLabelsFont", SegmentLabelsFont) ^
        SettingsHelper.CreateSetting(document, parent, "SegmentTimesFont", SegmentTimesFont) ^
        SettingsHelper.CreateSetting(document, parent, "SplitNameFont", SplitNameFont) ^
        SettingsHelper.CreateSetting(document, parent, "DisplayIcon", DisplayIcon) ^
        SettingsHelper.CreateSetting(document, parent, "IconSize", IconSize) ^
        SettingsHelper.CreateSetting(document, parent, "ShowSplitName", ShowSplitName) ^
        SettingsHelper.CreateSetting(document, parent, "SplitNameColor", SplitNameColor) ^
        SettingsHelper.CreateSetting(document, parent, "ShowSegmentNumber", ShowSegmentNumber) ^
        SettingsHelper.CreateSetting(document, parent, "SegmentNumberPosition", SegmentNumberPosition) ^
        SettingsHelper.CreateSetting(document, parent, "SegmentNumberColor", SegmentNumberColor) ^
        SettingsHelper.CreateSetting(document, parent, "SegmentNumberBold", SegmentNumberBold) ^
        SettingsHelper.CreateSetting(document, parent, "SegmentNumberLeftPadding", SegmentNumberLeftPadding) ^
        SettingsHelper.CreateSetting(document, parent, "SegmentNumberGap", SegmentNumberGap) ^
        SettingsHelper.CreateSetting(document, parent, "SegmentTextVerticalOffset", SegmentTextVerticalOffset) ^
        SettingsHelper.CreateSetting(document, parent, "PrioritizeSegmentNumberWhenTight", PrioritizeSegmentNumberWhenTight) ^
        SettingsHelper.CreateSetting(document, parent, "BackgroundEnabled", BackgroundEnabled) ^
        SettingsHelper.CreateSetting(document, parent, "BackgroundColor", BackgroundColor) ^
        SettingsHelper.CreateSetting(document, parent, "BackgroundColor2", BackgroundColor2) ^
        SettingsHelper.CreateSetting(document, parent, "BackgroundColor3", BackgroundColor3) ^
        SettingsHelper.CreateSetting(document, parent, "BackgroundColorCount", BackgroundColorCount) ^
        SettingsHelper.CreateSetting(document, parent, "BackgroundGradient", BackgroundGradient) ^
        SettingsHelper.CreateSetting(document, parent, "BackgroundCornerRadius", BackgroundCornerRadius) ^
        SettingsHelper.CreateSetting(document, parent, "BackgroundCorners", BackgroundCorners) ^
        SettingsHelper.CreateSetting(document, parent, "Comparison", Comparison) ^
        SettingsHelper.CreateSetting(document, parent, "Comparison2", Comparison2) ^
        SettingsHelper.CreateSetting(document, parent, "HideComparison", HideComparison) ^
        SettingsHelper.CreateSetting(document, parent, "TimingMethod", TimingMethod) ^
        SettingsHelper.CreateSetting(document, parent, "DecimalsSize", DecimalsSize) ^
        SettingsHelper.CreateSetting(document, parent, "SegmentTimerDecimalsSize", SegmentTimerDecimalsSize);
    }

    private void ColorButtonClick(object sender, EventArgs e)
    {
        SettingsHelper.ColorButtonClick((Button)sender, this);
        UpdateBackgroundControlStates();
        UpdateSegmentNumberControlStates();
    }

    private bool HasVisibleBackground()
    {
        if (BackgroundGradient == DeltasGradientType.Plain)
        {
            return BackgroundColor.A > 0;
        }

        return BackgroundColor.A > 0 ||
               BackgroundColor2.A > 0 ||
               (BackgroundColorCount == 3 && BackgroundColor3.A > 0);
    }

    private static TEnum ParseEnum<TEnum>(XmlElement element, TEnum fallback)
        where TEnum : struct
    {
        if (element != null &&
            Enum.TryParse(element.InnerText, out TEnum value) &&
            Enum.IsDefined(typeof(TEnum), value))
        {
            return value;
        }

        return fallback;
    }

    private void AlternativeTimerSettings_Load(object sender, EventArgs e)
    {
        chkOverrideTimerColors_CheckedChanged(null, null);
        chkDisplayIcon_CheckedChanged(null, null);
        chkSplitName_CheckedChanged(null, null);
        UpdateBackgroundControlStates();
        UpdateSegmentNumberControlStates();

        if (Mode == LayoutMode.Horizontal)
        {
            trkSize.DataBindings.Clear();
            trkSize.Minimum = 50;
            trkSize.Maximum = 500;
            trkSize.DataBindings.Add("Value", this, "Width", false, DataSourceUpdateMode.OnPropertyChanged);
            lblSize.Text = T("Width:");
        }
        else
        {
            trkSize.DataBindings.Clear();
            trkSize.Minimum = 20;
            trkSize.Maximum = 150;
            trkSize.DataBindings.Add("Value", this, "Height", false, DataSourceUpdateMode.OnPropertyChanged);
            lblSize.Text = T("Height:");
        }
    }

    private void btnSegmentLabelsFont_Click(object sender, EventArgs e)
    {
        CustomFontDialog.FontDialog dialog = SettingsHelper.GetFontDialog(SegmentLabelsFont, 7, 26);
        dialog.FontChanged += (s, ev) => SegmentLabelsFont = ((CustomFontDialog.FontChangedEventArgs)ev).NewFont;
        dialog.ShowDialog(this);
        lblSegmentLabelsFont.Text = SegmentLabelsFontString;
    }

    private void btnSegmentTimesFont_Click(object sender, EventArgs e)
    {
        CustomFontDialog.FontDialog dialog = SettingsHelper.GetFontDialog(SegmentTimesFont, 7, 26);
        dialog.FontChanged += (s, ev) => SegmentTimesFont = ((CustomFontDialog.FontChangedEventArgs)ev).NewFont;
        dialog.ShowDialog(this);
        lblSegmentTimesFont.Text = SegmentTimesFontString;
    }
    private void btnSplitNameFont_Click(object sender, EventArgs e)
    {
        CustomFontDialog.FontDialog dialog = SettingsHelper.GetFontDialog(SplitNameFont, 7, 26);
        dialog.FontChanged += (s, ev) => SplitNameFont = ((CustomFontDialog.FontChangedEventArgs)ev).NewFont;
        dialog.ShowDialog(this);
        lblSplitNameFont.Text = SplitNameFontString;
    }
}
