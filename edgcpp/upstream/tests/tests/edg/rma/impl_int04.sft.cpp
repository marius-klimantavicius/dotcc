//options_all:-r -x -tused
//options: --cfront_3.0;cn:--diag_warn=260;cn

/*
This test can be run in both C and C++ modes.  Here are the expected
results:

                   C     pcc    ANSI-C    C++   cfront  ANSI-C++
                         mode                   mode

  static f();	remark	silent	remark	warning	remark	error
  g();		warning	silent	error	warning	remark	error
  h(){}		remark	silent	remark	warning	remark	error
  extern i;	warning	warning	warning	warning warning	error
  j;		error	warning	error	error	error	error
  k = 0;	error	warning	error	error	error	error

*/

static f();
g();
h(){}
extern i;
j;
k = 0;

