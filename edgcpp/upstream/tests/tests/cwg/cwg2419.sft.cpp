//type:rp
//options_all:--c++17 -tused -A

  int *p1 = new int;
  int *p2 = &*p1;

int main()
{
  bool b1 = p1 < p1+1;  // well-defined
    if (!b1)
        return(1);
  bool b2 = p2 < p2+1;  // well-defined
    if (!b2)
         return(2);
    return(0);
}

//cwg: 2419
//title: Loss of generality treating pointers to objects as one-element arrays
//meeting: Belfast 11/19
//edg_status: Passes
