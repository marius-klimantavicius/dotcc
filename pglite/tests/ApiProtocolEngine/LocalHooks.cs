namespace Managed.PGlite.ProductTests;

internal static partial class PgliteApiCases
{
    static partial void Services(PgliteDatabase database) => CoreProtocolCases.Run(database);
}
