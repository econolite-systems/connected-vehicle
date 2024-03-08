// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Models.ConnectedVehicle.Status.Db
{
    public sealed record ConnectedVehicleMessageTypeMinuteTotalsDocument
        (DateTime minuteOfDay, 
        ConnectedVehicleMessageTypeEnum type, 
        int messageCount, 
        DateTime lastModified);
}
