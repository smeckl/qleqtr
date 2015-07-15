using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeParser;

namespace PEAttrib
{
    class Program
    {
        static void Main(string[] args)
        {
            var peHeader = new PEHeader(@"Z:\Downloads\APT1_MALWARE_FAMILIES\BANGAT\BANGAT_sample\BANGAT_sample_468FF2C12CFFC7E5B2FE0EE6BB3B239E");

            Console.WriteLine(peHeader.ToString());
        }
    }
}
