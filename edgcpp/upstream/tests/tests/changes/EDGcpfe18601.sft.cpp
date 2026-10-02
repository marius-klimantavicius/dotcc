//type:fp
//options_all:--c++11
//remark:[6.5] Core issue 2061: extension of namespace defined in inline namespace
// 12/19/22 [EDGcpfe/18601,EDGcpfe/19811,EDGcpfe/21859,EDGcpfe/21999,
//           EDGcpfe/25532,EDGcpfe/25816]
//
// Core issue 2061: extension of namespace defined in inline namespace
//
// The resolution of Core issue 2061 allows a namespace defined in an inline
// namespace to be extended from the enclosing namespace.
// --c++11:
//
// The front-end now implements that resolution (except in Microsoft bugs mode).
inline namespace inl {
  namespace ns { template<class> struct A; }
}
namespace ns {                    // Extends inl::ns
  template<> struct A<void> { };  // Previously a spurious error.  Now okay.
}
