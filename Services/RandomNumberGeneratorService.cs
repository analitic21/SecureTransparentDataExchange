using SecureTransparentDataExchange.Models;
using System;
using System.Security.Cryptography;

namespace SecureTransparentDataExchange.Services
{
    public class RandomNumberGeneratorService
    {
        // Generate random bytes
        public byte[] GenerateRandomBytes(int size)
        {
            byte[] randomBytes = new byte[size]; // Create an array of the desired size

            using (var rng = RandomNumberGenerator.Create()) // Use the correct way to create a random number generator
            {
                rng.GetBytes(randomBytes); // Fill the array with random bytes
            }

            return randomBytes; // Return random bytes
        }

        // Generate a token
        public string GenerateToken(int size = 32)
        {
            var randomBytes = GenerateRandomBytes(size); // Generate random bytes
            return Convert.ToBase64String(randomBytes); // Convert bytes to a string
        }

        // Method that returns the result as a model
        public RandomNumberGeneratorModel GenerateRandomData(int size = 32)
        {
            var model = new RandomNumberGeneratorModel
            {
                Size = size,
                Token = GenerateToken(size), // Generate a token
                RandomBytes = Convert.ToBase64String(GenerateRandomBytes(size)) // Generate random bytes as a string
            };

            return model; // Return the model with the results
        }
    }
}