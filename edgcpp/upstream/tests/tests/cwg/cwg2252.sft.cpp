//options_all:--c++23 -tused 
int main()
{
 enum class E {};
  struct X { operator E(); };
  E e = E{X()}; // ok? 
}

//cwg: 2252
//title: Enumeration list-initialization from the same type
//meeting: Kona 11/23
//edg_status: Passes
