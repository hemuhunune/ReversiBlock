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
                if(openStatus == false)
                {
                    Console.WriteLine("Audio File Open Error");
                }

            }

            public void Start()
            {
                if (openStatus == false) return;
                bgm.LoopMode = true;
                bgm.Gain = 0.5;
                    while(EndStatus == false)
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

                for(int i = 0; i < argNum; i++)
                {
                    se[i] = new fk_AudioWavBuffer();
                    openStatus[i] = false;
                    playStatus[i] = false;
                }
            }

            public bool LoadData(int argID,string argFileName)
            {
                if(argID < 0 || argID >= se.Length)
                {
                    return false;
                }
                openStatus[argID] = se[argID].Open(argFileName);
                if(openStatus[argID] == false)
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

                for(i = 0; i < se.Length; i++)
                {
                    if (openStatus[i] == false) return;
                }
                while(EndStatus == false)
                {
                    for(i = 0; i < se.Length; i++)
                    {
                        if(playStatus[i] == true)
                        {
                            playStatus[i] = se[i].Play();
                        }
                    }
                    Thread.Sleep(10);
                }
            }
            public void Dispose()
            {
                for(int i = 0; i < se.Length; i++)
                {
                    se[i].Dispose();
                }
            }



        }
    class Program
    {
        static void Main(string[] args)
        {


            Field c_field = new Field();
            c_field.map[0, 0] = 0;

            var fk_win = new fk_AppWindow();
            var camera = new fk_Model();
            var light = new fk_Light();
            var lightmodel = new fk_Model();
       
            //BGM
            var bgm = new MyBGM("BGM/fullon_demo.ogg");
            /*曲候補
            Heart_Precaution_bms.ogg
            Dawn_to_Earth.ogg
            Icycave.ogg
            fullon_demo.ogg
            */
            var bgmTask = new Task(bgm.Start);
            double volume = 0.5;

            var se = new MySE(4);
            var seTask = new Task(se.Start);
            se.LoadData(0, "SE/sword1.wav");
            se.LoadData(1, "SE/strange_wave.wav");
            se.LoadData(2, "SE/reflection.wav");
            se.LoadData(3, "SE/light_saber1.wav");


            //マテリアルの初期化
            fk_Material.InitDefault();
            //色
            var black_col = new fk_Color(0.0, 0.0, 0.0);
            var BG_black_col = new fk_Color(0.05, 0.05, 0.05);
            var BG_white_col = new fk_Color(0.85, 0.85, 0.85);
            var white_col = new fk_Color(1.0, 1.0, 1.0);
            var gray_col = new fk_Color(0.2, 0.2, 0.2);
            var gray_col_line = new fk_Color(0.3, 0.3, 0.3);
            var green_col = new fk_Color(0.0, 0.8, 0.0);
            var blue_col = new fk_Color(0.0, 0.0, 0.8);
            var yellow_col = new fk_Color(0.8, 0.8, 0.0);
            var red_col = new fk_Color(0.8, 0.0, 0.0);
            var purple_col = new fk_Color(0.8, 0.0, 0.8);
            var model_col_previous = new fk_Color(1.0, 1.0, 1.0);//プレイヤーの色の保存
            
            //int
            int touch_flag = 0;
            int change_color = 1;
            int start_x = 0;
            int start_z = 0;
            int change_yuka_count = 0;
            int camera_mode = 0;
            int[] warp_x = new int[22];
            int[] warp_z = new int[22];
            int[] change_yuka_x = new int[22];
            int[] change_yuka_z = new int[22];
            int warp_num = 0;
            //double
            double parallel = 1;
            double move_speed = FK.PI / 50.0;
            double move_time = 0.0;
            double move_time_end = 10;
            double move_time_end_count;
            double warp_timing = 1000;
            double move_warp_x = 0.0;
            double move_warp_z = 0.0;
            double warp_camera_x = 0.0;
            double warp_camera_y = 0.0;
            double warp_camera_z = 0.0;
            double move_up = 1.0;
            double move_count = 0;
            double warp_speed = 0;
            double warp_angle_speed = 0;
            double move_spd;
            //bool
            bool debug = false;
            bool change_flag = true;
            bool warp_flag = false;
            bool warp_move_flag = false;
            bool frozen = false;
            bool change_color_move_flag = false;
            bool change_yuka_flag_black = false;
            bool change_yuka_flag_white = false;
            bool goal_flag = false;
            bool SE_flag = false;
            //ウインドウの生成と設定

            fk_win.Size = new fk_Dimension(800, 600);
           fk_win.BGColor = BG_black_col;

            //プレイヤー設定
            var model = new fk_Model();
            
            double player_size = 20.0;
            int move_x = 0;
            int move_z = 0;
            fk_Vector pos;
           
            model.Shape = new fk_Block(player_size, player_size, player_size);
            var mat = new fk_Material();

            

            model.DrawMode = fk_DrawMode.POLYMODE | fk_DrawMode.LINEMODE ;
            model.LineColor = white_col;
            model.LineWidth = 5.0;
            mat.Alpha = 0.8f;
            mat.Ambient = black_col; 
            mat.Diffuse = black_col; 
            mat.Emission = black_col; 
            mat.Specular = black_col; 
            mat.Shininess = 64.0f; 
            model.Material = mat;
          
            //床
            fk_Model[,] yuka = new fk_Model[22,22];
            fk_Vector yuka_pos;
            //床の配置
            for (int i = 0; i < 22; i++)
            {
                for (int j = 0; j < 22; j++)
                {
                    //無
                    if (c_field.map[i, j] != 0)
                    {
                        yuka[i, j] = new fk_Model();
                        yuka[i, j].Shape = new fk_Block(player_size, 0.05, player_size);
                        //黒
                        if (c_field.map[i, j] == 1)
                        {
                            yuka[i, j].Material = fk_Material.MatBlack;
                            yuka[i, j].DrawMode = model.DrawMode;
                            yuka[i, j].LineColor = gray_col_line;
                        }
                        //白
                        else if (c_field.map[i, j] == 2)
                        {
                            yuka[i, j].Material = fk_Material.TrueWhite;
                            yuka[i, j].DrawMode = model.DrawMode;
                            yuka[i, j].LineColor = gray_col_line;

                        }
                        //灰色　チェンジカラー
                        else if (c_field.map[i, j] == 3)
                        {
                            var mat_gray = new fk_Material();
                            mat_gray.Alpha = 1.0f;
                            mat_gray.Ambient = gray_col; 
                            mat_gray.Diffuse = gray_col; 
                            mat_gray.Emission = gray_col; 
                            mat_gray.Specular = gray_col; 
                            mat_gray.Shininess = 64.0f; 
                            yuka[i, j].Material = mat_gray;
                            yuka[i, j].DrawMode = model.DrawMode;
                            yuka[i, j].LineColor = new fk_Color(0.5, 0.5, 0.5);

                        }
                        //赤　ゴール地点
                        else if (c_field.map[i, j] == 4)
                        {
                            var mat_red = new fk_Material();
                            mat_red.Alpha = 1.0f;
                            mat_red.Ambient = red_col; 
                            mat_red.Diffuse = red_col; 
                            mat_red.Emission = red_col; 
                            mat_red.Specular = red_col; 
                            mat_red.Shininess = 64.0f; 
                            yuka[i, j].Material = mat_red;
                            yuka[i, j].DrawMode = model.DrawMode;
                            yuka[i, j].LineColor = new fk_Color(0.5, 0.0, 0.0);

                        }
                        //青　スタート地点
                        else if (c_field.map[i, j] == 5)
                        {
                            var mat_blue = new fk_Material();
                            mat_blue.Alpha = 1.0f;
                            mat_blue.Ambient = blue_col; 
                            mat_blue.Diffuse = blue_col; 
                            mat_blue.Emission = blue_col;
                            mat_blue.Specular = blue_col;
                            mat_blue.Shininess = 64.0f;
                            yuka[i, j].Material = mat_blue;
                            yuka[i, j].DrawMode = model.DrawMode;
                            yuka[i, j].LineColor = new fk_Color(0.0, 0.0, 0.5);
                            start_x = j * 20;
                            start_z = i * 20;
                            move_x = j;
                            move_z = i;
                            model.GlMoveTo(j * 20, 0.0, i * 20);
                            fk_win.Entry(model);
                        }
                        //黄色　マップ変更 白
                        else if (c_field.map[i, j] == 6)
                        {
                            var mat_yellow = new fk_Material();
                            mat_yellow.Alpha = 1.0f;
                            mat_yellow.Ambient = yellow_col; 
                            mat_yellow.Diffuse = yellow_col;
                            mat_yellow.Emission = yellow_col; 
                            mat_yellow.Specular = yellow_col; 
                            mat_yellow.Shininess = 64.0f; 
                            yuka[i, j].Material = mat_yellow;
                            yuka[i, j].DrawMode = model.DrawMode;
                            yuka[i, j].LineColor = new fk_Color(0.5, 0.5, 0.0);


                        }

                        //黄色　マップ変更　黒
                        else if (c_field.map[i, j] == 7)
                        {
                            var mat_yellow = new fk_Material();
                            mat_yellow.Alpha = 1.0f;
                            mat_yellow.Ambient = yellow_col; 
                            mat_yellow.Diffuse = yellow_col; 
                            mat_yellow.Emission = yellow_col; 
                            mat_yellow.Specular = yellow_col;
                            mat_yellow.Shininess = 64.0f;
                            yuka[i, j].Material = mat_yellow;
                            yuka[i, j].DrawMode = model.DrawMode;
                            yuka[i, j].LineColor = new fk_Color(0.5, 0.5, 0.0);
                            
                            
                        }
                        //黒　変更される床
                        else if (c_field.map[i, j] == 8)
                        {
                            yuka[i, j].Material = fk_Material.MatBlack;
                            yuka[i, j].DrawMode = model.DrawMode;
                            yuka[i, j].LineColor = gray_col_line;
                            change_yuka_x[0] = i;
                            change_yuka_z[0] = j;

                        }
                        //白　変更される床
                        else if (c_field.map[i, j] == 9)
                        {
                            yuka[i, j].Material = fk_Material.TrueWhite;
                            yuka[i, j].DrawMode = model.DrawMode;
                            yuka[i, j].LineColor = gray_col_line;
                            change_yuka_x[1] = i;
                            change_yuka_z[1] = j;

                        }
                        //緑　ワープ
                        else if (c_field.map[i, j] >= 10)
                        {
                            var mat_green = new fk_Material();
                            mat_green.Alpha = 1.0f;
                            mat_green.Ambient = green_col; 
                            mat_green.Diffuse = green_col;
                            mat_green.Emission = green_col; 
                            mat_green.Specular = green_col; 
                            mat_green.Shininess = 64.0f; 
                            yuka[i, j].Material = mat_green;
                            yuka[i, j].DrawMode = model.DrawMode;
                            yuka[i, j].LineColor = new fk_Color(0.0, 0.5, 0.0);
                            if(c_field.map[i, j] == 10)
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
                                yuka[i, j].DrawMode = model.DrawMode;
                                yuka[i, j].LineColor = gray_col_line;
                                warp_x[14] = 9;
                                warp_z[14] = 20;
                            }
                            else if (c_field.map[i, j] == 15)
                            {
                                
                                warp_x[15] = 15;
                                warp_z[15] = 6;
                            }
                        }
                        yuka[i, j].LineWidth = model.LineWidth;
                        yuka_pos = yuka[i, j].Position;
                        fk_win.Entry(yuka[i, j]);
                        yuka[i, j].GlMoveTo(j * 20.0, -10.0, i * 20.0);
                    }                    
                }
            }
            //カメラ
            fk_Vector camera_pos;
            fk_win.CameraModel = camera;
            //ウィンドウのサイズを設定
            fk_win.Size = new fk_Dimension(1080, 720);
            
            //ウィンドウを開く
            fk_win.Open();
            bgmTask.Start();
            seTask.Start();
            //メインループ
            while (fk_win.Update() == true)
            {
                

                move_time_end_count = move_time_end * 2;
                move_spd = player_size / move_time_end;                
                camera_pos = camera.Position;
                move_count += 1;
                pos = model.Position;

                //色変換
                if (c_field.map[move_z, move_x] == 3 && debug == false)
                {

                    

                    if (change_flag == true && move_count == 5)
                    {
                       
                        change_color_move_flag = true;
                        se.StartSE(3);

                    }
                    else { change_flag = true; }

                    if (change_color_move_flag == true)
                    {
                        

                        if (change_color == 1)
                        {
                            model.LineColor = black_col;
                            fk_win.BGColor = BG_white_col;
                            mat.Alpha = 0.8f;
                            mat.Ambient = white_col; 
                            mat.Diffuse = white_col; 
                            mat.Emission = white_col; 
                            mat.Specular = white_col;
                            mat.Shininess = 64.0f; 
                            model.Material = mat;
                           
                            parallel = -1.5;
                            pos.y += (-20.0 - pos.y) / 15;
                            model.GlMoveTo(pos.x, pos.y, pos.z);
                            frozen = true;
                            if (pos.y <= -19.5)
                            {
                                pos.y = -20.0;
                                change_color = -1;
                                change_flag = false;
                                change_color_move_flag = false;
                                frozen = false;
                            }
                        }
                        else if (change_color == -1)
                        {
                            fk_win.BGColor = BG_black_col;
                            model.LineColor = white_col;
                            mat.Alpha = 0.8f;
                            mat.Ambient = black_col; 
                            mat.Diffuse = black_col;
                            mat.Emission = black_col; 
                            mat.Specular = black_col; 
                            mat.Shininess = 64.0f; 
                            model.Material = mat;
                            parallel = 1;
                            pos.y += (0 - pos.y) / 15;
                            model.GlMoveTo(pos.x, pos.y, pos.z);
                            frozen = true;
                            if (pos.y >= -0.5)
                            {
                                pos.y = 0;
                                change_color = 1;
                                change_flag = false;
                                change_color_move_flag = false;
                                frozen = false;
                            }                                                  
                        }
                        change_flag = false;
                    }
                    
                }
                //ワープ関連
                if (c_field.map[move_z, move_x] >= 10 && debug == false && warp_flag == false)
                {
                    
                    if (move_count == 5)
                    {
                        se.StartSE(1);
                        frozen = true;
                        if(c_field.map[move_z, move_x] == 14)
                        {
                            var mat_purple = new fk_Material();
                            mat_purple.Alpha = 1.0f;
                            mat_purple.Ambient = purple_col; 
                            mat_purple.Diffuse = purple_col; 
                            mat_purple.Emission = purple_col; 
                            mat_purple.Specular = purple_col; 
                            mat_purple.Shininess = 64.0f; 
                            yuka[move_z, move_x].Material = mat_purple;
                            yuka[move_z, move_x].DrawMode = model.DrawMode;
                            yuka[move_z, move_x].LineColor = new fk_Color(0.5, 0.0, 0.5);

                        }
                    }
                    
                    if (frozen == true)
                    {
                        if (warp_move_flag == false)
                        {
                            for (int i = 10; i < 22; i++)
                            {
                                if (c_field.map[move_z, move_x] == i)
                                {
                                    warp_num = i;
                                    break;
                                }
                            }
                            move_warp_x = pos.x;
                            move_warp_z = pos.z;
                            warp_speed += 0.16f * change_color;
                            warp_angle_speed += 0.16f;
                            pos.y += warp_speed;
                            model.GlMoveTo(pos.x, pos.y, pos.z);
                            model.GlAngle(warp_speed, 0.0, 0.0);
                        }
                        if (pos.y >= warp_timing && change_color == 1)
                        {
                            pos.y = warp_timing - 15;
                            warp_move_flag = true;
                            warp_speed = -warp_speed;
                            warp_angle_speed = -warp_angle_speed;

                        }
                        else if (pos.y <= -warp_timing && change_color == -1)
                        {
                            pos.y = -warp_timing + 15;
                            warp_move_flag = true;
                            warp_speed = -warp_speed;
                            warp_angle_speed = -warp_angle_speed;
                        }
                        if (warp_move_flag == true)
                        {
                            warp_angle_speed += 0.16f * change_color;

                            if (pos.y <= 0 && change_color == 1)
                            {
                                warp_speed = 0;
                                pos.y = 0;
                                model.GlAngle(0.0, 0.0, 0.0);
                                move_x = warp_x[warp_num];
                                move_z = warp_z[warp_num];
                                model.GlMoveTo(pos.x, pos.y, pos.z);
                                warp_move_flag = false;
                                warp_flag = true;
                                frozen = false;
                            }
                            else if (pos.y >= -20 && change_color == -1)
                            {
                                warp_speed = 0;
                                pos.y = -20;
                                model.GlAngle(0.0, 0.0, 0.0);
                                move_x = warp_x[warp_num];
                                move_z = warp_z[warp_num];
                                model.GlMoveTo(pos.x, pos.y, pos.z);
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

                                pos.y += warp_speed;
                                model.GlMoveTo(move_warp_x, pos.y, move_warp_z);
                                model.GlAngle(warp_angle_speed, 0.0, 0.0);
                            }
                        }

                    }

                }
                else if (c_field.map[move_z, move_x] < 10)
                {

                    warp_flag = false;
                }

                //マップ変換
                if (c_field.map[move_z, move_x] == 6 && debug == false && move_count == 5)
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
                        
                        if(SE_flag == false)
                        {
                            se.StartSE(2);
                            SE_flag = true;
                        }
                            
                        
                        
                       
                        var mat_gray = new fk_Material();
                        mat_gray.Alpha = 1.0f;
                        mat_gray.Ambient = gray_col; 
                        mat_gray.Diffuse = gray_col; 
                        mat_gray.Emission = gray_col; 
                        mat_gray.Specular = gray_col;
                        mat_gray.Shininess = 64.0f; 
                        yuka[change_yuka_x[1], change_yuka_z[1]].Material = mat_gray;
                        yuka[change_yuka_x[1], change_yuka_z[1]].DrawMode = model.DrawMode;
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


                if (c_field.map[move_z, move_x] == 7 && debug == false && move_count == 5)
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
                        mat_green.Ambient = green_col; 
                        mat_green.Diffuse = green_col;
                        mat_green.Emission = green_col; 
                        mat_green.Specular = green_col; 
                        mat_green.Shininess = 64.0f; 
                       
                        yuka[change_yuka_x[0], change_yuka_z[0]].Material = mat_green;
                        yuka[change_yuka_x[0], change_yuka_z[0]].DrawMode = model.DrawMode;
                        yuka[change_yuka_x[0], change_yuka_z[0]].LineColor = new fk_Color(0.0, 0.5, 0.0);

                        
                    }
                    if(change_yuka_count >= 180)
                    {
                        camera_mode = 0;
                        frozen = false;
                        change_yuka_count = 0;
                        change_yuka_flag_black = false;
                    }
                    
                }

                //ゴール処理
                if (c_field.map[move_z, move_x] == 4 && goal_flag == false && move_count == 5)
                {
                    goal_flag = true;
                }
                if(goal_flag == true)
                {
                    frozen = true;
                    warp_speed += 0.32f * change_color;
                    warp_angle_speed += 0.32f;
                    pos.y += warp_speed;
                    model.GlMoveTo(pos.x, pos.y, pos.z);
                    model.GlAngle(warp_speed, 0.0, 0.0);
                    if(pos.y >= 6000)
                    {
                        goal_flag = false;
                       
                    }
                }

                //移動関連
                    if (move_count >= 10 && debug == false && frozen == false && camera_mode == 0)
                {

                    if (fk_win.GetSpecialKeyStatus(fk_SpecialKey.RIGHT, fk_SwitchStatus.PRESS) == true && touch_flag == 0)
                    {

                        if (c_field.map[move_z, move_x + 1] != 0)
                        {
                            if (change_color == 1)
                            {
                                if (c_field.map[move_z, move_x + 1] != 2 && c_field.map[move_z, move_x + 1] != 9)
                                {
                                    move_time = 0;
                                    touch_flag = 1;
                                    move_x += 1;
                                }
                            }
                            else if (change_color == -1)
                            {
                                if (c_field.map[move_z, move_x + 1] != 1 && c_field.map[move_z, move_x + 1] != 8)
                                {
                                    move_time = 0;
                                    touch_flag = 1;
                                    move_x += 1;
                                }
                            }

                        }
                        else { touch_flag = 0; }


                    }
                    else if (fk_win.GetSpecialKeyStatus(fk_SpecialKey.LEFT, fk_SwitchStatus.PRESS) == true && touch_flag == 0)
                    {
                        if (c_field.map[move_z, move_x - 1] != 0)
                        {
                            if (change_color == 1)
                            {
                                if (c_field.map[move_z, move_x - 1] != 2 && c_field.map[move_z, move_x - 1] != 9)
                                {
                                    move_time = 0;
                                    touch_flag = 2;
                                    move_x -= 1;
                                }
                            }
                            else if (change_color == -1)
                            {
                                if (c_field.map[move_z, move_x - 1] != 1 && c_field.map[move_z, move_x - 1] != 8)
                                {
                                    move_time = 0;
                                    touch_flag = 2;
                                    move_x -= 1;
                                }

                            }

                        }
                        else { touch_flag = 0; }
                    }
                    else if (fk_win.GetSpecialKeyStatus(fk_SpecialKey.UP, fk_SwitchStatus.PRESS) == true && touch_flag == 0)
                    {
                       
                            if (change_color == 1)
                            {
                                if (c_field.map[move_z - 1, move_x] != 0)
                                {
                                if (c_field.map[move_z - 1, move_x] != 2 && c_field.map[move_z - 1, move_x] != 9)
                                {
                                    move_time = 0;
                                    touch_flag = 3;
                                    move_z -= 1;
                                }
                            }
                        }
                        else if (change_color == -1)
                        {
                            if(c_field.map[move_z + 1, move_x] != 0)
                            {
                                if (c_field.map[move_z + 1, move_x] != 1 && c_field.map[move_z + 1, move_x] != 8)
                                {
                                    move_time = 0;
                                    touch_flag = 3;
                                    move_z += 1;
                                }
                            }
                          

                        }

                        
                        else { touch_flag = 0; }
                    }
                    else if (fk_win.GetSpecialKeyStatus(fk_SpecialKey.DOWN, fk_SwitchStatus.PRESS) == true && touch_flag == 0)
                    {

                        if (change_color == 1)
                        {
                            if (c_field.map[move_z + 1, move_x] != 0)
                            {
                                if (c_field.map[move_z + 1, move_x] != 2 && c_field.map[move_z + 1, move_x] != 9)
                                {
                                    move_time = 0;
                                    touch_flag = 4;
                                    move_z += 1;
                                }
                            }
                        }
                        else if (change_color == -1)
                        {
                            if (c_field.map[move_z - 1, move_x] != 0)
                            {
                                if (c_field.map[move_z - 1, move_x] != 1 && c_field.map[move_z - 1, move_x] != 8)
                                {
                                    move_time = 0;
                                    touch_flag = 4;
                                    move_z -= 1;
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
                        model.GlRotateWithVec(pos, fk_Axis.Z, -move_speed * change_color);
                        pos.x += move_spd ;
                        
                        if(move_time <= move_time_end / 2)
                        {
                            model.GlMoveTo(pos.x, pos.y + move_up * parallel, pos.z);
                        }
                        else if(move_time > move_time_end / 2)
                        {
                            model.GlMoveTo(pos.x, pos.y - move_up * parallel, pos.z);
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
                        model.GlRotateWithVec(pos, fk_Axis.Z, move_speed * change_color);
                        pos.x -= move_spd ;
                        if (move_time <= move_time_end / 2)
                        {
                            model.GlMoveTo(pos.x, pos.y + move_up * parallel, pos.z);
                        }
                        else if (move_time > move_time_end / 2)
                        {
                            model.GlMoveTo(pos.x, pos.y - move_up * parallel, pos.z);
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
                        model.GlRotateWithVec(pos, fk_Axis.X, -move_speed * change_color);
                        if(change_color == 1)
                        {
                            pos.z -= move_spd;
                        }
                        else if(change_color == -1)
                        {
                            pos.z += move_spd;
                        }
                        if (move_time <= move_time_end / 2)
                        {
                            model.GlMoveTo(pos.x, pos.y + move_up * parallel, pos.z);
                        }
                        else if (move_time > move_time_end / 2)
                        {
                            model.GlMoveTo(pos.x, pos.y - move_up * parallel, pos.z);
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
                        model.GlRotateWithVec(pos, fk_Axis.X, move_speed * change_color);
                        if(change_color == 1)
                        {
                            pos.z += move_spd;
                        }
                       if(change_color == -1)
                        {
                            pos.z -= move_spd;
                        }
                        if (move_time <= move_time_end / 2)
                        {
                            model.GlMoveTo(pos.x, pos.y + move_up * parallel, pos.z);
                        }
                        else if (move_time > move_time_end / 2)
                        {
                            model.GlMoveTo(pos.x, pos.y - move_up * parallel, pos.z);
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
              
               //デバック関連
                if ((fk_win.GetKeyStatus('D', fk_SwitchStatus.DOWN) == true))
                {
                   if(debug == false)
                    {
                        debug = true;
                    }
                   else if(debug == true)
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
                        move_x += 1;
                    }
                    else if (fk_win.GetSpecialKeyStatus(fk_SpecialKey.LEFT, fk_SwitchStatus.PRESS) == true && touch_flag == 0)
                    {
                        move_time = 0;
                        touch_flag = 2;
                        move_x -= 1;
                    }
                    else if (fk_win.GetSpecialKeyStatus(fk_SpecialKey.UP, fk_SwitchStatus.PRESS) == true && touch_flag == 0)
                    {
                        move_time = 0;
                        touch_flag = 3;
                        move_z -= 1;
                    }
                    else if (fk_win.GetSpecialKeyStatus(fk_SpecialKey.DOWN, fk_SwitchStatus.PRESS) == true && touch_flag == 0)
                    {
                        move_time = 0;
                        touch_flag = 4;
                        move_z += 1;
                    }
                }

                if (fk_win.GetSpecialKeyStatus(fk_SpecialKey.ENTER, fk_SwitchStatus.DOWN) == true && touch_flag == 0 && frozen == false)
                {

                    if (camera_mode == 0)
                    {
                        camera_mode = 1;
                        model_col_previous = model.LineColor;
                        model.LineColor = new fk_Color(0.8, 0.0, 0.0);
                    }
                    else if(camera_mode == 1)
                    {
                        model.LineColor = model_col_previous;
                        camera_mode = 0;
                    }
                
                }
                //カメラ処理
                if (camera_mode == 0)
                {
                    warp_camera_y += (pos.y + (20.0 * parallel) - warp_camera_y) / 15;
                    fk_win.CameraPos = new fk_Vector(pos.x, warp_camera_y, pos.z + 150.0);
                    fk_win.CameraFocus = new fk_Vector(pos.x, 20.0 * parallel, 0.0);
                    warp_camera_x = pos.x;
                    warp_camera_z = pos.z;
                }
                     else if(camera_mode == 1)
                {
                    fk_win.CameraPos = new fk_Vector(((22 * 20) / 2), warp_camera_y, ((22 * 20) / 2));

                    if (change_color == 1)
                    {
                        
                        warp_camera_y += (800 - warp_camera_y) / 15;
                        
                    }
                    else
                    {
                        
                        warp_camera_y -= (800 + warp_camera_y) / 15;

                    }
                    fk_win.CameraFocus = new fk_Vector((22 * 20) / 2  , 1.0  , (22 * 20) / 2);
                  
                }
                else if(camera_mode == 2)
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

                bgm.Gain = volume;
               
                if(fk_win.GetKeyStatus('Z',fk_SwitchStatus.DOWN) == true)
                {
                    
                }
                //Task.WaitAll(new[] { bgmTask});
              //  se.EndStatus = true;

            }

            
        }

    }
}
