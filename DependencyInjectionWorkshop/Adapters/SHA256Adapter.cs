using System.Security.Cryptography;
using System.Text;

namespace DependencyInjectionWorkshop.Adapters
{
    public class SHA256Adapter : IHash
    {
        public string GetHash(string password)
        {
            var crypt = new SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByes in crypto)
            {
                hash.Append(theByes.ToString("x2"));
            }

            var hashPassword = hash.ToString();
            return hashPassword;
        }
    }
}