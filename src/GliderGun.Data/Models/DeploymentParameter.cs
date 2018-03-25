using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GliderGun.Data.Models
{
    /// <summary>
    ///     Represents the value of a parameter for a Glider Gun deployment.
    /// </summary>
    public class DeploymentParameter
    {
        /// <summary>
        ///     The Id of the deployment that the parameter applies to.
        /// </summary>
        [Key]
        public int DeploymentId { get; set; }

        /// <summary>
        ///     The parameter name.
        /// </summary>
        [Key, MinLength(2), MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        ///     Does the parameter contain sensitive data?
        /// </summary>
        /// <remarks>
        ///     If <c>true</c>, then <see cref="Value"/> will be <c>null</c> and the parameter value will instead be stored in Vault.
        /// </remarks>
        public bool IsSensitive { get; set; }

        /// <summary>
        ///     The parameter value (unless <see cref="IsSensitive"/> is <c>true</c>).
        /// </summary>
        [MaxLength(2048)]
        public string Value { get; set; }

        /// <summary>
        ///     A reference to the deployment that the parameter applies to.
        /// </summary>
        [JsonIgnore, ForeignKey(nameof(DeploymentId))]
        public virtual Deployment Deployment { get; set; }
    }
}
