//type:fp
//options_all:--c++20 -tused -A
//I am 100% sure on this one.
enum class E { ONE = 1, TWO, THREE };
enum EU { ONE = 1, TWO, THREE };

int main()
{
    E e = E::ONE;
    int one = static_cast<int>(e);
    E e2 = static_cast<E>(one);
    EU eu = static_cast<EU>(e2);
}

//cwg: 2338
//title: Undefined behavior converting to short enums with fixed underlying types
//meeting: Albuquerque 11/17
//edg_status: Passes
