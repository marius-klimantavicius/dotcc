//remark:Glvalue-to-prvalue conversion in templates
//options:--gnu=70000;fp

template<int TParam>
void fun(float* pos_array)
{
  float default_val = 0;
  float pos = (TParam == 1) ? pos_array[0] : default_val;
}

int main()
{
  fun<1>(nullptr);
}
