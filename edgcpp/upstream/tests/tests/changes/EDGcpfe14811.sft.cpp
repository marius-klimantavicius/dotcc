//type:fn
//options_all:--microsoft
//remark:[4.9] Microsoft bugs mode abort on invalid typename specifier
// 1/29/14  [EDGcpfe/14811]
//
// Microsoft bugs mode abort on invalid typename specifier
//
// In Microsoft bugs mode, the front end previously aborted on certain invalid
// "typename" specifiers (typically due to a null pointer being passed to
// is_incomplete_type).
//
// This is now fixed.
auto x = typename(); // Previously aborted in is_incomplete_type.
