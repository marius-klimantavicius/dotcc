//type:fp
//options_all:--g++
//remark:[5.0] GNU and Microsoft compatibility: Deleted operator delete and new-expressions
// 2/6/18   [EDGcpfe/19298]
//
// GNU and Microsoft compatibility: Deleted operator delete and new-expressions
//
// The GNU and Microsoft compilers fail to check that an "operator delete"
// corresponding to a new-expression is not "deleted" (i.e., defined with
// "= delete").
//
// The front end now emulates that behavior in GNU and Microsoft modes.
struct S {
  S();
  void operator delete(void*) = delete;
};
int main() {
  S *p = new S;  //  Ordinarily an error, but now accepted in GNU
}                //  and Microsoft modes.
