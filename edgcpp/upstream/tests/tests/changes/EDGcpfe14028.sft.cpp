//type:fp
//options_all:--c++11
//remark:[4.10.1] Incorrect cast to variably-sized type in lowered IL
// 2/13/15   [EDGcpfe/14028,EDGcpfe/15990]
//
// Incorrect cast to variably-sized type in lowered IL
//
// In cases where the result of a variably-sized "new" operator is partially
// initialized with values whose type includes a function with a parameter
// passed by a copy constructor, lowering had added an incorrect cast which had
// caused an assertion (in dump_expr).  The incorrect cast had resulted in an
// eok_bassign operation whose source size is zero, possibly causing a back end
// to omit the assignment altogether.
struct A {
  A(const A&);
};
template <class T> using B = void (*)(T);
void f(int i) {
  new B<A>[i] { {} };
}
