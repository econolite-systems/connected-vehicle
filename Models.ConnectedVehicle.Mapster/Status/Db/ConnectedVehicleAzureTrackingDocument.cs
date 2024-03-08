// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Models.ConnectedVehicle.Status.Db
{
    public sealed record ConnectedVehicleAzureTrackingDocument(
        DateTime TimeStamp,
        ConnectedVehicleAzureTrackingMetaField MetaData
    );
}
