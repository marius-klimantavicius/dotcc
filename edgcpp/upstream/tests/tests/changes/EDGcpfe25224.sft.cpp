//type:fp
//options_all:--gcc
//remark:[6.7] Abort on _Generic construct with GNU statement expression
// 1/25/24  [EDGcpfe/25224,EDGcpfe/26158]
//
// Abort on _Generic construct with GNU statement expression
//
// This previously aborted in configurations involving the C++-generating back end
// with IL_SHOULD_BE_WRITTEN_TO_FILE set to 1 (or TRUE).  The abort was due to an
// inconsistent representation of the source sequence entries for the statement
// expression within the _Generic construct.  That is now fixed.
void g(void) {
  _Generic(0, int : 0, default : sizeof({}));
}
