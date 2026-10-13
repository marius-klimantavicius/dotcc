// The pinned interpreter registers the exact (info, any-options)->info
// std::meta::data_member_spec intrinsic without requiring a function body.
// Empty options exercise its documented unset requests without a library layout.
namespace std::meta {
using info = decltype(^^void);
struct data_member_options {};
consteval info data_member_spec(info, data_member_options);
}

struct ReflectedRecord { int member; };
template<class T> struct ReflectedTemplate { T value; };
int reflected_variable = 4;
int reflected_function(int value) { return value + 1; }

constexpr auto reflected_type = ^^int;
constexpr auto reflected_record = ^^ReflectedRecord;
constexpr auto reflected_object = ^^reflected_variable;
constexpr auto reflected_routine = ^^reflected_function;
constexpr auto reflected_template = ^^ReflectedTemplate;
constexpr auto reflected_field = ^^ReflectedRecord::member;
constexpr auto reflected_member_spec = std::meta::data_member_spec(^^int, {});

template<class T> consteval auto reflect_template_parameter() { return ^^T; }
constexpr auto reflected_instantiated_parameter = reflect_template_parameter<int>();

int main() { return reflected_function(reflected_variable); }
