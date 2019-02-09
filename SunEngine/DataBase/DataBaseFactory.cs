using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using LinqToDB.Identity;

namespace SunEngine.DataBase
{
    public interface IDataBaseFactory : IConnectionFactory
    {
        DataBaseConnection CreateDb();
    }

    public class DataBaseFactory : IDataBaseFactory
    {
        private readonly string connectionString;
        private readonly IDataProvider dataProvider;
        private readonly SunEngineMappingSchema mappingSchema;

        public DataBaseFactory(string providerName, string connectionString, SunEngineMappingSchema mappingSchema)
        {
            DataConnection.GetDataProvider(providerName, connectionString);
            this.connectionString = connectionString;
            this.mappingSchema = mappingSchema;
        }
        
        public DataBaseFactory(IDataProvider dataProvider, string connectionString, SunEngineMappingSchema mappingSchema)
        {
            this.dataProvider = dataProvider;
            this.connectionString = connectionString;
            this.mappingSchema = mappingSchema;
        }

        public IDataContext GetContext()
        {
            return new DataContext(dataProvider, connectionString)
            {
                MappingSchema = mappingSchema
            };
        }

        public DataConnection GetConnection()
        {
            return new DataBaseConnection(dataProvider,connectionString,mappingSchema);
        }

        public DataBaseConnection CreateDb()
        {
            return new DataBaseConnection(dataProvider,connectionString,mappingSchema);
        }
    }
}