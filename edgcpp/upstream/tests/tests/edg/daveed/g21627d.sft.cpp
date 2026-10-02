//remark:GNU bug regression
//options:--c++14 --gnu=80300;fp

template< typename T >
class A
{
protected:
    T value;

public:
    template< typename U >
    A& operator=(U v)
    {
        value = v;
        return *this;
    }
};

template< typename T >
class B : public A<T>
{
public:
    using A<T>::operator=;

    template< typename U >
    B& operator=(U v)
    {
        this->value = v;
        return *this;
    }
};

int main()
{
    B<int> obj;
    obj = 2;
}
