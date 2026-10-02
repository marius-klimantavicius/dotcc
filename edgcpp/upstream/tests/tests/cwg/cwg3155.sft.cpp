//type:fn
//options: -A --c++26

consteval int g(int x) { return x; }

struct B {
  virtual int f(int) { return 0; }
};

template <typename T>
struct D : B {
  // Calling the consteval g with the parameter x makes f immediate-escalating
  // (f results from instantiating a constexpr templated entity), so f is
  // escalated to an immediate function. The escalation itself is valid.
  constexpr int f(int x) override { return g(x); }
};

// Evaluating the call instantiates f; absent CWG 3155 this would succeed (the
// escalated f is a valid immediate function and the assertion holds). It is
// ill-formed only because the escalated f is an immediate virtual function
// overriding the non-immediate B::f while D<int> is not a consteval-only type.
static_assert(D<int>{}.f(0) == 0);  // error

//cwg: 3155
//title: Escalation of virtual functions
//meeting: Croydon 3/26
