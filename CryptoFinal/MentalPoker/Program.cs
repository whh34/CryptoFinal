using System;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalPoker
{
    class Program
    {
        static void Main(string[] args)
        {

            HomomorphicEncryptor h = new HomomorphicEncryptor();

            List<BigInteger> e = h.Encrypt(BigInteger.Pow(2, 32));
            Console.WriteLine(h.Decrypt(e));
            
            Console.ReadKey();
            
        }
    }
}
