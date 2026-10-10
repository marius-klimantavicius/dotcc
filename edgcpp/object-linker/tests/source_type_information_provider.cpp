#include "source_type_information.h"

namespace SourceTypes {
enum class Opaque : unsigned short { Known = 42 };
int select(NullAlias) { return 11; }
int select(void*) { return 13; }
int character_kind(char) { return 1; }
int character_kind(signed char) { return 2; }
int character_kind(unsigned char) { return 3; }
int character_kind(wchar_t) { return 4; }
int character_kind(char8_t) { return 5; }
int character_kind(char16_t) { return 6; }
int character_kind(char32_t) { return 7; }
int enum_kind(Signed) { return 17; }
int enum_kind(SameStorage) { return 19; }
int enum_kind(Other::Signed) { return 23; }
int inspect(const TypeCatalog* value) { return value == nullptr ? 29 : 31; }
int consume_huge(Huge value) { return value == Huge::Bit100 ? 37 : 0; }
int consume_signed_huge(SignedHuge value) { return value == SignedHuge::Low ? 41 : 0; }
int consume_opaque(Opaque value) { return value == Opaque::Known ? 43 : 0; }
int consume_qualified(QualifiedInteger* value) { return value == nullptr ? 47 : 0; }
int consume_restricted(RestrictedPointer value) { return value == nullptr ? 53 : 0; }
}
