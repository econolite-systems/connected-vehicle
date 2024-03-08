// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using System.Text.Json;

namespace Econolite.Ode.Models.ConnectedVehicle.Status.Db
{
    public sealed record ConnectedVehicleMessageDocument(
        DateTime TimeStamp,
        ConnectedVehicleMetaField MetaData,
        JsonDocument LogEntry
    );
}
