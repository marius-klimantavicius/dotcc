//type:fn
//options:--clang_version 140000;fn:--clang_version 150000:--clang_version 160000:--clang_version 170000:--clang_version 180100:--clang_version 190100
//options_all:--c++20

struct Incomplete;

static_assert( __is_trivially_relocatable(Incomplete));
