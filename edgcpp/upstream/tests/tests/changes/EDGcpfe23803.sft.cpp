//type:fp
//options_all:--g++
//remark:[6.3] Source sequence entries and folded GNU statement expressions
// 7/27/21  [EDGcpfe/23803]
//
// Source sequence entries and folded GNU statement expressions
//
// In configurations with IL_SHOULD_BE_WRITTEN_TO_FILE, GNU_EXTENSIONS_ALLOWED,
// and GENERATE_SOURCE_SEQUENCE_LISTS set to TRUE, the front end could abort with
// an IL write-read error when a GNU statement expression was eliminated from the
// IL due to folding.
//
// In some configurations the GNU statement expression "({ 0; })" was folded to a
// simple zero constant and the expression was not kept in a backing expression.
// That resulted in IL write-read errors.  That is now fixed.
constexpr int f(int x) { return x; }
int g() {
  return f(({ 0; }));
}
