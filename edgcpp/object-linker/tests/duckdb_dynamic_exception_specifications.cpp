extern "C" void dynamic_empty_throw_specification() throw() {}
extern "C" void _Exit(int);
namespace std {
  typedef void (*unexpected_handler)();
  unexpected_handler set_unexpected(unexpected_handler) noexcept;
}
static int destroyed;
struct Guard { ~Guard() { ++destroyed; } };
struct Allowed { int value; };
static void permitted(int mode) throw(int, Allowed) {
  Guard guard;
  if(mode==0)throw 37;
  throw Allowed{41};
}
static void forbidden() throw() { Guard guard; throw 11; }
static void saw_unexpected() { _Exit(destroyed==1?73:74); }
int main(int argc, char**) {
  if(argc>1) {
    destroyed=0;
    std::set_unexpected(saw_unexpected);
    forbidden();
    return 99;
  }
  try { permitted(0); } catch(int value) { if(value!=37)return 1; }
  try { permitted(1); } catch(const Allowed &value) { if(value.value!=41)return 2; }
  return destroyed==2?0:3;
}
