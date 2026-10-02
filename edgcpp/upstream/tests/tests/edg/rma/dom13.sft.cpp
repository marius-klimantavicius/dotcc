//options_all:-r -x -tused
//options: --strict;cp

// EDGma00009

class V1 { public: virtual void Aout5() {} };
class V2 { public: virtual void Aout5() {} };

class A : public virtual V1, public virtual V2 { public: void Aout5() {}
};
class B : public virtual V1, public virtual V2 {};
class E : public A, public B {};

void f() { E e; e.Aout5(); }

