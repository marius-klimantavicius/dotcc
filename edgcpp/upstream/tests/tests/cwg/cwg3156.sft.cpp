//type:fn
//options: -A --c++20

struct C {
  C(int) = delete;
  C() {}
};

decltype([b = C(3)]() { return 4; }()) x; // error

//cwg: 3156
//title: Handling of deleted functions in unevaluated lambda-captures
//meeting: Croydon 3/26
//edg_status: Passes
