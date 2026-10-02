//remark:Nonreal friends and SSEs
//options:--c++17 --g++;fp

template<class T> class Policy
{

public:
    template<class S> class Base
    {
        Base(S&);
    private:
        template<void (*A)(Base*)> struct XXX
        {
        };

        static void f(Base* t)
        {
            t->s.callPrivate();
        }

        XXX<&Base::f> a;
        S& s;
    };
};
template<class POLICY>
class C:
    public POLICY::template Base<C<POLICY>>
{
public:
    typedef C<POLICY> SelfType;

    static void call()
    {}

private:
    void callPrivate()
    {}

    friend typename POLICY::template Base<SelfType>;
};

void f()
{
    C<Policy<int>>::call();
}
