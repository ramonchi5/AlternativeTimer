using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

using LiveSplit.Model;

namespace LiveSplit.UI.Components;

[GlobalFontConsumer(GlobalFont.TimerFont)]
public class AlternativeTimer : IComponent
{
    public Timer InternalComponent { get; set; }
    public SimpleLabel SplitName { get; set; }
    public SimpleLabel SegmentNumber { get; set; }
    public AlternativeTimerSettings Settings { get; set; }
    public GraphicsCache Cache { get; set; }
    protected int FrameCount { get; set; }

    protected int IconWidth { get; set; }

    public Image ShadowImage { get; set; }
    protected Image OldImage { get; set; }

    private readonly LiveSplitState _state;
    private int endedScrollOffset;

    public float PaddingTop => 0f;
    public float PaddingLeft => 7f;
    public float PaddingBottom => 0f;
    public float PaddingRight => 7f;

    public float VerticalHeight => Settings.Height;

    public float HorizontalWidth => Settings.Width;

    public float MinimumWidth => 20;

    public float MinimumHeight => 20;

    public IDictionary<string, Action> ContextMenuControls => null;

    private readonly Regex SubsplitRegex = new(@"^{(.+)}\s*(.+)$", RegexOptions.Compiled);
    private readonly Regex SegmentNumberRegex = new(@"^\s*(\d+)\s*/\s*(\d+)(?:\s+(.+))?\s*$", RegexOptions.Compiled);

    public AlternativeTimer(LiveSplitState state)
    {
        _state = state;
        InternalComponent = new Timer();
        Settings = new AlternativeTimerSettings()
        {
            CurrentState = state
        };
        IconWidth = 0;
        Cache = new GraphicsCache();
        SplitName = new SimpleLabel();
        SegmentNumber = new SimpleLabel();

        if (_state != null)
        {
            _state.OnStart += state_OnResetReviewScroll;
            _state.OnSplit += state_OnResetReviewScroll;
            _state.OnUndoSplit += state_OnResetReviewScroll;
            _state.OnSkipSplit += state_OnResetReviewScroll;
            _state.OnReset += state_OnResetReviewScroll;
            _state.OnScrollUp += state_OnScrollUp;
            _state.OnScrollDown += state_OnScrollDown;
        }
    }

    public void DrawGeneral(Graphics g, LiveSplitState state, float width, float height)
    {
        DrawBackground(g, width, height);

        int splitIndex = GetDisplayedSplitIndex(state);

        float originalDrawSize = Math.Min(Settings.IconSize, width - 14);
        Image icon = splitIndex >= 0 ? state.Run[splitIndex].Icon : null;
        if (Settings.DisplayIcon && icon != null)
        {
            if (OldImage != icon)
            {
                ImageAnimator.Animate(icon, (s, o) => { });
                OldImage = icon;
            }

            float drawWidth = originalDrawSize;
            float drawHeight = originalDrawSize;
            if (icon.Width > icon.Height)
            {
                float ratio = icon.Height / (float)icon.Width;
                drawHeight *= ratio;
            }
            else
            {
                float ratio = icon.Width / (float)icon.Height;
                drawWidth *= ratio;
            }

            ImageAnimator.UpdateFrames(icon);

            g.DrawImage(
                icon,
                7 + ((originalDrawSize - drawWidth) / 2),
                ((height - originalDrawSize) / 2.0f) + ((originalDrawSize - drawHeight) / 2),
                drawWidth,
                drawHeight);

            IconWidth = (int)(originalDrawSize + 7.5f);
        }
        else
        {
            IconWidth = 0;
        }

        InternalComponent.Settings.ShowGradient = Settings.TimerShowGradient;
        InternalComponent.Settings.OverrideSplitColors = Settings.OverrideTimerColors;
        InternalComponent.Settings.TimerColor = Settings.TimerColor;
        InternalComponent.Settings.DigitsFormat = Settings.DigitsFormat;
        InternalComponent.Settings.Accuracy = Settings.Accuracy;
        InternalComponent.Settings.DecimalsSize = Settings.DecimalsSize;

        DrawSegmentText(g, state, splitIndex, width, height);
    }

    public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
    {
        DrawGeneral(g, state, width, VerticalHeight);
        Matrix oldMatrix = g.Transform;
        InternalComponent.Settings.TimerHeight = VerticalHeight;
        InternalComponent.DrawVertical(g, state, width, clipRegion);
        g.Transform = oldMatrix;
    }

    public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
    {
        DrawGeneral(g, state, HorizontalWidth, height);
        Matrix oldMatrix = g.Transform;
        InternalComponent.Settings.TimerWidth = HorizontalWidth;
        InternalComponent.DrawHorizontal(g, state, height, clipRegion);
        g.Transform = oldMatrix;
    }

    public string ComponentName => "Alternative Timer";

    public Control GetSettingsControl(LayoutMode mode)
    {
        Settings.Mode = mode;
        return Settings;
    }

    public void SetSettings(XmlNode settings)
    {
        Settings.SetSettings(settings);
    }

    public XmlNode GetSettings(XmlDocument document)
    {
        return Settings.GetSettings(document);
    }

    public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
    {
        int splitIndex = GetDisplayedSplitIndex(state);
        var displayName = splitIndex >= 0
            ? GetDisplaySplitName(state, splitIndex)
            : new SegmentDisplayText(string.Empty, string.Empty);
        SplitName.Text = displayName.Name;
        SegmentNumber.Text = displayName.NumberText;

        InternalComponent.Settings.TimingMethod = Settings.TimingMethod;
        InternalComponent.Update(null, state, width, height, mode);

        Image icon = splitIndex >= 0 ? state.Run[splitIndex].Icon : null;

        Cache.Restart();
        Cache["SplitIcon"] = icon;
        if (Cache.HasChanged)
        {
            if (icon == null)
            {
                FrameCount = 0;
            }
            else
            {
                FrameCount = icon.GetFrameCount(new FrameDimension(icon.FrameDimensionsList[0]));
            }
        }

        Cache["SplitName"] = SplitName.Text;
        Cache["SegmentNumber"] = SegmentNumber.Text;
        Cache["InternalComponentText"] = InternalComponent.BigTextLabel.Text + InternalComponent.SmallTextLabel.Text;
        if (InternalComponent.BigTextLabel.Brush != null && invalidator != null)
        {
            if (InternalComponent.BigTextLabel.Brush is LinearGradientBrush brush)
            {
                Cache["TimerColor"] = brush.LinearColors.First().ToArgb();
            }
            else
            {
                Cache["TimerColor"] = InternalComponent.BigTextLabel.ForeColor.ToArgb();
            }
        }

        if (invalidator != null && (Cache.HasChanged || FrameCount > 1))
        {
            invalidator.Invalidate(0, 0, width, height);
        }
    }

    public void Dispose()
    {
        if (_state != null)
        {
            _state.OnStart -= state_OnResetReviewScroll;
            _state.OnSplit -= state_OnResetReviewScroll;
            _state.OnUndoSplit -= state_OnResetReviewScroll;
            _state.OnSkipSplit -= state_OnResetReviewScroll;
            _state.OnReset -= state_OnResetReviewScroll;
            _state.OnScrollUp -= state_OnScrollUp;
            _state.OnScrollDown -= state_OnScrollDown;
        }
    }

    public int GetSettingsHashCode()
    {
        return Settings.GetSettingsHashCode();
    }

    private int GetDisplayedSplitIndex(LiveSplitState state)
    {
        if (state.Run.Count == 0 || state.CurrentSplitIndex < 0)
        {
            return -1;
        }

        if (state.CurrentPhase != TimerPhase.Ended)
        {
            return Math.Min(state.CurrentSplitIndex, state.Run.Count - 1);
        }

        ClampEndedScrollOffset();
        return GetBaseReviewSplitIndex(state) + endedScrollOffset;
    }

    private int GetBaseReviewSplitIndex(LiveSplitState state)
    {
        if (state?.Run == null || state.Run.Count == 0)
        {
            return -1;
        }

        return Math.Min(Math.Max(state.CurrentSplitIndex, 0), state.Run.Count - 1);
    }

    private void state_OnResetReviewScroll(object sender, EventArgs e)
    {
        endedScrollOffset = 0;
    }

    private void state_OnResetReviewScroll(object sender, TimerPhase e)
    {
        endedScrollOffset = 0;
    }

    private void state_OnScrollUp(object sender, EventArgs e)
    {
        if (_state == null || _state.CurrentPhase != TimerPhase.Ended)
        {
            return;
        }

        endedScrollOffset--;
        ClampEndedScrollOffset();
    }

    private void state_OnScrollDown(object sender, EventArgs e)
    {
        if (_state == null || _state.CurrentPhase != TimerPhase.Ended)
        {
            return;
        }

        endedScrollOffset++;
        ClampEndedScrollOffset();
    }

    private void ClampEndedScrollOffset()
    {
        if (_state?.Run == null || _state.Run.Count == 0)
        {
            endedScrollOffset = 0;
            return;
        }

        int baseIndex = GetBaseReviewSplitIndex(_state);
        endedScrollOffset = Math.Min(
            Math.Max(endedScrollOffset, -baseIndex),
            _state.Run.Count - baseIndex - 1);
    }

    private SegmentDisplayText GetDisplaySplitName(LiveSplitState state, int splitIndex)
    {
        string name = state.Run[splitIndex].Name;

        bool isSubsplit = name.StartsWith("-") && splitIndex < state.Run.Count - 1;

        if (isSubsplit)
        {
            name = name.Substring(1);
        }
        else
        {
            Match match = SubsplitRegex.Match(name);
            if (match.Success)
            {
                name = match.Groups[2].Value;
            }
        }

        Match segmentNumberMatch = SegmentNumberRegex.Match(name);
        if (!segmentNumberMatch.Success)
        {
            return new SegmentDisplayText(name, string.Empty);
        }

        string cleanName = segmentNumberMatch.Groups[3].Success
            ? segmentNumberMatch.Groups[3].Value.Trim()
            : string.Empty;
        string numberText = $"{segmentNumberMatch.Groups[1].Value} of {segmentNumberMatch.Groups[2].Value}";
        return new SegmentDisplayText(cleanName, numberText);
    }

    private void DrawSegmentText(Graphics g, LiveSplitState state, int splitIndex, float width, float height)
    {
        if (splitIndex < 0)
        {
            return;
        }

        bool canDrawName = Settings.ShowSplitName && !string.IsNullOrWhiteSpace(SplitName.Text);
        bool canDrawNumber = Settings.ShowSegmentNumber && !string.IsNullOrWhiteSpace(SegmentNumber.Text);
        if (!canDrawName && !canDrawNumber)
        {
            return;
        }

        using Font segmentNumberFont = CreateSegmentNumberFont();
        float nameHeight = canDrawName ? MeasureTextHeight(g, Settings.SplitNameFont) : 0f;
        float numberHeight = canDrawNumber ? MeasureTextHeight(g, segmentNumberFont) : 0f;
        float gap = Settings.SegmentNumberGap;

        bool drawName = canDrawName;
        bool drawNumber = canDrawNumber;
        SegmentTextLayout layout = CalculateSegmentTextLayout(
            drawName,
            drawNumber,
            nameHeight,
            numberHeight,
            gap,
            height,
            Settings.SegmentTextVerticalOffset,
            Settings.SegmentNumberPosition);

        if (drawName && drawNumber && (layout.BlockHeight > height || layout.Top < 0f || layout.Bottom > height))
        {
            if (Settings.PrioritizeSegmentNumberWhenTight)
            {
                drawName = false;
            }
            else
            {
                drawNumber = false;
            }
        }

        if (drawName && nameHeight > height)
        {
            drawName = false;
        }

        if (drawNumber && numberHeight > height)
        {
            drawNumber = false;
        }

        if (!drawName && !drawNumber)
        {
            return;
        }

        layout = CalculateSegmentTextLayout(
            drawName,
            drawNumber,
            nameHeight,
            numberHeight,
            gap,
            height,
            Settings.SegmentTextVerticalOffset,
            Settings.SegmentNumberPosition);

        float rightLimit = Math.Max(0f, width - InternalComponent.ActualWidth - 5f);
        if (drawName)
        {
            DrawSimpleLabel(
                g,
                state,
                SplitName,
                SplitName.Text,
                Settings.SplitNameFont,
                Settings.SplitNameColor,
                IconWidth + 5f,
                layout.NameY,
                Math.Max(0f, rightLimit - IconWidth - 5f),
                nameHeight);
        }

        if (drawNumber)
        {
            float numberX = Math.Max(0f, Settings.SegmentNumberLeftPadding);
            DrawSimpleLabel(
                g,
                state,
                SegmentNumber,
                SegmentNumber.Text,
                segmentNumberFont,
                Settings.SegmentNumberColor,
                numberX,
                layout.NumberY,
                Math.Max(0f, rightLimit - numberX),
                numberHeight);
        }
    }

    private static SegmentTextLayout CalculateSegmentTextLayout(
        bool drawName,
        bool drawNumber,
        float nameHeight,
        float numberHeight,
        float gap,
        float containerHeight,
        float verticalOffset,
        AlternativeTimerSegmentNumberPosition position)
    {
        float nameRelativeY = 0f;
        float numberRelativeY = 0f;

        if (drawName && drawNumber)
        {
            if (position == AlternativeTimerSegmentNumberPosition.Below)
            {
                numberRelativeY = nameHeight + gap;
            }
            else
            {
                nameRelativeY = numberHeight + gap;
            }
        }

        float top = float.PositiveInfinity;
        float bottom = float.NegativeInfinity;

        if (drawName)
        {
            top = Math.Min(top, nameRelativeY);
            bottom = Math.Max(bottom, nameRelativeY + nameHeight);
        }

        if (drawNumber)
        {
            top = Math.Min(top, numberRelativeY);
            bottom = Math.Max(bottom, numberRelativeY + numberHeight);
        }

        if (!drawName && !drawNumber)
        {
            top = 0f;
            bottom = 0f;
        }

        float blockHeight = Math.Max(0f, bottom - top);
        float baseY = ((containerHeight - blockHeight) / 2f) + verticalOffset - top;
        float actualTop = baseY + top;
        float actualBottom = baseY + bottom;

        return new SegmentTextLayout(
            baseY + nameRelativeY,
            baseY + numberRelativeY,
            blockHeight,
            actualTop,
            actualBottom);
    }

    private Font CreateSegmentNumberFont()
    {
        FontStyle style = Settings.SegmentNumberBold
            ? Settings.SplitNameFont.Style | FontStyle.Bold
            : Settings.SplitNameFont.Style & ~FontStyle.Bold;
        float size = Math.Max(5f, Settings.SplitNameFont.Size * 0.5f);
        return new Font(Settings.SplitNameFont.FontFamily, size, style, Settings.SplitNameFont.Unit);
    }

    private static float MeasureTextHeight(Graphics g, Font font)
    {
        return g.MeasureString("Ay", font).Height;
    }

    private static void DrawSimpleLabel(
        Graphics g,
        LiveSplitState state,
        SimpleLabel label,
        string text,
        Font font,
        Color color,
        float x,
        float y,
        float width,
        float height)
    {
        label.Text = text;
        label.Font = font;
        label.X = x;
        label.Y = y;
        label.Width = width;
        label.Height = height;
        label.HorizontalAlignment = StringAlignment.Near;
        label.VerticalAlignment = StringAlignment.Center;
        label.ForeColor = color;
        label.HasShadow = state.LayoutSettings.DropShadows;
        label.ShadowColor = state.LayoutSettings.ShadowsColor;
        label.OutlineColor = state.LayoutSettings.TextOutlineColor;
        label.Draw(g);
    }

    private void DrawBackground(Graphics g, float width, float height)
    {
        if (!Settings.BackgroundEnabled || width <= 0f || height <= 0f)
        {
            return;
        }

        RectangleF rect = new(0f, 0f, Math.Max(width, 1f), Math.Max(height, 1f));
        if (Settings.BackgroundGradient == DeltasGradientType.Plain)
        {
            if (Settings.BackgroundColor.A <= 0)
            {
                return;
            }

            using SolidBrush solidBrush = new(Settings.BackgroundColor);
            FillBackground(g, solidBrush, rect, Settings.BackgroundCornerRadius, Settings.BackgroundCorners);
            return;
        }

        bool useThreeColors = Settings.BackgroundColorCount == 3;
        bool hasVisibleColor =
            Settings.BackgroundColor.A > 0 ||
            Settings.BackgroundColor2.A > 0 ||
            (useThreeColors && Settings.BackgroundColor3.A > 0);
        if (!hasVisibleColor)
        {
            return;
        }

        LinearGradientMode mode = Settings.BackgroundGradient == DeltasGradientType.Horizontal
            ? LinearGradientMode.Horizontal
            : LinearGradientMode.Vertical;
        using LinearGradientBrush gradientBrush = new(
            rect,
            Settings.BackgroundColor,
            useThreeColors ? Settings.BackgroundColor3 : Settings.BackgroundColor2,
            mode);
        if (useThreeColors)
        {
            gradientBrush.InterpolationColors = new ColorBlend
            {
                Positions = new[] { 0f, 0.5f, 1f },
                Colors = new[] { Settings.BackgroundColor, Settings.BackgroundColor2, Settings.BackgroundColor3 },
            };
        }

        FillBackground(g, gradientBrush, rect, Settings.BackgroundCornerRadius, Settings.BackgroundCorners);
    }

    private static void FillBackground(Graphics g, Brush brush, RectangleF rect, int radius, AlternativeTimerBackgroundCorners corners)
    {
        if (radius <= 0)
        {
            g.FillRectangle(brush, rect);
            return;
        }

        SmoothingMode oldSmoothing = g.SmoothingMode;
        try
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using GraphicsPath path = CreateRoundedRectanglePath(rect, radius, corners);
            g.FillPath(brush, path);
        }
        finally
        {
            g.SmoothingMode = oldSmoothing;
        }
    }

    private static GraphicsPath CreateRoundedRectanglePath(RectangleF rect, float radius, AlternativeTimerBackgroundCorners corners)
    {
        GraphicsPath path = new();
        float diameter = Math.Min(Math.Min(radius * 2f, rect.Width), rect.Height);
        if (diameter <= 0f)
        {
            path.AddRectangle(rect);
            path.CloseFigure();
            return path;
        }

        bool roundTop = corners == AlternativeTimerBackgroundCorners.All ||
                        corners == AlternativeTimerBackgroundCorners.Top;
        bool roundBottom = corners == AlternativeTimerBackgroundCorners.All ||
                           corners == AlternativeTimerBackgroundCorners.Bottom;

        path.StartFigure();
        if (roundTop)
        {
            path.AddArc(rect.Left, rect.Top, diameter, diameter, 180f, 90f);
            path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270f, 90f);
        }
        else
        {
            path.AddLine(rect.Left, rect.Top, rect.Right, rect.Top);
        }

        if (roundBottom)
        {
            path.AddLine(rect.Right, roundTop ? rect.Top + diameter : rect.Top, rect.Right, rect.Bottom - diameter);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0f, 90f);
            path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90f, 90f);
        }
        else
        {
            path.AddLine(rect.Right, roundTop ? rect.Top + diameter : rect.Top, rect.Right, rect.Bottom);
            path.AddLine(rect.Right, rect.Bottom, rect.Left, rect.Bottom);
            path.AddLine(rect.Left, rect.Bottom, rect.Left, roundTop ? rect.Top + diameter : rect.Top);
        }

        path.CloseFigure();
        return path;
    }

    private readonly struct SegmentDisplayText
    {
        public SegmentDisplayText(string name, string numberText)
        {
            Name = name;
            NumberText = numberText;
        }

        public string Name { get; }
        public string NumberText { get; }
    }

    private readonly struct SegmentTextLayout
    {
        public SegmentTextLayout(float nameY, float numberY, float blockHeight, float top, float bottom)
        {
            NameY = nameY;
            NumberY = numberY;
            BlockHeight = blockHeight;
            Top = top;
            Bottom = bottom;
        }

        public float NameY { get; }
        public float NumberY { get; }
        public float BlockHeight { get; }
        public float Top { get; }
        public float Bottom { get; }
    }
}
