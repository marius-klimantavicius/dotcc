//type:rp
//options_all:--c++17 -tused -A
struct S {
  const char *p = "foo";
  S() {}
  S(int) {}
};
S a, b, c(0), d(0);

int main() {
  if (a.p != b.p)
	return(1);
  if (c.p != d.p)
	return(2);
  if (a.p != c.p)
	return(3);
  return(0);
}

//cwg: 1823
//title: String literal uniqueness in inline functions
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
