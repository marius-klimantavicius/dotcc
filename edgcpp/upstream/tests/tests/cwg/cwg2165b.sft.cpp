//type:fp
//options_all:--c++20 -tused -A
typedef int Int;
enum E : int { a };
void f(int);       // #1
void f(Int) {}     // defines #1
void f(E) {}       // OK: another overload

struct X {
  static void f();
  void g();
  void g() const;  // OK
};
