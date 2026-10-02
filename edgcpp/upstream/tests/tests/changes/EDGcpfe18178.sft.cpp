//type:fp
//options_all:--g++
//remark:[4.14] GNU compatibility: References to static data members
// 4/10/17  [EDGcpfe/18178]
//
// GNU compatibility: References to static data members
//
// Ordinarily, static data members of class templates are instantiated whenever
// they are referenced (with some exceptions for constant-valued members or
// references from unused default arguments).  In GNU C++ modes, this is no
// longer the case if the address of the member is not taken and its value is not
// needed.
//
// Ordinarily, this example leads to an error because S<int>::m is referenced
// but it has no definition.  In GNU C++ mode, this is now accepted (the program
// also successfully links) because S<int>::m doesn't have its value or address
// used.
namespace {
  template<typename T> struct S {
    static int m;
  };
}
int main() {
  (void)S<int>::m;
}
