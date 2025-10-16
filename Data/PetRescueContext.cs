using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using PetRescue.Models;


namespace PetRescue.Data;

public class PetRescueContext : IdentityDbContext<User>
{
    public PetRescueContext(DbContextOptions<PetRescueContext> options)
        : base(options)
    {
        this.SavingChanges += BeforeSaving;
    }

    public DbSet<Client> Clients { get; set; } = default!;
    public DbSet<Pet> Pets { get; set; } = default!;
    public DbSet<Foster> Fosters { get; set; } = default!;

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

    private static void BeforeSaving(object? sender, SavingChangesEventArgs e)
    {
        var ctx = (PetRescueContext)sender!;
        var deletedClients = ctx.ChangeTracker.Entries().Where(c => c.Entity is Client && c.State == EntityState.Deleted).Select(c => (Client)c.Entity);
        foreach (var client in deletedClients)
        {
            var fosters = ctx.Fosters.Where(f => f.ClientID == client.Id);
            foreach (var foster in fosters)
            {
                foster.ClientID = 0;
            }
        }

        var deletedPets = ctx.ChangeTracker.Entries().Where(p => p.Entity is Pet && p.State == EntityState.Deleted).Select(p => (Pet)p.Entity);
        foreach (var pet in deletedPets)
        {
            var fosters = ctx.Fosters.Where(f => f.PetID == pet.Id);
            foreach (var foster in fosters)
            {
                foster.PetID = 0;
            }
        }
    }
};
