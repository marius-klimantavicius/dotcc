//type:cp
//options::--gnu_version 70300:--clang_version 90000
//options_all:--c++14

class a {
 public:
 
  ~a();
};
class b {
 public:
  b(a = a());
 
  ~b();
};
struct c {
  b i;
};
struct d {
  c
  e{};
};
class f {
  void g() { h = {}; }
  d
    h;
};
