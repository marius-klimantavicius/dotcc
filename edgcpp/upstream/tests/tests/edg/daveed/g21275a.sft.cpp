//remark:Deduction of auto template parameters
//options:--c++20;fp


int main()
{
        int ret = []<class T, auto x = T() + 29>(T y)
        {
                return x + y;
        }('A');

        if (ret != char() + 29 + 'A')
                return 1;

        return 0;
}
