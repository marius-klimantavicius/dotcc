//type:fp
//options:--c++14 -A

template<typename T>
constexpr int var = 1;

template<typename T>
constexpr int var<T *> = 2;

static_assert(var<int> == 1, "");
static_assert(var<int *> == 2, "");

//cwg: 1711
//title: Missing specification of variable template partial specializations
//meeting: Virtual 11/20
//edg_status: Passes
