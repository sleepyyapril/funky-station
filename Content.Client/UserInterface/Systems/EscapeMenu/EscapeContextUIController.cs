// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.UserInterface.Systems.Info;
using Content.Shared.Input;
using JetBrains.Annotations;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;

namespace Content.Client.UserInterface.Systems.EscapeMenu;

[UsedImplicitly]
public sealed class EscapeContextUIController : UIController
{
    [Dependency] private readonly IInputManager _inputManager = default!;

    [Dependency] private readonly CloseRecentWindowUIController _closeRecentWindowUIController = default!;
    [Dependency] private readonly EscapeUIController _escapeUIController = default!;

    public override void Initialize()
    {
        _inputManager.SetInputCommand(ContentKeyFunctions.EscapeContext,
            InputCmdHandler.FromDelegate(_ => CloseWindowOrOpenGameMenu()));
    }

    private void CloseWindowOrOpenGameMenu()
    {
        if (_closeRecentWindowUIController.HasClosableWindow())
        {
            _closeRecentWindowUIController.CloseMostRecentWindow();
        }
        else
        {
            _escapeUIController.ToggleWindow();
        }
    }
}
