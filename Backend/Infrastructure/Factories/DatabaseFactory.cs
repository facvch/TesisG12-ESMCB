using Application.Repositories;
using Domain.Others.Utils;
using Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using static Domain.Enums.Enums;

namespace Infrastructure.Factories
{
    internal static class DatabaseFactory
    {
        public static void CreateDataBase(this IServiceCollection services, string dbType, IConfiguration configuration)
        {
            switch (dbType.ToEnum<DatabaseType>())
            {
                case DatabaseType.MYSQL:
                case DatabaseType.MARIADB:
                case DatabaseType.SQLSERVER:
                    services.AddSqlServerRepositories(configuration);
                    break;
                case DatabaseType.SQLITE:
                    services.AddSqliteRepositories(configuration);
                    break;
                case DatabaseType.MONGODB:
                    services.AddMongoDbRepositories(configuration);
                    break;
                default:
                    throw new NotSupportedException(InfrastructureConstants.DATABASE_TYPE_NOT_SUPPORTED);
            }
        }

        private static IServiceCollection AddSqlServerRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<Repositories.Sql.StoreDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("SqlConnection"));
            }, ServiceLifetime.Scoped);

            //Habilitar para trabajar con Migrations
            var context = services.BuildServiceProvider().GetRequiredService<Repositories.Sql.StoreDbContext>();
            context.Database.Migrate();

            RegisterSqlRepositories(services);

            return services;
        }

        private static IServiceCollection AddSqliteRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<Repositories.Sql.StoreDbContext>(options =>
            {
                options.UseSqlite(configuration.GetConnectionString("SqliteConnection"));
            }, ServiceLifetime.Scoped);

            // Crear base de datos si no existe
            var context = services.BuildServiceProvider().GetRequiredService<Repositories.Sql.StoreDbContext>();
            context.Database.EnsureCreated();

            RegisterSqlRepositories(services);

            return services;
        }

        private static void RegisterSqlRepositories(IServiceCollection services)
        {
            // Legacy repositories
            services.AddTransient<IDummyEntityRepository, Repositories.Sql.DummyEntityRepository>();
            services.AddTransient<IAlumnoRepository, Repositories.Sql.AlumnoRepository>();

            // Veterinary system repositories
            services.AddTransient<IEspecieRepository, Repositories.Sql.EspecieRepository>();
            services.AddTransient<IRazaRepository, Repositories.Sql.RazaRepository>();
            services.AddTransient<IPropietarioRepository, Repositories.Sql.PropietarioRepository>();
            services.AddTransient<IPacienteRepository, Repositories.Sql.PacienteRepository>();
        }

        private static IServiceCollection AddMongoDbRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            ConventionRegistry.Register("Camel Case", new ConventionPack { new CamelCaseElementNameConvention() }, _ => true);

            Repositories.Mongo.StoreDbContext db = new(configuration.GetConnectionString("MongoConnection") ?? throw new NullReferenceException());
            services.AddSingleton(typeof(Repositories.Mongo.StoreDbContext), db);

            /* MongoDb Repositories */
            services.AddTransient<IDummyEntityRepository, Repositories.Mongo.DummyEntityRepository>();

            return services;
        }
    }
}

