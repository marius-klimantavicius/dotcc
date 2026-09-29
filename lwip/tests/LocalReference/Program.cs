using Managed.Networking;
#if !LWIP_NOSYS
#error Local relative NO_SYS selection did not reach the consumer.
#endif
if (Lwip.NO_SYS != 1) throw new InvalidOperationException("Wrong generated product.");
if (typeof(LwipStack).Assembly.GetType("Managed.Networking.LwipSockets") is not null)
    throw new InvalidOperationException("Consumer selected NO_SYS but facade selected sockets.");
if (UnrelatedReference.Leaf.Value != "unrelated transitive dependency")
    throw new InvalidOperationException("Unrelated transitive project reference was changed.");
using var stack = new LwipStack();
stack.Start();
stack.Poll();
Console.WriteLine("PASS local project: relative NO_SYS selection propagates to facade and product without CLI hints");
