//type:fp
//options_all:--microsoft_version=1915 -tused -w
//remark:[6.4] Microsoft mode regression with __restrict parameter
// 8/15/22  [EDGcpfe/25530]
//
// Microsoft mode regression with __restrict parameter
//
// The changes for EDGcpfe/24520 (in version 6.3) introduced a regression in
// Microsoft mode.
//
// The changes for EDGcpfe/24520 caused the front end to consider the redefinition
// of F to be incompatible with the original definition of F, which in turn caused
// the front end to emit an error on the redefinition.  However, it appears that
// MSVC ignores the __restrict qualifier such contexts.  The front end now
// emulates that behavior in Microsoft mode.
using F = void (void *__restrict);
using F = void (void *);  // Previously an error.  Now okay.
