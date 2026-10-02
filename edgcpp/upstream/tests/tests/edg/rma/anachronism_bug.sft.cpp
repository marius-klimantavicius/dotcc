//options_all:-r -x -tused
//options: --strict;cn

class A {
public:
  operator double();
};
int main()
{
  A a;
  a++;
}

