//type:rp
//header_unit_files:test.h
//options_all:--module_import_diagnostics -d-module_report --c++20
import "test.h";

template<typename T>
struct a_container {
  T result_value;
};

int main() {
  an_alias<a_container> value{0};
  return value.result_value;
}
