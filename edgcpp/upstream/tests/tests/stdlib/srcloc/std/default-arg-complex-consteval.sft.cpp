//type:cp
//options::--g++:--microsoft:--clang
//options_all:--c++20 --no_standard_includes --sys_include=$RUN_TEST_CURR_DIR/srcloc/inc --sys_include=$RUN_TEST_CURR_DIR/inc

#include <source_location>

using namespace std;

consteval source_location f(source_location p = source_location::current()) {
  return p;
}
consteval source_location g(source_location p = f()) { return p; }
consteval source_location go(int x, source_location p = g()) { return p; }
constexpr source_location pos_in_question = go(0);

#include <edg_constexpr_printing>

consteval int constexpr_main() {
  __report_constexpr_value("Line: ");
  __report_constexpr_value(pos_in_question.line());
  __report_constexpr_value("\n");
  __report_constexpr_value("Column: ");
  __report_constexpr_value(pos_in_question.column());
  __report_constexpr_value("\n");
  __report_constexpr_value("Function: ");
  __report_constexpr_value(pos_in_question.function_name());
  __report_constexpr_value("\n");
  __report_constexpr_value("File: ");
  __report_constexpr_value(pos_in_question.file_name());
  __report_constexpr_value("\n");
  return 0;
}

int main() {
  return constexpr_main();
}
