//type:rp
//options_all:--c++20 -tused -A
int main()
{
     int x = 0;
    char8_t k=0;
    for (k=0; k<= u8'\u007E'; k++, x++) {
            int g = k;
            if (g != k)
                return(1);
    }
    return(0);
}

//cwg: 2484
//title: char8_t and char16_t in integral promotions
//meeting: Virtual 10/21
//edg_status: Passes
