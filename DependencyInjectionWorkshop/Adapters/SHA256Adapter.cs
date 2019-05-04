using System.Security.Cryptography;
using System.Text;

namespace DependencyInjectionWorkshop.Adapters
{
    public class SHA256Adapter
    {
        public string GetHashedPassword(string password)
        {
            string hashedPassword;
            using (var crypt = new SHA256Managed())
            {
                var builder = new StringBuilder();
                var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
                foreach (var theByte in crypto)
                {
                    builder.Append(theByte.ToString("x2"));
                }

                hashedPassword = builder.ToString();
            }

            return hashedPassword;
        }
    }
}