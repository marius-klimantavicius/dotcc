//type:fn
//remark:[4.8] Missing error on invalid braced initializer for character array
// 9/6/13   [EDGcpfe/14435]
//
// Missing error on invalid braced initializer for character array
//
// The changes to support initializer lists in version 4.5 of the front end (see
// the entry of 8/26/12 for EDGcpfe/9170) caused the front end to erroneously
// accept a braced initializer for a character array where the first element in
// the braces is a string literal matching the array type but additional
// initializers follow.  The extraneous initializers were silently ignored.
//
// This problem is now fixed: One or more errors are now issued for such cases.
char const s[6] = { "a", 1 };  // Previously silent accepted; now an error.
