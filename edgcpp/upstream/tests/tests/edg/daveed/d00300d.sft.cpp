//remark:Named address space qualifiers
//type:fn
//name:
//options:;fn:-DPOS;fp
//options_all:--c99 --embedded_c
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

int _EDG_NAS_A a;  /* OK */

void f() {
  static int _EDG_NAS_A a;  /* OK */
  extern int _EDG_NAS_B b;  /* OK */
#ifndef POS
  int _EDG_NAS_C c;  /* Error */
#endif
}
