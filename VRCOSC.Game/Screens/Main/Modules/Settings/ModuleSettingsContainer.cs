﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK;
using VRCOSC.Graphics;
using VRCOSC.Graphics.UI;
using VRCOSC.SDK;

namespace VRCOSC.Screens.Main.Modules.Settings;

public partial class ModuleSettingsContainer : VisibilityContainer
{
    protected override bool OnMouseDown(MouseDownEvent e) => true;
    protected override bool OnClick(ClickEvent e) => true;
    protected override bool OnHover(HoverEvent e) => true;
    protected override bool OnScroll(ScrollEvent e) => true;

    protected override FillFlowContainer Content { get; }

    private Module? module;
    private readonly TextFlowContainer noSettingsDisplay;
    private readonly TextButton resetToDefault;

    public ModuleSettingsContainer()
    {
        InternalChild = new Container
        {
            RelativeSizeAxes = Axes.Both,
            Masking = true,
            CornerRadius = 10,
            BorderThickness = 3,
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colours.GRAY1
                },
                new Container
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(13),
                    Children = new Drawable[]
                    {
                        resetToDefault = new TextButton
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            RelativeSizeAxes = Axes.Y,
                            Width = 200,
                            BackgroundColour = Colours.BLUE0,
                            TextContent = "Reset To Default",
                            TextFont = Fonts.REGULAR.With(size: 25),
                            TextColour = Colours.WHITE0,
                            Action = () => module?.Settings.Values.ForEach(moduleSetting => moduleSetting.SetDefault())
                        },
                        new IconButton
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Size = new Vector2(36),
                            CornerRadius = 5,
                            Icon = Icons.Exit,
                            IconSize = 24,
                            IconColour = Colours.WHITE0,
                            BackgroundColour = Colours.RED0,
                            Action = Hide
                        }
                    }
                },
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding
                    {
                        Vertical = 3
                    },
                    Child = new BasicScrollContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.Both,
                        ClampExtension = 0,
                        ScrollbarVisible = false,
                        ScrollContent =
                        {
                            Child = Content = new FillFlowContainer
                            {
                                Name = "Settings Flow",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Width = 0.5f,
                                Direction = FillDirection.Vertical,
                                Padding = new MarginPadding
                                {
                                    Vertical = 10
                                },
                                Spacing = new Vector2(0, 10)
                            }
                        }
                    }
                },
                noSettingsDisplay = new TextFlowContainer(t =>
                {
                    t.Font = Fonts.REGULAR.With(size: 40);
                    t.Colour = Colours.WHITE2;
                })
                {
                    RelativeSizeAxes = Axes.Both,
                    TextAnchor = Anchor.Centre,
                    Text = "No Settings Available",
                }
            }
        };
    }

    public void SetModule(Module? module)
    {
        this.module = module;

        Clear();
        if (module is null) return;

        var settingsInGroup = new List<string>();

        module.Groups.ForEach(groupPair =>
        {
            var moduleSettingsGroupContainer = new ModuleSettingsGroupContainer(groupPair.Key);

            groupPair.Value.ForEach(settingLookup =>
            {
                settingsInGroup.Add(settingLookup);

                var moduleSetting = module.Settings[settingLookup];
                moduleSettingsGroupContainer.Add(moduleSetting.GetDrawable());
            });

            Add(moduleSettingsGroupContainer);
        });

        var miscModuleSettingsGroupContainer = new ModuleSettingsGroupContainer(module.Groups.Any() ? "Miscellaneous" : string.Empty);
        module.Settings.Where(settingPair => !settingsInGroup.Contains(settingPair.Key))
              .Select(settingPair => settingPair.Value)
              .ForEach(moduleSetting => miscModuleSettingsGroupContainer.Add(moduleSetting.GetDrawable()));

        if (miscModuleSettingsGroupContainer.Any()) Add(miscModuleSettingsGroupContainer);

        noSettingsDisplay.Alpha = this.Any() ? 0 : 1;
        resetToDefault.Alpha = this.Any() ? 1 : 0;
    }

    protected override void PopIn()
    {
        this.FadeInFromZero(250, Easing.OutCubic);
    }

    protected override void PopOut()
    {
        this.FadeOutFromOne(250, Easing.OutCubic).Finally(_ => SetModule(null));
    }
}
