//type:fn
//options:--gn 160000:--clang_version 200100
//options_all:--c++20

struct Incomplete;

static_assert(!__builtin_is_implicit_lifetime(Incomplete)); // error
