using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DataModel.Class;

namespace LocomotionEngines
{
	/// <summary>
	/// Connects to a database, allowing access and modification of the model within.
	/// </summary>
	public class DataModelContext : DbContext
	{
		public DbSet<User> Users { get; set; }

		public DbSet<Network> Networks { get; set; }

		public DbSet<Node> Nodes { get; set; }
		public DbSet<NodeOptimized> NodesOptimized { get; set; }

		public DbSet<Link> Links { get; set; }
		public DbSet<LinkOptimized> LinksOptimized { get; set; }

		public DbSet<Order> Orders { get; set; }

		public DbSet<Optimization> Optimizations { get; set; }

		public DataModelContext() : base()
		{ }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			#region User
			// Make both username and id keys, with id primary.
			modelBuilder.Entity<User>().HasKey(u => new { ID=u.ID, u.Username });
			#endregion

			#region Network organization
			// Link
			modelBuilder.Entity<Link>()
				.HasRequired<Node>(l => l.From)
				.WithMany(n => n.OutLinks)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<Link>()
				.HasRequired<Node>(l => l.To)
				.WithMany(n => n.InLinks)
				.WillCascadeOnDelete(false);

			// Order
			modelBuilder.Entity<Order>()
				.HasRequired<Node>(o => o.Origin)
				.WithMany(n => n.OutOrders)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<Order>()
				.HasRequired<Node>(o => o.Destination)
				.WithMany(n => n.InOrders)
				.WillCascadeOnDelete(false);

			#endregion

			#region Optimization organization
			// Optimization
			modelBuilder.Entity<Optimization>()
				.HasRequired<Network>(o => o.OptimizedNetwork)
				.WithOptional(n => n.OptimizationResult)
				.WillCascadeOnDelete();

			#endregion
		}
	}
}
