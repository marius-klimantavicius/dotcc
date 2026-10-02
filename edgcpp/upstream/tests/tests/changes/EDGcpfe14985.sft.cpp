//type:fp
//options_all:--microsoft
//remark:[4.10] Microsoft compatibility: String literals in array of character initializers
// 7/9/14   [EDGcpfe/14985]
//
// Microsoft compatibility: String literals in array of character initializers
//
// In Microsoft modes, the front end now treats a string literal appearing in the
// braced initializer for an array of characters matching the string literal type
// as if the literal were a comma-separated list of characters instead.
//
// is now treated as
//
// in Microsoft mode (assuming an ASCII-like character encoding).  A string
// literal in such a case cannot be followed by another initializer:
char str[] = { 48, "123" };
