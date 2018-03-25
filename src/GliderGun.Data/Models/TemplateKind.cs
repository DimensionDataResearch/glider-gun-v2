namespace GliderGun.Data
{
    /// <summary>
    ///     Well-known kinds of Glider Gun template.
    /// </summary>
    public enum TemplateKind
    {
        /// <summary>
        ///     An unknown template kind.
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///     A standard template (configuration is baked into the template image).
        /// </summary>
        Standard = 1,

        /// <summary>
        ///     An ad-hoc template (configuration is supplied when executing the template).
        /// </summary>
        AdHoc = 2
    }
}
