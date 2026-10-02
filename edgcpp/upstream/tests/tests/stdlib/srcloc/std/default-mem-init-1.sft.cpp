//type:cp
//options::--g++:--microsoft:--clang
//options_all:--c++20 --no_standard_includes --sys_include=$RUN_TEST_CURR_DIR/srcloc/inc --sys_include=$RUN_TEST_CURR_DIR/inc

#include <source_location>

using namespace std;

struct s {
  source_location member = source_location::current();
  int other_member;
  constexpr s(int blather) :
    other_member(blather)
  {}
};

constexpr source_location pos_in_question = s{10}.member;

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
