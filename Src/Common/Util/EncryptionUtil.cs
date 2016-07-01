// /*
// * Copyright (c) 2016, Alachisoft. All Rights Reserved.
// *
// * Licensed under the Apache License, Version 2.0 (the "License");
// * you may not use this file except in compliance with the License.
// * You may obtain a copy of the License at
// *
// * http://www.apache.org/licenses/LICENSE-2.0
// *
// * Unless required by applicable law or agreed to in writing, software
// * distributed under the License is distributed on an "AS IS" BASIS,
// * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// * See the License for the specific language governing permissions and
// * limitations under the License.
// */
using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using Alachisoft.NosDB.Common.DataStructures.Clustered;

namespace Alachisoft.NosDB.Common.Util
{
    public class EncryptionUtil
    {
       
        private static string PROF_MANAGER_TOKEN = "AUTHENTICATIONSTRINGFORPROFESSIONALEDITIONMANAGERANDSERVER";
        
        internal class TripleDES
        {
            private static TripleDESCryptoServiceProvider s_des;
            private static string s_key = "A41'D3a##asd[1-a;d zs[s`";
            //private static string s_key = "ahRA6q96";
            //private static string s_iv = "%1Az=-qT";
            //"j*>'[=1fAR#4*+.1"
            private static string s_iv = "KKNWLCZUNFKNSKZOJSZLEKIJUYGAXTMVZWHPBZVIOPBFYSJHKCDOOAFZCDBNWZXT";

            public static byte[] ConvertStringToByteArray(String s)
            {
                return (new ASCIIEncoding()).GetBytes(s);
            }

            // Encrypt the string.
            public static byte[] Encrypt(string PlainText)
            {
                try
                {
                    if (PlainText == null)
                        return null;
                    s_des = new TripleDESCryptoServiceProvider();
                    int block = s_des.BlockSize;
                    int j = s_des.KeySize;
                    //s_iv = RandomString(block);


                    byte[] k = ConvertStringToByteArray(s_key);
                    byte[] iv = ConvertStringToByteArray(s_iv);
                    //s_des.Key = ConvertStringToByteArray(s_key);
                    //s_des.IV = ConvertStringToByteArray(s_iv);

                    // Create a memory stream.
                    ClusteredMemoryStream ms = new ClusteredMemoryStream();

                    // Create a CryptoStream using the memory stream and the 
                    // CSP DES key.  
                    CryptoStream encStream = new CryptoStream(ms, s_des.CreateEncryptor(k, iv), CryptoStreamMode.Write);

                    // Create a StreamWriter to write a string
                    // to the stream.
                    StreamWriter sw = new StreamWriter(encStream);

                    // Write the plaintext to the stream.
                    sw.WriteLine(PlainText);

                    // Close the StreamWriter and CryptoStream.
                    sw.Close();
                    encStream.Close();

                    // Get an array of bytes that represents
                    // the memory stream.
                    byte[] buffer = ms.ToArray();

                    // Close the memory stream.
                    ms.Close();

                    // Return the encrypted byte array.
                    return buffer;
                }
                catch (Exception exception)
                {
                    throw exception;
                }
                return null;
            }

            // Decrypt the byte array.
            public static string Decrypt(byte[] CypherText)
            {
                try
                {
                    if (CypherText == null)
                        return "";
                    s_des = new TripleDESCryptoServiceProvider();

                    int block = s_des.BlockSize;
                    //s_iv = RandomString(block);
                    //s_des.Key = ConvertStringToByteArray(s_key);
                    //s_des.IV = ConvertStringToByteArray(s_iv);

                    byte[] k = ConvertStringToByteArray(s_key);
                    byte[] iv = ConvertStringToByteArray(s_iv);


                    // Create a memory stream to the passed buffer.
                    MemoryStream ms = new MemoryStream(CypherText);

                    // Create a CryptoStream using the memory stream and the 
                    // CSP DES key. 
                    CryptoStream encStream = new CryptoStream(ms, s_des.CreateDecryptor(k, iv), CryptoStreamMode.Read);

                    // Create a StreamReader for reading the stream.
                    StreamReader sr = new StreamReader(encStream);

                    // Read the stream as a string.
                    string val = sr.ReadLine();

                    // Close the streams.
                    sr.Close();
                    encStream.Close();
                    ms.Close();
                    return val;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            private static Random random = new Random((int)DateTime.Now.Ticks);
            private static string RandomString(int size)
            {
                StringBuilder builder = new StringBuilder();
                char ch;
                for (int i = 0; i < size; i++)
                {
                    ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                    builder.Append(ch);
                }

                return builder.ToString();
            }


            /// <summary>
            /// Encrypt user provided key with the default key stored; This key is obfuscated
            /// </summary>
            /// <param name="key">Key</param>
            /// <returns>encrypted string</returns>
            internal static byte[] EncryptData(byte[] data)
            {
                try
                {
                    //byte[] data = ASCIIEncoding.ASCII.GetBytes(key);
                    s_des = new TripleDESCryptoServiceProvider();
                    int i = s_des.BlockSize;
                    int j = s_des.KeySize;
                    byte[] k = ConvertStringToByteArray(s_key);
                    byte[] IV = ConvertStringToByteArray(s_iv);
                    //s_des.Key = ConvertStringToByteArray(key);
                    //s_des.IV = ConvertStringToByteArray(s_iv);

                    // Create a memory stream.
                    ClusteredMemoryStream ms = new ClusteredMemoryStream();

                    // Create a CryptoStream using the memory stream and the 
                    // CSP DES key.  
                    CryptoStream encStream = new CryptoStream(ms, s_des.CreateEncryptor(k, IV), CryptoStreamMode.Write);

                    encStream.Write(data, 0, data.Length);
                    encStream.FlushFinalBlock();

                    // Get an array of bytes from the 
                    // MemoryStream that holds the 
                    // encrypted data.
                    byte[] ret = ms.ToArray();

                    // Close the memory stream.
                    ms.Close();
                    encStream.Close();

                    // Return the encrypted byte array.

                    //string temp = Convert.ToBase64String(ret, Base64FormattingOptions.None);
                    // temp = System.Security.SecurityElement.Escape(temp);
                    return ret;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            public static byte[] DecryptData(byte[] encodedData)
            {
                try
                {
                    // byte[] data = Convert.FromBase64String(encodedkey);
                    s_des = new TripleDESCryptoServiceProvider();
                    byte[] k = ConvertStringToByteArray(s_key);
                    byte[] IV = ConvertStringToByteArray(s_iv);


                    // Create a memory stream to the passed buffer.
                    ClusteredMemoryStream ms = new ClusteredMemoryStream(encodedData);

                    // Create a CryptoStream using the memory stream and the 
                    // CSP DES key. 
                    CryptoStream encStream = new CryptoStream(ms, s_des.CreateDecryptor(k, IV), CryptoStreamMode.Read);

                    byte[] fromEncrypt = new byte[encodedData.Length];

                    // Read the decrypted data out of the crypto stream
                    // and place it into the temporary buffer.
                    int length = encStream.Read(fromEncrypt, 0, fromEncrypt.Length);

                    // Close the streams.
                    encStream.Close();
                    ms.Close();

                    byte[] ret = new byte[length];
                    Array.Copy(fromEncrypt, ret, length);
                    return ret;//ASCIIEncoding.ASCII.GetString(ret);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        internal class AES
        {
            private static string s_iv = "#ZCX$GHK";
            public static string EncryptText(string text)
            {
                byte[] encryptedBytes;
                byte[] saltBytes = { 1, 2, 3, 4, 5, 6, 7, 8 };
                byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(text);

                using (var ms = new ClusteredMemoryStream())
                {
                    using (var aes = new RijndaelManaged())
                    {
                        aes.KeySize = 256;
                        aes.BlockSize = 128;
                        var encryptionKeyBytes = Encoding.UTF8.GetBytes(s_iv);
                        Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(encryptionKeyBytes, saltBytes, 1000);
                        aes.Key = key.GetBytes(aes.KeySize / 8);
                        aes.IV = key.GetBytes(aes.BlockSize / 8);
                        aes.Mode = CipherMode.CBC;

                        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                            cs.Close();
                        }
                        encryptedBytes = ms.ToArray();
                    }
                }
                return Convert.ToBase64String(encryptedBytes);
            }

            public static string DecryptText(string encryptedText)
            {
                byte[] decryptedBytes;
                byte[] bytesToBeDecrypted = Convert.FromBase64String(encryptedText);
                byte[] saltBytes = { 1, 2, 3, 4, 5, 6, 7, 8 };

                using (var ms = new ClusteredMemoryStream())
                {
                    using (var aes = new RijndaelManaged())
                    {
                        aes.KeySize = 256;
                        aes.BlockSize = 128;
                        var encryptionKeyBytes = Encoding.UTF8.GetBytes(s_iv);
                        Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(encryptionKeyBytes, saltBytes, 1000);
                        aes.Key = key.GetBytes(aes.KeySize / 8);
                        aes.IV = key.GetBytes(aes.BlockSize / 8);

                        aes.Mode = CipherMode.CBC;

                        using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                            cs.Close();
                        }
                        decryptedBytes = ms.ToArray();
                    }
                }
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }

        #region Token Validation for Professional Manager

        public static byte[] ValidateManagementToken(byte[] token)
        {
            try
            {
                if (token != null && token.Length > 0)
                {
                    var encryptedText = Encoding.UTF8.GetString(token);
                    var decryptedText = AES.DecryptText(encryptedText);
                    var parts = decryptedText.Split(':');

                    if (parts.Length == 2)
                    {
                        String responseText = parts[0] + ":";
                        responseText += parts[1] + ":";
                        responseText += PROF_MANAGER_TOKEN;

                        return Encoding.UTF8.GetBytes(AES.EncryptText(responseText));
                    }
                    return token;
                }
            }
            catch
            {
                return token;
            }
            return token;

        }

        public static bool ValidateManagementTokenResponse(byte[] respToken)
        {
            var respTokenText = Encoding.UTF8.GetString(respToken);
            respTokenText = AES.DecryptText(respTokenText);

            var respTokenTexParts = respTokenText.Split(':');

            return respTokenTexParts.Length == 3 && respTokenTexParts[2] == PROF_MANAGER_TOKEN;
        }

        public static byte[] GetProfManagerToken()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            var macAddress = string.Empty;
            if (nics.Length > 0)
            {
                var address = nics[0].GetPhysicalAddress();
                macAddress = address.ToString();
            }

            var text = macAddress + ":" + DateTime.Now.Ticks;
            var encryptedText = AES.EncryptText(text);

            return Encoding.UTF8.GetBytes(encryptedText);
        }
        #endregion

        #region Token Validation for Professional Client

        public static byte[] GetClientToken(string ip)
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            var macAddress = String.Empty;
            if (nics.Length > 0)
            {
                PhysicalAddress address = nics[0].GetPhysicalAddress();
                macAddress = address.ToString();
            }

            var text = macAddress + ":" + DateTime.Now.Ticks + ":" + ip;
            var encryptedText = AES.EncryptText(text);

            return Encoding.UTF8.GetBytes(encryptedText);
        }

        public static bool ValidateClientToken(byte[] token, string clientIp)
        {
            try
            {
                if (token != null && token.Length > 0)
                {
                    var encryptedText = Encoding.UTF8.GetString(token);
                    var decryptedText = AES.DecryptText(encryptedText);
                    var parts = decryptedText.Split(':');

                    return parts[2].Equals(clientIp);
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        public static bool ValidateClientTokenResponse(byte[] sentToken, byte[] receivedToken)
        {
            try
            {
                if (sentToken != null && sentToken.Length > 0 && receivedToken != null && receivedToken.Length > 0)
                {
                    var sentEncryptedText = Encoding.UTF8.GetString(sentToken);
                    var receivedEncryptedText = Encoding.UTF8.GetString(sentToken);

                    var sentDecryptedText = AES.DecryptText(sentEncryptedText);
                    var receivedDecryptedText = AES.DecryptText(receivedEncryptedText);

                    return sentDecryptedText.Equals(receivedDecryptedText);
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        #endregion

    }
}
