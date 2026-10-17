#include "selected-inline-metadata.hpp"

int selected_metadata_scope_b() noexcept
{
    Whole local(selected_metadata_argument());
    selected_metadata_note(700 + local.member.id);
    return local.prefix == 0x13579 ? local.member.id : -local.member.id;
}
