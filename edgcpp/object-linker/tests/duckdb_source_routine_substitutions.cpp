// A substituted I is the exact original type of begin's trailing return.
// The nested pointer argument exercises borrowing below a signature wrapper.
template<class C> auto begin(const C& c) -> decltype(c.begin()) { return c.begin(); }
template<class I,class P> I find_if(I first,I last,P predicate) {
  for(;first!=last;++first)if(predicate(*first))return first;
  return last;
}
struct Range { int values[2];const int* begin()const{return values;} };
struct Predicate { bool operator()(int value)const{return value==2;} };
const int* selected_slot;
template<class C> auto slot(const C& c) -> decltype(c.begin())* {
  selected_slot=c.begin();return &selected_slot;
}
template<class I> I identity(I value) {return value;}
int main() {
  Range range{{1,2}};
  auto selected=find_if(begin(range),begin(range)+2,Predicate{});
  auto nested=identity(slot(range));
  return selected==range.values+1 && *nested==range.values ? 0 : 1;
}
