//type:cppbe
//options_all:--c++11
//require:INCLUDE_UNRECOGNIZED_PRAGMAS_IN_IL 1
//require:INCLUDE_EDG_TEST_PRAGMAS 1
//match_regex:#pragma warning\(push\)
//match_regex:#pragma warning\(pop\)

// This test checks to ensure that "push" and "pop" are not lost.

#pragma warning(push)
#pragma test_immediate "x"
#pragma warning(pop)
