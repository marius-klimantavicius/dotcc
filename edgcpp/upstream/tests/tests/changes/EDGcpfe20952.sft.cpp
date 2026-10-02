//type:fp
//options_all:--c++03 --strict
//remark:[6.4] Support for in-class explicit template specializations
// 10/3/22  [EDGcpfe/20952,EDGcpfe/21043,EDGcpfe/23601,EDGcpfe/23732,
//           EDGcpfe/23971,EDGcpfe/24086,EDGcpfe/24563]
//
// Support for in-class explicit template specializations
//
// The resolution of Core issue 727 allows explicit template specializations to
// also be declared in class scope (previously, this was only allowed in Microsoft
// and Sun modes).  Because it was adopted as a defect report, the new rules apply
// to all C++ versions.
struct C {
  template<class T> struct B { };
  template<> struct B<void> { };  // Previously an error.  Now okay.
};
