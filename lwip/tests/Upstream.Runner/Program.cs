using Managed.Networking;

using var owner = new Lwip();
using var binding = owner.__DotCcEnter();
return args.Length == 1 && args[0] == "--control" ? owner.check_contract_control() : owner.main();
