//type:fn
//options_all:--c++17 -A -tused
//source_files:cwg2085b.C
//we can't really test for this, linker issue
struct X {
X(int, int);
X(int, int, int);
};
X::X(int, int = 0) { }
class D {
X x = 0;
};
D d1; // X(int, int) called by D()
int main()
{
}

//cwg: 2085
//title: Invalid example of adding special member function via default argument
//meeting: Jacksonville 2/16
//edg_status: N/A
//fixed_in: N/A
