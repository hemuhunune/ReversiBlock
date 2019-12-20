using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FK_CLI;

namespace Core3D_Final
{
    class Player
    {

        // プレイヤー設定
        public fk_Model model = new fk_Model();

        public double size = 20.0;
        public int x = 0;
        public int z = 0;
        public fk_Vector pos;
        public fk_Material material = new fk_Material();

        BlockColor block_color = new BlockColor();

        public Player()
        {
            PlayerSetup();
        }

        public void PlayerSetup()
        {
            //マテリアルの初期化
            fk_Material.InitDefault();
            //色
            fk_Color player_color = block_color.black_col;

            model.Shape = new fk_Block(size, size, size);
            model.DrawMode = fk_DrawMode.POLYMODE | fk_DrawMode.LINEMODE;
            model.LineColor = block_color.white_col;
            model.LineWidth = 5.0;
            fk_Material mat = new fk_Material();
            mat.Alpha = 0.8f;
            mat.Ambient = player_color;
            mat.Diffuse = player_color;
            mat.Emission = player_color;
            mat.Specular = player_color;
            mat.Shininess = 64.0f;
            model.Material = mat;
        }

        
    }
}
