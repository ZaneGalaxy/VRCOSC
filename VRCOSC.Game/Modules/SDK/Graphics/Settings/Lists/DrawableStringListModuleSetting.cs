﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.Modules.SDK.Attributes.Settings;

namespace VRCOSC.Game.Modules.SDK.Graphics.Settings.Lists;

public partial class DrawableStringListModuleSetting : DrawableListModuleSetting<Bindable<string>>
{
    public DrawableStringListModuleSetting(StringListModuleSetting moduleSetting)
        : base(moduleSetting)
    {
    }
}

public partial class DrawableStringListModuleSettingItem : DrawableListModuleSettingItem<Bindable<string>>
{
    public DrawableStringListModuleSettingItem(Bindable<string> item)
        : base(item)
    {
        Add(new StringTextBox
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 35,
            ValidCurrent = Item.GetBoundCopy()
        });
    }
}