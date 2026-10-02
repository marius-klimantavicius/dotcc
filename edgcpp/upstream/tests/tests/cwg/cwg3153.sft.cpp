//type:fp
//options: -A --c++26

struct A {
  friend consteval bool operator==(A, A) { return true; }
};

struct S {
  A a;
  bool operator==(const S &) const = default;
};

bool b = S{} == S{};

//cwg: 3153
//title: Immediate-escalating defaulted comparison
//meeting: Croydon 3/26
//edg_status: Passes
