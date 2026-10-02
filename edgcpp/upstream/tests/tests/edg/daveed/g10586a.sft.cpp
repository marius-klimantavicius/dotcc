//remark:Alias templates
//options:--c++0x;fp:--c++0x -A;fp

extern "C" void exit(int);

template <class T> struct A {template <class U> using _A1_ = U;
static _A1_<int> x ; 
};

template <class T> int A<T>::x;

A<short*> a;

int main()
{
        if (a.x)
                exit(1);

        return 0;
}
