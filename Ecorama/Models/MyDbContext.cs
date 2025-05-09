using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Ecorama.Models;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }




    public DbSet<SocialMediaLink> SocialMediaLinks { get; set; }

    public virtual DbSet<Announcement> Announcements { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseLesson> CourseLessons { get; set; }

    public virtual DbSet<CourseRegistration> CourseRegistrations { get; set; }

    public virtual DbSet<News> News { get; set; }

    public virtual DbSet<Partner> Partners { get; set; }

    public virtual DbSet<SliderItem> SliderItems { get; set; }

    public virtual DbSet<TeamMember> TeamMembers { get; set; }

    public virtual DbSet<TrainingProgram> TrainingPrograms { get; set; }

    public virtual DbSet<TrainingProgramRegistration> TrainingProgramRegistrations { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserCourseSubscription> UserCourseSubscriptions { get; set; }

    public virtual DbSet<Workshop> Workshops { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-5U44ISQ;Database=EcoramaDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Announce__3214EC075B99E322");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Courses__3214EC07BBB5243E");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.PdfUrl).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<CourseLesson>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CourseLe__3214EC078E5065E5");

            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Course).WithMany(p => p.CourseLessons)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__CourseLes__Cours__6754599E");
        });

        modelBuilder.Entity<CourseRegistration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CourseRe__3214EC070994F41A");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.RegisteredAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseRegistrations)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__CourseReg__Cours__628FA481");

            entity.HasOne(d => d.User).WithMany(p => p.CourseRegistrations)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__CourseReg__UserI__6383C8BA");
        });

        modelBuilder.Entity<News>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__News__3214EC07529309DE");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<Partner>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Partners__3214EC0751938E35");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<SliderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SliderIt__3214EC07EE374607");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ImageFilePath).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Title).HasMaxLength(100);
        });

        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TeamMemb__3214EC07BD3352F7");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.GitHubLink).HasMaxLength(255);
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.LinkedInLink).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Role).HasMaxLength(100);
        });

        modelBuilder.Entity<TrainingProgram>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Training__3214EC0797EA0B12");

            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<TrainingProgramRegistration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Training__3214EC07DEF1D759");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.ExperienceLevel).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.RegisteredAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.TrainingProgram).WithMany(p => p.TrainingProgramRegistrations)
                .HasForeignKey(d => d.TrainingProgramId)
                .HasConstraintName("FK__TrainingP__Train__5AEE82B9");

            entity.HasOne(d => d.User).WithMany(p => p.TrainingProgramRegistrations)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__TrainingP__UserI__5BE2A6F2");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC074A55BC80");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534B4E8CD06").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValue("User");
        });

        modelBuilder.Entity<UserCourseSubscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserCour__3214EC07F340B5B6");

            entity.Property(e => e.SubscribedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Course).WithMany(p => p.UserCourseSubscriptions)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__UserCours__Cours__71D1E811");

            entity.HasOne(d => d.TrainingProgram).WithMany(p => p.UserCourseSubscriptions)
                .HasForeignKey(d => d.TrainingProgramId)
                .HasConstraintName("FK__UserCours__Train__72C60C4A");

            entity.HasOne(d => d.User).WithMany(p => p.UserCourseSubscriptions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserCours__UserI__70DDC3D8");

            entity.HasOne(d => d.Workshop).WithMany(p => p.UserCourseSubscriptions)
                .HasForeignKey(d => d.WorkshopId)
                .HasConstraintName("FK__UserCours__Works__73BA3083");
        });

        modelBuilder.Entity<Workshop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Workshop__3214EC075B1E3EB7");

            entity.Property(e => e.Title).HasMaxLength(200);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
