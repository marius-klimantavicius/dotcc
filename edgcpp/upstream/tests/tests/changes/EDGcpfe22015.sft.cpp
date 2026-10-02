//type:fn
//options_all:--c++11 --gnu_version=50400
//remark:[6.0] Invalid nontype template arguments
// 11/19/19 [EDGcpfe/22015]
//
// Invalid nontype template arguments
//
// A number of changes were made to more consistently diagnose invalid nontype
// template arguments.
template<char(*)()> struct X {};
extern int g();
int main() {
  X<(char (*)())g> x;  // Previously accepted in GNU C++11 mode.
}                      // Now an error.
