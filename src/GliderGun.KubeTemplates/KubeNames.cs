using System;
using System.Text.RegularExpressions;

namespace GliderGun.KubeTemplates
{
    /// <summary>
    ///     Naming strategies for Kubernetes resources.
    /// </summary>
    public class KubeNames
    {
        /// <summary>
        ///     Regular expression for splitting PascalCase words.
        /// </summary>
        static Regex PascalCaseSplitter = new Regex(@"([a-z][A-Z])");

        /// <summary>
        ///     Create a new <see cref="KubeNames"/>.
        /// </summary>
        public KubeNames()
        {
        }

        // TODO: Add name generators.

        /// <summary>
        ///     Transform the specified Id so that it's safe for use in Kubernetes resource names.
        /// </summary>
        /// <param name="id">
        ///     The Id.
        /// </param>
        /// <returns>
        ///     The safe-for-Kubernetes name.
        /// </returns>
        public virtual string SafeId(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'id'.", nameof(id));
            
            // e.g. "DatabaseServer-1-A" -> "database-server-1-A"
            id = PascalCaseSplitter.Replace(id, match =>
            {
                return String.Concat(
                    match.Groups[0].Value[0],
                    '-',
                    Char.ToLowerInvariant(
                        match.Groups[0].Value[1]
                    )
                );
            });

            // e.g. "database-server-1-A" -> "database-server-1-a"
            id = id.ToLowerInvariant();

            return id;
        }
    }
}
