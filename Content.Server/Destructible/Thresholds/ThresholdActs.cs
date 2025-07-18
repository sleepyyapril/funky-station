// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Server.Destructible.Thresholds
{
    [Flags, FlagsFor(typeof(ActsFlags))]
    [Serializable]
    public enum ThresholdActs
    {
        None = 0,
        Breakage,
        Destruction
    }
}
