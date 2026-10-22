using Backend = Managed.PGlite.Translated.PGlite;

namespace Managed.PGlite;

public sealed unsafe partial class PgliteDatabase
{
    internal bool UsesStandardStrings()
    {
        RequireOwner();
        bool standard = false;
        // Read the exact lexer GUC while its generated owner is bound. Unlike
        // SHOW this works inside an aborted subtransaction that ROLLBACK TO must
        // recover, and it observes raw protocol SET commands without caching.
        if (Dispatch(() => standard = (int)Backend.Globals.standard_conforming_strings != 0) != 0)
            throw new InvalidOperationException("Cannot read the translated SQL lexer configuration.");
        return standard;
    }
}
