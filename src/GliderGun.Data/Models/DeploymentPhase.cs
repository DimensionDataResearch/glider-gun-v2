namespace GliderGun.Data.Models
{
    /// <summary>
    ///     Well-known action phases for Glider Gun deployments.
    /// </summary>
    public enum DeploymentPhase
    {
        /// <summary>
        ///     No action in progress.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Configure storage for deployment's workspace.
        /// </summary>
        Storage = 1,

        /// <summary>
        ///     Initialise or restore the deployment's workspace.
        /// </summary>
        LoadWorkspace = 2,

        /// <summary>
        ///     Execute the deployment.
        /// </summary>
        Execute = 3,

        /// <summary>
        ///     Persist the current state of the deployment's workspace.
        /// </summary>
        SaveWorkspace = 3
    }
}
