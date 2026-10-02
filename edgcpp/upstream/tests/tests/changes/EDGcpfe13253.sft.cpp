//type:fp
//remark:[4.6] Abort on class template with using-directive for new/delete
// 10/22/12 [EDGcpfe/13253]
//
// Abort on class template with using-directive for new/delete
//
// Previously, the front end sometimes aborted while processing a class template
// containing a using-directive for an operator new or an operator delete (the
// abort was in set_overload_set_traversal_symbol).
//
// This is now fixed.
template<class T> struct BT {
  static void operator delete(void*, __EDG_SIZE_TYPE__);
};
struct B: BT<B> {};
template<class T> struct D: B, BT<D<T> > {
  using BT<D<T> >::operator delete;
};  // Checking this class template previously caused an internal error.
