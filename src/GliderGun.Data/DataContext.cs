﻿using Microsoft.EntityFrameworkCore;
using System;

namespace GliderGun.Data
{
    using Models;
    
    /// <summary>
    ///     Entity context for the main Glider Gun database.
    /// </summary>
    public class DataContext
        : DbContext
    {
        /// <summary>
        ///     Create a new <see cref="DataContext"/>.
        /// </summary>
        /// <param name="options">
        ///     <see cref="DbContextOptions"/> used to configure the <see cref="DataContext"/>.
        /// </param>
        public DataContext(DbContextOptions options)
            : base(options)
        {
        }

        /// <summary>
        ///     The set of all <see cref="Template"/> entities.
        /// </summary>
        public virtual DbSet<Template> Templates => Set<Template>();

        /// <summary>
        ///     The set of all <see cref="TemplateParameter"/> entities.
        /// </summary>
        public virtual DbSet<TemplateParameter> TemplateParameters => Set<TemplateParameter>();

        /// <summary>
        ///     The set of all <see cref="Deployment"/> entities.
        /// </summary>
        public virtual DbSet<Deployment> Deployments => Set<Deployment>();

        /// <summary>
        ///     The set of all <see cref="DeploymentParameter"/> entities.
        /// </summary>
        public virtual DbSet<DeploymentParameter> DeploymentParameters => Set<DeploymentParameter>();

        /// <summary>
        ///     The set of all <see cref="Workspace"/> entities.
        /// </summary>
        public virtual DbSet<Workspace> Workspaces => Set<Workspace>();

        /// <summary>
        ///     Ensure that the specified entity is not tracked by the <see cref="DataContext"/>.
        /// </summary>
        /// <param name="entity">
        ///     The entity to detach.
        /// </param>
        /// <returns>
        ///     The detached entity.
        /// </returns>
        public TEntity Detach<TEntity>(TEntity entity)
            where TEntity : class
        {
            Entry(entity).State = EntityState.Detached;

            return entity;
        }
    }
}
