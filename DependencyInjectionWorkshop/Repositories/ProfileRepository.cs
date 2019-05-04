using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Repositories
{
    public class ProfileRepository
    {
        public string GetPasswordFromDb(string account)
        {
            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword",
                        new { Account = account },
                        commandType: CommandType.StoredProcedure)
                    .SingleOrDefault();
            }

            return passwordFromDb;
        }
    }
}