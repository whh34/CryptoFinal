using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Security.Cryptography;

namespace MentalPoker
{
    public class PracticalMentalPoker
    {
        // Unfinished implementation of the DNC
        public class DNC
        {
            public class Link
            {
                public DateTime Timestamp;
                public Subject Concept;
                public Attribute Attributes;

                public class Subject
                {

                }

                public class Attribute
                {

                }
            }
        }

        public DNC dnc;

        // Here we could put methods that combine all of the Utility methods to get estimates of running times

        public static void MockSetup()
        {
            // Select a large prime
            byte[] chunk = new byte[3];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(chunk);
            BigInteger field = new BigInteger(chunk);

            if (field < 0)
                field = -field;

            CardMatrix cards = new CardMatrix(field);

            HomomorphicEncryptor encryptor = new HomomorphicEncryptor();

            List<BigInteger>[,] enCards = new List<BigInteger>[52, 52];

            for (int i = 0; i < 52; i++)
            {
                for (int j = 0; j < 52; j++)
                {
                    //Console.WriteLine(cards.Matrix[i, j].ToString());
                    enCards[i, j] = encryptor.Encrypt(cards.Matrix[i, j]);
                    Console.WriteLine(enCards[i, j][0] + ", " + enCards[i, j][1] + ", " + enCards[i, j][2]);
                }
            }
        }
    }
}
