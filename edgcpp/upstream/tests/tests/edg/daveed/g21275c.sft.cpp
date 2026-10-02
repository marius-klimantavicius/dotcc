//remark:Deduction of auto template parameters
//options:--c++17;fp

template<auto>
struct Const {};

template<auto A>
auto function(Const<A>) -> Const<(decltype(A))A>
{
    return {};
}

int main()
{
    function(Const<1>{});
    return 0;
}
