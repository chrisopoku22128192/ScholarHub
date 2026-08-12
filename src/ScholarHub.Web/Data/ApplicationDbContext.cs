using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ScholarHub.Web.Models;

namespace ScholarHub.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Resource> Resources => Set<Resource>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Resource>(entity =>
        {
            entity.HasIndex(r => r.Status);
            entity.HasIndex(r => r.CourseCode);
            entity.HasIndex(r => r.Department);

            entity.HasOne(r => r.UploadedBy)
                .WithMany()
                .HasForeignKey(r => r.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
