//type: rp
//options:  vrp3-aux.cc
# 0 "./opt/vrp3.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./opt/vrp3.C"





# 1 "./opt/vrp3.h" 1
struct R
{
  long long r1, r2;
  void copy (R const &r) { r1 = r.r1; r2 = r.r2; }
  R ();
  explicit R (int, int);
  R (R const &r) { copy (r); }
  static int compare (R const &, R const &);
};
# 7 "./opt/vrp3.C" 2

struct M
{
  M (R m);
  R val;
  static int compare (M const &, M const &);
};

inline M const &
min (M const & t1, M const & t2)
{
  return R::compare (t1.val, t2.val) < 0 ? t1 : t2;
}

M::M (R m)
{
  val = m;
}

M
test (M *x)
{
  M n (R (0, 0));

  for (int i = 0; i < 2; i++)
    {
      M p = x[i];
      n = min (n, p);
    }

  if (n.val.r2 != 2 || n.val.r1 != 1)
    __builtin_abort ();
  return n;
}

int
main ()
{
  M x[2] = { M (R (1, 2)), M (R (1, 1)) };
  test (x);
}
