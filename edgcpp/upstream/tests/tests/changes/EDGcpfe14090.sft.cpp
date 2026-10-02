//type:fn
//remark:[4.8] Abort on malformed friend declaration of main()
// 7/22/13  [EDGcpfe/14090,EDGcpfe/14200,EDGcpfe/9817,EDGcpfe/10253,
//           EDGcpfe/10846,EDGcpfe/12881]
//
// Abort on malformed friend declaration of main()
//
// The front end previously aborted (in corresponding_param_type; class_decl.c)
// when processing a malformed friend declaration of main() that attempts to add
// default arguments.
//
// This is now fixed.
template <class T> class S {
  friend int main (T x = 2) ;
};
int main() {
  S<bool> s;  // Previously triggered an abort.
}
