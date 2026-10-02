//option_all:--c++23 -tused -A
struct NonTriv {
  NonTriv(int);
  ~NonTriv();
};
struct V { V() = default; V(NonTriv); };
struct Q { Q(); };
struct A : virtual V, Q {
  using V::V;
  A() : A(42) { }    // #1, A(42) is equivalent to V(42)
};
struct B : A { };
void foo() { B b; }
