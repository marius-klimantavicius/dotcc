//type:fp
//options_all:--microsoft
//remark:[4.2] Microsoft compatibility: dllimport static data members are not instantiated
// 1/26/10  [EDGcpfe/10018,EDGcpfe/10345]
//
// Microsoft compatibility: dllimport static data members are not instantiated
//
// In Microsoft mode, the definitions of static data members of class templates
// are no longer instantiated.
template<typename> struct S {
  __declspec(dllimport) static int x;
};
template <typename T> int S<T>::x = S<T>::y;
template int S<void>::x;  // Previously an error, because S<void>::y was
                          // not found during instantiation.  Now okay,
                          // since the definition of S<void>::x is not
                          // instantiated.
