//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cp

extern "C" int printf( const char*, ... );

class T
{
        int i;
public:
        T(int n) : i(n>0?n:0)
        {
                printf( "T::T(%d) called (i is %d)\n", n, i );
        }
        operator int()
        {
                printf( "T::operator int() called (returns %d)\n", i );
                return i;
        }
        ~T()
        {
                printf( "T::~T() called (i was %d)\n", i);
                i = 0;
        }
};

int main( void )
{
        int k = 2;

        while( T j = k-- )
                continue;

        k = 2;

        for( ; T j = k; k = j - 1 )
                continue;
}

