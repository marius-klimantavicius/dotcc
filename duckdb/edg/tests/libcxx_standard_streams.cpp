#include <iostream>
#include <sstream>
#include <type_traits>
// A locale implementation macro must not leak into the consumer.
namespace consumer { enum class DatePart { ERA }; }
#include <langinfo.h>
static_assert(std::is_same<decltype(std::cout), std::ostream>::value, "cout object type");
static_assert(std::is_same<decltype(std::cin), std::istream>::value, "cin object type");
static_assert(std::is_same<decltype(std::cerr), std::ostream>::value, "cerr object type");
static_assert(std::is_same<decltype(std::clog), std::ostream>::value, "clog object type");
static_assert(std::is_same<decltype(std::wcin), std::wistream>::value, "wcin object type");
static_assert(std::is_same<decltype(std::wcout), std::wostream>::value, "wcout object type");
static_assert(std::is_same<decltype(std::wcerr), std::wostream>::value, "wcerr object type");
static_assert(std::is_same<decltype(std::wclog), std::wostream>::value, "wclog object type");
#ifndef ERA
#error explicit public langinfo include must still expose ERA
#endif
static bool early_initialized;
struct EarlyCheck {
 EarlyCheck() { early_initialized = std::cout.rdbuf() && std::cin.tie()==&std::cout; }
} early_check;
int main() {
 if (!early_initialized || std::cerr.tie()!=&std::cout) return 1;
 if (std::clog.rdbuf()!=std::cerr.rdbuf() || !(std::cerr.flags()&std::ios_base::unitbuf)) return 2;
 if (std::wcin.tie()!=&std::wcout || std::wcerr.tie()!=&std::wcout) return 3;
 if (std::wclog.rdbuf()!=std::wcerr.rdbuf() || !(std::wcerr.flags()&std::ios_base::unitbuf)) return 4;
 std::wostringstream wide_output;
 auto old_wcout_buffer=std::wcout.rdbuf(wide_output.rdbuf());
 std::wcout << L"wide:" << 42;
 std::wcout.rdbuf(old_wcout_buffer);
 if (wide_output.str()!=L"wide:42") return 7;
 std::wistringstream wide_input(L"23");
 auto old_wcin_buffer=std::wcin.rdbuf(wide_input.rdbuf());
 int wide_value=0;std::wcin>>wide_value;
 std::wcin.rdbuf(old_wcin_buffer);
 if (wide_value!=23) return 8;
 int value=0;std::cin>>value;if(value!=41)return 5;
 std::cout << "standard streams: " << value+1 << '\n';
 std::cerr << "standard stream error channel\n";
 std::clog << "standard stream log channel\n";
 return 0;
}
