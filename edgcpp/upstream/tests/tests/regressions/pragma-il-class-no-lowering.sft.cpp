//type:fp
//options_all:--c++11 --no_il_lowering --il_display
//require:DO_IL_LOWERING 1
//require:INCLUDE_EDG_TEST_PRAGMAS 1
//match_regex:^file-scope field@X+\nsource_corresp:(\n  .+?)+?\n  has_associated_pragma:\s+?TRUE
//match_regex:^file-scope scope@X+(\n.+?)+?\nkind:\s+?sck_class_struct_union(\n.+?)+?\npragmas:\s+?file-scope pragma@X+

// This test checks to ensure that the pragma is added to the class scope and
// the associated field is marked as having a pragma.

class foo {
  #pragma test_next_decl
  int x;
};
