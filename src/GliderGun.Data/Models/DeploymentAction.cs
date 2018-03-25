namespace GliderGun.Data.Models
{
    /// <summary>
    ///     An action performed on a Glider Gun deployment.
    /// </summary>
    public enum DeploymentAction
    {
        /// <summary>
        ///     No action.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Create / update the deployment and its associated resources.
        /// </summary>
        Deploy = 1,

        /// <summary>
        ///     Destroy the deployment and its associated resources.
        /// </summary>
        Destroy = 2
    }
}
