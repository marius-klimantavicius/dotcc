//options_all:-r -x -tused
//options: --microsoft -n;cp

typedef void (*PM)(void);
template <class B> int IST(B value);
class CB {
  public:
    ~CB(void) { IV(this); }
    int IV(CB *pccb) {
      return IST(pccb->m_value)  ;
    }
  protected:
        PM m_value;
};
inline int IST(PM pmc) { }
void abcd( CB *j)
{
  delete(j);
}

