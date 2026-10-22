using Managed.PGlite;

if (args.Length != 3) throw new ArgumentException("Usage: ApiCodecs.Engine <new-data-directory> <bootstrap-prefix> <work-directory>");
using var database = PgliteDatabase.Create(new PgliteOptions(args[0], args[1], args[2]));
int assertions = PgliteCodecCases.Run(database);
Console.WriteLine($"API codec/template actual-engine cases: {assertions} assertions passed.");
