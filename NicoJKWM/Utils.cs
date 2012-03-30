using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Reflection;
using System.IO;

namespace NiCoMiudon
{
    class Utils
    {
        // taken from http://d.hatena.ne.jp/kazuv3/20080605/1212656674
        // The codes below are licensed under the NYSL, so I NYS'd
        public static string UrlEncode(string s, Encoding enc)
        {
            StringBuilder rt = new StringBuilder();
            foreach (byte i in enc.GetBytes(s))
                if (i == 0x20)
                    rt.Append('+');
                else if (i >= 0x30 && i <= 0x39 || i >= 0x41 && i <= 0x5a || i >= 0x61 && i <= 0x7a)
                    rt.Append((char)i);
                else
                    rt.Append("%" + i.ToString("X2"));
            return rt.ToString();
        }

        public static string UrlDecode(string s, Encoding enc)
        {
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '%')
                    bytes.Add((byte)int.Parse(s[++i].ToString() + s[++i].ToString(), NumberStyles.HexNumber));
                else if (c == '+')
                    bytes.Add((byte)0x20);
                else
                    bytes.Add((byte)c);
            }
            return enc.GetString(bytes.ToArray(), 0, bytes.Count);
        }

        public static void DebugLog(object value)
        {
            System.Diagnostics.Debug.WriteLine(value);
        }

        public static List<Dictionary<string, string>> ParseCookie(string cookieStr)
        {
            if (cookieStr == null) { return null; }

            var cookies = new List<Dictionary<string, string>>();

            /*
            redirect_to=%2Fiphone;
            expires=Sun, 08-Nov-2009 12:07:47 GMT;
            path=/;
            httponly,
            */
            /*
            user_session=deleted;
            expires=Mon, 10-Aug-2009 09:27:00 GMT,
            
            user_session=deleted;
            expires=Mon, 10-Aug-2009 09:27:00 GMT;
            path=/,
            
            user_session=deleted;
            expires=Mon, 10-Aug-2009 09:27:00 GMT;
            path=/;
            domain=.nicovideo.jp,
             
            user_session=user_session_xxxxxx_17548354641831261284;
            expires=Thu, 09-Sep-2010 09:27:01 GMT;
            path=/;
            domain=.nicovideo.jp
            */
            int pos = 0;

            var cookie = new Dictionary<string, string>();

            cookieStr += ",";

            while (pos < cookieStr.Length)
            {
                bool next = false;
                string key = null, value = null;

                int posEq = cookieStr.IndexOf('=', pos);
                int posSc = cookieStr.IndexOf(';', pos);
                int posCm = cookieStr.IndexOf(',', pos);

                //DebugLog("eq=" + posEq.ToString() + ", sc=" + posSc.ToString() + ", cm=" + posCm.ToString());

                if (posCm == -1)
                {
                    break;
                }

                if (posEq >= 0 && posEq < posSc && posEq < posCm)
                {
                    // KEY= ...
                    key = cookieStr.Substring(pos, posEq - pos);

                    if(key == "expires")
                    {
                        int posCm2 = cookieStr.IndexOf(',', posCm + 1);
                        if (posCm2 >= 0) { posCm = posCm2; }
                    }

                    if(posCm < posSc)
                    {
                        // expires=Mon, .... ,
                        next = true;
                        value = cookieStr.Substring(posEq + 1, posCm - posEq - 1);
                        pos = posCm + 1;
                    }
                    else
                    {
                        // expires=Mon, .... ;
                        value = cookieStr.Substring(posEq + 1, posSc - posEq - 1);
                        pos = posSc + 2;
                    }
                }
                else if (posSc >= 0 && posSc < posEq && posSc < posCm)
                {
                    // KEY;
                    key = cookieStr.Substring(pos, posSc - pos);
                    value = "";
                    pos = posSc + 2;
                }
                else if (posCm >= 0 && posCm < posEq && posCm < posSc)
                {
                    // KEY,
                    key = cookieStr.Substring(pos, posCm - pos);
                    value = "";
                    pos = posCm + 1;
                }
                else
                {
                    pos++;
                }

                if (key != null)
                {
                    //DebugLog("key=" + key + ", value=" + value);
                    cookie[key] = value;
                }

                if (next)
                {
                    //Utils.DebugLog("add");
                    cookies.Add(cookie);
                    cookie = new Dictionary<string, string>();
                }
            }

            cookies.Add(cookie);

            return cookies;
        }

        public static string GetExecutingAssemblyVersion()
        {
            //Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            //return ver.Major.ToString() + "." + ver.Minor.ToString();

            // AssemblyInformationalVersionを使うよう変更(2010/05/01)
            Assembly asm = Assembly.GetExecutingAssembly();
            AssemblyInformationalVersionAttribute ver = (AssemblyInformationalVersionAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyInformationalVersionAttribute));
            return ver.InformationalVersion;
        }

        // from TwitterWM
        public static void ReadLine(string path, Action<string> delg)
        {
            try
            {
                using (var sr = new StreamReader(path, Encoding.UTF8))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                        delg(line);
                }
            }
            catch { }
        }

        // cf. http://www.atmarkit.co.jp/fdotnet/dotnettips/980unixtime/unixtime.html
        private static DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0);
        public static long GetUnixTime(DateTime dt)
        {
            DateTime dtUtc = dt.ToUniversalTime();
            TimeSpan elapsed = dtUtc - UNIX_EPOCH;
            return (long)elapsed.TotalSeconds;
        }
        public static long GetUnixTimeMS(DateTime dt)
        {
            DateTime dtUtc = dt.ToUniversalTime();
            TimeSpan elapsed = dtUtc - UNIX_EPOCH;
            return (long)elapsed.TotalMilliseconds;
        }
    }
}
