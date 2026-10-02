//type:fp
//options: -A --c++20

struct D : decltype([] {}) {};

//cwg: 3151
//title: Closure types that are final
//meeting: Croydon 3/26
//edg_status: Passes
