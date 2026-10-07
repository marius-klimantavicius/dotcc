#include <algorithm>
#include <charconv>
#include <cstdio>
#include <limits>
#include <mutex>
#include <random>
#include <regex>
#include <sstream>
#include <stdexcept>
#include <string>
#include <thread>
#include <vector>

template<class T> bool check_sort() {
    T values[257];
    unsigned counts[17] = {};
    for (unsigned i=0;i<257;++i) {
        unsigned value=(i*31+i/7)%17;
        values[i]=static_cast<T>(value);
        ++counts[value];
    }
    std::sort(values,values+257);
    unsigned position=0;
    for (unsigned value=0;value<17;++value)
        for (unsigned i=0;i<counts[value];++i)
            if (values[position++]!=static_cast<T>(value))return false;
    return position==257;
}

template<class T> bool check_format() {
    T values[] = {T(0),T(-0.0),T(1.25),T(-17.5),
        std::numeric_limits<T>::min(),std::numeric_limits<T>::max(),
        std::numeric_limits<T>::denorm_min(),std::numeric_limits<T>::infinity(),
        -std::numeric_limits<T>::infinity(),std::numeric_limits<T>::quiet_NaN()};
    for(T value:values) {
        char buffer[128];
        auto result=std::to_chars(buffer,buffer+127,value);
        if(result.ec!=std::errc())return false;
        *result.ptr=0;
        std::puts(buffer);
    }
    char short_buffer[1];
    auto failure=std::to_chars(short_buffer,short_buffer+1,T(123.5));
    return failure.ec==std::errc::value_too_large && failure.ptr==short_buffer+1;
}

int main() {
    std::string value(100,'x');value.replace(10,3,"hello");
    std::ostringstream stream;stream<<value.size()<<':'<<42;
    if(stream.str()!="102:42")return 1;
    std::istringstream input("34 25");int a,b;input>>a>>b;if(a+b!=59)return 2;
    std::mutex mutex;int count=0;
    std::thread first([&] {for(int i=0;i<100;++i){std::lock_guard<std::mutex> guard(mutex);++count;}});
    std::thread second([&] {for(int i=0;i<100;++i){std::lock_guard<std::mutex> guard(mutex);++count;}});
    first.join();second.join();if(count!=200)return 3;
    std::regex pattern("[a-z]+[0-9]+");
    if(!std::regex_match(std::string("duck123"),pattern))return 4;
    std::random_device random;(void)random();
    try {std::vector<int> values;values.at(2);return 5;}catch(const std::out_of_range&){}
    if(!check_sort<char>() || !check_sort<wchar_t>() ||
       !check_sort<signed char>() || !check_sort<unsigned char>() ||
       !check_sort<short>() || !check_sort<unsigned short>() ||
       !check_sort<int>() || !check_sort<unsigned>() ||
       !check_sort<long>() || !check_sort<unsigned long>() ||
       !check_sort<long long>() || !check_sort<unsigned long long>() ||
       !check_sort<float>() || !check_sort<double>() || !check_sort<long double>())return 6;
    if(!check_format<float>() || !check_format<double>())return 7;
    std::puts("libc++ source provider: passed");
    return 0;
}
