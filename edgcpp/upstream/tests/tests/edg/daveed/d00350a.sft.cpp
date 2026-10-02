//remark:Microsoft for-init scope
//type:fp
//name:
//options:;fp:-DNEG;fn
//options_all:--microsoft_version=1310
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

struct S { S(); ~S(); };
void f() {
  for (S *p, s; false; ) {}
  p = 0;
#ifdef NEG
  &s;
#endif
}

