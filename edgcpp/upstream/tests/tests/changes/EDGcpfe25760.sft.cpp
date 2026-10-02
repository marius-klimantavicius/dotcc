//type:fp
//options_all:--set_flag force_ms_type_info_not_in_namespace_std --target=win32 "--microsoft" "--microsoft_bugs" "--microsoft_version" "1928" "--pack_alignment" "8" "--rtti" "--new_for_init" "--wchar_t_keyword" "--c++" "--no_warnings" "--no_deprecated_string_conv" "--exceptions" "--ms_c++latest" "--no_ms_permissive" "--no_code_gen" --set_flag=no_very_expensive_checking
//remark:[6.8] Instantiation of friend function template noexcept specifier
// 9/15/25  [EDGcpfe/25760,EDGcpfe/28425]
//
// Instantiation of friend function template noexcept specifier
//
// Ordinarily, the noexcept specifier of a function template is instantiated when
// the exception specification is needed.  However, for a friend function
// template, the instantiation was not always delayed when the enclosing class
// template was instantiated from within an explicit specialization declaration.
template<typename T> struct D {
  template<typename U>
  friend void f(D, U) noexcept(T::v);  // Previously an error.  Now okay.
};
template<typename> struct C;
template<> struct C<int> {
  D<int> d;
};
