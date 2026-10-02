//type:fp
//options_all:--microsoft
//remark:[4.6] Microsoft compatibility: Explicit dllimport class template instantiation
// 12/18/12 [EDGcpfe/13533]
//
// Microsoft compatibility: Explicit dllimport class template instantiation
//
// In Microsoft mode with microsoft_version >= 1310, the front end now no longer
// attempts the instantiation of a nested class during the explicit instantiation
// of a dllimport enclosing template class.
template<typename T> struct S {
  struct N { T x; };
};
template struct __declspec(dllimport) S<void>;
  // Previously this resulted in an error because S<void>::N::x cannot have
  // type void.  Now it is accepted because S<void>::N is not instantiated.
