using System.Text.Json;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<Option> Options => Set<Option>();
    public DbSet<UserStats> UserStats => Set<UserStats>();
    public DbSet<Warning> Warnings => Set<Warning>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureUsers(modelBuilder);
        ConfigureQuizzes(modelBuilder);
        ConfigureQuizQuestions(modelBuilder);
        ConfigureOptions(modelBuilder);
        ConfigureUserStats(modelBuilder);
        ConfigureWarnings(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(u => u.Password)
                .IsRequired();
            entity.Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(30)
                .HasDefaultValue("User");
            entity.Property(u => u.IsBanned)
                .HasDefaultValue(false);
            entity.Property(u => u.BanReason)
                .HasMaxLength(1000);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Username).IsUnique();
        });
    }

    private static void ConfigureQuizzes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(q => q.ID);
            entity.Property(q => q.Title)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(q => q.Description)
                .HasMaxLength(2000);
            entity.Property(q => q.TimesPlayed);

            entity.HasOne(q => q.Creator)
                .WithMany()
                .HasForeignKey(q => q.CreatorID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(q => q.Questions)
                .WithOne(q => q.Quiz)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureQuizQuestions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QuizQuestion>(entity =>
        {
            entity.HasKey(q => q.Id);
            entity.Property(q => q.QuestionText)
                .IsRequired();
            entity.Property(q => q.CorrectOptionIndex)
                .IsRequired();
            entity.Property(q => q.TimeLimit)
                .IsRequired();

            entity.HasMany(q => q.Options)
                .WithOne(q => q.QuizQuestion)
                .HasForeignKey(q => q.QuizQuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureOptions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Option>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.OptionText)
                .IsRequired();
        });
    }

    private static void ConfigureWarnings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Warning>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Message)
                .IsRequired()
                .HasMaxLength(1000);
            entity.Property(w => w.CreatedAt)
                .IsRequired();

            entity.HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(w => w.Admin)
                .WithMany()
                .HasForeignKey(w => w.AdminId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureUserStats(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserStats>(entity =>
        {
            entity.HasKey(s => s.UserId);
            entity.Property(s => s.GamesPlayed);
            entity.Property(s => s.GamesWon);
            entity.Property(s => s.QuizzesCreated);
            entity.Property(s => s.QuizPlays);

            entity.HasOne(s => s.User)
                .WithOne(u => u.Stats)
                .HasForeignKey<UserStats>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}