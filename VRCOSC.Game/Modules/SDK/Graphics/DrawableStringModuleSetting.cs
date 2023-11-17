﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.Modules.SDK.Attributes;

namespace VRCOSC.Game.Modules.SDK.Graphics;

public partial class DrawableStringModuleSetting : DrawableValueModuleSetting<StringModuleSetting>
{
    public DrawableStringModuleSetting(StringModuleSetting moduleSetting)
        : base(moduleSetting)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new StringTextBox
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 35,
            ValidCurrent = ModuleSetting.Attribute.GetBoundCopy(),
            EmptyIsValid = { Value = ModuleSetting.EmptyIsValid }
        });
    }
}
