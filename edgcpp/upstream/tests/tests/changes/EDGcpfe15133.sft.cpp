//type:fp
//remark:[4.10] Internal error in C++-generating back end on some local declarations
// 7/24/14  [EDGcpfe/15133]
//
// Internal error in C++-generating back end on some local declarations
//
// When a local declaration starts with a template-id that includes an argument
// that is also the first declaration of a class type, the front end previously
// produced an invalid sequence of source sequence entries.  This in turn resulted
// in an internal error in the C++-generating back end (in function
// check_for_and_take_source_seq_entry).
//
// This is now fixed.
template<typename> struct X {}; 
int main() {
  X<struct S> x;  // Previously triggered an internal error in the
}                 // C++-generating back end.
