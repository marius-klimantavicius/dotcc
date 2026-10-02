//remark:GNU: init_priority attribute
//type:fn
//name:
//options:
//options_all:--g++
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

struct S {};

struct S f();

namespace N {
	struct S x __attribute__((__init_priority__(65536))) = f();
}
struct S y __attribute__((__init_priority__(1))) = f();

struct X {
	static struct S sm;
};
struct S X::sm __attribute__((__init_priority__(1000)));

void g() {
  struct S lx __attribute__((__init_priority__(102))) = f();
  static struct S sx __attribute__((__init_priority__(102))) = f();
}

#ifdef __sun__
// Should be negative test, but attribute ignored on Solaris
int int;
#endif
