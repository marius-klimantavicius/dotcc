//type:fp
//options_all:--gn 100200 --c++20
//remark:[6.2] Abbreviated function templates and source sequence entries
// 9/22/20  [EDGcpfe/23400]
//
// Abbreviated function templates and source sequence entries
//
// Configurations with GENERATE_SOURCE_SEQUENCE_LISTS set to TRUE usually aborted
// on abbreviated function templates because extraneous source sequence entries
// not pointing to any entities were recorded.
//
// That is now fixed.  In addition, the C++-generating back end has been updated
// to correctly render such abbreviated function templates.
auto f(auto i) { return 2*i; }  // Previously aborted in some configurations.
