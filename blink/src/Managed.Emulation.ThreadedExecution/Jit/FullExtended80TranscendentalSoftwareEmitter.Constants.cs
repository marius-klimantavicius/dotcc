// Generated independently by math/generate.py; exact rational endpoint floors agree.
namespace Managed.Emulation.Execution;
internal static partial class FullExtended80TranscendentalSoftwareEmitter
{
    private ref partial struct Writer
    {
        private void Pi(int p) => Constant(p, 1, 0x020bbea63b139b22UL, 0x29024e088a67cc74UL, 0xc4c6628b80dc1cd1UL, 0xc90fdaa22168c234UL);
        private void HalfPi(int p) => Constant(p, 0, 0x020bbea63b139b22UL, 0x29024e088a67cc74UL, 0xc4c6628b80dc1cd1UL, 0xc90fdaa22168c234UL);
        private void QuarterPi(int p) => Constant(p, -1, 0x020bbea63b139b22UL, 0x29024e088a67cc74UL, 0xc4c6628b80dc1cd1UL, 0xc90fdaa22168c234UL);
        private void TwoOverPi(int p) => Constant(p, -1, 0xfe5163abdebbc561UL, 0xdb6295993c439041UL, 0xfc2757d1f534ddc0UL, 0xa2f9836e4e441529UL);
        private void Ln2(int p) => Constant(p, -1, 0x8a0d175b8baafa2bUL, 0x40f343267298b62dUL, 0xc9e3b39803f2f6afUL, 0xb17217f7d1cf79abUL);
        private void TwoOverLn2(int p) => Constant(p, 1, 0x8b25166cd1a13247UL, 0xeb577aa8dd695a58UL, 0xbe87fed0691d3e88UL, 0xb8aa3b295c17f0bbUL);
    }
}
