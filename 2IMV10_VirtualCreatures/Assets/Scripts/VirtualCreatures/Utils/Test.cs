using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.VirtualCreatures.Utils
{
    class Test
    {
        class A { }
        class AA : A { }

        static int Main(string[] args)
        {
            A a = new A();
            A aa = new AA();
            Console.Out.WriteLine((a is A));
            Console.Out.WriteLine((a is AA));
            Console.Out.WriteLine((aa is A));
            Console.Out.WriteLine((aa is AA));
            return 0;
        }
    }
}
