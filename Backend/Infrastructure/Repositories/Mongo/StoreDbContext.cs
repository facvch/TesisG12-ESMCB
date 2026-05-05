using Core.Infraestructure.Repositories.MongoDb;
using MongoDB.Driver;

namespace Infrastructure.Repositories.Mongo
{
    /// <summary>
    /// Contexto de almacenamiento en base de datos. Aca se definen los nombres de 
    /// las colecciones, y los mapeos entre los objetos
    /// </summary>
    internal class StoreDbContext : DbContext
    {
        public StoreDbContext(string connectionString) : base(connectionString)
        {
        }

        public override IMongoCollection<T> GetCollection<T>()
        {
            return null;
        }

        private static void MapTypes()
        {
        }
    }
}
