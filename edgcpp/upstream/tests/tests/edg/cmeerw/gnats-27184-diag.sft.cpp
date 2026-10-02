//type:fn
//options:--ms_c++20 --microsoft_version 1936 --ms_permissive:--ms_c++20 --microsoft_version 1936:--c++20 -A

namespace unknown_base_class_name
{
  template<typename T>
  struct C : B                  // error at definition or instantiation time
  { };

  template struct C<void>;
}

namespace qualified_id_variable
{
  int B;

  template<typename T>
  struct C : ::B                // error
  { };
}

namespace qualified_id_typedef
{
  using B = int;

  template<typename T>
  struct C : ::B                // error
  { };
}

namespace lookup_variable
{
  int B;

  template<typename T>
  struct C : B                  // error
  { };
}

namespace lookup_variable_template
{
  template<typename T>
  int B;

  template<typename T>
  struct C : B<void>            // error
  { };
}

namespace lookup_typedef
{
  using B = int;

  template<typename T>
  struct C : B                  // error
  { };
}
