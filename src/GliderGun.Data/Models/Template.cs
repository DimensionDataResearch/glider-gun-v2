using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GliderGun.Data.Models
{
    /// <summary>
    ///     Represents a Glider Gun template.
    /// </summary>
    public class Template
    {
        /// <summary>
        ///     The template's unique Id.
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        ///     The template name.
        /// </summary>
        [Required, MinLength(5), MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        ///     The template's container image name.
        /// </summary>
        [Required, MinLength(5), MaxLength(150)]
        public string ImageName { get; set; }

        /// <summary>
        ///     The template's container image tag.
        /// </summary>
        [Required, MinLength(5), MaxLength(150)]
        public string ImageTag { get; set; }

        /// <summary>
        ///     An optional description of the template.
        /// </summary>
        [MaxLength(250)]
        public string Description { get; set; }

        /// <summary>
        ///     The kind of Glider Gun template (e.g. <see cref="TemplateKind.Standard"/> or <see cref="TemplateKind.AdHoc"/>).
        /// </summary>
        [Required]
        public TemplateKind Kind { get; set; }

        /// <summary>
        ///     The template's parameter definitions.
        /// </summary>
        [InverseProperty(nameof(TemplateParameter.TemplateId))]
        public virtual ICollection<TemplateParameter> Parameters { get; set; } = new HashSet<TemplateParameter>();

        /// <summary>
        ///     <see cref="Deployments"/> created from the template.
        /// </summary>
        [InverseProperty(nameof(Deployment.TemplateId))]
        public virtual ICollection<Deployment> Deployments { get; set; } = new HashSet<Deployment>();
    }
}
