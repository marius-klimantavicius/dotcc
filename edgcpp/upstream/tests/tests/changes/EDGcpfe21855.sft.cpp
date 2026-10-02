//type:fp
//options_all:--c++14 --g++
//remark:[6.0] Deleted copy constructors causing classes to be marked as non-trivally copyable
// 10/17/19 [EDGcpfe/21855]
//
// Deleted copy constructors causing classes to be marked as non-trivally copyable
//
// When a copy or move constructor is declared with a signature that does not
// match that which would be implicitly generated, and this constructor is marked
// as deleted, the front end would still mark these classes as non-trivially
// copyable because of the mismatched function signature.  This resulted in
// unexpected behavior in certain cases, such as copy elision.
//
// This is now fixed.
struct S
{
  S(const S&&) = delete;
};
void f(S param = {}); // Spurious "deleted function referenced" diagnostic
