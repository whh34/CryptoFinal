﻿using System;
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
            FastMentalPoker.MockInitialization(4, 100);
            PracticalMentalPoker.MockSetup(4, 100);
            
            Console.ReadKey();

            
        }
    }
}
