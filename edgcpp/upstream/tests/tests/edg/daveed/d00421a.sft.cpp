//remark:format attribute on member calls
//type:fp
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

struct S {
  void f(char const *fmt, ...) __attribute__((format(printf, 2, 3)));
};
void f(S &s) {
  s.f("%s %d", "x", 3);
}
