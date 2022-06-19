﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.Containers.Screens;
using VRCOSC.Game.Graphics.Updater;

namespace VRCOSC.Game;

public abstract class VRCOSCGame : VRCOSCGameBase
{
    private VRCOSCUpdateManager updateManager;

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new ScreenManager(),
            updateManager = CreateUpdateManager()
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        Scheduler.AddDelayed(() => Task.Run(() => updateManager.CheckForUpdate()).ConfigureAwait(false), 1000);
    }

    public abstract VRCOSCUpdateManager CreateUpdateManager();
}
