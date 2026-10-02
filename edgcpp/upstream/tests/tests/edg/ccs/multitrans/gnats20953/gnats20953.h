inline int foo = 1;

inline int ifct( void )
{
   return foo;
}

class C
{
public:
   static constexpr int cfoo = 1;
};
