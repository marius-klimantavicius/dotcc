#include "source_type_information.h"

int main()
{
    using namespace SourceTypes;
    NullAlias null_value = nullptr;
    if (select(null_value) != 11 || select(static_cast<void*>(nullptr)) != 13) return 1;
    if (character_kind('a') != 1 || character_kind(static_cast<signed char>(1)) != 2 ||
        character_kind(static_cast<unsigned char>(1)) != 3 || character_kind(L'a') != 4 ||
        character_kind(u8'a') != 5 || character_kind(u'a') != 6 || character_kind(U'a') != 7) return 2;
    if (enum_kind(Signed::Negative) != 17 || enum_kind(SameStorage::Negative) != 19 ||
        enum_kind(Other::Signed::Different) != 23) return 3;
    if (inspect(nullptr) != 29 || consume_huge(Huge::Bit100) != 37 ||
        consume_signed_huge(SignedHuge::Low) != 41 || consume_opaque(static_cast<Opaque>(42)) != 43 ||
        consume_qualified(nullptr) != 47 || consume_restricted(nullptr) != 53) return 4;
    return 0;
}
