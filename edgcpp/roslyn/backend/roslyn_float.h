/* Exact active EDG IEEE representation decoder; never persists bytes. */
struct RoslynFloatParts { std::string classification,significand,payload,format; long exponent=0;bool negative=false,quiet=false; };
static void roslyn_decimal_double_add(std::string &value,int bit) {
 int carry=bit;for(size_t i=value.size();i!=0;--i){int digit=(value[i-1]-'0')*2+carry;value[i-1]=static_cast<char>('0'+digit%10);carry=digit/10;}if(carry)value.insert(value.begin(),static_cast<char>('0'+carry));
}
static RoslynFloatParts roslyn_float_parts(a_float_kind kind,const an_internal_float_value &value) {
 RoslynFloatParts r;int fraction=0,exponent_bits=0,bias=0;bool explicit_integer=false;
 // Mirror store_host_fp_value/fetch_host_fp_value's ACTIVE internal storage.
 // Target kind is retained separately on arithmetic_type, including float80.
 if(kind==fk_float||kind==fk_std_float32||kind==fk_std_bfloat16){fraction=23;exponent_bits=8;bias=127;r.format="binary32";}
 else if(kind==fk_double||((kind==fk_long_double||kind==fk_float64x)&&(targ_ldbl_mant_dig==53||!FP_HAS_LONG_DOUBLE))||(is_extended_flt_kind(kind)&&flt_type_size[static_cast<int>(kind)]==8)){fraction=52;exponent_bits=11;bias=1023;r.format="binary64";}
 else if(kind==fk_std_float16||kind==fk_float16||kind==fk_fp16){
#if HOST_HAS_FLOAT16_TYPE || USE_SOFTFLOAT
 fraction=10;exponent_bits=5;bias=15;r.format="binary16";
#else
 fraction=23;exponent_bits=8;bias=127;r.format="binary32";
#endif
 }
#if HOST_FP_VALUE_IS_128BIT
 else if((kind==fk_long_double||kind==fk_float64x)&&targ_ldbl_mant_dig==64){fraction=63;exponent_bits=15;bias=16383;explicit_integer=true;r.format="binary80";}
 else if((kind==fk_long_double||kind==fk_float64x)&&targ_ldbl_mant_dig==113){fraction=112;exponent_bits=15;bias=16383;r.format="binary128";}
 else if(kind==fk_float128||kind==fk_std_float128||kind==fk_float80){fraction=112;exponent_bits=15;bias=16383;r.format="binary128";}
#elif USE_DOUBLE_FOR_HOST_FP_VALUE
 else if(kind==fk_long_double||kind==fk_float64x||kind==fk_float80){fraction=52;exponent_bits=11;bias=1023;r.format="binary64";}
#elif FP_LONG_DOUBLE_IS_80BIT_EXTENDED
 else if(kind==fk_long_double||kind==fk_float64x||kind==fk_float80){fraction=63;exponent_bits=15;bias=16383;explicit_integer=true;r.format="binary80";}
#elif FP_LONG_DOUBLE_IS_BINARY128
 else if(kind==fk_long_double||kind==fk_float64x||kind==fk_float80||kind==fk_float128||kind==fk_std_float128){fraction=112;exponent_bits=15;bias=16383;r.format="binary128";}
#endif
 else {r.classification="unsupported";r.significand="0";return r;}
 int sigbits=fraction+(explicit_integer?1:0);int bits=sigbits+exponent_bits+1;int bytes=(bits+7)/8;
 unsigned char data[16]={};std::memcpy(data,&value,static_cast<size_t>(bytes));bool little=host_little_endian;
#if USE_SOFTFLOAT
 if(explicit_integer){extFloat80_t typed;std::memcpy(&typed,&value,sizeof(typed));for(int i=0;i<8;++i)data[i]=static_cast<unsigned char>(typed.signif>>(i*8));data[8]=static_cast<unsigned char>(typed.signExp);data[9]=static_cast<unsigned char>(typed.signExp>>8);little=true;}
#endif
 auto bit_at=[&](int bit)->int {
 return (data[little?bit/8:bytes-1-bit/8]>>(bit%8))&1;
 };
 auto decimal_range=[&](int high,int low,bool implicit)->std::string{std::string result=implicit?"1":"0";for(int bit=high;bit>=low;--bit)roslyn_decimal_double_add(result,bit_at(bit));return result;};
 unsigned exp=0;for(int bit=sigbits+exponent_bits-1;bit>=sigbits;--bit)exp=(exp<<1)|static_cast<unsigned>(bit_at(bit));r.negative=bit_at(sigbits+exponent_bits)!=0;
 std::string significand=decimal_range(sigbits-1,0,false);
 if(exp==((1u<<exponent_bits)-1)){
 std::string frac=decimal_range(fraction-1,0,false);
 if(frac=="0"){r.classification="infinity";r.significand="0";}
 else{r.classification="nan";r.quiet=bit_at(fraction-1)!=0;r.payload=decimal_range(fraction-2,0,false);r.significand="0";}
 }else if(!exp&&significand=="0"){r.classification="zero";r.significand="0";}
 else{r.classification="finite";r.significand=decimal_range(sigbits-1,0,exp&&!explicit_integer);r.exponent=(exp?static_cast<long>(exp)-bias:1-bias)-fraction;}
 return r;
}
