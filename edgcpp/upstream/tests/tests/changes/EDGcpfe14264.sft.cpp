//type:fp
//options_all:--microsoft
//remark:[4.8] IL write-read error with Microsoft attributes in parameter types
// 7/2/13   [EDGcpfe/14264]
//
// IL write-read error with Microsoft attributes in parameter types
//
// In cases where a routine definition and a previous declaration of the routine
// differ (e.g., because of default arguments), the process of reconciling the
// declarations had inadvertently lost any Microsoft attributes that were applied
// to parameter types, resulting in an IL write-read assertion failure in some
// configurations.
void f([SA_Pre(Null=SA_No)] int x = 0);
void f([SA_Pre(Null=SA_No)] int x) {}
