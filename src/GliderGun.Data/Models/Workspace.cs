using System;
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
        ///     The workspace name.
        /// </summary>
        [Key, MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        ///     The absolute path of the workspace's storage file (i.e. archive).
        /// </summary>
        public string StoreFile { get; set; }

        /// <summary>
        ///     Is the workspace currently checked out?
        /// </summary>
        public bool IsCheckedOut { get; set; }

        /// <summary>
        ///     The absolute path of the workspace's working directory (if checked out, otherwise <c>null</c>).
        /// </summary>
        public string WorkingDirectory { get; set; }
    }
}
