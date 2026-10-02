//type: fn
//options:  --c++11
# 1 "SemaTemplate/resolve-single-template-id.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 452 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaTemplate/resolve-single-template-id.cpp" 2


namespace std {
  class type_info {};
}

void one() { }
void two() { }
void two(int) { }

template<class T> void twoT() { }
template<class T> void twoT(int) { }

template<class T> void oneT() { }
template<class T, class U> void oneT(U) { }
# 27 "SemaTemplate/resolve-single-template-id.cpp"
template<void (*p)(int)> struct test { };

int main()
{
   one;
   two;
   oneT<int>;
   twoT<int>;
   typeid(oneT<int>);
  sizeof(oneT<int>);
  sizeof(twoT<int>);
  decltype(oneT<int>)* fun = 0;

  *one;
  *oneT<int>;
  *two;
  *twoT<int>;
  !oneT<int>;
  +oneT<int>;
  -oneT<int>;
  oneT<int> == 0;



  0 == oneT<int>;


  0 != oneT<int>;


  (false ? one : oneT<int>);
  void (*p1)(int); p1 = oneT<int>;

  int i = (int) (false ? (void (*)(int))twoT<int> : oneT<int>);
  (twoT<int>) == oneT<int>;
  bool b = oneT<int>;
  void (*p)() = oneT<int>;
  test<oneT<int> > ti;
  void (*u)(int) = oneT<int>;

  b = (void (*)()) twoT<int>;

  one < one;



  oneT<int> < oneT<int>;



  two < two;
  twoT<int> < twoT<int>;
  oneT<int> == 0;




}

struct rdar9108698 {
  template<typename> void f();
};

void test_rdar9108698(rdar9108698 x) {
  x.f<int>;
}

namespace GCC_PR67898 {
  void f(int);
  void f(float);
  template<typename T, T F, T G, bool b = F == G> struct X {
    static_assert(b, "");
  };
  template<typename T> void test1() { X<void(T), f, f>(); }
  template<typename T> void test2() { X<void(*)(T), f, f>(); }
  template void test1<int>();
  template void test2<int>();
}
