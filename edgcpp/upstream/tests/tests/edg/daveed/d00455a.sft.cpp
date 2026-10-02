//remark:GNU C++ in-class static const pointer initializer
//type:rp
//name:
//options:--g++ --gnu_version=30200:--g++ --gnu_version=30300;fn:;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

struct foo {
     static const char * const str = "test";
} f;

char const *p = foo::str;

char const*const foo::str;

extern "C" int printf(char const*, ...);

int main() {
	printf("%s\n", p);
}
