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

    public virtual DbSet<AboutU> AboutUs { get; set; }

    public virtual DbSet<Announcement> Announcements { get; set; }

    public virtual DbSet<ContactU> ContactUs { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseLesson> CourseLessons { get; set; }

    public virtual DbSet<CourseRegistration> CourseRegistrations { get; set; }

    public virtual DbSet<District> Districts { get; set; }

    public virtual DbSet<Education> Educations { get; set; }

    public virtual DbSet<Governorate> Governorates { get; set; }

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<News> News { get; set; }

    public virtual DbSet<Partner> Partners { get; set; }

    public virtual DbSet<Residence> Residences { get; set; }

    public virtual DbSet<SliderItem> SliderItems { get; set; }

    public virtual DbSet<SocialMediaLink> SocialMediaLinks { get; set; }

    public virtual DbSet<TeamMember> TeamMembers { get; set; }

    public virtual DbSet<TrainingProgram> TrainingPrograms { get; set; }

    public virtual DbSet<TrainingProgramRegistration> TrainingProgramRegistrations { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserCourseSubscription> UserCourseSubscriptions { get; set; }

    public virtual DbSet<Village> Villages { get; set; }

    public virtual DbSet<Workshop> Workshops { get; set; }

    public virtual DbSet<WorkshopRegistration> WorkshopRegistrations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-5U44ISQ;Database=EcoramaDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AboutU>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AboutUs__3214EC079A402192");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("ImageURL");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Announce__3214EC07CC41CDF5");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<ContactU>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ContactU__3214EC0778275091");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Subject).HasMaxLength(150);
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Courses__3214EC07A584E77D");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PdfUrl).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<CourseLesson>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CourseLe__3214EC077A08F984");

            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Course).WithMany(p => p.CourseLessons)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__CourseLes__Cours__7D439ABD");
        });

        modelBuilder.Entity<CourseRegistration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CourseRe__3214EC07E96F2233");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.RegisteredAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseRegistrations)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__CourseReg__Cours__787EE5A0");

            entity.HasOne(d => d.User).WithMany(p => p.CourseRegistrations)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__CourseReg__UserI__797309D9");
        });

        modelBuilder.Entity<District>(entity =>
        {
            entity.HasKey(e => e.DistrictId).HasName("PK__District__85FDA4C66D0EE7A7");

            entity.HasIndex(e => new { e.GovernorateId, e.Name }, "UQ_District_Governorate").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.Governorate).WithMany(p => p.Districts)
                .HasForeignKey(d => d.GovernorateId)
                .HasConstraintName("FK_Districts_Governorates");
        });

        modelBuilder.Entity<Education>(entity =>
        {
            entity.HasKey(e => e.EducationId).HasName("PK__Educatio__4BBE3805768876EA");

            entity.ToTable("Education");

            entity.Property(e => e.EducationLevel).HasMaxLength(100);
            entity.Property(e => e.ProgramType).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.Educations)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Education_Users");
        });

        modelBuilder.Entity<Governorate>(entity =>
        {
            entity.HasKey(e => e.GovernorateId).HasName("PK__Governor__D314AD9A2E980C72");

            entity.HasIndex(e => e.Name, "UQ__Governor__737584F6C73E2ED3").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(e => e.LanguageId).HasName("PK__Language__B93855AB9628052B");

            entity.Property(e => e.CustomLanguageName).HasMaxLength(100);
            entity.Property(e => e.LanguageName).HasMaxLength(100);
            entity.Property(e => e.ProficiencyLevel).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.Languages)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Languages_Users");
        });

        modelBuilder.Entity<News>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__News__3214EC07C91BC7C5");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<Partner>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Partners__3214EC07494A9101");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Residence>(entity =>
        {
            entity.HasKey(e => e.ResidenceId).HasName("PK__Residenc__FA66BEB25FFA3818");

            entity.Property(e => e.District).HasMaxLength(100);
            entity.Property(e => e.Governorate).HasMaxLength(100);
            entity.Property(e => e.IsCustomVillage).HasDefaultValue(false);
            entity.Property(e => e.Village).HasMaxLength(100);

            entity.HasOne(d => d.DistrictNavigation).WithMany(p => p.Residences)
                .HasForeignKey(d => d.DistrictId)
                .HasConstraintName("FK_Residences_District");

            entity.HasOne(d => d.GovernorateNavigation).WithMany(p => p.Residences)
                .HasForeignKey(d => d.GovernorateId)
                .HasConstraintName("FK_Residences_Governorate");

            entity.HasOne(d => d.User).WithMany(p => p.Residences)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Residences_Users");

            entity.HasOne(d => d.VillageNavigation).WithMany(p => p.Residences)
                .HasForeignKey(d => d.VillageId)
                .HasConstraintName("FK_Residences_Village");
        });

        modelBuilder.Entity<SliderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SliderIt__3214EC07BD532269");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ImageFilePath).HasMaxLength(500);
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Title).HasMaxLength(100);
        });

        modelBuilder.Entity<SocialMediaLink>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SocialMe__3214EC072C91EBCA");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IconClass).HasMaxLength(100);
            entity.Property(e => e.IconColor).HasMaxLength(7);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Url).HasMaxLength(500);
        });

        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TeamMemb__3214EC078785F779");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.GitHubLink).HasMaxLength(255);
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.LinkedInLink).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Role).HasMaxLength(100);
        });

        modelBuilder.Entity<TrainingProgram>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Training__3214EC07EE6B3F39");

            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<TrainingProgramRegistration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Training__3214EC07C942C4D0");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.ExperienceLevel).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.RegisteredAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.TrainingProgram).WithMany(p => p.TrainingProgramRegistrations)
                .HasForeignKey(d => d.TrainingProgramId)
                .HasConstraintName("FK__TrainingP__Train__70DDC3D8");

            entity.HasOne(d => d.User).WithMany(p => p.TrainingProgramRegistrations)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__TrainingP__UserI__71D1E811");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC077477A910");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534C982D159").IsUnique();

            entity.HasIndex(e => e.NationalId, "UQ__Users__E9AA32FAC939EE34").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.MiddleName).HasMaxLength(50);
            entity.Property(e => e.NationalId).HasMaxLength(20);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.ProfileImagePath).HasMaxLength(255);
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValue("User");
        });

        modelBuilder.Entity<UserCourseSubscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserCour__3214EC0735D3C5FF");

            entity.Property(e => e.SubscribedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Course).WithMany(p => p.UserCourseSubscriptions)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__UserCours__Cours__0B91BA14");

            entity.HasOne(d => d.TrainingProgram).WithMany(p => p.UserCourseSubscriptions)
                .HasForeignKey(d => d.TrainingProgramId)
                .HasConstraintName("FK__UserCours__Train__0C85DE4D");

            entity.HasOne(d => d.User).WithMany(p => p.UserCourseSubscriptions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserCours__UserI__0A9D95DB");

            entity.HasOne(d => d.Workshop).WithMany(p => p.UserCourseSubscriptions)
                .HasForeignKey(d => d.WorkshopId)
                .HasConstraintName("FK__UserCours__Works__0D7A0286");
        });

        modelBuilder.Entity<Village>(entity =>
        {
            entity.HasKey(e => e.VillageId).HasName("PK__Villages__1A7F53982642C31E");

            entity.HasIndex(e => new { e.DistrictId, e.Name }, "UQ_Village_District").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.District).WithMany(p => p.Villages)
                .HasForeignKey(d => d.DistrictId)
                .HasConstraintName("FK_Villages_Districts");
        });

        modelBuilder.Entity<Workshop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Workshop__3214EC0753531171");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<WorkshopRegistration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Workshop__3214EC0730504D1C");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.Organization).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.RegisteredAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.WorkshopRegistrations)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__WorkshopR__UserI__6B24EA82");

            entity.HasOne(d => d.Workshop).WithMany(p => p.WorkshopRegistrations)
                .HasForeignKey(d => d.WorkshopId)
                .HasConstraintName("FK__WorkshopR__Works__6A30C649");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
