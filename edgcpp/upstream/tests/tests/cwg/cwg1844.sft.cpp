//type:fp
//options: -A --c++20

template<typename T, unsigned S = sizeof(T)>
struct X {};

template<typename T>
X<T> foo(T *);

void foo(...);

void test() {
  struct S *s;
  foo(s);
}

//cwg: 1844
//title: Defining immediate context
//meeting: Croydon 3/26
//edg_status: Passes
