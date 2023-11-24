﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Profiles;

namespace VRCOSC.Game.Screens.Main.Profiles;

public partial class ProfilesList : Container
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

    private readonly FillFlowContainer flowWrapper;
    private readonly BasicScrollContainer scrollContainer;
    private readonly Container header;

    protected override FillFlowContainer Content { get; }

    public ProfilesList()
    {
        RelativeSizeAxes = Axes.Both;

        InternalChild = flowWrapper = new FillFlowContainer
        {
            Name = "Flow Wrapper",
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Masking = true,
            CornerRadius = 5,
            Children = new Drawable[]
            {
                header = new Container
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    Height = 50,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colours.GRAY0
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(10),
                            Child = new SpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = Fonts.REGULAR.With(size: 28),
                                Text = "Profiles"
                            }
                        }
                    }
                },
                scrollContainer = new BasicScrollContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    ClampExtension = 0,
                    ScrollbarVisible = false,
                    ScrollContent =
                    {
                        Child = Content = new FillFlowContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical
                        }
                    }
                },
                new Box
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    Height = 5,
                    Colour = Colours.GRAY0
                }
            }
        };
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        appManager.ProfileManager.Profiles.BindCollectionChanged(onProfileCollectionChanged, true);
    }

    private void onProfileCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Clear();

        var even = false;

        appManager.ProfileManager.Profiles.OrderBy(profile => profile.Name.Value).ForEach(profile =>
        {
            Add(new ProfileListInstance(profile, even));
            even = !even;
        });
    }

    protected override void UpdateAfterChildren()
    {
        if (flowWrapper.DrawHeight >= DrawHeight)
        {
            scrollContainer.AutoSizeAxes = Axes.None;
            scrollContainer.Height = DrawHeight - header.DrawHeight - 5;
        }
        else
        {
            scrollContainer.AutoSizeAxes = Axes.Y;
        }
    }
}

public partial class ProfileListInstance : Container
{
    private readonly Profile profile;

    protected override Container Content { get; }
    private readonly SpriteText nameText;

    public ProfileListInstance(Profile profile, bool even)
    {
        this.profile = profile;

        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        InternalChildren = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = even ? Colours.GRAY4 : Colours.GRAY2
            },
            Content = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(10),
                Children = new Drawable[]
                {
                    nameText = new SpriteText
                    {
                        Font = Fonts.REGULAR.With(size: 25)
                    }
                }
            }
        };
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        profile.Name.BindValueChanged(e => nameText.Text = e.NewValue, true);
    }
}
