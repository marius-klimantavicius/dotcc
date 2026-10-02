//options_all:-r -x -tused
//options: --strict;cn: --diag_suppress=102;cp

// EDGqa01324
namespace Geometry {
  class Point {
  public:
    Point();
    Point(int,int,enum GraphicSymbol);
    enum GraphicSymbol { rectangleE=1, circleE=2 };
  };
}

