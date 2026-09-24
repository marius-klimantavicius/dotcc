#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    private sealed class OwnedRandomState
    {
        internal readonly object Gate = new();
        internal Random Rand = new(1), Posix = new(1);
    }

    private static readonly ConditionalWeakTable<RuntimeContext, OwnedRandomState> ownedRandomStates = new();
    private static OwnedRandomState? OwnedRandom => RuntimeContext.Current is { } owner
        ? ownedRandomStates.GetValue(owner, static _ => new()) : null;
}
