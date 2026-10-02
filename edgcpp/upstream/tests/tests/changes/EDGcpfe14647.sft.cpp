//type:fp
//options_all:--g++
//remark:[4.9] GNU compatibility: Less checking of functional notation casts in templates
// 11/6/13  [EDGcpfe/14647]
//
// GNU compatibility: Less checking of functional notation casts in templates
//
// In GNU C++ mode, the front end now relaxes checking of functional notation
// casts appearing in template contexts.
struct S { S(); };
template<class T> S g() {
  return S(3);  // Normally an error when the template is parsed, but
}               // now only an error in GNU C++ mode if the template is
                // instantiated.
