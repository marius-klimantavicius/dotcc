//type:fp
//options_all:--c++17 -A
struct A{};
template<class T> struct B {
template<class R> int operator*(R&); // #1
};
template<class T, class R> int operator*(T&, R&); // #2
// The declaration of B::operator* is transformed into the equivalent of
// template<class R> int operator*(B<A>&, R&); // #1a
int main() {
A a;
B<A> b;
b * a; // calls #1a
}

//cwg: 1446
//title: Member function with no ref-qualifier and non-member function with rvalue reference
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
