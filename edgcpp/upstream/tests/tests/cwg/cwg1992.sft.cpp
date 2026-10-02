//type:rp
//options_all:--c++17 -A
typedef decltype(sizeof(0)) size_t;
namespace std { struct nothrow_t { } nothrow; }

void *operator new[](size_t                size,
                     const std::nothrow_t& nothrow_arg);

int main()
{
  try {
    int c2 = (-2);
    // Throws via generated call to __throw_bad_array_new_length
    char (*p)[2][3] = new (std::nothrow) char [c2][2][3];
  } catch (...) {
    return 1;
  }
  return 0;
}

//cwg: 1992
//title: new (std::nothrow) int[N] can throw
//meeting: Jacksonville 2/16
//edg_status: EDGcpfe/21035
