using System;

using LiveSplit.Model;
using LiveSplit.UI.Components;

[assembly: ComponentFactory(typeof(AlternativeTimerFactory))]

namespace LiveSplit.UI.Components;

public class AlternativeTimerFactory : IComponentFactory
{
    public string ComponentName => "Alternative Timer";

    public string Description => "Current segment name timer companion for SplitDetail, without a segment timer.";

    public ComponentCategory Category => ComponentCategory.Timer;

    public IComponent Create(LiveSplitState state)
    {
        return new AlternativeTimer(state);
    }

    public string UpdateName => ComponentName;

    public string XMLURL => string.Empty;

    public string UpdateURL => string.Empty;

    public Version Version => Version.Parse("1.0.0");
}
