//type:fp
//options_all:--microsoft_version=1929 --ms_c++latest --no_ms_permissive -w --target win32
//remark:[6.3] Abort in interpreter on recursive std::construct_at call
// 3/2/21   [EDGcpfe/23990]
//
// Abort in interpreter on recursive std::construct_at call
//
// The front end previously could abort in the constexpr interpreter when
// std::construct_at is invoked recursively.
//
// That is now fixed.
void* operator new(decltype(sizeof(0)), void*);
void operator delete(void*, void*);
namespace std {
  template<typename T, typename ... Args>
  constexpr T* construct_at(T *p, Args &&...args) {
    return new((void*)p) T(args...);
  }
}
struct I {
  int i;
  constexpr I(int i): i(i) {}
  constexpr ~I() {}
};
struct X {
  I i;
  constexpr X(): i(42) {
    this->i.~I();
    std::construct_at(&this->i, 8);  // Previously triggered an abort in
  }                                  // the interpreter.
  constexpr ~X() {}
};
struct Y {
  X x;
  constexpr Y(): x() {
    this->x.~X();
    std::construct_at(&this->x);
  }
  constexpr ~Y() {}
};
constexpr Y y{};
