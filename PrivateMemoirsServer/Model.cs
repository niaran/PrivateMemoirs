namespace PrivateMemoirs
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Model : DbContext
    {
        public Model(string constr)
            : base(constr)
        {
            Database.Connection.ConnectionString = constr;
        }

        public virtual DbSet<MEMOIRS> MEMOIRS { get; set; }
        public virtual DbSet<USERS> USERS { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MEMOIRS>()
                .Property(e => e.MEMOIR_TEXT)
                .IsUnicode(false);

            modelBuilder.Entity<MEMOIRS>()
                .Property(e => e.MEMOIR_TITLE)
                .IsUnicode(false);

            modelBuilder.Entity<USERS>()
                .Property(e => e.USER_HASH)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<USERS>()
                .HasMany(e => e.MEMOIRS)
                .WithRequired(e => e.USERS)
                .WillCascadeOnDelete(false);
        }
    }
}