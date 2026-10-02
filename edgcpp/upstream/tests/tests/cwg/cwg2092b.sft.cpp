//type:fn
//options_all:--c++17 -tused -A
template<int i> class A { /* ... */ };
template<short s> void f(A<s>);
void k1() {
A<1> a;
f(a); // error: deduction fails for conversion from int
}
