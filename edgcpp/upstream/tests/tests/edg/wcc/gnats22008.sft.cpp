//type:fp
//options:--gnu_version 80100
//options_all:--g++ -W

typedef int V __attribute__((__vector_size__(16)));

template<typename T, int N>
using V3 [[gnu::__vector_size__(N)]] = T;

static_assert(sizeof(V) == sizeof(V3<int, 16>), "assertion failure");
