namespace GliderGun.KubeTemplates
{
    /// <summary>
    ///     Settings for Kubernetes templates.
    /// </summary>
    public class KubeTemplateOptions
    {
        /// <summary>
        ///     The default namespace for generated resources.
        /// </summary>
        public string DefaultNamespace { get; set; } = "default";
    }
}