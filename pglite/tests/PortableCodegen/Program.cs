using PG = Managed.PGlite.Probes.PortableCodegen;

int result = PG.dpg_portable_codegen_check();
if (result != 0) throw new InvalidOperationException($"PostgreSQL portable algorithm assertion {result}");
Console.WriteLine("PASS PostgreSQL software CRC32C/popcount: known vectors and 771 length/alignment cases");
