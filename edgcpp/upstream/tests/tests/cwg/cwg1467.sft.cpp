//type:rp
//options_all:--c++17 -tused -A
namespace std
{
  template<class _E>
  struct initializer_list
  {
      typedef const _E* iterator;
      typedef const _E* const_iterator;
      iterator _M_array;
      unsigned int _M_len;
      constexpr initializer_list() noexcept
      : _M_array(0), _M_len(0) { }
     constexpr initializer_list(const_iterator __a, int __l)
      : _M_array(__a), _M_len(__l) { }
  };
}
 
int a_(int) { return 1; }
int a_( std::initializer_list<char> ) { return 2; }
int b_() {
    return a_( {3} );
}
 
 
int main()
{
  if (b_() != 2)
	return(1);
  return 0;
}

//cwg: 1467
//title: List-initialization of aggregate from same-type object
//meeting: Urbana-Champaign 11/14
//edg_status: EDGcpfe/16802
//fixed_in: 6.7
