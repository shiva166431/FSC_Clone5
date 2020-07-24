using ServiceCatalog.BL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ServiceCatalog.BL.Models;

namespace Encrypt
{
    class Program
    {
        public static void Execute(string choice)
        {
            var digest = new HmacDigest();
            byte[] inputBuffer = new byte[1024];
            Stream inputStream = Console.OpenStandardInput(inputBuffer.Length);
            Console.SetIn(new StreamReader(inputStream, Console.InputEncoding, false, inputBuffer.Length));
            Console.SetBufferSize(128, 1024);
            if (choice == "1")
            {
                Console.WriteLine("Please enter text for Encrypt:");
                string inputText = Console.ReadLine();
                Console.WriteLine("\nEncryptKey:");
                CryptoAlgorithm settingsDecryptor = new CryptoAlgorithm("ENCRYPT", "FSCS", new AesManaged());
                var value = settingsDecryptor.Encrypt(inputText);
                Console.WriteLine(value);
            }
            else if (choice == "2")
            {
                Console.WriteLine("Public App Key: " + digest.PublicKey.ToString());
                Console.WriteLine("\nDigest: " + digest.HashedStringBase64);
                Console.WriteLine("\nEpoch Time: " + digest.EpochTime.ToString());
            }
            else if (choice == "3")
            {
                Console.WriteLine("Hit Enter to exit, Cya!!!");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Invalid Choice, Hit Enter to exit");
            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("--------FSC Dev Helper--------------" +
                "\n Choose 1 or 2 \n 1.Encrypt Connection String   2.Get Key & Digest  3.Exit!");
            var choice = Console.ReadLine();
            Start:
            if (choice != "3")
            {
                Execute(choice);
                Console.WriteLine("\nChoose 1 or 2 \n1.Encrypt Connection String  2.Get Key & Digest  3.Exit!");
                choice = Console.ReadLine();
                goto Start;
            }
            else
            {
                Console.WriteLine("Hit Enter to exit, Cya!!!");
            }
            Console.ReadLine();
        }
    }
}