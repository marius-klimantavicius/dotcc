extern "C" int unavailable_function(int);
extern "C" int unavailable_variable;
extern "C" int call_unavailable() { return unavailable_function(7); }
extern "C" int indirect_unavailable() {
  int (*target)(int)=unavailable_function;
  return target(9);
}
extern "C" int read_unavailable() { return unavailable_variable; }
extern "C" int* address_unavailable() { return &unavailable_variable; }
extern "C" int read_constant_address() { int* const address=&unavailable_variable;return *address; }
extern "C" int ordinary_function() { return 42; }
