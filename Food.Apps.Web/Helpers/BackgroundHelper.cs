using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;


namespace ITWebNet.Food.Site.Helpers
{
    public static class BackgroundHelper
    {
        public static byte GetBackgroundNumber()
        {
            string banners = Path.Combine(Environment.CurrentDirectory,"/Image/Background");

            int maxId = Directory.GetFiles(banners, "*.png").Length;

            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
            {
                byte[] randomNumber = new byte[1];

                do
                {
                    rngCsp.GetNonZeroBytes(randomNumber);
                }
                while (randomNumber[0] > maxId);
                return randomNumber[0];
            }
        }
    }
}