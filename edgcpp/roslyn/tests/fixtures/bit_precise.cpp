using SignedNine = _BitInt(9);
using UnsignedWide = unsigned _BitInt(129);
constexpr SignedNine nine_value = -7;
constexpr UnsignedWide wide_value = 0x1234;
constexpr unsigned long nine_storage = sizeof(SignedNine);
constexpr unsigned long wide_storage = sizeof(UnsignedWide);
int bit_precise(SignedNine value) { return static_cast<int>(value + nine_value); }
