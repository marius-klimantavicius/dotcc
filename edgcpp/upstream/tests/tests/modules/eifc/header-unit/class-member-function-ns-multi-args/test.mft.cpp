//type:cp
//header_unit_files:test.h
//options_all:--module_import_diagnostics -d-module_report --c++20
import "test.h";

int bar() {
  my_class value;

  return value.do_stuff(0, 1);
}
