class _State {
public:
  template<typename _Res, typename _Arg>
  struct _Setter;

  template<typename _Res, typename _Arg>
  struct _Setter<_Res, _Arg&> {
  };
};

template<typename _Res>
class promise {
// Hunky dory when following line uncommented.
// public:
  template<typename, typename> friend class _State::_Setter;
};
