//options_all:-r -x -tused
//options: --strict;cn:;cn


class T {
public:
        int a;
        T * tp; 
        T(int i=0) : a(i){ tp=0; }
        T(T& t, T t1=5){a=t.a; tp= &t1; }
        ~T(){}
};

int    main (){
        T x(1), y(2);
        T z(T(3),T(4));
        T s=y;
}

