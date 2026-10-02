//remark:C++23 multi-subscript
//options:--c++23;fp

struct S {
  static int operator[](int i);
} s;
int r = s[1];

