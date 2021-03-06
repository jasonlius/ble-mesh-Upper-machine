using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace ble.net.sampleapp.BleMesh
{
   static class AesCmac
   {
      public static byte[] GetAesCmac(byte[] key, byte[] data)
      {
         // copy a new date using passed paramater to prevent them from changed in memoery. 
         byte[] dataCopy = new byte[data.Length];
         Buffer.BlockCopy(data, 0, dataCopy, 0, data.Length);

         // SubKey generation
         // step 1, AES-128 with key K is applied to an all-zero input block.
         byte[] L = AESEncrypt(key, new byte[16], new byte[16]);

         // step 2, K1 is derived through the following operation:
         byte[] FirstSubkey = Rol(L); //If the most significant bit of L is equal to 0, K1 is the left-shift of L by 1 bit.
         if ((L[0] & 0x80) == 0x80)
            FirstSubkey[15] ^= 0x87; // Otherwise, K1 is the exclusive-OR of const_Rb and the left-shift of L by 1 bit.

         // step 3, K2 is derived through the following operation:
         byte[] SecondSubkey = Rol(FirstSubkey); // If the most significant bit of K1 is equal to 0, K2 is the left-shift of K1 by 1 bit.
         if ((FirstSubkey[0] & 0x80) == 0x80)
            SecondSubkey[15] ^= 0x87; // Otherwise, K2 is the exclusive-OR of const_Rb and the left-shift of K1 by 1 bit.

         // MAC computing
         if (((dataCopy.Length != 0) && (dataCopy.Length % 16 == 0)) == true)
         {
            // If the size of the input message block is equal to a positive multiple of the block size (namely, 128 bits),
            // the last block shall be exclusive-OR'ed with K1 before processing
            for (int j = 0; j < FirstSubkey.Length; j++)
               dataCopy[dataCopy.Length - 16 + j] ^= FirstSubkey[j];
         }
         else
         {
            // Otherwise, the last block shall be padded with 10^i
            byte[] padding = new byte[16 - dataCopy.Length % 16];
            padding[0] = 0x80;

            dataCopy = dataCopy.Concat<byte>(padding.AsEnumerable()).ToArray();

            // and exclusive-OR'ed with K2
            for (int j = 0; j < SecondSubkey.Length; j++)
               dataCopy[dataCopy.Length - 16 + j] ^= SecondSubkey[j];
         }

         // The result of the previous process will be the input of the last encryption.
         byte[] encResult = AESEncrypt(key, new byte[16], dataCopy);

         byte[] HashValue = new byte[16];
         Array.Copy(encResult, encResult.Length - HashValue.Length, HashValue, 0, HashValue.Length);

         return HashValue;
      }


      private static byte[] AESEncrypt(byte[] key, byte[] iv, byte[] dataCopy)
      {
         using (MemoryStream ms = new MemoryStream())
         {
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();

            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;

            using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write))
            {
               cs.Write(dataCopy, 0, dataCopy.Length);
               cs.FlushFinalBlock();

               return ms.ToArray();
            }
         }
      }

      private static byte[] Rol(byte[] b)
      {
         byte[] r = new byte[b.Length];
         byte carry = 0;

         for (int i = b.Length - 1; i >= 0; i--)
         {
            ushort u = (ushort)(b[i] << 1);
            r[i] = (byte)((u & 0xff) + carry);
            carry = (byte)((u & 0xff00) >> 8);
         }

         return r;
      }
   }
}
