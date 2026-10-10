#ifndef SOURCE_TYPE_INFORMATION_H
#define SOURCE_TYPE_INFORMATION_H

namespace SourceTypes {
using Null = decltype(nullptr);
using NullAlias = Null;
typedef int Integer;
using IntegerAlias = Integer;
using QualifiedInteger = const volatile IntegerAlias;
using RestrictedPointer = int* __restrict;

enum class Signed : int { Negative = -17, Zero = 0, Alias = Zero, Positive = 23 };
enum class SameStorage : int { Negative = -17, Zero = 0, Positive = 23 };
enum class Unsigned : unsigned long long {
    Zero = 0,
    High = 0x8000000000000000ULL,
    Maximum = 0xffffffffffffffffULL,
};
enum class Huge : unsigned __int128 {
    Zero = 0,
    Bit100 = (static_cast<unsigned __int128>(1) << 100),
    Maximum = ~static_cast<unsigned __int128>(0),
};
enum class SignedHuge : __int128 {
    Low = -(static_cast<__int128>(1) << 100),
    High = (static_cast<__int128>(1) << 100),
};
enum class Opaque : unsigned short;
enum Unscoped { UnscopedNegative = -3, UnscopedPositive = 7 };
namespace Other { enum class Signed : int { Different = 91 }; }

struct TypeCatalog {
    NullAlias null_value;
    QualifiedInteger* qualified_integer;
    RestrictedPointer restricted_pointer;
    const int* const_pointer;
    int* const constant_pointer;
    volatile int* volatile_pointer;
    char character;
    signed char signed_character;
    unsigned char unsigned_character;
    wchar_t wide_character;
    char8_t utf8_character;
    char16_t utf16_character;
    char32_t utf32_character;
    Signed signed_enum;
    SameStorage same_storage_enum;
    Unsigned unsigned_enum;
    Huge huge_enum;
    SignedHuge signed_huge_enum;
    Opaque opaque_enum;
    Unscoped unscoped_enum;
    Other::Signed other_signed_enum;
};

int select(NullAlias);
int select(void*);
int character_kind(char);
int character_kind(signed char);
int character_kind(unsigned char);
int character_kind(wchar_t);
int character_kind(char8_t);
int character_kind(char16_t);
int character_kind(char32_t);
int enum_kind(Signed);
int enum_kind(SameStorage);
int enum_kind(Other::Signed);
int inspect(const TypeCatalog*);
int consume_huge(Huge);
int consume_signed_huge(SignedHuge);
int consume_opaque(Opaque);
int consume_qualified(QualifiedInteger*);
int consume_restricted(RestrictedPointer);
}

#endif
