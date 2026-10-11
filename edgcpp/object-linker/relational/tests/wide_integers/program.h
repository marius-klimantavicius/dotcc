typedef __int128 Wide;
typedef unsigned __int128 UWide;
extern const UWide wide_values[3];
UWide transform(UWide value);
Wide divide_signed(Wide value);
UWide variadic_wide(int marker, ...);
template<UWide N = ((UWide)1 << 100)> UWide selected() { return N; }
