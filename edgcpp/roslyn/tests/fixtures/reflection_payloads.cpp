namespace std::meta {
using info = decltype(^^void);
template<class T> consteval info reflect_constant(T);
struct absent_option { bool engaged; };
struct annotation_range { const info* begin; const info* end; };
struct data_member_options {
  absent_option name, alignment, bit_width;
  bool no_unique_address;
  annotation_range annotations;
};
consteval info data_member_spec(info, data_member_options);
}

constexpr std::meta::info annotation_values[] = {
  std::meta::reflect_constant(17), std::meta::reflect_constant(23)
};
constexpr auto annotated_member_spec = std::meta::data_member_spec(
  ^^int, {{false}, {false}, {false}, false,
          {annotation_values, annotation_values + 2}});

