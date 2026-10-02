//type:fp
//options_all:--gn 80100
//remark:[6.7] Qualified elaborated type specifier in template friend declaration
// 10/23/24 [EDGcpfe/22136,EDGcpfe/27630]
//
// Qualified elaborated type specifier in template friend declaration
//
// When a qualified elaborated type specifier is used in a template friend
// declaration, the front end previously did not look in namespaces made visible
// by using directives.
namespace ns {
  template<typename> class A;
}
using namespace ns;
class C {
  template<typename>
  friend class ::A;  // Previously a spurious error.  Now okay.
};
