//type:fp
//options_all:--c++17
//remark:[4.14] C++17: Selection statements with initializers
// 4/14/17  [EDGcpfe/17414,EDGcpfe/17706]
//
// C++17: Selection statements with initializers
//
// The front end now accepts a leading "initializer statement" as part of the
// control value construct of a selection statement (i.e., "if" or "switch"
// statement).
//
// This feature was added to the working paper for C++17 through paper P0305R1.
int f();
void g() {
  int x;
  if (f(); int x = 1) {}
  if (int x = 1; f()) {}
  if (x = 1; f()) {}
  if (int x = 1; int y = 1) {}
  if (int x = 1, h(), z = f(); int y = 1) {}
}
