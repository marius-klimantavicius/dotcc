//remark:C++23 multi-subscript
//options:--c++23;fn

struct I { I(int); };
struct S {
  int operator[](I) = delete;
  operator int*();
} s;
int r = s[1];  // Should be ambiguous.

