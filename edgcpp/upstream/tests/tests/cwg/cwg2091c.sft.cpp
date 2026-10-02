//type:fp
//options_all:--c++17 -tused -A
template<const short cs> class B { };
template<short s> void g(B<s>);
void k2() {
B<1> b;
g(b); // OK: cv-qualifiers are ignored on template parameter types
}
