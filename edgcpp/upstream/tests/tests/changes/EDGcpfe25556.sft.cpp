//type:fp
//options_all:--g++
//remark:Abort on arithmetic on GNU-mode label address
// 4/10/26  [EDGcpfe/25556]
//
// Abort on arithmetic on GNU-mode label address
//
// This previously elicited an internal error in the constant-evaluation
// interpreter.  That is now fixed.
void g(void*);
int main() {
label:
  g(&&label + 42);
}
