﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Game.Packages.Sources;

public enum PackageSourceState
{
    Unknown,
    MissingRepo,
    MissingLatestRelease,
    InvalidPackageFile,
    SDKIncompatible,
    Valid
}
