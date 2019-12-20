using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using FK_CLI;

namespace Core3D_Final
{


    class MyBGM : IDisposable
    {
        public bool EndStatus { get; set; }
        private fk_AudioStream bgm;
        private bool openStatus;

        //コンストラクタ　引数は音源ファイル名
        public MyBGM(string argFileName)
        {
            EndStatus = false;
            bgm = new fk_AudioStream();
            openStatus = bgm.Open(argFileName);
            if (openStatus == false)
            {
                Console.WriteLine("Audio File Open Error");
            }

        }

        public void Start()
        {
            if (openStatus == false) return;
            bgm.LoopMode = true;
            bgm.Gain = 0.5;
            while (EndStatus == false)
            {
                bgm.Play();
                Thread.Sleep(50);
            }
        }

        public double Gain
        {
            set
            {
                bgm.Gain = value;
            }
        }

        public void Dispose()
        {
            bgm.Dispose();
        }
    }

    class MySE : IDisposable
    {
        public bool EndStatus { get; set; }
        private fk_AudioWavBuffer[] se;
        private bool[] openStatus;
        private bool[] playStatus;

        //コンストラクタ　引数は音源の個数
        public MySE(int argNum)
        {
            EndStatus = false;
            if (argNum < 1) return;
            se = new fk_AudioWavBuffer[argNum];
            openStatus = new bool[argNum];
            playStatus = new bool[argNum];

            for (int i = 0; i < argNum; i++)
            {
                se[i] = new fk_AudioWavBuffer();
                openStatus[i] = false;
                playStatus[i] = false;
            }
        }

        public bool LoadData(int argID, string argFileName)
        {
            if (argID < 0 || argID >= se.Length)
            {
                return false;
            }
            openStatus[argID] = se[argID].Open(argFileName);
            if (openStatus[argID] == false)
            {
                Console.WriteLine("Audio File ({0}) Open Error", argFileName);

            }
            se[argID].LoopMode = false;
            se[argID].Gain = 0.5;
            return true;
        }
        //SE 開始メソッド
        public void StartSE(int argID)
        {
            if (argID < 0 || argID >= se.Length) return;
            playStatus[argID] = true;
            se[argID].Seek(0.0);

        }
        //SE再生処理
        public void Start()
        {
            int i;

            for (i = 0; i < se.Length; i++)
            {
                if (openStatus[i] == false) return;
            }
            while (EndStatus == false)
            {
                for (i = 0; i < se.Length; i++)
                {
                    if (playStatus[i] == true)
                    {
                        playStatus[i] = se[i].Play();
                    }
                }
                Thread.Sleep(10);
            }
        }
        public void Dispose()
        {
            for (int i = 0; i < se.Length; i++)
            {
                se[i].Dispose();
            }
        }



    }

    class Program
    {
        static Field c_field = new Field();


        static fk_AppWindow fk_win = new fk_AppWindow();
        static fk_Model camera = new fk_Model();
        static fk_Light light = new fk_Light();
        static fk_Model lightmodel = new fk_Model();

        static BlockColor block_color = new BlockColor();
        static Player player = new Player();
        //カメラ
        static fk_Vector camera_pos;

        static int[] warp_x = new int[22];
        static int[] warp_z = new int[22];
        static int[] change_yuka_x = new int[22];
        static int[] change_yuka_z = new int[22];

        static fk_Color BGColor = new fk_Color(0.0, 0.0, 0.0);

        //色
        static fk_Color model_col_previous = new fk_Color(1.0, 1.0, 1.0);//プレイヤーの色の保存

        //int
        static int touch_flag = 0;
        static int change_color = 1;
        static int change_yuka_count = 0;
        static int camera_mode = 0;

        static int warp_num = 0;
        //double
        static double parallel = 1;
        static double move_speed = FK.PI / 50.0;
        static double move_time = 0.0;
        static double move_time_end = 10;
        static double move_time_end_count;
        static double warp_timing = 1000;
        static double move_warp_x = 0.0;
        static double move_warp_z = 0.0;
        static double warp_camera_x = 0.0;
        static double warp_camera_y = 0.0;
        static double warp_camera_z = 0.0;
        static double move_up = 1.0;
        static double move_count = 0;
        static double warp_speed = 0;
        static double warp_angle_speed = 0;
        static double move_spd;
        static double step = FK.PI / 1000;
        static double theta = 0;
        //bool
        static bool start_flag = false;
        static bool debug = false;
        static bool change_flag = true;
        static bool warp_flag = false;
        static bool warp_move_flag = false;
        static bool frozen = false;
        static bool change_color_move_flag = false;
        static bool change_yuka_flag_black = false;
        static bool change_yuka_flag_white = false;
        static bool goal_flag = false;
        static bool SE_flag = false;

        static void Main(string[] args)
        {

            c_field.map[0, 0] = 0;
            //BGM
            var bgm = new MyBGM("BGM/fullon_demo.ogg");
            var bgmTask = new Task(bgm.Start);
            double volume = 0.5;
            //SE
            var se = new MySE(4);
            var seTask = new Task(se.Start);
            se.LoadData(0, "SE/sword1.wav");
            se.LoadData(1, "SE/strange_wave.wav");
            se.LoadData(2, "SE/reflection.wav");
            se.LoadData(3, "SE/light_saber1.wav");

            
            //マテリアルの初期化
            fk_Material.InitDefault();
            
            //ウインドウの生成と設定

            fk_win.Size = new fk_Dimension(480, 270);
            fk_win.BGColor = block_color.BG_black_col;

            //床
            fk_Model[,] yuka = new fk_Model[32, 22];
            fk_Vector yuka_pos = new fk_Vector(0.0,0.0,0.0);
            //床の配置
            yukaSet(yuka, yuka_pos);
            //カメラ
            fk_Vector camera_pos;
            fk_win.CameraModel = camera;
            //ウィンドウのサイズを設定
            fk_win.Size = new fk_Dimension(800, 600);

            //fk_win.Size = new fk_Dimension(480, 270);
            //ウィンドウを開く
            fk_win.Open();
            bgmTask.Start();
            seTask.Start();
            //メインループ
            while (fk_win.Update() == true)
            {
                    move_time_end_count = move_time_end * 2;
                    move_spd = player.size / move_time_end;
                    camera_pos = camera.Position;
                    move_count += 1;
                    player.pos = player.model.Position;
                    fk_win.BGColor = BGColor;
                    //色変換
                    if (c_field.map[player.z, player.x] == 3 && debug == false)
                    {
                        if (change_flag == true && move_count == 5)
                        {

                            change_color_move_flag = true;
                            se.StartSE(3);

                        }
                        else { change_flag = true; }

                        if (change_color_move_flag == true)
                        {
                            changePlayerColor();
                            change_flag = false;
                        }

                    }
                    //ワープ関連
                    if (c_field.map[player.z, player.x] >= 10 && debug == false && warp_flag == false)
                    {

                        if (move_count == 5)
                        {
                            se.StartSE(1);
                            frozen = true;
                            if (c_field.map[player.z, player.x] == 14)
                            {
                                ColorSet(yuka[player.z, player.x], block_color.purple_col, new fk_Color(0.5, 0.0, 0.5),1.0f);
                            }
                        }

                        if (frozen == true)
                        {
                            playerWarp();
                        }

                    }
                    else if (c_field.map[player.z, player.x] < 10)
                    {

                        warp_flag = false;
                    }

                    //マップ変換
                    if (c_field.map[player.z, player.x] == 6 && debug == false && move_count == 5)
                    {

                        frozen = true;

                        camera_mode = 3;
                        change_yuka_flag_white = true;
                        c_field.map[change_yuka_x[1], change_yuka_z[1]] = 3;

                    }

                    if (change_yuka_flag_white == true)
                    {
                        change_yuka_count += 1;
                        if (change_yuka_count >= 120 && change_yuka_count < 180)
                        {

                            if (SE_flag == false)
                            {
                                se.StartSE(2);
                                SE_flag = true;
                            }

                            var mat_gray = new fk_Material();
                            mat_gray.Alpha = 1.0f;
                            mat_gray.Ambient = block_color.gray_col;
                            mat_gray.Diffuse = block_color.gray_col;
                            mat_gray.Emission = block_color.gray_col;
                            mat_gray.Specular = block_color.gray_col;
                            mat_gray.Shininess = 64.0f;
                            yuka[change_yuka_x[1], change_yuka_z[1]].Material = mat_gray;
                            yuka[change_yuka_x[1], change_yuka_z[1]].DrawMode = player.model.DrawMode;
                            yuka[change_yuka_x[1], change_yuka_z[1]].LineColor = new fk_Color(0.5, 0.5, 0.5);

                        }
                        if (change_yuka_count >= 180)
                        {
                            camera_mode = 0;
                            frozen = false;
                            change_yuka_count = 0;
                            change_yuka_flag_white = false;
                            SE_flag = false;
                        }

                    }


                    if (c_field.map[player.z, player.x] == 7 && debug == false && move_count == 5)
                    {
                        frozen = true;

                        camera_mode = 2;
                        change_yuka_flag_black = true;
                        c_field.map[change_yuka_x[0], change_yuka_z[0]] = 16;
                        if (c_field.map[change_yuka_x[0], change_yuka_z[0]] == 16)
                        {
                            warp_x[16] = 9;
                            warp_z[16] = 13;
                        }
                    }

                    if (change_yuka_flag_black == true)
                    {
                        change_yuka_count += 1;
                        if (change_yuka_count >= 120 && change_yuka_count < 180)
                        {
                            if (change_yuka_count <= 121)
                            {

                                se.StartSE(2);
                            }
                            var mat_green = new fk_Material();
                            mat_green.Alpha = 1.0f;
                            mat_green.Ambient = block_color.green_col;
                            mat_green.Diffuse = block_color.green_col;
                            mat_green.Emission = block_color.green_col;
                            mat_green.Specular = block_color.green_col;
                            mat_green.Shininess = 64.0f;

                            yuka[change_yuka_x[0], change_yuka_z[0]].Material = mat_green;
                            yuka[change_yuka_x[0], change_yuka_z[0]].DrawMode = player.model.DrawMode;
                            yuka[change_yuka_x[0], change_yuka_z[0]].LineColor = new fk_Color(0.0, 0.5, 0.0);


                        }
                        if (change_yuka_count >= 180)
                        {
                            camera_mode = 0;
                            frozen = false;
                            change_yuka_count = 0;
                            change_yuka_flag_black = false;
                        }

                    }

                    //ゴール処理
                    if (c_field.map[player.z, player.x] == 4 && goal_flag == false && move_count == 5)
                    {
                        goal_flag = true;
                    }
                    if (goal_flag == true)
                    {
                        frozen = true;
                        warp_speed += 0.32f * change_color;
                        warp_angle_speed += 0.32f;
                        player.pos.y += warp_speed;
                        player.model.GlMoveTo(player.pos.x, player.pos.y, player.pos.z);
                        player.model.GlAngle(warp_speed, 0.0, 0.0);
                    }

                //移動関連      
                if (start_flag == true)
                {
                    if (move_count >= 10 && debug == false && frozen == false && camera_mode == 0)
                    {

                        if (fk_win.GetSpecialKeyStatus(fk_SpecialKey.RIGHT, fk_SwitchStatus.PRESS) == true && touch_flag == 0)
                        {

                            if (c_field.map[player.z, player.x + 1] != 0)
                            {
                                if (change_color == 1)
                                {
                                    if (c_field.map[player.z, player.x + 1] != 2 && c_field.map[player.z, player.x + 1] != 9)
                                    {
                                        move_time = 0;
                                        touch_flag = 1;
                                        player.x += 1;
                                    }
                                }
                                else if (change_color == -1)
                                {
                                    if (c_field.map[player.z, player.x + 1] != 1 && c_field.map[player.z, player.x + 1] != 8)
                                    {
                                        move_time = 0;
                                        touch_flag = 1;
                                        player.x += 1;
                                    }
                                }

                            }
                            else { touch_flag = 0; }


                        }
                        else if (fk_win.GetSpecialKeyStatus(fk_SpecialKey.LEFT, fk_SwitchStatus.PRESS) == true && touch_flag == 0)
                        {
                            if (c_field.map[player.z, player.x - 1] != 0)
                            {
                                if (change_color == 1)
                                {
                                    if (c_field.map[player.z, player.x - 1] != 2 && c_field.map[player.z, player.x - 1] != 9)
                                    {
                                        move_time = 0;
                                        touch_flag = 2;
                                        player.x -= 1;
                                    }
                                }
                                else if (change_color == -1)
                                {
                                    if (c_field.map[player.z, player.x - 1] != 1 && c_field.map[player.z, player.x - 1] != 8)
                                    {
                                        move_time = 0;
                                        touch_flag = 2;
                                        player.x -= 1;
                                    }

                                }

                            }
                            else { touch_flag = 0; }
                        }
                        else if (fk_win.GetSpecialKeyStatus(fk_SpecialKey.UP, fk_SwitchStatus.PRESS) == true && touch_flag == 0)
                        {

                            if (change_color == 1)
                            {
                                if (c_field.map[player.z - 1, player.x] != 0)
                                {
                                    if (c_field.map[player.z - 1, player.x] != 2 && c_field.map[player.z - 1, player.x] != 9)
                                    {
                                        move_time = 0;
                                        touch_flag = 3;
                                        player.z -= 1;
                                    }
                                }
                            }
                            else if (change_color == -1)
                            {
                                if (c_field.map[player.z + 1, player.x] != 0)
                                {
                                    if (c_field.map[player.z + 1, player.x] != 1 && c_field.map[player.z + 1, player.x] != 8)
                                    {
                                        move_time = 0;
                                        touch_flag = 3;
                                        player.z += 1;
                                    }
                                }


                            }


                            else { touch_flag = 0; }
                        }
                        else if (fk_win.GetSpecialKeyStatus(fk_SpecialKey.DOWN, fk_SwitchStatus.PRESS) == true && touch_flag == 0)
                        {

                            if (change_color == 1)
                            {
                                if (c_field.map[player.z + 1, player.x] != 0)
                                {
                                    if (c_field.map[player.z + 1, player.x] != 2 && c_field.map[player.z + 1, player.x] != 9)
                                    {
                                        move_time = 0;
                                        touch_flag = 4;
                                        player.z += 1;
                                    }
                                }
                            }
                            else if (change_color == -1)
                            {
                                if (c_field.map[player.z - 1, player.x] != 0)
                                {
                                    if (c_field.map[player.z - 1, player.x] != 1 && c_field.map[player.z - 1, player.x] != 8)
                                    {
                                        move_time = 0;
                                        touch_flag = 4;
                                        player.z -= 1;
                                    }
                                }


                            }


                            else { touch_flag = 0; }
                        }
                    }

                    //移動処理

                    if (touch_flag == 1)
                    {

                        move_time++;
                        if (move_time <= move_time_end)
                        {
                            move_speed = FK.PI / move_time_end_count;
                            player.model.GlRotateWithVec(player.pos, fk_Axis.Z, -move_speed * change_color);
                            player.pos.x += move_spd;

                            if (move_time <= move_time_end / 2)
                            {
                                player.model.GlMoveTo(player.pos.x, player.pos.y + move_up * parallel, player.pos.z);
                            }
                            else if (move_time > move_time_end / 2)
                            {
                                player.model.GlMoveTo(player.pos.x, player.pos.y - move_up * parallel, player.pos.z);
                            }
                        }
                        else
                        {
                            se.StartSE(0);
                            move_speed = 0.0;
                            touch_flag = 0;
                            move_spd = 0;
                            move_count = 0;
                        }
                    }

                    else if (touch_flag == 2)
                    {

                        move_time++;
                        if (move_time <= move_time_end)
                        {
                            move_speed = FK.PI / move_time_end_count;
                            player.model.GlRotateWithVec(player.pos, fk_Axis.Z, move_speed * change_color);
                            player.pos.x -= move_spd;
                            if (move_time <= move_time_end / 2)
                            {
                                player.model.GlMoveTo(player.pos.x, player.pos.y + move_up * parallel, player.pos.z);
                            }
                            else if (move_time > move_time_end / 2)
                            {
                                player.model.GlMoveTo(player.pos.x, player.pos.y - move_up * parallel, player.pos.z);
                            }
                        }
                        else
                        {
                            se.StartSE(0);
                            move_speed = 0.0;
                            touch_flag = 0;
                            move_spd = 0;
                            move_count = 0;
                        }
                    }

                    else if (touch_flag == 3)
                    {

                        move_time++;
                        if (move_time <= move_time_end)
                        {
                            move_speed = FK.PI / move_time_end_count;
                            player.model.GlRotateWithVec(player.pos, fk_Axis.X, -move_speed);
                            if (change_color == 1)
                            {
                                player.pos.z -= move_spd;
                            }
                            else if (change_color == -1)
                            {
                                player.pos.z += move_spd;
                            }
                            if (move_time <= move_time_end / 2)
                            {
                                player.model.GlMoveTo(player.pos.x, player.pos.y + move_up * parallel, player.pos.z);
                            }
                            else if (move_time > move_time_end / 2)
                            {
                                player.model.GlMoveTo(player.pos.x, player.pos.y - move_up * parallel, player.pos.z);
                            }
                        }
                        else
                        {
                            se.StartSE(0);
                            move_speed = 0.0;
                            touch_flag = 0;
                            move_count = 0;
                        }
                    }

                    else if (touch_flag == 4)
                    {

                        move_time++;
                        if (move_time <= move_time_end)
                        {
                            move_speed = FK.PI / move_time_end_count;
                            player.model.GlRotateWithVec(player.pos, fk_Axis.X, move_speed);
                            if (change_color == 1)
                            {
                                player.pos.z += move_spd;
                            }
                            if (change_color == -1)
                            {
                                player.pos.z -= move_spd;
                            }
                            if (move_time <= move_time_end / 2)
                            {
                                player.model.GlMoveTo(player.pos.x, player.pos.y + move_up * parallel, player.pos.z);
                            }
                            else if (move_time > move_time_end / 2)
                            {
                                player.model.GlMoveTo(player.pos.x, player.pos.y - move_up * parallel, player.pos.z);
                            }

                        }
                        else
                        {
                            se.StartSE(0);
                            move_speed = 0.0;
                            touch_flag = 0;
                            move_count = 0;
                        }
                    }
                }
                    //デバック関連
                          if ((fk_win.GetKeyStatus('D', fk_SwitchStatus.DOWN) == true))
                          {
                              if (debug == false)
                              {
                                  debug = true;
                              }
                              else if (debug == true)
                              {
                                  debug = false;
                              }

                          }
                          
                    if (debug == true)
                    {
                        if (fk_win.GetSpecialKeyStatus(fk_SpecialKey.RIGHT, fk_SwitchStatus.PRESS) == true && touch_flag == 0)
                        {
                            move_time = 0;
                            touch_flag = 1;
                            player.x += 1;
                        }
                        else if (fk_win.GetSpecialKeyStatus(fk_SpecialKey.LEFT, fk_SwitchStatus.PRESS) == true && touch_flag == 0)
                        {
                            move_time = 0;
                            touch_flag = 2;
                            player.x -= 1;
                        }
                        else if (fk_win.GetSpecialKeyStatus(fk_SpecialKey.UP, fk_SwitchStatus.PRESS) == true && touch_flag == 0)
                        {
                            move_time = 0;
                            touch_flag = 3;
                            player.z -= 1;
                        }
                        else if (fk_win.GetSpecialKeyStatus(fk_SpecialKey.DOWN, fk_SwitchStatus.PRESS) == true && touch_flag == 0)
                        {
                            move_time = 0;
                            touch_flag = 4;
                            player.z += 1;
                        }
                    }

                    if (fk_win.GetSpecialKeyStatus(fk_SpecialKey.ENTER, fk_SwitchStatus.DOWN) == true && touch_flag == 0 && frozen == false)
                    {

                        if (camera_mode == 0)
                        {
                            camera_mode = 1;
                            model_col_previous = player.model.LineColor;
                            player.model.LineColor = new fk_Color(0.8, 0.0, 0.0);
                            player.pos.y *= -1;

                        }
                        else if (camera_mode == 1)
                        {
                            player.model.LineColor = model_col_previous;
                            if (change_color == -1)
                            {
                                player.pos.y = -20.0;
                                player.model.GlMoveTo(player.pos.x, player.pos.y, player.pos.z);
                            }
                            camera_mode = 0;
                        }

                    }
                    //カメラ処理

                    if (camera_mode == 0)
                    {
                        warp_camera_y += (player.pos.y + (20.0 * parallel) - warp_camera_y) / 15;
                        if (parallel == 1)
                        {
                            fk_win.CameraPos = new fk_Vector(player.pos.x, warp_camera_y + (20 * parallel), player.pos.z + 170);
                            fk_win.CameraFocus = new fk_Vector(player.pos.x, 5.0 * parallel, 0.0);
                        }
                        else
                        {
                            fk_win.CameraPos = new fk_Vector(player.pos.x, warp_camera_y + (12.5 * parallel), player.pos.z + 170);
                            fk_win.CameraFocus = new fk_Vector(player.pos.x, 5.0 * parallel, 0.0);
                        }
                        warp_camera_x = player.pos.x;
                        warp_camera_z = player.pos.z;
                    }
                    else if (camera_mode == 1)
                    {
                        fk_win.CameraPos = new fk_Vector(((22 * 20) / 2), warp_camera_y, ((32 * 20) / 2));
                        if (change_color == 1)
                        {

                            warp_camera_y += (1000 - warp_camera_y) / 15;

                        }
                        else
                        {
                            player.pos.y = 0;
                            player.model.GlMoveTo(player.pos.x, player.pos.y, player.pos.z);
                            warp_camera_y += (1000 - warp_camera_y) / 15;
                        }
                        fk_win.CameraFocus = new fk_Vector((22 * 20) / 2, 1.0, (32 * 20) / 2);
                    }
                    else if (camera_mode == 2)
                    {
                        warp_camera_x += ((change_yuka_x[0] * 20) - warp_camera_x) / 15;
                        warp_camera_y += (300 - warp_camera_y) / 20;
                        warp_camera_z += ((change_yuka_z[0] * 20) - warp_camera_z) / 15;
                        fk_win.CameraPos = new fk_Vector(warp_camera_z, warp_camera_y, warp_camera_x);
                        fk_win.CameraFocus = new fk_Vector(warp_camera_z, 1.0 * change_color * parallel, warp_camera_x);
                    }
                    else if (camera_mode == 3)//マップ変換
                    {
                        warp_camera_x += ((change_yuka_x[1] * 20) - warp_camera_x) / 15;
                        warp_camera_y += (300 - warp_camera_y) / 20;
                        warp_camera_z += ((change_yuka_z[1] * 20) - warp_camera_z) / 15;
                        fk_win.CameraPos = new fk_Vector(warp_camera_z, warp_camera_y, warp_camera_x);
                        fk_win.CameraFocus = new fk_Vector(warp_camera_z, 1.0 * change_color * parallel, warp_camera_x);
                    }
                else if (camera_mode == 4)
                {
                    theta += step;
                    warp_camera_x = 200* Math.Cos(theta + step);
                    warp_camera_y = 400;
                    warp_camera_z = 200* Math.Sin(theta + step);
                    fk_win.CameraPos = new fk_Vector(warp_camera_z, warp_camera_y, warp_camera_x);
                    fk_win.CameraFocus = new fk_Vector(220, 0, 320);
                }

                if (start_flag == false)
                {
                    camera_mode = 4;
                    if (fk_win.GetKeyStatus(' ', fk_SwitchStatus.DOWN) || fk_win.GetSpecialKeyStatus(fk_SpecialKey.ENTER, fk_SwitchStatus.DOWN) == true)
                    {
                        camera_mode = 0;
                        start_flag = true;
                    }
                }

                bgm.Gain = volume;

                }

           
            
            }
        //ブロックの色を設定する
        static void ColorSet(fk_Model colorModel, fk_Color blockColor, fk_Color lineColor, float alpha)
        {
            var set_color = new fk_Material();
            fk_Color set_model_color = blockColor;
            fk_Color set_draw_color = lineColor;

            set_color.Alpha = alpha;
            set_color.Ambient = set_model_color;
            set_color.Diffuse = set_model_color;
            set_color.Emission = set_model_color;
            set_color.Specular = set_model_color;
            set_color.Shininess = 64.0f;
            colorModel.Material = set_color;
            colorModel.DrawMode = player.model.DrawMode;
            colorModel.LineColor = set_draw_color;
        }
        //床設定
        static void yukaSet(fk_Model[,] yuka, fk_Vector yuka_pos)
        {
            int start_x = 0;
            int start_z = 0;

            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < 22; j++)
                {
                    //無
                    if (c_field.map[i, j] != 0)
                    {
                        yuka[i, j] = new fk_Model();
                        yuka[i, j].Shape = new fk_Block(player.size, 0.05, player.size);
                        //黒
                        if (c_field.map[i, j] == 1)
                        {
                            ColorSet(yuka[i, j], block_color.black_col, block_color.black_white_line_col,1.0f);
                        }
                        //白
                        else if (c_field.map[i, j] == 2)
                        {
                            ColorSet(yuka[i, j], block_color.white_col, block_color.black_white_line_col,1.0f);
                        }
                        //灰色　チェンジカラー
                        else if (c_field.map[i, j] == 3)
                        {
                            ColorSet(yuka[i, j], block_color.gray_col, block_color.gray_line_col,1.0f);

                        }
                        //赤　ゴール地点
                        else if (c_field.map[i, j] == 4)
                        {
                            ColorSet(yuka[i, j], block_color.red_col, block_color.red_line_col,1.0f);

                        }
                        //青　スタート地点
                        else if (c_field.map[i, j] == 5)
                        {
                            ColorSet(yuka[i, j], block_color.blue_col, block_color.blue_line_col,1.0f);
                            start_x = j * 20;
                            start_z = i * 20;
                            player.x = j;
                            player.z = i;
                            player.model.GlMoveTo(j * 20, 0.0, i * 20);
                            fk_win.Entry(player.model);
                        }
                        //黄色　マップ変更 白
                        else if (c_field.map[i, j] == 6)
                        {
                            ColorSet(yuka[i, j], block_color.yellow_col, block_color.yellow_line_col,1.0f);
                        }

                        //黄色　マップ変更　黒
                        else if (c_field.map[i, j] == 7)
                        {
                            ColorSet(yuka[i, j], block_color.yellow_col, block_color.yellow_line_col,1.0f);
                        }
                        //黒　変更される床
                        else if (c_field.map[i, j] == 8)
                        {
                            ColorSet(yuka[i, j], block_color.black_col, block_color.black_white_line_col,1.0f);
                            change_yuka_x[0] = i;
                            change_yuka_z[0] = j;

                        }
                        //白　変更される床
                        else if (c_field.map[i, j] == 9)
                        {
                            ColorSet(yuka[i, j], block_color.white_col, block_color.black_white_line_col,1.0f);
                            change_yuka_x[1] = i;
                            change_yuka_z[1] = j;

                        }
                        //緑　ワープ
                        else if (c_field.map[i, j] >= 10)
                        {
                            ColorSet(yuka[i, j], block_color.green_col, block_color.green_line_col,1.0f);
                            if (c_field.map[i, j] == 10)
                            {
                                warp_x[10] = 12;
                                warp_z[10] = 10;
                            }
                            else if (c_field.map[i, j] == 11)
                            {
                                warp_x[11] = 9;
                                warp_z[11] = 13;
                            }
                            else if (c_field.map[i, j] == 12)
                            {
                                warp_x[12] = 1;
                                warp_z[12] = 5;
                            }
                            else if (c_field.map[i, j] == 13)
                            {
                                warp_x[13] = 19;
                                warp_z[13] = 20;
                            }
                            else if (c_field.map[i, j] == 14)
                            {
                                yuka[i, j].Material = fk_Material.MatBlack;
                                yuka[i, j].DrawMode = player.model.DrawMode;
                                yuka[i, j].LineColor = block_color.gray_line_col;
                                warp_x[14] = 9;
                                warp_z[14] = 20;
                            }
                            else if (c_field.map[i, j] == 15)
                            {

                                warp_x[15] = 15;
                                warp_z[15] = 6;
                            }
                        }
                        yuka[i, j].LineWidth = player.model.LineWidth;
                        yuka_pos = yuka[i, j].Position;
                        fk_win.Entry(yuka[i, j]);
                        yuka[i, j].GlMoveTo(j * 20.0, -10.0, i * 20.0);
                    }
                }
            }
        }
        //プレイヤーの色チェンジ(灰色の床に乗ったときの動き)
        static void changePlayerColor()
        {
            if (change_color == 1)
            {
                ColorSet(player.model, block_color.white_col, block_color.black_col,0.8f);
                BGColor = block_color.BG_white_col;

                parallel = -1.5;
                player.pos.y += (-20.0 - player.pos.y) / 15;
                player.model.GlMoveTo(player.pos.x, player.pos.y, player.pos.z);
                frozen = true;
                if (player.pos.y <= -19.5)
                {
                    player.pos.y = -20.0;
                    change_color = -1;
                    change_flag = false;
                    change_color_move_flag = false;
                    frozen = false;
                }
            }
            else if (change_color == -1)
            {
                BGColor = block_color.BG_black_col;
                ColorSet(player.model, block_color.black_col, block_color.white_col,0.8f);

                parallel = 1;
                player.pos.y += (0 - player.pos.y) / 15;
                player.model.GlMoveTo(player.pos.x, player.pos.y, player.pos.z);
                frozen = true;
                if (player.pos.y >= -0.5)
                {
                    player.pos.y = 0;
                    change_color = 1;
                    change_flag = false;
                    change_color_move_flag = false;
                    frozen = false;
                }
            }
        }
        //プレイヤーワープ(緑色の床乗ったときの動き)
        static void playerWarp()
        {
            if (warp_move_flag == false)
            {
                for (int i = 10; i < 22; i++)
                {
                    if (c_field.map[player.z, player.x] == i)
                    {
                        warp_num = i;
                        break;
                    }
                }
                move_warp_x = player.pos.x;
                move_warp_z = player.pos.z;
                warp_speed += 0.16f * change_color;
                warp_angle_speed += 0.16f;
                player.pos.y += warp_speed;
                player.model.GlMoveTo(player.pos.x, player.pos.y, player.pos.z);
                player.model.GlAngle(warp_speed, 0.0, 0.0);
            }
            if (player.pos.y >= warp_timing && change_color == 1)
            {
                player.pos.y = warp_timing - 15;
                warp_move_flag = true;
                warp_speed = -warp_speed;
                warp_angle_speed = -warp_angle_speed;

            }
            else if (player.pos.y <= -warp_timing && change_color == -1)
            {
                player.pos.y = -warp_timing + 15;
                warp_move_flag = true;
                warp_speed = -warp_speed;
                warp_angle_speed = -warp_angle_speed;
            }
            if (warp_move_flag == true)
            {
                warp_angle_speed += 0.16f * change_color;

                if (player.pos.y <= 0 && change_color == 1)
                {
                    warp_speed = 0;
                    player.pos.y = 0;
                    player.model.GlAngle(0.0, 0.0, 0.0);
                    player.x = warp_x[warp_num];
                    player.z = warp_z[warp_num];
                    player.model.GlMoveTo(player.pos.x, player.pos.y, player.pos.z);
                    warp_move_flag = false;
                    warp_flag = true;
                    frozen = false;
                }
                else if (player.pos.y >= -20 && change_color == -1)
                {
                    warp_speed = 0;
                    player.pos.y = -20;
                    player.model.GlAngle(0.0, 0.0, 0.0);
                    player.x = warp_x[warp_num];
                    player.z = warp_z[warp_num];
                    player.model.GlMoveTo(player.pos.x, player.pos.y, player.pos.z);
                    warp_flag = true;
                    frozen = false;
                    warp_move_flag = false;
                }
                else
                {
                    warp_speed += 0.16f * change_color;
                    if (move_warp_x != warp_x[warp_num] * 20)
                    {
                        move_warp_x += (warp_x[warp_num] * 20 - move_warp_x) / 10;
                    }

                    if (move_warp_z != warp_z[warp_num] * 20)
                    {
                        move_warp_z += (warp_z[warp_num] * 20 - move_warp_z) / 10;
                    }

                    player.pos.y += warp_speed;
                    player.model.GlMoveTo(move_warp_x, player.pos.y, move_warp_z);
                    player.model.GlAngle(warp_angle_speed, 0.0, 0.0);
                }
            }
        }
        }
    }

   

