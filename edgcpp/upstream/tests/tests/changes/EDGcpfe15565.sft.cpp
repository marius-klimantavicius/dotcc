//type:fp
//options_all:--microsoft
//remark:[4.10] Microsoft and GNU compatibility: Partial specialization via a using-declaration
// 10/17/14 [EDGcpfe/15565]
//
// Microsoft and GNU compatibility: Partial specialization via a using-declaration
//
// In GNU and Microsoft C++ modes, the front end now accepts partial template
// specializations using a qualified name that involve a using-declaration.
namespace N { template<typename> struct X {}; }
namespace M { using N::X; }
template<typename> struct Y;
namespace N {
  template<typename T> struct M::X<Y<T>>;  // Previously an error.  Now a
}                                          // warning in GNU/Microsoft mode.
