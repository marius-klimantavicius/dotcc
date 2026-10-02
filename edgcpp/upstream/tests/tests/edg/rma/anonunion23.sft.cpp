//options_all:-r -x -tused
//options: --cfront_3.0;cn

// Used to get internal error with --cfront_2_1 --exceptions
struct S {
  S& operator=(const struct S& s);
};
 
struct T {
  union U {
    S s;
  };
};
 
int main()
{
  T u1, u2;
  u1 = u2;
}

