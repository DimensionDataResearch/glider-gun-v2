using System;

namespace GliderGun.Workspaces
{
    /// <summary>
    ///     Settings for workspace management.
    /// </summary>
    public class WorkspaceOptions
    {
        /// <summary>
        ///     The base directory for workspace storage.
        /// </summary>
        public string StoreDirectory { get; set; }

        /// <summary>
        ///     The base directory for working directories.
        /// </summary>
        public string WorkDirectory { get; set; }

        /// <summary>
        ///     Ensure that the workspace options are valid.
        /// </summary>
        public void EnsureValid()
        {
            if (String.IsNullOrWhiteSpace(StoreDirectory))
                throw new InvalidOperationException($"{nameof(WorkspaceOptions)} is invalid ({nameof(StoreDirectory)} is missing or empty).");

            if (String.IsNullOrWhiteSpace(WorkDirectory))
                throw new InvalidOperationException($"{nameof(WorkspaceOptions)} is invalid ({nameof(WorkDirectory)} is missing or empty).");
        }
    }
}
