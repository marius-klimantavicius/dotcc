//type:fp
//remark:[4.6] Spurious error on elaborated enum name qualified with dependent class name
// 12/11/12 [EDGcpfe/13490]
//
// Spurious error on elaborated enum name qualified with dependent class name
//
// The changes adding support for opaque enum declarations (see entry of 6/20/12)
// caused the front end to emit a spurious error when it encountered an
// elaborated enum name qualified with a dependent class name.
//
// This regression is now fixed.
template<class> struct S { enum E { e }; };
template<class T> struct R {
  enum S<T>::E ev;  // This previously triggered a spurious error.
};
