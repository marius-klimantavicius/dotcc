//type:fp
//options_all:--microsoft_version 1929 --ms_c++17
//remark:[6.7] Microsoft compatibility: Direct reference-binding via user-defined conversion
// 12/24/24 [EDGcpfe/24533,EDGcpfe/24622,EDGcpfe/25528,EDGcpfe/27754]
//
// Microsoft compatibility: Direct reference-binding via user-defined conversion
//
// The front end emulates an old Microsoft-mode reference binding bug that makes
// the example above ambiguous.  However, MSVC has since fixed that nonstandard
// behavior and the front end now restores the corresponding standard behavior
// when microsoft_version >= 1900.
struct S {
  struct C {};
  operator C&();
  operator const C&() const;
  void g() {
    C const& c(*this);  // Previously ambiguous.  Now okay.
  }
};
