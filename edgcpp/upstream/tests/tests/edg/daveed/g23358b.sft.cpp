//remark:Member function template overloading
//options:--c++17 --clang;fp

struct S {
  template<typename T>
    void f(int);
  template<typename T>
    static int f(int);
};

