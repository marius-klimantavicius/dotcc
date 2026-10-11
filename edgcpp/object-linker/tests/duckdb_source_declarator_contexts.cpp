// These pointer signatures are reparsed inside actual function/class template
// instances. Their I argument retains the source begin() decltype recipe.
template<class C> auto begin(C& c)->decltype(c.begin()){return c.begin();}
struct Range {int values[2];int* begin(){return values;}};
template<class I>I same(I value){return value;}
template<class I>void discard(I){}
template<class I>struct Callbacks {I(*a)(I);void(*b)(I);};
template<class I>struct Partial;
template<class I>struct Partial<I*> {I*(*a)(I*);void(*b)(I*);};
template<class I>int invoke(I first,I(*a)(I),void(*b)(I)) {b(first);return a(first)==first;}
template<class I>int check(I first) {
  I(*a)(I)=same<I>;void(*b)(I)=discard<I>;
  Callbacks<I> callbacks{same<I>,discard<I>};
  b(first);callbacks.b(first);
  return a(first)==first && callbacks.a(first)==first && invoke(first,same<I>,discard<I>);
}
template<class I>int nested(I first) {
  I*(*a)(I*)=same<I*>;void(*b)(I*)=discard<I*>;
  Partial<I*> callbacks{same<I*>,discard<I*>};
  b(&first);callbacks.b(&first);return a(&first)==&first && callbacks.a(&first)==&first;
}
int main(){Range r{{1,2}};return check(begin(r)) && nested(begin(r)) ? 0:1;}
