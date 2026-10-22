using System.Text;
using Inventory = Managed.PGlite.LocaleControl.LocaleInventory;

internal static unsafe class LocaleInventoryProgram
{
    public static int Main(string[] args)
    {
        string output = Path.GetFullPath(args[1]);
        byte[] expected = File.ReadAllBytes(args[0]);
        for (int i = 0; i < 3; i++)
        {
            using var owner = new Inventory.Libc.RuntimeContext();
            using var binding = owner.Enter();
            int status;
            fixed (byte* path = Encoding.UTF8.GetBytes(output + "\0"))
                status = Inventory.locale_inventory_copy(path);
            if (status != 0 || !File.ReadAllBytes(output).SequenceEqual(expected))
                throw new InvalidOperationException($"Emitted OS locale inventory differs from native control (status {status}).");
        }
        Console.WriteLine("PASS 3 fresh emitted C owners: actual OS locale bytes, stdio EOF and pclose status match native control");
        return 0;
    }
}
