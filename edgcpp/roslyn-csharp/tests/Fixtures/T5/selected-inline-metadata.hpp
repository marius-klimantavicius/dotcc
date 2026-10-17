#ifndef SELECTED_INLINE_METADATA_HPP
#define SELECTED_INLINE_METADATA_HPP

extern int selected_metadata_created;
extern int selected_metadata_destroyed;
extern int selected_metadata_live;
extern int selected_metadata_argument_count;
void selected_metadata_note(int event) noexcept;
int selected_metadata_argument() noexcept;

struct Member {
    int id;

    Member(int tag) noexcept : id(++selected_metadata_created)
    {
        ++selected_metadata_live;
        selected_metadata_note(100 + id + tag);
    }

    ~Member() noexcept
    {
        selected_metadata_note(300 + id);
        ++selected_metadata_destroyed;
        --selected_metadata_live;
    }
};

struct Whole {
    int prefix;
    Member member;

    Whole(int prefix_value) noexcept : prefix(prefix_value), member(0)
    {
        selected_metadata_note((prefix == 0x13579 ? 200 : 1200) + member.id);
    }

    ~Whole() noexcept
    {
        selected_metadata_note((prefix == 0x13579 ? 400 : 1400) + member.id);
    }
};

int selected_metadata_scope_b() noexcept;

#endif
