//type:fp
//options_all:--microsoft
//remark:[4.4] Microsoft C++ compatibility: void parameters in template instantiations
// 9/6/11   [EDGcpfe/12029]
//
// Microsoft C++ compatibility: void parameters in template instantiations
//
// In Microsoft C++ mode, the front end sometimes accepts a function declarator
// of the form "(T)" where T is a template parameter that is instantiated with
// T=void (see entry for EDGcpfe/6674, EDGcpfe/8978, EDGcpfe/9401, EDGcpfe/9430
// on 12/11/08).  Additional cases are now supported.
template<typename T, void (*F)(T)> struct C {};
template <void (*F)(void)> struct M {
  typedef C<void, F> MF;  // Previously triggered an error for attempting
};                        // to substitute T=void in void (*F)(T); now
                          // accepted in Microsoft mode.
