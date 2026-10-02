//type: fp
//options: 
# 0 "./auto-init-uninit-pred-1_a.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./auto-init-uninit-pred-1_a.C"


# 1 "./uninit-pred-1_a.C" 1



typedef long long int64;
void incr ();
bool is_valid (int);
int get_time ();

class A
{
public:
  A ();
  ~A () {
    if (I) delete I;
  }

private:
  int* I;
};

bool get_url (A *);

class M {

 public:
__attribute__ ((always_inline)) int GetC (int *c) {

    A details_str;
    if (!get_url (&details_str))
      {
        incr ();
        return 1;
      }

    *c = get_time ();
    return -1;
  }

  void do_sth();
  void do_sth2();

  void P (int64 t)
    {
      int cc;
      if (GetC (&cc) >= 0 )
        return;

      if (t && cc <= 0 )
        {
          this->do_sth();
          return;
        }

    do_sth2();
  }
};

M* m;
void foo(int x)
{
  m = new M;
  m->P(x);
}
# 4 "./auto-init-uninit-pred-1_a.C" 2
