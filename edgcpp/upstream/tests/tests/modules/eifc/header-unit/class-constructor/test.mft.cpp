//type:cp
//header_unit_files:test.h
//options_all:--module_import_diagnostics -d-module_report --c++20
import "test.h";

my_class value_a = my_class();
my_class value_b = my_class(10);
my_class value_c = my_class(10, 20);
