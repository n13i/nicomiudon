using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;

namespace NiCoMiudon
{
    public sealed class Settings : Dictionary<string, string>
    {
        private static readonly Settings instance = new Settings();

        public static string Email
        {
            get { return GetValue("email"); }
            set { instance["email"] = value; }
        }
        public static string Password
        {
            get
            {
                try
                {
                    byte[] src = Convert.FromBase64String(GetValue("password"));
                    return Encoding.Unicode.GetString(src, 0, src.Length);
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            set
            {
                try
                {
                    byte[] src = Encoding.Unicode.GetBytes(value);
                    instance["password"] = Convert.ToBase64String(src);
                }
                catch (Exception e)
                {
                    instance["password"] = null;
                }
            }
        }
        public static bool AlwaysAnonymize
        {
            get { return (GetValue("anonymize") == "1"); }
            set { instance["anonymize"] = (value ? "1" : "0"); }
        }
        public static int CommentFps
        {
            get { return int.Parse(GetValue("comment_fps")); }
            set { instance["comment_fps"] = value.ToString(); }
        }

        // ------------------------------------------------------------------

        public static string AppDataPath
        {
            get
            {
                Assembly asm = Assembly.GetExecutingAssembly();

                object[] prodArray = asm.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                string product = ((AssemblyProductAttribute)prodArray[0]).Product;

                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                return Path.Combine(appData, product);
            }
        }

        public static string ExePath
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName);
            }
        }

        public static string FilePath
        {
            get
            {
                // 設定ファイルのパス
                string iniFileName = Path.ChangeExtension(Assembly.GetExecutingAssembly().ManifestModule.Name, "ini");
                return Path.Combine(Settings.AppDataPath, iniFileName);
            }
        }

        private Settings()
        {
        }

        private static string GetValue(string key)
        {
            if (instance.ContainsKey(key))
            {
                return instance[key];
            }
            else
            {
                return null;
            }
        }

        // from TwitterWM
        public static void Load()
        {
            // FIXME 他のところで作るべき？
            if (!Directory.Exists(Settings.AppDataPath))
            {
                Directory.CreateDirectory(Settings.AppDataPath);
            }

            // デフォルト値を設定
            Settings.Email = null;
            Settings.Password = null;
            Settings.AlwaysAnonymize = true;
            Settings.CommentFps = 10;

            Utils.ReadLine(Settings.FilePath, line =>
            {
                if (line.StartsWith(";")) return;

                int pos = line.IndexOf('=');
                if (pos < 0) return;

                var key = line.Substring(0, pos);
                var value = line.Substring(pos + 1);
                instance[key] = value;
            });
        }

        public static bool Save()
        {
            try
            {
                using (var sw = new StreamWriter(Settings.FilePath))
                {
                    foreach (string key in instance.Keys)
                    {
                        sw.Write(key);
                        sw.Write("=");
                        sw.WriteLine(instance[key]);
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
