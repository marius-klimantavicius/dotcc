//type:fp
//options::--gnu_version 70400:--clang_version 60000:--microsoft_version 1920
//options_all:--c++17

template<typename T, typename U> struct same;
template<typename T> struct same<T, T> { ~same(); };

struct A {
  int i = 0;
  int &r = i;
  volatile int &vr = i;
  const int &cr = i;
  const volatile int &cvr = i;
};

void g() {
  const auto [i,r,vr,cr,cvr] = A();

  same<decltype((i)), const int&>();
  same<decltype((r)), int&>();
  same<decltype((vr)), volatile int&>();
  same<decltype((cr)), const int&>();
  same<decltype((cvr)), const volatile int&>();
}

void h() {
  typedef const A CA;
  auto &[i,r,vr,cr,cvr] = CA(); // type of var is 'const A &'

  same<decltype((i)), const int&>();
  same<decltype((r)), int&>();
  same<decltype((vr)), volatile int&>();
  same<decltype((cr)), const int&>();
  same<decltype((cvr)), const volatile int&>();
}
