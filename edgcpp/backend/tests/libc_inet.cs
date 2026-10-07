using System;
using System.Text;
using Edg.Runtime;

unsafe class Program
{
    static void Main()
    {
        using LibcContext libc = new();
        string? line;
        while ((line = Console.ReadLine()) != null)
        {
            int separator = line.IndexOf('\t');
            int family = int.Parse(line.AsSpan(0, separator));
            byte[] source = Encoding.UTF8.GetBytes(line[(separator + 1)..] + "\0");
            byte[] result = new byte[20];
            Array.Fill(result, (byte)0xa5);
            libc.SetErrno(123);
            int status;
            fixed (byte* input = source, output = result)
                status = libc.InetPton(family, (sbyte*)input, output + 2);
            Console.WriteLine($"{status} {*libc.ErrnoLocation()} {Convert.ToHexString(result)}");
        }
        // Unsupported families must not inspect either pointer.
        libc.SetErrno(123);
        if (libc.InetPton(-1, null, null) != -1 || *libc.ErrnoLocation() != 97)
            throw new Exception("Unsupported-family contract");
        // Errno belongs to the source program owner and current managed thread.
        using LibcContext other = new();
        other.SetErrno(55);
        System.Threading.Thread worker = new(() => {
            if (*libc.ErrnoLocation() != 0) throw new Exception("Inherited errno");
            libc.InetPton(-1, null, null);
            if (*libc.ErrnoLocation() != 97) throw new Exception("Thread errno");
        });
        libc.SetErrno(44); worker.Start(); worker.Join();
        if (*libc.ErrnoLocation() != 44 || *other.ErrnoLocation() != 55)
            throw new Exception("Owner/thread errno isolation");
    }
}
