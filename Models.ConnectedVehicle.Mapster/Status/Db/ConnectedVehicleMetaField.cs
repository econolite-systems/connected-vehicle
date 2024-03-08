// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using System.Text.Json.Serialization;

namespace Econolite.Ode.Models.ConnectedVehicle.Status.Db
{
    public sealed record ConnectedVehicleMetaField(
        //Timestamp has to be in the metadata in order to use it for a search on the delete command
        DateTime TimeStamp,
        ConnectedVehicleMessageTypeEnum Type,
        long LogEntryByteSize
    );

}
