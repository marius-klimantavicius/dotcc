using static Managed.Smb.LibSmb2;

internal static unsafe class Program
{
    private static void Main()
    {
        // No SMB/network work: the explicit assembly root is the qualification
        // under test. This type anchor also confirms the referenced public ABI.
        Console.WriteLine($"ProductAudit: smb2_context={sizeof(smb2_context)}");
    }
}
