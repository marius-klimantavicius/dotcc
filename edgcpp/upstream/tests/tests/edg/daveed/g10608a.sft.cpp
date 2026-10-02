//remark:Exception specification compatibility
//options:--c++;fp:--c++11;fp

    typedef void f();
    void g() throw(f);
    void g() throw(f*);
    void h() throw(int);
    void h() throw(const int);
