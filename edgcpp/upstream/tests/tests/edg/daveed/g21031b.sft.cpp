//remark:GNU constant folding
//options:--gcc --c11;fp:--c11;fn

typedef void * tr_sys_dir_t;
_Static_assert (((tr_sys_dir_t)((void *)0)) == ((void *)0), "values should match");
_Static_assert ((((void *)0)) == ((void *)0), "values should match");

