//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn

//         A{i}
//          |
//     X{i} V
//      |  / \
//      Y  B  C
//       \ | /
//         D
//
struct A { int i; int j; };
struct V : public A { };
struct B : virtual public V { };
struct C : virtual public V { int f() { return i; } };
struct X { int i; };
struct Y : private X { };
struct D : public B, public C, private Y { int g() { return i; } };

