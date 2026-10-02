//type:fp
//options_all:--c++17
//remark:[6.2] Abort on generic lambda nested in local non-generic lambda
// 9/23/20  [EDGcpfe/19404,EDGcpfe/23405]
//
// Abort on generic lambda nested in local non-generic lambda
//
// The front end previously could abort in scope_depth_for_capture (expr.c) with
// an internal error when checking capturing by a generic lambda nested in a local
// non-generic lambda.
//
// This is now fixed.
int main() {
  const int N = 42;
  auto nl = [](){ return [](auto){ return N; }; };
  nl()(8);
}
