//type:fc
//require:COMPILE_MULTIPLE_TRANSLATION_UNITS
//header_unit_files:test.h
//source_files:helper.c
//options_all:--c++20 --multi_trans_unit
import "test.h";

int bar() {
  return do_stuff(0, 1);
}
