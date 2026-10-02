//options_all:-r -x -tused
//options: --strict;cn

class A
{
 public:
     union  Z{};
};

class X
{
    union A::Z;            // Error: declarator missing. Declaration ill-formed
}ox;

