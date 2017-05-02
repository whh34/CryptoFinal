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
        public static List<BigInteger> cards;
        public static BigInteger G;

        public static double AvgSetupTime;
        public static double AvgShuffleTime;

        // Mock's the setup protocol
        public static void MockSetup(int primeSize, int tries)
        {
            //Console.WriteLine("Starting the mock setup. " + tries + " trials, 256 ^ " + primeSize + " field size");
            List<double> runtimes = new List<double>();

            for (int i = 0; i < tries; i++)
            {
                DateTime start = DateTime.Now;    

                // Select a large prime
                byte[] chunk = new byte[primeSize];
                var rng = RandomNumberGenerator.Create();
                rng.GetBytes(chunk);
                BigInteger field = new BigInteger(chunk);

                if (field < 0)
                    field = -field;

                while (!Utilities.IsPrime(field))
                    field++;

                G = field;

                DateTime end1 = DateTime.Now;
                double t = (end1.Subtract(start)).TotalMilliseconds;
                //Console.WriteLine("Selected a large prime. " + t + "ms");

                // Find 52 distinct primitive roots
                List<BigInteger> generators = Utilities.GetPrimitiveRoots(field, 52);
                cards = generators;

                DateTime end2 = DateTime.Now;
                t = (end2.Subtract(end1)).TotalMilliseconds;
                //Console.WriteLine("Got 52 primitive roots in the field. " + t + "ms");

                t = (end2.Subtract(start)).TotalMilliseconds;
                runtimes.Add(t);
            }

            double totalTime = 0;
            foreach (double t in runtimes)
                totalTime += t;

            totalTime /= runtimes.Count;
            AvgSetupTime = totalTime;

            //Console.WriteLine("Done. " + AvgSetupTime + "ms, average runtime.");
        }

        // Mocks the shuffle step that each player takes in the Fast Mental Poker paper
        public static void MockShuffle(int tries)
        {
            List<double> runtimes = new List<double>();

            for (int i = 0; i < tries; i++)
            {
                DateTime start = DateTime.Now;

                int size = (int)(BigInteger.Log(G) / BigInteger.Log(256));
                byte[] secret = new byte[size];
                var rng = RandomNumberGenerator.Create();
                rng.GetBytes(secret);

                // Get a random secret integer < G
                BigInteger x = new BigInteger(secret);

                if (x < 0)
                    x = -x;

                // Get a random permutation of the deck
                BigInteger[] perm = cards.ToArray();
                perm = Utilities.RandomPermutation(perm.Length, perm);

                // Now we encrypt each of the cards in the deck
                for (int j = 0; j < perm.Length; j++)
                    perm[j] = BigInteger.ModPow(perm[j], x, G);

                // Then we would in theory prove to the other players that our shuffle was legitimate...
                DateTime end = DateTime.Now;
                double t = end.Subtract(start).TotalMilliseconds;
                runtimes.Add(t);
            }

            double totalTime = 0;
            foreach (double t in runtimes)
                totalTime += t;

            totalTime /= runtimes.Count;
            AvgShuffleTime = totalTime;
        }

        // Runs both the mock setup and mock shuffle together
        public static void MockInitialization(int primeSize, int tries)
        {
            MockSetup(primeSize, tries);
            MockShuffle(tries);

            Console.WriteLine("Average setup time: " + AvgSetupTime + "ms");
            Console.WriteLine("Average shuffle time: " + AvgShuffleTime + "ms");
            Console.WriteLine("Total run time of initialization protocol: " + (AvgSetupTime + AvgShuffleTime) + "ms");
        }
    }
}
