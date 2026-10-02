//type:fn
//options_all:--c++20 -tused
int main()
{
  for (int i = 0; i< 10; ++i){
  auto f = [](){
    break; // #1
  };
}
}

//cwg: 2616
//title: Imprecise restrictions on break and continue
//meeting: Kona 11/22
//edg_status: Passes
