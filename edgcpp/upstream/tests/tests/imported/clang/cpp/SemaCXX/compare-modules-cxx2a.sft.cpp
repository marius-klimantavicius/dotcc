//type: fn
//options:  --exceptions --c++20
# 1 "SemaCXX/compare-modules-cxx2a.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 412 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/compare-modules-cxx2a.cpp" 2
# 14 "SemaCXX/compare-modules-cxx2a.cpp"
struct CC { CC(...); };

void a() { void(0 <=> 0); }

struct A {
  CC operator<=>(const A&) const = default;
};
auto va = A() <=> A();





void b() { void(0 <=> 0); }

struct B {
  CC operator<=>(const B&) const = default;
};
auto vb = B() <=> B();



void c() { void(0 <=> 0); }

struct C {
  CC operator<=>(const C&) const = default;
};
auto vc = C() <=> C();
# 54 "SemaCXX/compare-modules-cxx2a.cpp"
void g() { void(0.0 <=> 0.0); }
