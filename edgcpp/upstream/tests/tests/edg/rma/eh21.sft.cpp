//options_all:-r -x -tused
//options: --strict;cn:;rp

extern "C" void printf(char *, ...);

struct base1 {
  base1() {}
  ~base1() {}
};

struct base2: base1 {
  base2() {}
  ~base2() {}
};

struct base3: base1 {
  base3() {}
  ~base3() {}
};

struct derived: base2, base3 {
  derived() {}
  ~derived() {}
};

extern void f() { throw derived(); }

void g()
{
  try {
    f();
  }
  catch (base1) { printf("caught base1\n"); }
  catch (derived) { printf("caught derived\n"); }
}

main()
{
  g();
}

