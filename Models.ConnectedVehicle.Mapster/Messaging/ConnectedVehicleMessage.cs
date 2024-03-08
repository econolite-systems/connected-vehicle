// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using System.Text.Json;

namespace Econolite.Ode.Models.ConnectedVehicle.Messaging
{
    //Added an ErrorType field so we can know which JsonDocument type we have

    public sealed record UnknownConnectedVehicleMessage(
        string Type, 
        string Data, 
        string UnErrorType);

    public sealed record NonParseableConnectedVehicleMessage(
        string Type,
        string Data,
        string NpErrorType,
        Exception Exception
    );
}
