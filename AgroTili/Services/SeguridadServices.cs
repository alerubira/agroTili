using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace AgroTili.Services
{
    public class SeguridadService
    {
     private readonly string globalSalt;

        public SeguridadService()
        {
            globalSalt = Environment.GetEnvironmentVariable("GLOBAL_SALT") 
                          ?? throw new Exception("GLOBAL_SALT no está configurada");
        }


        public string HashearContraseña(string contraseña)
        {
            // Combinar el salt global con uno aleatorio por usuario
            byte[] saltBytes = System.Text.Encoding.UTF8.GetBytes(globalSalt);

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: contraseña,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return hashed;
        }

        public bool VerificarContraseña(string contraseñaIngresada, string hashGuardado)
        {
            string hashIngresado = HashearContraseña(contraseñaIngresada);
            return hashGuardado == hashIngresado;
        }
    }
}