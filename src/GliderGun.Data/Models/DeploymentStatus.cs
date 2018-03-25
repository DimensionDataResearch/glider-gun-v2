namespace GliderGun.Data.Models
{
    /// <summary>
    ///     The status of a Glider Gun deployment.
    /// </summary>
    public enum DeploymentStatus
    {
        /// <summary>
        ///     Deployment status is unknown.
        /// </summary>
        /// <remarks>
        ///     Used to detect uninitialised values; do not use directly.
        /// </remarks>
        Unknown = 0,

        /// <summary>
        ///     Deployment is pending (i.e. has not started running yet).
        /// </summary>
        Pending = 1,

        /// <summary>
        ///     Deployment action is in progress.
        /// </summary>
        InProgress = 2,

        /// <summary>
        ///     Deployment completed successfully.
        /// </summary>
        Succeeded = 3,

        /// <summary>
        ///     Deployment failed.
        /// </summary>
        Failed = 4
    }
}
