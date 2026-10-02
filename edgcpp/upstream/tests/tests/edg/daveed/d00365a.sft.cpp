//remark:Flexible array initializers
//type:fp
//name:
//options:--gcc;fn:--gcc -DPOS;fp:--microsoft --c;fp:--microsoft;fn:--microsoft -DPOS;fp:--c99 -DPOS;fn
//options_all:
//cases:
//source_files:
//input_files:
//output_files:
//ulimit:
//linker_options:
//execution_args:

  struct S {
    int n;
    int a[];  // Flexible array member.
  } s = { 1, { 2, 3, 4 } };  // Now accepted in GNU C and Microsoft modes.

#if !(defined(__GNUC__) && defined(POS))
  struct X {
    struct S s;
  } x = { { 1, { 2, 3, 4 } } };  // Accepted in Microsoft modes, but not in
                                 // GNU C mode.
#endif

#ifdef __cplusplus
  struct D {
    D(int);
#ifndef POS
    ~D();
#endif
  };
  struct E {
    int n;
    D d[];
  } e = { 1, { 2, 3, 4 } };  // Not accepted in any mode.
#endif

