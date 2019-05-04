using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Repositories
{
    public class ProfileRepository : IProfile
    {
        public string GetPassword(string account)
        {
            string profilePassword;
            var connectionString = "my connection string";
            var spName = "spGetUserPassword";
            using (var connection = new SqlConnection(connectionString))
            {
                profilePassword = connection.Query<string>(spName,
                    new { Id = account },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return profilePassword;
        }
    }
}