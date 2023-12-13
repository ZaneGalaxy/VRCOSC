﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.Actions;

/// <summary>
/// Represents an action that has a progress associated with it
/// </summary>
public abstract class ProgressAction
{
    /// <summary>
    /// The title of this <see cref="ProgressAction"/>
    /// </summary>
    public virtual string Title => string.Empty;

    /// <summary>
    /// Marks if this <see cref="ProgressAction"/> is complete. This is true after <see cref="Perform"/> has completed
    /// </summary>
    public bool IsComplete { get; private set; }

    /// <summary>
    /// Executes this <see cref="ProgressAction"/>
    /// </summary>
    public async Task Execute()
    {
        await Perform();
        IsComplete = true;
    }

    /// <summary>
    /// Performs the action
    /// </summary>
    protected abstract Task Perform();

    /// <summary>
    /// Returns the progress
    /// </summary>
    public abstract float GetProgress();
}
