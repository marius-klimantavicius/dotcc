//type:fp
//options_all:--gn 90400
//remark:[6.4] GNU compatibility: "inline" asm-qualifier for asm statements
// 6/17/22  [EDGcpfe/25414]
//
// GNU compatibility: "inline" asm-qualifier for asm statements
//
// Since GCC 7.5.0, the "inline" asm-qualifier has been accepted on asm statements
// and the front end now also accepts it.
int main() {
  asm inline ("");
  asm __inline ("");
  return 0;
}
