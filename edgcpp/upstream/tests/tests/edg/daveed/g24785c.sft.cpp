//remark:C++23 multi-subscript
//options:--c++23;fp:--c++23 -DNEG;fn

template<typename T> struct S {
  int operator[](int, int);
  int operator[]();
};

S<int> s;

#ifndef NEG
int r1 = s[];
int r2 = s[1, 2];
#else
int r3 = s[1];
int r4 = s[1, 2, 3];
#endif

