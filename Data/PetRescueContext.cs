using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace PetRescue.Data
{
    public class PetRescueContext : IdentityDbContext<Models.User>
    {
        public PetRescueContext(DbContextOptions<PetRescueContext> options)
            : base(options)
        {
        }

        public DbSet<Models.Client> Clients { get; set; } = default!;
        public DbSet<Models.Pet> Pets { get; set; } = default!;
        public DbSet<Models.Foster> Fosters { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(ent => ent.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.SetNull;
            }
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties(typeof(Enum)).HaveConversion<string>().HaveColumnType("TEXT");
        }
    }
}
