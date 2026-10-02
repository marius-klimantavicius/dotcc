//type:fp
//options_all:--clang
//remark:[4.9] clang compatibility: changes to __has_feature, __has_extension, and GNU
// 4/13/14  [EDGcpfe/14846,EDGcpfe/14872]
//
// clang compatibility: changes to __has_feature, __has_extension, and GNU
// version macros
//
// Several changes have been made to the implementation of the clang
// __has_feature and __has_extension feature-test macros.  First, contrary to
// published documentation but conforming to current clang implementations,
// the type trait helper names can now be tested with __has_feature;
// previously they could only be used with __has_extension.  Second, feature
// and type trait helper names can now have an optional "__" prefix and
// suffix.  Finally, the "attribute_deprecated_with_message" feature name is
// now supported, with the value 1 if the current emulation allows a string
// argument for the "deprecated" attribute (currently, if gnu_version is at
// least 40500 -- see the change for EDGcpfe/12407).
// --clang:
//
// In addition, the clang compiler unconditionally defines the GNU version
// macros __GNUC__, __GNUG__, __GNUC_MINOR__, and __GNUC_PATCHLEVEL__ to
// reflect version 4.2.1, and the front end has been changed to do the same in
// clang mode.  (This change does not affect the value of gnu_version, which
// should normally be set to a later version for maximum compatibility with
// the clang implementation.)
int i = __has_feature(__is_pod__);  // Now has value 1
// Now has value 1 if gnu_version >= 40500:
int j = __has_feature(attribute_deprecated_with_message);
