//type:fn
//options:--gnu_version 80100
//options_all:--g++ -W

template<typename T, int N>
using V3 [[deprecated]] = T;

V3<int, 16> val;
