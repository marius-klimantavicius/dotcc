//remark:C++11 alignof constraints
//options:--c++11;fn:--c++11 --g++;fp

static_assert(alignof(int(int)) == 1, "");
