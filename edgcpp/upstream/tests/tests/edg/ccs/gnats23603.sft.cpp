//type:cp
//options::--gnu_version 70300:--clang_version 90000
//options_all:--c++17

class a {
 public:
  ~a();
};
class b {
  a c;
};
class C {
 public:
  static a d(b);
};
struct e {
  a f = C::d(b());
};
struct g {
  e h{};
};
void i(g = {});
