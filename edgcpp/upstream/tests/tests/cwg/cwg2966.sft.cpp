//type:fp
//options: -A --c++20

using nullptr_t = decltype(nullptr);

static_assert(alignof(nullptr_t) == alignof(void *));
static_assert(sizeof(nullptr_t) == sizeof(void *));

//cwg: 2966
//title: Alignment and value representation of std::nullptr_t
//meeting: Croydon 3/26
//edg_status: Passes
