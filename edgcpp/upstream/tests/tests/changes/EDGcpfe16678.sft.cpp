//type:fp
//options_all:--user_defined_literals --no_microsoft_bugs
//remark:[4.11] Incorrect processing of function name strings with user-defined literals
// 11/25/15 [EDGcpfe/16678]
//
// Incorrect processing of function name strings with user-defined literals
//
// In Microsoft mode and in gnu_mode with gnu_version < 30400, with
// user-defined literals enabled, spurious errors could result if a function
// template containing a use of __FUNCTION__, __PRETTY_FUNCTION__, or
// __FUNCDNAME__ is instantiated.  This is now fixed.
// --user_defined_literals --no_microsoft_bugs:
template <typename T> struct S {
  void f(int) {
    const char *p = __FUNCTION__;
  }
};
int main() {
  int i = 0;
  S<int>().f(i);  // Previously reported too few arguments in call
}
