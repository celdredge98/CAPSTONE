using CanTheSpam.Data.CanTheSpamRepository.Models;
using Microsoft.EntityFrameworkCore;

namespace CanTheSpam.Data.CanTheSpamRepository
{
   public partial class CanTheSpamContainer : DbContext
   {
      public CanTheSpamContainer()
      {
      }

      public CanTheSpamContainer(DbContextOptions<CanTheSpamContainer> options)
          : base(options)
      {
      }

      public virtual DbSet<AspNetRoleClaims> AspNetRoleClaims { get; set; }
      public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
      public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
      public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }
      public virtual DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
      public virtual DbSet<AspNetUserTokens> AspNetUserTokens { get; set; }
      public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
      public virtual DbSet<EmailList> EmailList { get; set; }
      public virtual DbSet<Log> Log { get; set; }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
         modelBuilder.HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

         modelBuilder.Entity<AspNetRoleClaims>(entity =>
         {
            entity.HasIndex(e => e.RoleId);

            entity.Property(e => e.RoleId).IsRequired();

            entity.HasOne(d => d.Role)
                .WithMany(p => p.AspNetRoleClaims)
                .HasForeignKey(d => d.RoleId);
         });

         modelBuilder.Entity<AspNetRoles>(entity =>
         {
            entity.HasIndex(e => e.NormalizedName)
                .HasName("RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Name).HasMaxLength(256);

            entity.Property(e => e.NormalizedName).HasMaxLength(256);
         });

         modelBuilder.Entity<AspNetUserClaims>(entity =>
         {
            entity.HasIndex(e => e.UserId);

            entity.Property(e => e.UserId).IsRequired();

            entity.HasOne(d => d.User)
                .WithMany(p => p.AspNetUserClaims)
                .HasForeignKey(d => d.UserId);
         });

         modelBuilder.Entity<AspNetUserLogins>(entity =>
         {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId);

            entity.Property(e => e.LoginProvider).HasMaxLength(128);

            entity.Property(e => e.ProviderKey).HasMaxLength(128);

            entity.Property(e => e.UserId).IsRequired();

            entity.HasOne(d => d.User)
                .WithMany(p => p.AspNetUserLogins)
                .HasForeignKey(d => d.UserId);
         });

         modelBuilder.Entity<AspNetUserRoles>(entity =>
         {
            entity.HasKey(e => new { e.UserId, e.RoleId });

            entity.HasIndex(e => e.RoleId);

            entity.HasOne(d => d.Role)
                .WithMany(p => p.AspNetUserRoles)
                .HasForeignKey(d => d.RoleId);

            entity.HasOne(d => d.User)
                .WithMany(p => p.AspNetUserRoles)
                .HasForeignKey(d => d.UserId);
         });

         modelBuilder.Entity<AspNetUserTokens>(entity =>
         {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);

            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User)
                .WithMany(p => p.AspNetUserTokens)
                .HasForeignKey(d => d.UserId);
         });

         modelBuilder.Entity<AspNetUsers>(entity =>
         {
            entity.HasIndex(e => e.NormalizedEmail)
                .HasName("EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName)
                .HasName("UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Email).HasMaxLength(256);

            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);

            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

            entity.Property(e => e.UserName).HasMaxLength(256);
         });

         modelBuilder.Entity<EmailList>(entity =>
         {
            entity.HasIndex(e => e.Email)
                .HasName("UX_EmailList_Email")
                .IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256)
                .IsUnicode(false);
         });

         modelBuilder.Entity<Log>(entity =>
         {
            entity.Property(e => e.Date).HasColumnType("datetime");

            entity.Property(e => e.Exception).IsUnicode(false);

            entity.Property(e => e.Level)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.Logger)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.Property(e => e.Message)
                .IsRequired()
                .IsUnicode(false);

            entity.Property(e => e.Thread)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);
         });
      }
   }
}