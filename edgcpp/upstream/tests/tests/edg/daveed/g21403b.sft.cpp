//remark:Glvalue-to-prvalue conversion in templates
//options:--gnu=70000;fp

  template<int N> void g(float *p, float d) {
    float r = (N == 1) ? p[0] : d;
  }
//  int main() { g<1>(nullptr); } 
