// The provider closure uses the real allocator; no test-only exception stubs.
extern "C" void free(void *);
void operator delete(void *pointer, unsigned long) noexcept { free(pointer); }
static int mode, sequence, argument_constructed, argument_destroyed;
static int element_constructed, element_destroyed, value_sum;
static unsigned long long events;
static void event(unsigned value) { events = events * 17 + value; }
static int next_argument() { return ++sequence; }
struct Argument {
  int value;
  Argument(int n) : value(n) {
    ++argument_constructed; event(1);
    if(mode == 1 && n == 2) { event(7); throw 71; }
  }
  ~Argument() { ++argument_destroyed; event(2); }
};
struct Element {
  int value;
  Element(Argument argument = Argument(next_argument())) : value(argument.value) {
    ++element_constructed; event(3);
    if(mode == 2 && value == 2) { event(8); throw 72; }
  }
  ~Element() { ++element_destroyed; value_sum += value; event(4); }
};
static int first_site() { Element elements[3]; if(elements[2].value!=3)return 1; event(5); return 0; }
static int second_site() { Element elements[3]; if(elements[2].value!=3)return 1; event(5); return 0; }
static int run(int selected, int site) {
  mode=selected; sequence=argument_constructed=argument_destroyed=0;
  element_constructed=element_destroyed=value_sum=0; events=0;
  int caught=0;
  try { if(site ? second_site() : first_site())return 1; }
  catch(int value) { caught=value; event(6); }
  unsigned long long expected=0;
  if(selected==0) {
    unsigned order[]={1,3,2,1,3,2,1,3,2,5,4,4,4};
    for(unsigned value:order)expected=expected*17+value;
    return caught || sequence!=3 || argument_constructed!=3 || argument_destroyed!=3 ||
      element_constructed!=3 || element_destroyed!=3 || value_sum!=6 || events!=expected;
  }
  if(selected==1) {
    unsigned order[]={1,3,2,1,7,4,6};
    for(unsigned value:order)expected=expected*17+value;
    return caught!=71 || sequence!=2 || argument_constructed!=2 || argument_destroyed!=1 ||
      element_constructed!=1 || element_destroyed!=1 || value_sum!=1 || events!=expected;
  }
  unsigned order[]={1,3,2,1,3,8,2,4,6};
  for(unsigned value:order)expected=expected*17+value;
  return caught!=72 || sequence!=2 || argument_constructed!=2 || argument_destroyed!=2 ||
    element_constructed!=2 || element_destroyed!=1 || value_sum!=1 || events!=expected;
}
int main() { for(int site=0;site!=2;++site)for(int selected=0;selected!=3;++selected)if(run(selected,site))return site*3+selected+1; return 0; }
