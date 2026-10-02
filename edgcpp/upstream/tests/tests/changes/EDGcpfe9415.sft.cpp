//type:fp
//remark:[4.2] Pure specifier ("= 0") in templates
// 8/17/09  [EDGcpfe/9415,EDGcpfe/10021]
//
// Pure specifier ("= 0") in templates
//
// In Microsoft, GNU, and Sun modes, the front end now accepts the "= 0" specifier
// following a member function declaration of a class template even when the
// function is certain not to be virtual.
//
// Similarly, in Microsoft mode and in GNU mode with gnu_version < 40200 a member
// function template can be followed by "= 0": No diagnostic is issued until the
// template is instantiated.
//
// For all such cases, an error is still issued if the template is instantiated.
template<typename T> struct S {
  void f() = 0;  // Now silently accepted in GNU, Microsoft, and Sun C++
};               // modes.  An error is issued if the template is
                 // instantiated.
