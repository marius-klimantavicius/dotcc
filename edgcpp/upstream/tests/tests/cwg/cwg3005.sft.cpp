//type:fn
//options:--c++26
//options_all:-A

void f()
{
  int _ = 1;
  int _ = 2;                    // OK
}

void g(int _, int _)            // error
{ }

//cwg: 3005
//title: Function parameters should never be name-independent
//meeting: Kona 11/25
//edg_status: Passes
