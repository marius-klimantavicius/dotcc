//type:fp
//options_all:--microsoft
//remark:[4.4] Microsoft compatibility: enumerator constant overflow
// 10/4/11  [EDGcpfe/12251]
//
// Microsoft compatibility: enumerator constant overflow
//
// In Microsoft mode, when an enumerator constant value is too large to be
// represented by the underlying integral type, the front end now issues a
// warning (instead of an error) and uses the lower bits of the value
// representation for the enumerator value.
enum E: signed char {
  e = 1000  // Previously an error.  Now a warning, and e has the
};          // value -24.
