using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GliderGun.Data.Models
{
    /// <summary>
    ///     Represents the definition of a parameter for a Glider Gun template.
    /// </summary>
    public class TemplateParameter
    {
        /// <summary>
        ///     The Id of the template that the parameter applies to.
        /// </summary>
        [Key]
        public int TemplateId { get; set; }

        /// <summary>
        ///     The parameter name.
        /// </summary>
        [Key, MinLength(2), MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        ///     Is the parameter value likely to contain sensitive information?
        /// </summary>
        public bool IsSensitive { get; set; }

        /// <summary>
        ///     A reference to the template that the parameter applies to.
        /// </summary>
        [JsonIgnore, ForeignKey(nameof(TemplateId))]
        public virtual Template Template { get; set; }
    }
}
