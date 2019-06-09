using System;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web.Security;


namespace Tournament.Code
{
    public static class Secret
    {
        const string purpose = "Tournament:Forgotten";
        public static string Protect(string message )
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;
            byte[] data = Encoding.ASCII.GetBytes(message);
            var value = MachineKey.Protect(data, purpose);
            var x = BitConverter.ToString(value).Replace("-", "");
            return x;

        }

        public static string Unprotect(string hex)
        {
            if (String.IsNullOrWhiteSpace(hex))
                return null;
            var bytes = Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
            var ret =  MachineKey.Unprotect(bytes, purpose);
            if (ret == null || ret.Length == 0)
                return null;
            return Encoding.ASCII.GetString(ret);
        }

       
    }
}