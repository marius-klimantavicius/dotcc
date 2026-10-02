//type: fn
//options: --c++11
# 0 "./lookup/missing-std-include-6.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lookup/missing-std-include-6.C"




template<class T>
void test_make_shared ()
{
  auto p = std::make_shared<T>();



}

std::shared_ptr<int> test_shared_ptr;


std::unique_ptr<int> test_unique_ptr;


std::weak_ptr<int> test_weak_ptr;




void test_make_tuple (int i, int j, int k)
{
  auto t = std::make_tuple (i, j, k);

}



template<class T>
void test_forward(T&& arg)
{
  std::forward<T>(arg);


}

void test_make_pair (int i, int j)
{
  auto p = std::make_pair (i, j);

}

template<class T>
void test_move(T&& arg)
{
  std::move<T>(arg);


}

void test_array ()
{
  std::array a;

}

void test_tuple ()
{
  std::tuple<int,float> p;


}
