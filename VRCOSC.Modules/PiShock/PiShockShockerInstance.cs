﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.ModuleAttributes.Attributes;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.Modules.Attributes;

namespace VRCOSC.Modules.PiShock;

public class PiShockShockerInstance : IEquatable<PiShockShockerInstance>
{
    [JsonProperty("key")]
    public Bindable<string> Key = new(string.Empty);

    [JsonProperty("username")]
    public Bindable<string> Username = new(string.Empty);

    [JsonProperty("sharecode")]
    public Bindable<string> Sharecode = new(string.Empty);

    [JsonConstructor]
    public PiShockShockerInstance()
    {
    }

    public PiShockShockerInstance(PiShockShockerInstance other)
    {
        Key.Value = other.Key.Value;
        Username.Value = other.Username.Value;
        Sharecode.Value = other.Sharecode.Value;
    }

    public bool Equals(PiShockShockerInstance? other)
    {
        if (ReferenceEquals(null, other)) return false;

        return Key.Value.Equals(other.Key.Value) && Username.Value.Equals(other.Username.Value) && Sharecode.Value.Equals(other.Sharecode.Value);
    }
}

public class PiShockShockerInstanceListAttribute : ModuleAttributeList<PiShockShockerInstance>
{
    public override Drawable GetAssociatedCard() => new PiShockShockerInstanceAttributeCardList(this);

    protected override IEnumerable<PiShockShockerInstance> JArrayToType(JArray array) => array.Select(value => new PiShockShockerInstance(value.ToObject<PiShockShockerInstance>()!)).ToList();
    protected override IEnumerable<PiShockShockerInstance> GetClonedDefaults() => Default.Select(defaultValue => new PiShockShockerInstance(defaultValue)).ToList();
}

public partial class PiShockShockerInstanceAttributeCardList : AttributeCardList<PiShockShockerInstanceListAttribute, PiShockShockerInstance>
{
    public PiShockShockerInstanceAttributeCardList(PiShockShockerInstanceListAttribute attributeData)
        : base(attributeData)
    {
    }

    protected override void OnInstanceAdd(PiShockShockerInstance instance)
    {
        AddToList(new GridContainer
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            ColumnDimensions = new[]
            {
                new Dimension(),
                new Dimension(GridSizeMode.Absolute, 5),
                new Dimension(),
                new Dimension(GridSizeMode.Absolute, 5),
                new Dimension(),
            },
            RowDimensions = new[]
            {
                new Dimension(GridSizeMode.AutoSize)
            },
            Content = new[]
            {
                new Drawable?[]
                {
                    new StringTextBox
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = 30,
                        Masking = true,
                        CornerRadius = 5,
                        BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                        BorderThickness = 2,
                        ValidCurrent = instance.Key.GetBoundCopy(),
                        PlaceholderText = "Key"
                    },
                    null,
                    new StringTextBox
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = 30,
                        Masking = true,
                        CornerRadius = 5,
                        BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                        BorderThickness = 2,
                        ValidCurrent = instance.Username.GetBoundCopy(),
                        PlaceholderText = "Username"
                    },
                    null,
                    new StringTextBox
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = 30,
                        Masking = true,
                        CornerRadius = 5,
                        BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                        BorderThickness = 2,
                        ValidCurrent = instance.Sharecode.GetBoundCopy(),
                        PlaceholderText = "Sharecode"
                    }
                }
            }
        });
    }

    protected override PiShockShockerInstance CreateInstance() => new();
}