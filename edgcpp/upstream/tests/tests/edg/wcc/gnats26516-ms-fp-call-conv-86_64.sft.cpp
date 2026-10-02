//type:cn
//options_all:--c++20 --microsoft --target linux_x86_64

template<typename T>
void x();

struct foo {};

#define CC_TEST(convention) \
  /* Specialization of function pointer */ \
  template<> \
  void x<void (convention *)()>() { } \
  /* Overload of function pointer */ \
  void foo(void (convention * z)()) { } \
  /* Specialization of pointer to member */ \
  template<> \
  void x<void (convention foo::*)()>() { } \
  /* Overload of pointer to member */ \
  void foo(void (convention foo::* z)()) { }

CC_TEST(__cdecl)
// CC_TEST(__clrcall) is excluded due to the complexity/practicality of this
// appearing since it requires the compiler to be in C++/CLI mode.
CC_TEST(__stdcall)
CC_TEST(__fastcall)
CC_TEST(__thiscall)
CC_TEST(__vectorcall)
