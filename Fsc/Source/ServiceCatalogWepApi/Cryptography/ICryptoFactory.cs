﻿using System;
using System.Security.Cryptography;

namespace ServiceCatalog.WepApi.Cryptography
{
    public interface ICryptoFactory
    {
        ICryptoAlgorithm Create<T>(string password, string salt) where T : SymmetricAlgorithm, new();
        ICryptoAlgorithm Create(Type type, string password, string salt);
    }
}