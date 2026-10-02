//type:fp
//options:--c++26
//options_all:-A -tused

#define likely(x) x
#define unlikely(a, b) a + b

#undef likely
#undef unlikely

//cwg: 3053
//title: Allowing #undef likely
//meeting: Kona 11/25
//edg_status: Passes
