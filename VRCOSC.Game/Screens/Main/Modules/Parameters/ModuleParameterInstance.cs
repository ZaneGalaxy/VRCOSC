﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Graphics;
using VRCOSC.Graphics.UI.List;
using VRCOSC.Graphics.UI.Text;
using VRCOSC.SDK.Attributes.Parameters;
using VRCOSC.SDK.Parameters;

namespace VRCOSC.Screens.Main.Modules.Parameters;

public partial class ModuleParameterInstance : HeightLimitedScrollableListItem
{
    private readonly ModuleParameter moduleParameter;

    public ModuleParameterInstance(ModuleParameter moduleParameter)
    {
        this.moduleParameter = moduleParameter;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Child = new Container
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Padding = new MarginPadding(7),
            Child = new GridContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Relative, 0.1f),
                    new Dimension(GridSizeMode.Relative, 0.4f),
                    new Dimension(GridSizeMode.Relative, 0.1f),
                    new Dimension(GridSizeMode.Relative, 0.1f),
                    new Dimension()
                },
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize)
                },
                Content = new[]
                {
                    new Drawable?[]
                    {
                        new TextFlowContainer(defaultCreationParameters)
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            TextAnchor = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Text = moduleParameter.Metadata.Title
                        },
                        new TextFlowContainer(defaultCreationParameters)
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            TextAnchor = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Text = moduleParameter.Metadata.Description
                        },
                        new TextFlowContainer(defaultCreationParameters)
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            TextAnchor = Anchor.Centre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Text = moduleParameter.Metadata.ExpectedType.ToReadableName()
                        },
                        new TextFlowContainer(defaultCreationParameters)
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            TextAnchor = Anchor.Centre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Text = moduleParameter.Metadata.Mode.ToReadableName()
                        },
                        new ParameterNameTextBox
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.X,
                            Height = 30,
                            ValidCurrent = moduleParameter.Name.GetBoundCopy()
                        }
                    }
                }
            }
        };
    }

    private void defaultCreationParameters(SpriteText t)
    {
        t.Font = Fonts.REGULAR.With(size: 23);
        t.Colour = Colours.WHITE0;
    }

    private partial class ParameterNameTextBox : StringTextBox
    {
        public ParameterNameTextBox()
        {
            BackgroundUnfocused = Colours.GRAY0;
            BackgroundFocused = Colours.GRAY0;
            BackgroundCommit = Colours.GRAY0;
        }
    }
}
