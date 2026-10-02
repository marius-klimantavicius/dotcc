//type:fp
//options_all:--microsoft
//remark:[4.4] Microsoft C++ compatibility: Nested classes in __interface classes
// 9/20/11  [EDGcpfe/12200]
//
// Microsoft C++ compatibility: Nested classes in __interface classes
//
// Previously, the front end did not accept nested classes in __interface
// classes.  Now such nested classes are accepted.
__interface IC {
  struct N {};  // Previously an error; now okay.
};
