//type:fp
//options_all:--c++14 -tused -A
 template<class T> void j(T const(&)[3]);

int main()
{
     j({42});                 // T deduced to int, array bound not considered
}
