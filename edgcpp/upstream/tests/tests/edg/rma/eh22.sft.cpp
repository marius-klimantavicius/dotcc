//options_all:-r -x -tused
//options: --strict;cn:;rp

extern "C" void printf(char *, ...);
struct base1 { };
struct base2: base1 { };
struct base3: base1 { };
struct derived: base2, private base3 { };
main() {
  try { throw base2(); }
  catch (base1) { printf("caught base1\n"); }
  catch (base2) { printf("caught base2\n"); }    // handler is masked
  catch (...)   { printf("default\n"); }
  try { throw derived(); }
  catch (base1) { printf("caught base1\n"); }
  catch (base2) { printf("caught base2\n"); }    // handler isn't masked
  catch (...)   { printf("default\n"); }
  try { throw base3(); }
  catch (base1) { printf("caught base1\n"); }
  catch (base3) { printf("caught base3\n"); }    // handler is masked
  catch (...)   { printf("default\n"); }
  try { throw derived(); }
  catch (base1) { printf("caught base1\n"); }
  catch (base3) { printf("caught base3\n"); }    // handler isn't masked
  catch (...)   { printf("default\n"); }
}

