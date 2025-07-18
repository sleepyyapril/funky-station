// SPDX-FileCopyrightText: 2023 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Zombies;

/// <summary>
/// Zombified entities with this component cannot infect other entities by attacking.
/// </summary>
[RegisterComponent]
public sealed partial class NonSpreaderZombieComponent: Component
{

}
