using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GliderGun.Data.Models
{
    // TODO: Create DeploymentConfiguration to represent the configuration (e.g. Terraform files, Ansible playbooks, etc) for an ad-hoc deployment.

    /// <summary>
    ///     Represents a Glider Gun deployment.
    /// </summary>
    public class Deployment
    {
        /// <summary>
        ///     The deployment's unique Id.
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        ///     The Id of the template that the deployment was created from.
        /// </summary>
        [Required, ForeignKey(nameof(Template))]
        public int TemplateId { get; set; }

        /// <summary>
        ///     The Id of the workspace used to store deployment state.
        /// </summary>
        [Required, ForeignKey(nameof(Workspace))]
        public int WorkspaceId { get; set; }
        
        /// <summary>
        ///     The name of the container image used by the deployment.
        /// </summary>
        [Required, MinLength(5), MaxLength(150)]
        public string ImageName { get; set; }
        
        /// <summary>
        ///     The tag of the container image used by the deployment.
        /// </summary>
        public string ImageTag { get; set; }
        
        /// <summary>
        ///     The deployment's current status.
        /// </summary>
        public DeploymentStatus Status { get; set; }
        
        /// <summary>
        ///     The deployment's current action (if any).
        /// </summary>
        public DeploymentAction Action { get; set; }
        
        /// <summary>
        ///     The deployment's current phase (if any).
        /// </summary>
        public DeploymentPhase Phase { get; set; }

        /// <summary>
        ///     The template that the deployment was created from.
        /// </summary>
        [JsonIgnore]
        public virtual Template Template { get; set; }

        /// <summary>
        ///     The workspace used to store deployment state.
        /// </summary>
        [JsonIgnore]
        public Workspace Workspace { get; set; }

        /// <summary>
        ///     The deployment's current parameter values.
        /// </summary>
        [InverseProperty(nameof(DeploymentParameter.DeploymentId))]
        public virtual ICollection<DeploymentParameter> Parameters { get; set; } = new HashSet<DeploymentParameter>();
    }
}
