using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GliderGun.Data.Models
{
    /// <summary>
    ///     Represents a Glider Gun workspace and its contents.
    /// </summary>
    public class Workspace
    {
        /// <summary>
        ///     The workspace's unique Id.
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        ///     The workspace's current contents (if any), in ZIP format.
        /// </summary>
        [MaxLength(2048 * 1024)]
        public byte[] Content { get; set; }

        /// <summary>
        ///     A concurrency-check token for the workspace (used for optimistic concurrency).
        /// </summary>
        [ConcurrencyCheck]
        public byte[] UpdateToken { get; set; }
    }
}
