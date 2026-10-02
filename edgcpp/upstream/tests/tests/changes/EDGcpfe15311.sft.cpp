//type:fp
//options_all:--clang --c++11
//remark:[4.10] Default gnu_version in clang mode
// 9/14/14  [EDGcpfe/15311]
//
// Default gnu_version in clang mode
//
// As noted in the description of the change for EDGcpfe/14634 (1/2/14), clang
// mode is treated in the front end as a variant of gnu mode.  Consequently,
// the feature set available in clang mode defaults to the features enabled by
// the value of gnu_version.  This dependency could inadvertently exclude some
// features (e.g., user-defined literals) that should be enabled in clang mode
// but are not supported with the default setting of gnu_version.  The front
// end has now been changed to set gnu_version to 40800 when clang mode is in
// effect and --gnu_version is not specified on the command line.
// with --clang --c++11:
int operator "" _a(const char *);  // Previously rejected (assuming default
                                   // gnu_version < 40700), now accepted
