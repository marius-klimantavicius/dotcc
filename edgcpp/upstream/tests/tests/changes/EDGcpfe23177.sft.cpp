//type:fp
//options_all:--gnu=90000
//remark:[6.2] GNU compatibility: GNU-style attributes on an alias declaration
// 8/4/20   [EDGcpfe/23177]
//
// GNU compatibility: GNU-style attributes on an alias declaration
//
// Previously the presence of GNU-style attributes on an alias declaration
// had caused spurious errors; now fixed.
struct A {
  using T __attribute__((deprecated)) = char;
};
