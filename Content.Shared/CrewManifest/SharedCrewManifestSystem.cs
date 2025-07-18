// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Eui;
using NetSerializer;
using Robust.Shared.Serialization;

namespace Content.Shared.CrewManifest;

/// <summary>
///     A message to send to the server when requesting a crew manifest.
///     CrewManifestSystem will open an EUI that will send the crew manifest
///     to the player when it is updated.
/// </summary>
[Serializable, NetSerializable]
public sealed class RequestCrewManifestMessage : EntityEventArgs
{
    public NetEntity Id { get; }

    public RequestCrewManifestMessage(NetEntity id)
    {
        Id = id;
    }
}

[Serializable, NetSerializable]
public sealed class CrewManifestEuiState : EuiStateBase
{
    public string StationName { get; }
    public CrewManifestEntries? Entries { get; }

    public CrewManifestEuiState(string stationName, CrewManifestEntries? entries)
    {
        StationName = stationName;
        Entries = entries;
    }
}

[Serializable, NetSerializable]
public sealed class CrewManifestEntries
{
    /// <summary>
    ///     Entries in the crew manifest. Goes by department ID.
    /// </summary>
    // public Dictionary<string, List<CrewManifestEntry>> Entries = new();
    public CrewManifestEntry[] Entries = Array.Empty<CrewManifestEntry>();
}

[Serializable, NetSerializable]
public sealed class CrewManifestEntry
{
    public string Name { get; }

    public string JobTitle { get; }

    public string JobIcon { get; }

    public string JobPrototype { get; }

    public CrewManifestEntry(string name, string jobTitle, string jobIcon, string jobPrototype)
    {
        Name = name;
        JobTitle = jobTitle;
        JobIcon = jobIcon;
        JobPrototype = jobPrototype;
    }
}

/// <summary>
///     Tells the server to open a crew manifest UI from
///     this entity's point of view.
/// </summary>
[Serializable, NetSerializable]
public sealed class CrewManifestOpenUiMessage : BoundUserInterfaceMessage
{}
