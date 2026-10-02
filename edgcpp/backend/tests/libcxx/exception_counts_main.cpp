#include <exception>
static int cleanup_count=-1;
static int cleanup_handled=-1;
struct Guard {~Guard(){cleanup_count=std::uncaught_exceptions();cleanup_handled=!!std::current_exception();}};
extern "C" int counts_now() {return std::uncaught_exceptions();}
extern "C" int main_cleanup_ok() {return cleanup_count==1 && cleanup_handled==0;}
int main() {Guard guard;throw 303;}
