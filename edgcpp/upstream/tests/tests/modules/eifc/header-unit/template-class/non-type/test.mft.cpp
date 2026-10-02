//type:rp
//header_unit_files:test.h
//cases:3
//options_all:--module_import_diagnostics -d-module_report --c++20
import "test.h";

int main() {
  add_ten_class<-10> value;
  return value.result_value;
}
