//type:fp
//remark:[4.12] IL write-read error on type declared in function template declaration
// 9/22/16  [EDGcpfe/17517]
//
// IL write-read error on type declared in function template declaration
//
// In configurations with the configuration macros GENERATE_SOURCE_SEQUENCE_LISTS
// and IL_SHOULD_BE_WRITTEN_TO_FILE set to TRUE, the front end could abort with
// an IL write-read error (an internal error) when a class is first declared in a
// function template declaration but parsing of function templates is disabled.
//
// This is now fixed.
template<class V> void g(struct C*) {}
  // C is first declared in this function template.  Previously,
  // this could result in an internal error in some configurations.
