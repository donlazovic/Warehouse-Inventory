using System.Security.Cryptography;
using System.Text;

namespace Warehouse.BusinessLayer.Common;

public static class TokenHasher
{
    public static string Sha256(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
