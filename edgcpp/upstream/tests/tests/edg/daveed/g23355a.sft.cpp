//remark:Value-initialization checks
//options:--c++11;fn

struct XX {
    union {
        const int n;
    } m;
};
//XX x;
static_assert(!__is_constructible(XX));;
