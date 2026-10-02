//type:cp
//header_unit_files:test.h
//options_all:--module_import_diagnostics -d-module_report --c++20
import "test.h";

int bar() {
  return do_stuff();
}
