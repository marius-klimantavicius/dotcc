//remark:Qualified friend templ decl
//options:--c++14;fn:--c++14 --gnu=100100;fp:--c++14 --clang_v=70000;fp

extern "C" int printf(const char *,...);

namespace foo {
  template<typename T1>
  void _Construct(T1* p, int) { printf("Func1 %d ",p->x); }
#ifdef OK
  template<typename T1>
  void _Construct(T1* p, double) { printf("Func2 %d ",p->x); }
#endif
}

template <class T>
class blah {
  int x;
public:
  blah(int w) { x = w; }
  template<typename T1> friend void foo::_Construct(T1*,int);
  template<typename T1> friend void foo::_Construct(T1* p, double);
  template<typename T1> friend void foo::_Construct(T1* p, const char *);
};

int main()
{
  blah<int> x(5);
  foo::_Construct(&x, 5);
  foo::_Construct(&x,5.5);

  return 0;
}
