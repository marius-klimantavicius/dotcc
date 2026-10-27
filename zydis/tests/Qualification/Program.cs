using Raw = Managed.Zydis.Tests.ZydisQualification;

if (args.Length is < 1 or > 2 || !uint.TryParse(args[0], out uint group) || group > 5)
{
    Console.Error.WriteLine("Expected numeric qualification group 0..5");
    return 2;
}
if (group == 0)
{
    if (args.Length != 2) throw new ArgumentException("Actual emitted ABI requires native observations");
    ActualAbi.Check(args[1]);
}
if (group == 1) ManagedHooks.Check();
return Raw.ZqRun(group);
