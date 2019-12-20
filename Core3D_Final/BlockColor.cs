using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FK_CLI;

namespace Core3D_Final
{
    public class BlockColor
    {
        //ブロック色
        public fk_Color black_col = new fk_Color(0.0, 0.0, 0.0);
        public fk_Color white_col = new fk_Color(1.0, 1.0, 1.0);
        public fk_Color gray_col = new fk_Color(0.2, 0.2, 0.2);
        public fk_Color green_col = new fk_Color(0.0, 0.8, 0.0);
        public fk_Color blue_col = new fk_Color(0.0, 0.0, 0.8);
        public fk_Color yellow_col = new fk_Color(0.8, 0.8, 0.0);
        public fk_Color red_col = new fk_Color(0.8, 0.0, 0.0);
        public fk_Color purple_col = new fk_Color(0.8, 0.0, 0.8);

        //ライン色
        public fk_Color black_white_line_col = new fk_Color(0.3, 0.3, 0.3);
        public fk_Color gray_line_col = new fk_Color(0.5, 0.5, 0.5);
        public fk_Color green_line_col = new fk_Color(0.0, 0.5, 0.0);
        public fk_Color blue_line_col = new fk_Color(0.0, 0.0, 0.5);
        public fk_Color yellow_line_col = new fk_Color(0.5, 0.5, 0.0);
        public fk_Color red_line_col = new fk_Color(0.5, 0.0, 0.0);
        public fk_Color purple_line_col = new fk_Color(0.8, 0.0, 0.8);

        //背景色
        public fk_Color BG_black_col = new fk_Color(0.05, 0.05, 0.05);
        public fk_Color BG_white_col = new fk_Color(0.85, 0.85, 0.85);
    }
}
