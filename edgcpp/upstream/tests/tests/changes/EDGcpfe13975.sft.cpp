//type:fp
//options_all:--c++11
//remark:[4.8] Allow enumeration type in range-based for statement
// 6/10/13  [EDGcpfe/13975]
//
// Allow enumeration type in range-based for statement
//
// A spurious error had been reported when a range-based for statement
// had an enumeration type and there was no overloaded "operator !=" defined
// for the enumeration type.  Now fixed.
extern "C" int printf (const char *, ...);
enum E { e1, e2, e3, X };
E operator*(E e) { return e; }
E begin(E e) { return e; }
E end(E e) { return X; };
E operator++(E& e) { return e = E(e+1); }
int main() {
  for (auto e: e1) {
    printf ("%d ", e);
  }
}
