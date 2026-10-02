//type:cp
//options::--gnu_version 40800:--gnu_version 50000:--clang:--microsoft;fn
//options_all:--c++11

struct S { const int x; };
struct S2 { int x; };

static_assert(__is_trivially_copyable(S), "S not copyable");
static_assert(__is_trivially_copyable(S2), "S2 not copyable");
