using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui;

namespace Zenvestify.Shared.Services
{
    public class Essentials
    {
        /// <summary>
        /// Gets the current application version and build number.
        /// </summary>
        public static string GetAppVersion()
        {
            var version = VersionTracking.Default.CurrentVersion;
            var build = VersionTracking.Default.CurrentBuild;
            var platform = DeviceInfo.Platform;
            return $"Zenvesity {platform} v {version} (Build {build})";
        }

        /// <summary>
        /// Get metadata from VersionTracker
        /// </summary>
        public static string GetAppMetaData()
        {
            var tracker = VersionTracking.Default;
            var metaData = new List<string>
            {
                tracker.IsFirstLaunchEver.ToString(),
                tracker.IsFirstLaunchForCurrentVersion.ToString(),
                tracker.IsFirstLaunchForCurrentBuild.ToString(),
                tracker.CurrentVersion.ToString(),
                tracker.CurrentBuild.ToString(),
                String.Join(',', tracker.VersionHistory),
                String.Join(',', tracker.BuildHistory),
                tracker?.ToString() ?? "none",
                tracker?.ToString() ?? "none",
            };

            return String.Join('\n', metaData);
        }
    }
}
