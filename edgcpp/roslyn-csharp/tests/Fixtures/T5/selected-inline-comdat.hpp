#ifndef SELECTED_INLINE_COMDAT_HPP
#define SELECTED_INLINE_COMDAT_HPP

extern int selected_inline_created;
extern int selected_inline_destroyed;
extern int selected_inline_live;
void selected_inline_note(int event) noexcept;

struct InlineTracked {
    int id;

    InlineTracked() noexcept : id(++selected_inline_created)
    {
        ++selected_inline_live;
        selected_inline_note(100 + id);
    }

    ~InlineTracked() noexcept
    {
        selected_inline_note(300 + id);
        ++selected_inline_destroyed;
        --selected_inline_live;
    }
};

extern InlineTracked selected_inline_first;
extern InlineTracked selected_inline_second;
int selected_inline_scope_b() noexcept;

#endif
