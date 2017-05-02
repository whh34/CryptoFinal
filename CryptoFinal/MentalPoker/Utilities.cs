using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalPoker
{
    class Utilities
    {
        // A mundane primality test for small numbers
        private static bool isPrime(int n)
        {
            if (n == 1)
                return false;
            else if (n < 4)
                return true;
            else if (n % 2 == 0)
                return false;
            else if (n < 9)
                return true;
            else if (n % 3 == 0)
                return false;
            else
            {
                int r = (int)(Math.Sqrt(n));
                int f = 5;

                while (f <= r)
                {
                    if (n % f == 0)
                        return false;
                    if (n % (f + 2) == 0)
                        return false;

                    f += 6;
                }

                return true;
            }
        }

        // Miller-Rabin test for the compositeness of a number
        private static bool IsComposite(BigInteger n, BigInteger a)
        {
            if (n.IsEven)
                return true;
            if (1 < BigInteger.GreatestCommonDivisor(n, a) && BigInteger.GreatestCommonDivisor(n, a) < n)
                return true;

            BigInteger q = n - 1;
            BigInteger k = 0;
            while (q.IsEven)
            {
                q = q / 2;
                k++;
            }

            a = BigInteger.ModPow(a, q, n);

            if (a % n == 1)
                return false;

            for (BigInteger i = 0; i < k; i++)
            {
                if (a % n == n - 1)
                    return false;
                a = BigInteger.ModPow(a, 2, n);
            }

            return true;
        }

        // Probabilistic test for if an arbitrarily large number is prime
        //      Has about a 10^-60 chance of incorrectly identifying a number as prime
        public static bool IsPrime(BigInteger n)
        {
            var rng = RandomNumberGenerator.Create();
            bool primality = true;
            int size = (int)(BigInteger.Log(n) / BigInteger.Log(256));

            // If the number is too small, just do a more simple prime test
            if (size < 3)
                return isPrime((int)n);

            byte[] randomWitness = new byte[size - 2];

            for (int i = 0; i < 100; i++)
            {
                rng.GetBytes(randomWitness);
                BigInteger a = new BigInteger(randomWitness);
                if (a < 0)
                    a = -a;

                primality &= !IsComposite(n, a);

                if (!primality)
                    return false;
            }

            return primality;
        }

        // The Extended GCD algorithm to find the inverses of numbers in a field
        // Returns (x, y, r) such that:
        //      x is the multiplicative inverse of a in the field of b
        //      y is the multiplicative inverse of b in the field of a
        //      r is the GCD of a and b
        public static Tuple<BigInteger, BigInteger, BigInteger> ExtendedGCD(BigInteger a, BigInteger b)
        {
            BigInteger s = 0;
            BigInteger t = 1;
            BigInteger r = b;

            BigInteger old_s = 1;
            BigInteger old_t = 0;
            BigInteger old_r = a;

            while (r != 0)
            {
                BigInteger quotient = old_r / r;

                BigInteger prov = r;
                r = old_r - (quotient * prov);
                old_r = prov;

                prov = s;
                s = old_s - (quotient * s);
                old_s = prov;

                prov = t;
                t = old_t - (quotient * t);
                old_t = prov;
            }

            return new Tuple<BigInteger, BigInteger, BigInteger>(old_s, old_t, old_r);
        }

        // Knuth shuffle for a random permutation
        //      Generates a random permutation of n elements within the input sequence
        //      (So typically n should be the length of the sequence)
        public static BigInteger[] RandomPermutation(int n, BigInteger[] sequence)
        {
            Random rand = new Random();

            for (int i = 0; i < n - 1; i++)
            {
                int j = rand.Next(n - i);
                BigInteger t = sequence[i];
                sequence[i] = sequence[i + j];
                sequence[i + j] = t;
            }

            return sequence;
        }

        // A really not optimized PrimeFactorization algorithm, that also really only works with small numbers
        //      Returns a list of Tuples that are in the format <Prime factor, exponent>
        public static List<Tuple<BigInteger, BigInteger>> PrimeFactorization(BigInteger n)
        {
            List<Tuple<BigInteger, BigInteger>> factors = new List<Tuple<BigInteger, BigInteger>>();

            if (IsPrime(n))
            {
                factors.Add(new Tuple<BigInteger, BigInteger>(n, 1));
                return factors;
            }
            else
            {
                BigInteger rootN = BigInteger.Pow(10, (int)(BigInteger.Log10(n) / 2) + 1);

                for (BigInteger i = 2; i < rootN; i++)
                {
                    if (!IsPrime(i))
                        continue;

                    BigInteger c = 0;
                    while (n % i == 0)
                    {
                        n /= i;
                        c++;
                    }

                    if (c > 0)
                        factors.Add(new Tuple<BigInteger, BigInteger>(i, c));

                    if (n == 1)
                        break;
                }

                return factors;
            }
        }

        // Retrieves a number of primitives roots in a given field
        public static List<BigInteger> GetPrimitiveRoots(BigInteger field, BigInteger number)
        {
            List<BigInteger> roots = new List<BigInteger>();

            List<Tuple<BigInteger, BigInteger>> factorization = PrimeFactorization(field - 1);

            bool flag = true;
            for (BigInteger g = 2; g < field; g++)
            {
                for (int i = 0; i < factorization.Count; i++)
                {
                    flag &= (BigInteger.ModPow(g, ((field - 1) / factorization[i].Item1), field) != 1);
                    if (!flag)
                        break;
                }

                if (!flag)
                {
                    flag = true;
                    continue;
                }
                else
                {
                    roots.Add(g);
                    number--;

                    if (number <= 0)
                        return roots;
                }
            }

            return roots;
        }

        // Picks a prime field, and corresponding generator to create a bit commitment for a secret number
        //      Returns a tuple that contains <The prime field, the generator for the field, and the commitment number>
        //      The commitment number, c, is the solution to the following equation: (g ^ secret) mod prime = c
        public static Tuple<BigInteger, BigInteger, BigInteger> BitCommit(BigInteger secret)
        {
            var rng = RandomNumberGenerator.Create();
            byte[] hope = new byte[4];
            rng.GetBytes(hope);

            BigInteger prime = new BigInteger(hope);

            if (prime < 0)
                prime = -prime;

            while (!IsPrime(prime))
                prime++;

            BigInteger generator = GetPrimitiveRoots(prime, 1)[0];
            BigInteger c = BigInteger.ModPow(generator, (int)secret, prime);

            return new Tuple<BigInteger, BigInteger, BigInteger>(prime, generator, c);
        }
    }

    // Class that represents the card matrix in Practical Mental Poker
    public class CardMatrix
    {
        public BigInteger Zprime;
        public BigInteger[,] Matrix;

        public CardMatrix(BigInteger prime)
        {
            Zprime = prime;

            // Fail safe to ensure that the prime chosen doesn't mess up the card representation
            if (Zprime < 53)
                Zprime = 53;

            Matrix = buildMatrix();
        }

        // Initializes a new card matrix by picking a random shuffle of the deck and filling in all of the elements of the matrix
        private BigInteger[,] buildMatrix()
        {
            BigInteger[,] permutationMatrix = new BigInteger[52, 52];
            BigInteger[] cards = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13,
                            14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26,
                            27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
                            40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52 };

            // Select a random permutation
            BigInteger[] chosenPerm = Utilities.RandomPermutation(52, cards);
            Random rand = new Random();

            // Assign a card to a random slot in each row of the matrix
            for (int i = 0; i < chosenPerm.Length; i++)
            {
                int index = rand.Next(permutationMatrix.GetLength(1));
                permutationMatrix[i, index] = chosenPerm[i];
            }

            // Now add a random amount of the chosen prime to each position in the matrix
            for (int i = 0; i < permutationMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < permutationMatrix.GetLength(1); j++)
                    permutationMatrix[i, j] += Zprime * rand.Next((int)Zprime);
            }

            return permutationMatrix;
        }

        // Retrieves the card from an input row
        public BigInteger GetCard(int index)
        {
            for (int i = 0; i < Matrix.GetLength(1); i++)
            {
                BigInteger card = (Matrix[index, i] % Zprime);
                if (card != 0)
                    return card;
            }

            return 0;
        }

        // Converts a CardMatrix to now use a different Zprime value
        public void Transform(int newPrime)
        {
            // Clear the matrix
            for (int i = 0; i < Matrix.GetLength(0); i++)
            {
                for (int j = 0; j < Matrix.GetLength(1); j++)
                    Matrix[i, j] %= Zprime;
            }

            Zprime = newPrime;
            Random rand = new Random();

            for (int i = 0; i < Matrix.GetLength(0); i++)
            {
                for (int j = 0; j < Matrix.GetLength(1); j++)
                    Matrix[i, j] += Zprime * rand.Next((int)Zprime);
            }
        }

        public static bool operator==(CardMatrix a, CardMatrix b)
        {
            for (int i = 0; i < a.Matrix.GetLength(0); i++)
            {
                if (a.GetCard(i) != b.GetCard(i))
                    return false;
            }

            return true;
        }

        public static bool operator!=(CardMatrix a, CardMatrix b)
        {
            return !(a == b);
        }
    }

    // A simple homomorphic encryptor
    public class HomomorphicEncryptor
    {
        public int d = 3;
        public BigInteger m;

        private BigInteger r;
        private BigInteger m_;
        private BigInteger rinverse;
        
        public HomomorphicEncryptor()
        {
            constructM();
            constructR(m);
        }

        // Generates M and M'
        //      M > 10 ^ 100
        //      M' ~ 256 ^ 6
        private void constructM()
        {
            BigInteger upperBound = BigInteger.Pow(10, 200);
            BigInteger lowerBound = BigInteger.Pow(10, 100);

            BigInteger m = 1;
            BigInteger[] factors = new BigInteger[3];

            byte[] multiplier = new byte[2];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(multiplier);

            while (m < lowerBound && m < upperBound)
            {
                BigInteger temp = new BigInteger(multiplier);

                m *= temp;
                factors[2] = factors[1];
                factors[1] = factors[0];
                factors[0] = temp;

                rng.GetBytes(multiplier);
            }

            this.m = m;
            this.m_ = factors[0] * factors[1] * factors[2];
        }

        // Chooses a number R such that R has an inverse
        //      (And it finds said inverse)
        private void constructR(BigInteger m)
        {
            var rng = RandomNumberGenerator.Create();
            byte[] randomR = new byte[5];
            rng.GetBytes(randomR);

            BigInteger r = new BigInteger(randomR);
            Tuple<BigInteger, BigInteger, BigInteger> gcdResult = Utilities.ExtendedGCD(r, m);

            while (gcdResult.Item3 != 1)
            {
                rng.GetBytes(randomR);
                r = new BigInteger(randomR);
                gcdResult = Utilities.ExtendedGCD(r, m);
            }

            // Now we have found an r that has an inverse mod m
            BigInteger rinverse = gcdResult.Item1;

            // Verify that rinverse is actually the inverse...
            BigInteger result = (r * rinverse) % m;
            if (result != 1 && result != -m + 1)
                Console.WriteLine("Finding the inverse failed, " + r + " * " + rinverse + " % " + m + " = " + result);
            else
            {
                this.r = r;
                this.rinverse = rinverse;
            }
        }

        // Randomly splits the input number into 3 chunks
        private List<BigInteger> split(BigInteger a)
        {
            List<BigInteger> toReturn = new List<BigInteger>();
            var rng = RandomNumberGenerator.Create();
            int size = (int)(BigInteger.Log(a) / BigInteger.Log(256));

            if (size <= 1)
                return tinySplit(a);

            byte[] randomChunk = new byte[size - 1];

            // split the number into 3 random pieces
            rng.GetBytes(randomChunk);
            BigInteger i = new BigInteger(randomChunk);
            size = (int)(BigInteger.Log(a - i) / BigInteger.Log(256));
            randomChunk = new byte[size];
            rng.GetBytes(randomChunk);
            BigInteger j = new BigInteger(randomChunk);
            BigInteger k = a - i - j;

            // Add a random number of m_'s to each number
            byte[] multiplier = new byte[1];
            rng.GetBytes(multiplier);
            i += new BigInteger(multiplier) * m_;
            rng.GetBytes(multiplier);
            j += new BigInteger(multiplier) * m_;
            rng.GetBytes(multiplier);
            k += new BigInteger(multiplier) * m_;

            toReturn.Add(i);
            toReturn.Add(j);
            toReturn.Add(k);

            return toReturn;
        }

        // Necessary when the encrypted value is very small, a < 256^2
        private List<BigInteger> tinySplit(BigInteger a)
        {
            List<BigInteger> toReturn = new List<BigInteger>();

            Random rand = new Random();
            BigInteger randomChunk = rand.Next((int)a);

            // Split into three pieces
            BigInteger i = a - randomChunk;
            randomChunk = rand.Next((int)(a - i));

            BigInteger j = a - i - randomChunk;

            BigInteger k = a - i - j;

            // tack on random amounts of m_
            int multiplier = rand.Next(256);
            i += m_ * multiplier;
            multiplier = rand.Next(256);
            j += m_ * multiplier;
            multiplier = rand.Next(256);
            k += m_ * multiplier;

            toReturn.Add(i);
            toReturn.Add(j);
            toReturn.Add(k);

            return toReturn;
        }

        public List<BigInteger> Encrypt(BigInteger a)
        {
            if (a > m_)
            {
                Console.WriteLine("Encrypted value was not in the field of m'.");
                return new List<BigInteger>(3);
            }
            List<BigInteger> encryptedA = split(a);

            for (int i = 0; i < encryptedA.Count; i++)
                encryptedA[i] = (encryptedA[i] * BigInteger.Pow(r, i + 1)) % m;

            return encryptedA;
        }

        public BigInteger Decrypt(List<BigInteger> encryptedA)
        {
            BigInteger a = 0;

            for (int i = 0; i < encryptedA.Count; i++)
            {
                encryptedA[i] = (encryptedA[i] * BigInteger.Pow(rinverse, i + 1)) % m;
                if (encryptedA[i] < 0)
                    encryptedA[i] += m;
            }

            foreach (BigInteger ai in encryptedA)
                a += ai;
            
            a = a % m_;
            return a;
        }
        
    }
    
}
