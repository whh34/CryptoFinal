using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Security.Cryptography;

namespace MentalPoker
{
    class FastMentalPoker
    {
        // Mock's the setup protocol
        public static void MockSetup()
        {
            DateTime start = DateTime.Now;
            Console.WriteLine("Starting the mock setup. " + start);

            // Select a large prime
            byte[] chunk = new byte[3];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(chunk);
            BigInteger field = new BigInteger(chunk);

            if (field < 0)
                field = -field;

            while (!Utilities.IsPrime(field))
                field++;

            DateTime end1 = DateTime.Now;
            Console.WriteLine("Selected a large prime. " + (end1.Subtract(start)).TotalMilliseconds + "ms");

            // Find 52 distinct primitive roots
            List<BigInteger> generators = Utilities.GetPrimitiveRoots(field, 52);

            DateTime end2 = DateTime.Now;
            Console.WriteLine("Got 52 primitive roots in the field. " + (end2.Subtract(end1)).TotalMilliseconds + "ms");

            Console.WriteLine("Done.");
            
        }
    }
}
