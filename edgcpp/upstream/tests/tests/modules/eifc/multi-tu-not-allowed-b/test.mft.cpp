//type:fc
//require:EXPORT_ENABLING_POSSIBLE
//header_unit_files:test.h
//options_all:--c++20 --export
import "test.h";

int bar() {
  return do_stuff(0, 1);
}
