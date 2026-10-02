//type:fp
//options_all:--gnu_version=70100 --c++14
//remark:[6.1] Spurious "unexpected designator" error in class template assignment
// 1/24/20  [EDGcpfe/22116]
//
// Spurious "unexpected designator" error in class template assignment
//
// When a braced-initializer containing designated initializers is used to
// initialize a template-dependent type, the front end would issue a spurious
// "unexpected designator" error while parsing the prototype template.  This
// also affected non-dependent types in gnu versions earlier than 70200, as
// these types are treated as dependent in expression contexts.
//
// This is now fixed.
struct Foo { int a; };
template<typename T> struct Bar {
    T baz;
    T fun() { return this->baz = { .a = 42 }; } // Spurious error here
};
int main(){
    Bar<Foo> *b = new Bar<Foo>();
    b->fun();
}
