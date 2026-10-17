#include "selected-inline-comdat.hpp"

InlineTracked selected_inline_second;

int selected_inline_scope_b() noexcept
{
    InlineTracked local;
    selected_inline_note(700 + local.id);
    return local.id;
}
