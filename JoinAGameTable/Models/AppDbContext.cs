using Microsoft.EntityFrameworkCore;

namespace JoinAGameTable.Models
{
    /// <summary>
    /// Application database context.
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Build a new instance with injected value.
        /// </summary>
        /// <param name="options">Database context options</param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Executes create, read, update, and delete operations on <see cref="UserAccountModel"/>.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public virtual DbSet<UserAccountModel> UserAccount { get; private set; }

        /// <summary>
        /// Executes create, read, update, and delete operations on <see cref="EventModel"/>.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public virtual DbSet<EventModel> Event { get; private set; }

        /// <summary>
        /// Executes create, read, update, and delete operations on <see cref="GameTableModel"/>.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public virtual DbSet<GameTableModel> GameTable { get; private set; }

        /// <summary>
        /// Executes create, read, update, and delete operations on <see cref="GameTableMetaDataModel"/>.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public virtual DbSet<GameTableMetaDataModel> GameTableMetaData { get; private set; }

        /// <summary>
        /// Executes create, read, update, and delete operations on <see cref="FileMetaDataModel"/>.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public virtual DbSet<FileMetaDataModel> FileMetaData { get; private set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAccountModel>()
                .ToTable("account")
                .HasIndex(record => record.Email)
                .IsUnique();

            modelBuilder.Entity<EventModel>()
                .ToTable("event");
            modelBuilder.Entity<EventModel>()
                .HasIndex("Id", "OwnerId");

            modelBuilder.Entity<GameTableModel>()
                .ToTable("game_table");
            modelBuilder.Entity<GameTableModel>()
                .HasIndex(record => record.Name)
                .IsUnique();

            modelBuilder.Entity<GameTableMetaDataModel>()
                .ToTable("game_table_metadata");
            modelBuilder.Entity<GameTableMetaDataModel>()
                .HasIndex("GameTableId", "Key")
                .IsUnique();

            modelBuilder.Entity<FileMetaDataModel>()
                .ToTable("file_metadata");
        }
    }
}
