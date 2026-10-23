using System.Runtime.InteropServices;
using System.Text;
using Managed.S2nTls.Crypto;

internal static unsafe class NameChecks
{
    private static void Check(bool value) { if (!value) throw new InvalidOperationException("Name differential assertion"); }

    private static void PrintName(void* name)
    {
        var indices = new List<int>();
        for (int previous = -1; ;)
        {
            int index = OpenSslAbi.X509_NAME_get_index_by_NID(name, 13, previous);
            if (index < 0) break;
            indices.Add(index); previous = index;
        }
        Console.WriteLine("entries " + indices.Count);
        foreach (int index in indices)
        {
            void* entry = OpenSslAbi.X509_NAME_get_entry(name, index);
            void* data = OpenSslAbi.X509_NAME_ENTRY_get_data(entry);
            Check(data != null);
            int length = OpenSslAbi.ASN1_STRING_length(data);
            string hex = Convert.ToHexString(new ReadOnlySpan<byte>(OpenSslAbi.ASN1_STRING_get0_data(data), length)).ToLowerInvariant();
            Console.WriteLine($"entry 13 {OpenSslAbi.ASN1_STRING_type(data)} {hex}");
        }
        byte* der = null; ulong size = 0;
        Check(OpenSslAbi.X509_NAME_get0_der(name, &der, &size) == 1);
        Console.WriteLine($"der {size} {Convert.ToHexString(new ReadOnlySpan<byte>(der, checked((int)size))).ToLowerInvariant()}");
    }

    public static void Run(string pemPath)
    {
        while (OpenSslAbi.ERR_get_error() != 0) { }
        int contexts = OpenSslAbi.LiveContexts;
        string pem = File.ReadAllText(pemPath);
        var item = System.Security.Cryptography.PemEncoding.Find(pem);
        byte[] certificateDer = Convert.FromBase64String(pem.AsSpan()[item.Base64Data].ToString());
        void* certificate = null; void* name = null;
        try
        {
            fixed (byte* data = certificateDer)
            {
                byte* cursor = data;
                certificate = OpenSslAbi.d2i_X509(null, &cursor, certificateDer.Length);
                Check(certificate != null && cursor == data + certificateDer.Length);
            }
            name = OpenSslAbi.X509_NAME_new(); Check(name != null);
            int[] types = [22, 29, 22]; string[] values = ["", "invalid", "valid"];
            for (int i = 0; i < 3; i++)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(values[i] + "\0");
                fixed (byte* data = bytes)
                    Check(OpenSslAbi.X509_NAME_add_entry_by_NID(name, 13, types[i], data, -1, -1, 1) == 1);
                Check(OpenSslAbi.X509_set_subject_name(certificate, name) == 1);
                PrintName(OpenSslAbi.X509_get_subject_name(certificate));
                byte* encoded = null;
                int size = OpenSslAbi.i2d_X509(certificate, &encoded);
                try { Console.WriteLine("cached-certificate-der " + (size == certificateDer.Length && new ReadOnlySpan<byte>(encoded, size).SequenceEqual(certificateDer) ? 1 : 0)); }
                finally { NativeMemory.Free(encoded); }
            }
            OpenSslAbi.X509_NAME_free(name); name = null;
            PrintName(OpenSslAbi.X509_get_subject_name(certificate));
            Check(OpenSslAbi.ERR_get_error() == 0);
        }
        finally
        {
            OpenSslAbi.X509_NAME_free(name);
            OpenSslAbi.X509_free(certificate);
        }
        Check(OpenSslAbi.LiveContexts == contexts);
    }
}
