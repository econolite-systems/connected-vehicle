// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

namespace Econolite.Ode.Repository.ConnectedVehicle
{
    public static class ConnectedVehicleRepositoryHelper
    {
        /// <summary>
        /// Calculates the number of bytes we need to delete in order to get under the configured repository maximum storage size
        /// </summary>
        /// <param name="repoTotalByteSize"></param>
        /// <param name="configMaxSize"></param>
        /// <returns></returns>
        public static long GetBytesToPurge(long repoTotalByteSize, long configMaxSize)
        {
            long bytesToDelete = 0;
            //is the repository storage greater than the requested max size
            if (repoTotalByteSize > configMaxSize)
            {
                //calculate how much data do we need to lose to hit the max
                bytesToDelete = repoTotalByteSize - configMaxSize;
            }
            return bytesToDelete;
        }
    }
}
