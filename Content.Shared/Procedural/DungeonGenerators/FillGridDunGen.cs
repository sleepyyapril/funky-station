// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Maps;
using Robust.Shared.Prototypes;

namespace Content.Shared.Procedural.DungeonGenerators;

/// <summary>
/// Fills unreserved tiles with the specified entity prototype.
/// </summary>
/// <remarks>
/// DungeonData keys are:
/// - Fill
/// </remarks>
public sealed partial class FillGridDunGen : IDunGenLayer
{
    /// <summary>
    /// Tiles the fill can occur on.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<ContentTileDefinition>>? AllowedTiles;
}
