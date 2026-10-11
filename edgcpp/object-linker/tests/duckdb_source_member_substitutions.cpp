// Ordinary class-template members borrow the enclosing class arguments; they
// have no independent routine template argument list.
template<class C> auto begin(C& c)->decltype(c.begin()){return c.begin();}
struct Range {int values[2];int* begin(){return values;}};
template<class I>struct Unwrap {static I unwrap(I value){return value;}};
template<class I>struct Call {static I call(const I& value){return value;}};
template<class I>struct Partial;
template<class I>struct Partial<I*> {static I from_partial(I* value){return *value;}};
template<class I>I identity(I value){return value;}
template<class I>int check(I first){
    return Unwrap<I>::unwrap(first)==Call<I>::call(first) &&
           Partial<I*>::from_partial(&first)==identity(first);
}
int main(){Range r{{1,2}};return check(begin(r))?0:1;}
