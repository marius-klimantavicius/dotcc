//type:fp
//options_all:--gn 70300
//remark:[6.1] GNU C++ compatibility: Vacuous destructor calls and vector types
// 12/5/19  [EDGcpfe/22084]
//
// GNU C++ compatibility: Vacuous destructor calls and vector types
//
// The front end now accepts vacuous destructor calls applied to vector types.
typedef int Vec __attribute__((__vector_size__(8)));
Vec& g();
int main() {
  g().~Vec();  // Previously an error.  Now okay.
}
