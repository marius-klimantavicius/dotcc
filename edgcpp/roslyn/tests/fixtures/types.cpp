enum [[deprecated("retained enum")]] Colour { red=2, blue=7 };
enum class Status : unsigned short { ready=3, done=5 };
typedef int Integer __attribute__((deprecated("retained typedef")));
using Count = unsigned long;
struct { int value; } anonymous_record{9};
int named_types(Integer value) { return value+red+static_cast<int>(Status::done)+anonymous_record.value; }

typedef int AlignedInteger __attribute__((aligned(32)));
using QualifiedInteger = const Integer;
constexpr unsigned long integer_size=sizeof(Integer);
constexpr unsigned long integer_alignment=alignof(Integer);
constexpr unsigned long aligned_integer_size=sizeof(AlignedInteger);
constexpr unsigned long aligned_integer_alignment=alignof(AlignedInteger);
constexpr unsigned long qualified_integer_size=sizeof(QualifiedInteger);
constexpr bool retained_type_trait=__is_class(decltype(anonymous_record));
#if defined(__BITINT_MAXWIDTH__)
using SignedNine = _BitInt(9);
using UnsignedWide = unsigned _BitInt(129);
constexpr SignedNine nine_value = -7;
constexpr UnsignedWide wide_value = 0x1234;
constexpr unsigned long nine_storage=sizeof(SignedNine);
constexpr unsigned long wide_storage=sizeof(UnsignedWide);
#endif
