struct S {
    int member;
    S(int value):member(value){}
    S(S&& other):member([](int& value)->decltype((value)){return value;}(other.member)){}
};
auto echo(int value, double unused) -> decltype(value) {return value + int(unused);}
auto other(int different) -> decltype(different) {return different;}
auto nested(int value) -> void(*)(decltype(value)) {return nullptr;}
void accepts(int value, auto(*function)(int inner)->decltype(value)) { (void)value; (void)function; }
template<class T> concept Nested = requires(T value) {
    requires requires(decltype(value) inner) { inner + value; };
};
static_assert(Nested<int>);
template<Nested T> struct NestedOwner { T value = T(); };
int identity(int value){return value;}
int main(){int check_value=17;auto converted=+[](int& value)->decltype((value)){return value;};if(converted(check_value)!=17)return 2;S original(17);S moved(static_cast<S&&>(original));accepts(4,identity);return moved.member==17 && echo(3,0)==3 && other(8)==8 && nested(4)==nullptr ? 0 : 1;}
