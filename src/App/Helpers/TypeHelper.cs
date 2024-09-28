using System.Numerics;
using Google.Protobuf;

namespace App.Helpers;

// Classe Helper para fazer a conversão de tipos entre Protobuff e tipos do C#
public static class TypeHelper
{
    public static BigInteger ConvertByteStringToBigInteger(ByteString byteString)
    {
        byte[] bytes = byteString.ToArray();
        // return new BigInteger(bytes, isUnsigned: true, isBigEndian: true) % 16; //TODO: Descomentar para simular igual ao video de apresentação
        return new BigInteger(bytes, isUnsigned: true, isBigEndian: true);
    }
}
