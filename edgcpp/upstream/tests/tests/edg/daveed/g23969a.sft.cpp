//remark:Aggregate initializer lifetime within conditional expression
//options:--c++11;rp

#include <initializer_list>

extern "C" int printf(const char*,...);

template<typename T> struct V
{
      V(std::initializer_list<T>) { printf("Ctor\n"); }
      ~V() { printf("Dtor\n"); }
};

struct S { V<char> data; } ;

struct W {
  void f(S&&) {}
};

int main() {
  W p;
  bool detected = true;
  detected ? p.f({{0x01}}) : p.f({{0x02}});
}
