using System.Runtime.CompilerServices;
using Cui.Managed;
using Cui.ManagedGallery;
using static Managed.Forms.Cui;

internal static unsafe partial class Program
{
    private static string BioText(Session s) => s.State.Demo.Bio.Text;
    private static int GridSelectedCount(Session s) => s.State.Demo.SelectedGridRows;
    private static bool GridSelectionAt(Session s, int row) => s.State.Demo.IsGridRowSelected(row);
    private static int GridSortColumn(Session s) => s.State.Demo.GridSort.Column;
    private static string GridName(Session s, int row) => s.State.Demo.GridRowName(row);
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "ctx")]
    private static extern ref cui_ctx* Context(Ui ui);

    // The showcase itself has no raw access. This qualification adapter keeps
    // independent node/edit/lifetime assertions identical to the reference suite.
    private sealed class SafeApp : IDisposable
    {
        private readonly Application app = new(new ApplicationOptions
        {
            Title = "CUI authored gallery qualification", Width = 1240, Height = 820,
            Continuous = true, Theme = (ThemeMode)GalleryClockFixture.ThemeIndex, Accent = (Accent)GalleryClockFixture.AccentIndex
        });
        internal Ui ManagedUi => app.Ui;
        internal cui_ctx* Ui => Context(app.Ui);
        internal bool NextFrame() => app.NextFrame();
        internal void Render() => app.Render();
        internal byte[] CapturePixels(out int width, out int height) => app.CapturePixels(out width, out height);
        public void Dispose() => app.Dispose();
    }
    private sealed class ObservedGallery
    {
        internal readonly GalleryDemo Demo = new((ThemeMode)GalleryClockFixture.ThemeIndex, (Accent)GalleryClockFixture.AccentIndex);
        internal int page { get => Demo.Page; set => Demo.Page = value; }
        internal CBool newsletter => Demo.Newsletter;
        internal CBool bluetooth => Demo.Bluetooth;
        internal CBool terms => Demo.Terms;
        internal cui_rect bluetooth_rect => new() { x = Demo.BluetoothRect.X, y = Demo.BluetoothRect.Y, w = Demo.BluetoothRect.W, h = Demo.BluetoothRect.H };
        internal int plan => Demo.Plan;
        internal float volume => Demo.Volume;
        internal CBool loading => Demo.Loading;
        internal int tab => Demo.Tab;
        internal int step => Demo.Step;
        internal CBool dialog_open => Demo.DialogOpen;
        internal CBool drawer => Demo.DrawerOpen;
        internal CBool dark => Demo.Dark;
        internal int accent => (int)Demo.SelectedAccent;
    }
}
