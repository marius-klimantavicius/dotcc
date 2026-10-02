//type: fp
//options: 
# 0 "./opt/vrp3-aux.cc"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./opt/vrp3-aux.cc"



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
# 5 "./opt/vrp3-aux.cc" 2

R::R ()
{
  r1 = r2 = 1;
}

R::R (int n, int d)
{
  r1 = n;
  r2 = d;
}

int
R::compare (R const &r, R const &s)
{
  return (int) (r.r1 * s.r2 - s.r1 * r.r2);
}
