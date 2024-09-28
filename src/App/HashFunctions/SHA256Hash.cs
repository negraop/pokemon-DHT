using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace src.HashFunctions;

public static class SHA256Hash
{
    public static BigInteger GenerateHashBigInteger(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        BigInteger bigInt = new BigInteger(hashBytes, isUnsigned: true, isBigEndian: true);
        // return bigInt % 16; //TODO: Descomentar para simular igual ao video de apresentação
        return bigInt;
    }

    public static byte[] GenerateHashBytes(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        return hashBytes;
    }
}
