//type:fp
//options_all:--c++20

#pragma pack(push, 1)
struct P {};
#pragma pack(pop)
#if (0 + 1)
#endif
struct NP { char c; long long l; };
static_assert(sizeof(NP) == 16);
