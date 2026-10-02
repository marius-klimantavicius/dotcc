//type:cp
//options:--c++20
//options_all:-tused

union U { volatile int x ; };

template <class T> constexpr T f1(T&& x) {return x;}

constexpr int f2(int x = f1(U().x)) {return x;}
