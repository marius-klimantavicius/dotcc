namespace Managed.Database;

public static unsafe partial class ValkeyHost
{
    // Qualification status is documentation, not command authorization. Only
    // replication and operations that need a child snapshot are excluded here.
    private static readonly System.Collections.Generic.HashSet<string> ForkDependentCommands = new(System.StringComparer.OrdinalIgnoreCase)
    {
        "bgsave", "bgrewriteaof",
        "sync", "psync", "replconf", "replicaof", "slaveof", "failover",
        "cluster|replicate", "cluster|failover", "cluster|migrateslots", "cluster|syncslots"
    };
}
