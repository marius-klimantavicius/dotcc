//options_all:-r -x -tused
//options: --strict;cp

//        A (declares int i)
//      /   \
//    X       Y (declares i float i)
//    |       |
//     \      Z (declares i double i)
//      \   /
//        B
struct A { int i; };
struct X : virtual public A {};
struct Y : virtual public A { float i; };
struct Z : public Y { double i; };
struct B : public X, public Z { void f(); };
void B::f() {
  A::i = 0;
  X::i = 0;
  Y::i = 0;
  Z::i = 0;
  B::i = 0;
  i = 0;
}

