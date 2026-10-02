//type:rp
//options_all:--c++14 -tused -A
  int a;
  int* p1 = &a;
  int* p2 = p1 + 1;    // Defined behavior

int main()
{
  bool b = p2 > p1;    // Defined behavior, with value 
  if (!b)
    return(1);
  return(0);
}

//cwg: 1596
//title: Non-array objects as array[1]
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
