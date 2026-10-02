//type:fp
//options_all:--microsoft
//remark:[4.1] Microsoft compatibility: Interface templates
// 1/20/09  [EDGcpfe/9462]
//
// Microsoft compatibility: Interface templates
//
// In Microsoft mode, interface templates are now accepted.
//
// (See also the Changes entry of 6/2/03 that introduced non-template interface
// class types.)
template<typename T> __interface IF {
  void f(T);
};
