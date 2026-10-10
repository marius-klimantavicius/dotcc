namespace std {
template<class T> class initializer_list {
  const T* data_; __SIZE_TYPE__ size_;
public:
  constexpr initializer_list(const T* pointer,__SIZE_TYPE__ count):data_(pointer),size_(count){}
  const T* begin()const{return data_;}
  const T* end()const{return data_+size_;}
  __SIZE_TYPE__ size()const{return size_;}
};
}
extern "C" void _Exit(int);
int live=0,fail=0;
struct Report {~Report(){if(live!=0)_Exit(61);}} report;
__attribute__((init_priority(200))) Report early_report;
struct Item {
  int value;
  Item(int n):value(n){if(n==fail)throw n;++live;}
  Item(const Item& other):value(other.value){++live;}
  ~Item(){--live;}
};
struct Pair {const char* name;Item value;};
Item make(std::initializer_list<Pair> items){return Item((int)items.size());}
struct Row {const char* name;Item item;};
Row rows[]={{"a",Item(1)},{"b",make({{"x",Item(2)},{"y",Item(3)}})},{"c",Item(4)},{"d",make({{"u",Item(5)},{"v",Item(6)}})}};
int main(){
  if(live!=4 || rows[1].item.value!=2)_Exit(1);
  for(fail=0;fail<=6;++fail){
    try {
      Row local[]={{"a",Item(1)},{"b",make({{"x",Item(2)},{"y",Item(3)}})},{"c",Item(4)},{"d",make({{"u",Item(5)},{"v",Item(6)}})}};
      if(fail || live!=8 || local[1].item.value!=2)_Exit(2);
    } catch(int value){if(value!=fail)_Exit(3);}
    if(live!=4)_Exit(4);
  }
  return 0;
}
