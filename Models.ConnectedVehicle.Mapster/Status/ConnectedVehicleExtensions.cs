// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Cloud.Common.Models;

namespace Econolite.Ode.Models.ConnectedVehicle.Status
{
    public static partial class ConnectedVehicleExtensions
    {
        /// <summary>
        /// Adds the mobiliby signal id and name based on the jpo intersection id to the intersection totals
        /// </summary>
        /// <param name="intersectionTotal"></param>
        /// <returns></returns>
        public static ConnectedVehicleIntersectionTypeCountAndSize AdaptToIntersectionName(this ConnectedVehicleIntersectionTypeCountAndSize intersectionTotal)
        {
            //default the signal name to the JPO intersection name
            intersectionTotal.SignalName = intersectionTotal.IntersectionName;

            //TODO:  ko - try to go get the signalId based on the JPO intersectionId; if we find a match set the SignalName and SignalId
            //TODO:  ko - careful here; we don't want a database call for every record; maybe pass in a query result that we can filter

            return intersectionTotal;
        }

        public static ConnectedVehicleRepositoryTypeCountAndSize AdaptToTypeCountAndSize(this AzureContainerSummary? summary)
        {
            var archiveTotals = new ConnectedVehicleRepositoryTypeCountAndSize()
            {
                ByteSize = 0,
                Type = ConnectedVehicleRepositoryTypeEnum.Archive,
                MessageCount = 0,
            };
            if (summary != null)
            {
                archiveTotals.ByteSize = summary.ContainerByteSize;
                archiveTotals.MessageCount = summary.BlobCount;
            }
            return archiveTotals;
        }
    }
}
