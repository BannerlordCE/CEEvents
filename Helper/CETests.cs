using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaptivityEvents.Helper
{
    internal class CETests
    {
        public static string RunTestOne()
        {
            try
            {
                return "Test One: Success";
            }
            catch (Exception e)
            {
                return "Test One: Failed - " + e;
            }
           
        }

        public static string RunTestTwo()
        {
            try
            {
                return "Test Two: Success";
            }
            catch (Exception e)
            {
                return "Test Two: Failed - " + e;
            }
        }
    }
}