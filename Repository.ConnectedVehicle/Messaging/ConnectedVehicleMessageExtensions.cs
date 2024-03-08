// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using System.Text.Json;
using Econolite.Ode.Models.ConnectedVehicle;
using Econolite.Ode.Models.ConnectedVehicle.Status.Db;

namespace Econolite.Ode.Repository.ConnectedVehicle.Messaging
{
    public static partial class ConnectedVehicleMessageExtensions
    {
        public static ConnectedVehicleMessageDocument AdaptToDocument(this JsonDocument message, ConnectedVehicleMessageTypeEnum type)
        {
            var jsonString = message.RootElement.ToString();
            var messageByteSize = jsonString == null ? 0 : jsonString.Length;
            var timeStamp = DateTime.UtcNow;
            var metaData = new ConnectedVehicleMetaField(timeStamp, type, messageByteSize);
            return new ConnectedVehicleMessageDocument(timeStamp, metaData, message);
        }
        public static ConnectedVehicleAzureTrackingDocument AdaptToAzureTracking(this ConnectedVehicleMessageDocument message)
        {
            var metaData = new ConnectedVehicleAzureTrackingMetaField(message.MetaData.TimeStamp, message.MetaData.LogEntryByteSize);
            return new ConnectedVehicleAzureTrackingDocument(message.MetaData.TimeStamp, metaData);
        }
    }
}
