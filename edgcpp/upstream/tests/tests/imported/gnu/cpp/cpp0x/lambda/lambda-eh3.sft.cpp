//type: fp
//options:  --c++11
// PR c++/56388
// { dg-require-effective-target c++11 }

int main()
{
    bool /*const*/ condition = false;

    [&]{
        try{}
        catch(...){
            if(condition){}
        }
    }();
}
