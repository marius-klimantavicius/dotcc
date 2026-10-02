//type:fp
//options_all:--microsoft
//remark:[4.3] Microsoft compatibility: Casts on wide string literals in initializers
// 3/1/11   [EDGcpfe/11392]
//
// Microsoft compatibility: Casts on wide string literals in initializers
//
// In Microsoft mode, the front end previously treated ordinary string literals
// that are explicitly cast to a pointer to the underlying character type
// appearing in brace-enclosed initializers as if the cast weren't there.  Now
// that special treatment is also applied for wide-character string literals.
//
// (See also entry of 6/16/05.)
wchar_t s[] = { (wchar_t*)L"wide" };  // Now accepted in Microsoft mode.
