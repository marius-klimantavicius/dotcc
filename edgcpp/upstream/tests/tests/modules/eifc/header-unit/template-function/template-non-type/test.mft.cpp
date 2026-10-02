//type:rp
//header_unit_files:test.h
//options_all:--module_import_diagnostics -d-module_report --c++20
import "test.h";

template<int X>
struct wrapper {
  wrapper()
    : value(X)
  {}
  int value;
};


int main() {
  wrapper<-10> wrapped_value;
  return add_ten(wrapped_value);
}
