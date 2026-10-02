//type:fn
//options_all:--c++17
//remark:[5.1] Abort with malformed raw string in macro definition
// 4/1/19   [EDGcpfe/21074]
//
// Abort with malformed raw string in macro definition
//
// The front end could abort with an internal error ("assoc_source_line_modif:
// bad address") if a macro definition ends with a malformed raw string
// literal.  This is now fixed.
#define MM(...) __VA_ARGS__
#define M( X , Y , ... ) X ## Y MM ( R" )
int main() {
  M(f,1);
}
