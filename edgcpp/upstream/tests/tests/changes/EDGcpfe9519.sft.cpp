//type:fp
//options_all:--microsoft
//remark:[4.1] Microsoft compatibility: For-init-scope and condition-scope hiding
// 2/18/09  [EDGcpfe/9519]
//
// Microsoft compatibility: For-init-scope and condition-scope hiding
//
// In Microsoft C++ mode, the front end now allows declaring a variable in a
// loop with the same name as a for-init-scope or condition-scope declaration
// if a for-statement previously appeared in the loop.
//
// Some such cases were already accepted when microsoft_version < 1400 because of
// nonstandard for-init scope rules in those modes (e.g., see Changes entry of
void f() {
  for (int i = 0; int c = i<10 ; ++i) {
    for (; 0;);
    int i, c;  // Now accepted in all Microsoft C++ modes.
  }
}
