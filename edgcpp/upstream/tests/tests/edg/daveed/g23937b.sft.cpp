//remark:Microsoft layout and explicit alignment
//options:--microsoft;fp

struct alignas(16) S1 { char c; };

#pragma pack(push, 8)
struct S2 { S1 s1; };
#pragma pack(pop)
 
static_assert(alignof(S2) == 16, "");
