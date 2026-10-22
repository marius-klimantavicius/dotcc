using Managed.PGlite.Hosting;
using Core = Managed.PGlite.Translated.PGlite;

try
{
    // Statically name the ordinary referenced product. A diagnostic subset with
    // a matching class name still cannot pass the workflow availability guard.
    Console.Error.WriteLine($"{nameof(Core)} embedded SQL workflow requested.");
    EmbeddingAvailability.RequireSqlWorkflow();
    return 1;
}
catch (Exception error)
{
    Console.Error.WriteLine(error.Message);
    return 1;
}
