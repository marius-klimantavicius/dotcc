namespace Cui.Managed;

/// <summary>A copied snapshot of wrapper-owned storage and registrations.</summary>
/// <remarks>Byte counts cover the wrapper frame arena only. Translated engine,
/// renderer, editable buffers, terminal buffers and driver storage are separate.</remarks>
public readonly record struct UiDiagnostics(int FrameArenaReservedBytes, int FrameArenaUsedBytes,
    int ActiveScopes, int LeasedTextBuffers, int LeasedTerminals, int StateOwners,
    int TextureOwners, int CallbackRegistrations, int CanvasPainters);

public sealed unsafe partial class Ui
{
    public UiDiagnostics GetDiagnostics()
    {
        Check();
        return new UiDiagnostics(_arena.ReservedBytes, _arena.UsedBytes, _scopes.Count, _buffers.Count,
            _terminals.Count, _states.Count, _textures.Count, _callbackRoots.Count, _painters.Count);
    }
}
