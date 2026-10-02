// EDG's __exception_started marker precedes the non-elided payload copy.
// Native Itanium runtimes mark the throw after that copy; preserve both oracles.
namespace std {int uncaught_exceptions() noexcept;}
#ifndef EDG_SOURCE_NATIVE_COPY_COUNT
#define EDG_SOURCE_NATIVE_COPY_COUNT 1
#endif
static int bad,copies;
struct Payload {
  Payload() {}
  Payload(const Payload&) {
    ++copies;
    int expected=copies==1?EDG_SOURCE_NATIVE_COPY_COUNT:1;
    if(std::uncaught_exceptions()!=expected)++bad;
  }
  ~Payload() {if(std::uncaught_exceptions()!=0)++bad;}
};
struct Guard {~Guard(){if(std::uncaught_exceptions()!=1)++bad;}};
int main() {
  {Payload original;
   try {Guard guard;throw original;}catch(Payload) {if(std::uncaught_exceptions()!=0)++bad;}
  }
  return bad || copies!=2;
}
