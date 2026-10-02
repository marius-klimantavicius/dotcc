//type:fp
//options_all:--microsoft
//remark:[4.4] Microsoft C++ compatibility: Attributes on templates
// 8/25/11  [EDGcpfe/10939,EDGcpfe/11622,EDGcpfe/11883,EDGcpfe/11622,
//           EDGcpfe/11967, EDGcpfe/12073]
//
// Microsoft C++ compatibility: Attributes on templates
//
// In Microsoft C++ mode with microsoft_version >= 1400 the front end now accepts
// bracketed attributes (see Changes entry of 7/30/03) on function templates and
// class templates.
//
// See also the related Changes entry of 7/6/11.
struct S {
  template <class>
  [returnvalue:SA_Post(MustCheck=SA_Yes)]  // Now accepted in Microsoft
  int f() {                                // C++ mode.
    return 0;
  }
};
