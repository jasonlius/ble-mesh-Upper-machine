using System;
namespace ble.net.sampleapp.BleMesh
{
   public class K2KeyMaterial
   {
         public byte[] NID = new byte[1];
         public byte[] EncryptionKey = new byte[32];
         public byte[] PrivacyKey = new byte[32];
   }
}
