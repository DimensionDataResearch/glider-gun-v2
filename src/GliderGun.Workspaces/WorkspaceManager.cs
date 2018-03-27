using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace GliderGun.Workspaces
{
    using Data;
    using Data.Models;

    // TODO: Crypto

    /// <summary>
    ///     Persistence for Glider Gun workspaces.
    /// </summary>
    public class WorkspaceManager
    {
        /// <summary>
        ///     Create a new <see cref="WorkspaceManager"/>.
        /// </summary>
        /// <param name="data">
        ///     The Glider Gun data context.
        /// </param>
        /// <param name="settings">
        ///     Settings for the workspace manager.
        /// </param>
        public WorkspaceManager(DataContext data, IOptions<WorkspaceOptions> settings)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            WorkspaceOptions options = settings.Value;
            options.EnsureValid();
            
            Data = data;
            StoreDirectory = new DirectoryInfo(options.StoreDirectory);
            WorkDirectory = new DirectoryInfo(options.WorkDirectory);
        }

        /// <summary>
        ///     The Glider Gun data context.
        /// </summary>
        DataContext Data { get; }

        /// <summary>
        ///     The base directory for workspace storage.
        /// </summary>
        DirectoryInfo StoreDirectory { get; }

        /// <summary>
        ///     The base directory for working directories.
        /// </summary>
        DirectoryInfo WorkDirectory { get; }

        /// <summary>
        ///     Create a new workspace and check it out.
        /// </summary>
        /// <param name="workspaceName">
        ///     The name of the workspace to create.
        /// </param>
        /// <param name="cancellationToken">
        ///     An optional <see cref="CancellationToken"/> that can be used to cancel the operation.
        /// </param>
        /// <returns>
        ///     The working directory where the workspace has been checked out.
        /// </returns>
        public async Task<string> Initialize(string workspaceName, CancellationToken cancellationToken = default)
        {
            if (String.IsNullOrWhiteSpace(workspaceName))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'name'.", nameof(workspaceName));
            
            Workspace workspace = new Workspace
            {
                Name = workspaceName,
                IsCheckedOut = true,
                WorkingDirectory = GetWorkingDirectory(workspaceName),
                StoreFile = GetStoreFile(workspaceName)
            };

            Data.Workspaces.Add(workspace);
            await Data.SaveChangesAsync(cancellationToken);

            DirectoryInfo workingDirectory = new DirectoryInfo(workspace.WorkingDirectory);
            if (!workingDirectory.Exists)
                workingDirectory.Create();

            return workspace.WorkingDirectory;
        }

        /// <summary>
        ///     Lock a workspace and restore its contents to a working directory .
        /// </summary>
        /// <param name="workspaceName">
        ///     The name of the target workspace.
        /// </param>
        /// <param name="cancellationToken">
        ///     An optional <see cref="CancellationToken"/> that can be used to cancel the operation.
        /// </param>
        /// <returns>
        ///     The working directory where the workspace has been checked out.
        /// </returns>
        public async Task<string> CheckOut(string workspaceName, CancellationToken cancellationToken = default)
        {
            if (String.IsNullOrWhiteSpace(workspaceName))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'workspaceName'.", nameof(workspaceName));
            
            Workspace workspace = await Data.Workspaces.FindAsync(new[] { workspaceName }, cancellationToken);
            if (workspace == null)
                throw new InvalidOperationException($"Workspace '{workspaceName}' not found.");

            if (workspace.IsCheckedOut)
                throw new InvalidOperationException($"Workspace '{workspaceName}' is already checked out.");

            FileInfo storeFile = new FileInfo(workspace.StoreFile);
            if (!storeFile.Exists)
                throw new InvalidOperationException($"Cannot find store file '{storeFile.FullName}' for workspace '{workspaceName}'.");

            DirectoryInfo workingDirectory = new DirectoryInfo(workspace.WorkingDirectory);
            if (workingDirectory.Exists)
            {
                // Purge directory contents, but don't delete the directory iself (in case it's still mounted).
                foreach (FileSystemInfo entry in workingDirectory.EnumerateFileSystemInfos())
                {
                    if (entry is DirectoryInfo directory)
                        directory.Delete(recursive: true);
                    else if (entry is FileInfo file)
                        file.Delete();
                }
            }

            // TODO: Consider creating an async implementation of this.
            ZipFile.ExtractToDirectory(
                sourceArchiveFileName: storeFile.FullName,
                destinationDirectoryName: workingDirectory.FullName
            );

            return workingDirectory.FullName;
        }

        /// <summary>
        ///     Persist workspace content and unlock the workspace.
        /// </summary>
        /// <param name="workspaceName">
        ///     The name of the target workspace.
        /// </param>
        /// <param name="leaveCheckedOut">
        ///     Leave the workspace checked out (locked)?
        /// </param>
        /// <param name="cancellationToken">
        ///     An optional <see cref="CancellationToken"/> that can be used to cancel the operation.
        /// </param>
        /// <returns>
        ///     A <see cref="Task"/> representing the operation.
        /// </returns>
        public async Task CheckIn(string workspaceName, bool leaveCheckedOut = false, CancellationToken cancellationToken = default)
        {
            if (String.IsNullOrWhiteSpace(workspaceName))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'workspaceName'.", nameof(workspaceName));
            
            Workspace workspace = await Data.Workspaces.FindAsync(new[] { workspaceName }, cancellationToken);
            if (workspace == null)
                throw new InvalidOperationException($"Workspace '{workspaceName}' not found.");

            if (!workspace.IsCheckedOut)
                throw new InvalidOperationException($"Workspace '{workspaceName}' is not checked out.");

            DirectoryInfo workingDirectory = new DirectoryInfo(workspace.WorkingDirectory);
            if (!workingDirectory.Exists)
                throw new DirectoryNotFoundException($"Cannot find working directory '{workingDirectory.FullName}' for workspace '{workspaceName}'.");

            FileInfo storeFile = new FileInfo(workspace.StoreFile);
            if (storeFile.Exists)
                storeFile.Delete();

            // TODO: Consider creating an async implementation of this.
            ZipFile.CreateFromDirectory(
                sourceDirectoryName: workingDirectory.FullName,
                destinationArchiveFileName: storeFile.FullName,
                compressionLevel: CompressionLevel.Fastest,
                includeBaseDirectory: false
            );

            if (leaveCheckedOut)
                return;

            workingDirectory.Delete(recursive: true);

            workspace.IsCheckedOut = false;
            workspace.WorkingDirectory = null;

            await Data.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        ///     Convert a workspace name to a base path name (for use in working directory and store file paths).
        /// </summary>
        /// <param name="workspaceName">
        ///     The name of the target workspace.
        /// </param>
        /// <returns>
        ///     The base path name.
        /// </returns>
        string WorkspaceNameToBasePath(string workspaceName)
        {
            if (String.IsNullOrWhiteSpace(workspaceName))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'name'.", nameof(workspaceName));

            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? workspaceName.Replace('/', '\\') : workspaceName.Replace('\\', '/');
        }

        /// <summary>
        ///     Get the full path of the 
        /// </summary>
        /// <param name="workspaceName">
        ///     The name of the target workspace.
        /// </param>
        /// <returns>
        ///     The full path of the workspace store file.
        /// </returns>
        string GetStoreFile(string workspaceName)
        {
            if (String.IsNullOrWhiteSpace(workspaceName))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'name'.", nameof(workspaceName));
            
            return Path.Combine(
                StoreDirectory.FullName,
                WorkspaceNameToBasePath(workspaceName) + ".zip"
            );
        }

        /// <summary>
        ///     Determine the working directory for the specified workspace.
        /// </summary>
        /// <param name="workspaceName">
        ///     The name of the target workspace.
        /// </param>
        /// <returns>
        ///     The full path of the workspace working directory.
        /// </returns>
        string GetWorkingDirectory(string workspaceName)
        {
            if (String.IsNullOrWhiteSpace(workspaceName))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'name'.", nameof(workspaceName));
            
            return Path.Combine(
                WorkDirectory.FullName,
                WorkspaceNameToBasePath(workspaceName)
            );
        }
    }
}
