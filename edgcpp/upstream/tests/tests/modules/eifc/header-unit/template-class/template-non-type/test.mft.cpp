//type:rp
//header_unit_files:test.h
//cases:3
//options_all:--module_import_diagnostics -d-module_report --c++20
import "test.h";

template<int X>
struct wrapper {
  wrapper()
    : value(X)
  {}
  wrapper(int value)
    : value(value)
  {}
  int value;
};


int main() {
  wrapper<-10>               wrapped_value;
  add_ten_via_proxy<wrapper> value(wrapped_value);
  return value.wrapper.value;
}
