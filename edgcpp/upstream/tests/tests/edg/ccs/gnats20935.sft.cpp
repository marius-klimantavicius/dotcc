//type:fp
//options::--gnu_version 70400
//options_all:--c++11

template<typename T, typename U> struct same;
template<typename T> struct same<T, T> {};

typedef struct {
  unsigned long long int f1:4;
  unsigned long long int f2:28;
} S;

void foo() {
  volatile decltype(S{}.f1 + S{}.f1) x = 1; x = -x;
  same<decltype(S{}.f1), unsigned long long>();
  same<decltype(+S{}.f1), int>();
  same<decltype(S{}.f2), unsigned long long>();
  same<decltype(+S{}.f2), int>();
  same<decltype(x), volatile int>();
}

void bar(S s) {
  volatile decltype(s.f1 + s.f1) x = 1; x = -x;
  same<decltype(s.f1), unsigned long long>();
  same<decltype(+s.f1), int>();
  same<decltype(s.f2), unsigned long long>();
  same<decltype(+s.f2), int>();
  same<decltype(x), volatile int>();
}
