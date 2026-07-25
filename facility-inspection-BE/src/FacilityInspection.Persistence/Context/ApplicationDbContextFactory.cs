using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FacilityInspection.Persistence.Context;

/// <summary>
/// Design-time factory used by the EF Core tools (<c>dotnet ef</c>) to build the
/// context without starting the API host. The connection string is only used for
/// commands that touch the database (e.g. <c>database update</c>); <c>migrations add</c>
/// generates SQL from the model and does not connect.
/// <para>
/// Override via the <c>FACILITYINSPECTION_DB</c> environment variable when needed.
/// </para>
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    private const string DefaultDesignTimeConnection =
        "Server=localhost;Database=FacilityInspection;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";

    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("FACILITYINSPECTION_DB")
            ?? DefaultDesignTimeConnection;

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString, sql =>
                sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
            .Options;

        return new ApplicationDbContext(options);
    }
}
