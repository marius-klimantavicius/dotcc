//type:fp
//options_all:--c++17 -tused --gnu_version 90200
int i;
auto           x2a(i);    // decltype(x2a) is int
decltype(auto) x2d(i);    // decltype(x2d) is int
auto           x3a = i;   // decltype(x3a) is int
decltype(auto) x3d = i;   // decltype(x3d) is int
static_assert(__is_same_as(int,decltype(x2a)));
static_assert(__is_same_as(int,decltype(x2d)));
static_assert(__is_same_as(int,decltype(x3a)));
static_assert(__is_same_as(int,decltype(x3d)));

//cwg: 1951
//title: Cv-qualification and literal types
//meeting: Lenexa 5/15
//edg_status: Passes
