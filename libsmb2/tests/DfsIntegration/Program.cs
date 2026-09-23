using System.Text;
using Managed.Smb;

if (args.Length != 1) throw new ArgumentException("Expected isolated Samba server address");
await using var client = SmbDfsClient.Create("smbprobe", Environment.GetEnvironmentVariable("LIBSMB2_PASSWORD")
    ?? throw new ArgumentException("LIBSMB2_PASSWORD is required"));
foreach (var (link, share) in new[] { ("link-a", "alpha"), ("link-b", "beta") })
{
    string unc = $@"\\{args[0]}\dfs\{link}\資料\marker.txt";
    var targets = await client.ResolvePathAsync(unc);
    if (targets.Count != 1 || targets[0].Share != share || targets[0].RelativePath != "資料/marker.txt")
        throw new IOException("Referral target or Unicode suffix differs");
    // Repeat through the cached referral and compare a direct-share read.
    foreach (string path in new[] { unc, unc, $@"\\{args[0]}\{share}\資料\marker.txt" })
    {
        await using var file = await client.OpenReadAsync(path);
        byte[] buffer = new byte[128];
        int count = await file.ReadAsync(buffer);
        if (Encoding.UTF8.GetString(buffer, 0, count) != share) throw new IOException("Target contents differ");
    }
    var entries = await client.ListAsync($@"\\{args[0]}\dfs\{link}\資料");
    if (!entries.Any(entry => entry.Name == "marker.txt")) throw new IOException("Referred listing differs");
}
Console.WriteLine("DfsIntegration: PASS two links, distinct shares, Unicode suffix, cached referrals, direct-share reads");
