using maho_tan資源下載器.Function;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace maho_tan資源下載器
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 同時下載的線程池上限
        /// </summary>
        int pool = 50;

        private async void btn_download_Click(object sender, RoutedEventArgs e)
        {
            // 歸零下載計數器
            App.glocount = 0;

            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            while (true)
            {
                System.Windows.MessageBox.Show("需要選擇輸出資料夾" + Environment.NewLine + "(內部應包含hotupdate、local_res兩個資料夾)", "注意事項");

                openFolderDialog.InitialFolder = App.Outpath;
                if (openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    App.Outpath = openFolderDialog.Folder;
                    App.Hotpath = Path.Combine(App.Outpath, "hotupdate/");
                    App.Respath = Path.Combine(App.Outpath, "local_res/res/");
                }
                if (Directory.Exists(App.Hotpath) && Directory.Exists(App.Respath))
                    break;
            }

            App.Project = Path.Combine(App.Hotpath, "project.manifest");
            App.Cfg1 = Path.Combine(App.Respath, "data/cfg_out1_high.json");
            App.Cfg3 = Path.Combine(App.Respath, "data/cfg_out3_high.json");
            App.Cfg5 = Path.Combine(App.Respath, "data/cfg_out5_high.json");

            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.InitialDirectory = App.Root;

            if (!File.Exists(App.Project))
            {
                openFileDialog.Filter = "project.manifest|*.manifest;*.json";
                if (openFileDialog.ShowDialog() == true)
                {
                    App.Project = openFileDialog.FileName;
                }
            }
            if (!File.Exists(App.Cfg1))
            {
                openFileDialog.Filter = "cfg_out1_high.json|*.json";
                if (openFileDialog.ShowDialog() == true)
                {
                    App.Cfg1 = openFileDialog.FileName;
                }
            }
            if (!File.Exists(App.Cfg3))
            {
                openFileDialog.Filter = "cfg_out3_high.json|*.json";
                if (openFileDialog.ShowDialog() == true)
                {
                    App.Cfg3 = openFileDialog.FileName;
                }
            }
            if (!File.Exists(App.Cfg5))
            {
                openFileDialog.Filter = "cfg_out5_high.json|*.json";
                if (openFileDialog.ShowDialog() == true)
                {
                    App.Cfg5 = openFileDialog.FileName;
                }
            }

            List<string> HotList = new List<string>();
            JObject ProjectFile = JObject.Parse(File.ReadAllText(App.Project));
            JObject AssetsList = JObject.Parse(ProjectFile["assets"].ToString());

            foreach (KeyValuePair<string, JToken> pair in AssetsList)
            {
                string name = pair.Key;
                bool compressed = Convert.ToBoolean(pair.Value["compressed"].ToString());
                //string md5 = pair.Value["md5"].ToString();

                // src應該全部在game.min.js中，而且沒找到下載點
                if (!name.Contains("src/"))
                    HotList.Add(name);

                if (compressed == true)
                {
                    System.Windows.MessageBox.Show($"發現例外狀況 {name} 的 compressed 為 True", "中止");
                    return;
                }
            }
            
            List<string> LocalList = new List<string>();

            /* CFG_1 */
            
            JObject cfg1 = JObject.Parse(File.ReadAllText(App.Cfg1));

            JArray cfg_equip = JArray.Parse(cfg1["cfg_equip"].ToString());
            int num = -1;
            try
            {
                for (int i = 0; i < cfg_equip.Count; i++)
                {
                    if (i != 0)
                        LocalList.Add($"equip/arm_{cfg_equip[i][num].ToString().PadLeft(5, '0')}.png");
                    else
                        num = JArray.Parse(cfg_equip[i].ToString()).ToObject<List<string>>().IndexOf("picindex");
                }
            }
            catch (Exception ex)
            {
                if (num < 0)
                    System.Windows.MessageBox.Show($"Index : {num}, 請檢查程式", "equip fail");
                else
                    System.Windows.MessageBox.Show(ex.Message.ToString(), "equip fail");
                return;
            }

            JArray cfg_partner = JArray.Parse(cfg1["cfg_partner"].ToString());
            List<KeyValuePair<string, bool>> PartnerList = new List<KeyValuePair<string, bool>>();
            Dictionary<string, string> pid = new Dictionary<string, string>
            {
                { "10001", "cha_00001"},
                { "10002", "cha_00002"},
                { "10003", "cha_00003"}
            };
            PartnerList.Add(new KeyValuePair<string, bool>("cha_00001", true));
            PartnerList.Add(new KeyValuePair<string, bool>("cha_00002", true));
            PartnerList.Add(new KeyValuePair<string, bool>("cha_00003", true));
            num = -1;
            int num1 = -1;
            int num2 = -1;
            int num3 = -1;
            try
            {
                for (int i = 0; i < cfg_partner.Count; i++)
                {
                    if (i != 0)
                    {
                        if (!String.IsNullOrEmpty(cfg_partner[i][num].ToString()))
                            PartnerList.Add((new KeyValuePair<string, bool>(cfg_partner[i][num].ToString(), Convert.ToBoolean(Convert.ToInt32(cfg_partner[i][num2].ToString())))));
                        else
                            PartnerList.Add((new KeyValuePair<string, bool>(cfg_partner[i][num1].ToString(), Convert.ToBoolean(Convert.ToInt32(cfg_partner[i][num2].ToString())))));
                        if (!String.IsNullOrEmpty(cfg_partner[i][num3].ToString()))
                            pid.Add(cfg_partner[i][num3].ToString(), cfg_partner[i][num].ToString());
                    }
                    else
                    {
                        num = JArray.Parse(cfg_partner[i].ToString()).ToObject<List<string>>().IndexOf("picindex");
                        num1 = JArray.Parse(cfg_partner[i].ToString()).ToObject<List<string>>().IndexOf("avt");
                        num2 = JArray.Parse(cfg_partner[i].ToString()).ToObject<List<string>>().IndexOf("spine");
                        num3 = JArray.Parse(cfg_partner[i].ToString()).ToObject<List<string>>().IndexOf("partnerid");
                    }
                }
            }
            catch (Exception ex)
            {
                if (num < 0 && num1 < 0)
                    System.Windows.MessageBox.Show($"Index : ({num}, {num1}), 請檢查程式", "partner fail");
                else
                    System.Windows.MessageBox.Show(ex.Message.ToString(), "partner fail");
                return;
            }

            foreach (string s in pid.Values)
            {
                LocalList.Add($"plot/pictures_fetter_role_{s}.png");
            }
            
            foreach (KeyValuePair<string, bool> Kpr in PartnerList)
            {
                string pr = Kpr.Key;
                bool spine = Kpr.Value;

                LocalList.Add($"hero/big/{pr}.png");

                for (int i = 1; i <= 4; i++)
                {
                    LocalList.Add($"hero/cg/{pr}_{i}.json");
                    LocalList.Add($"hero/cg/{pr}_{i}.png");
                    if (i != 3)
                        LocalList.Add($"hero/cg/{pr}_{i}_c.png");
                }

                LocalList.Add($"hero/head/{pr}_bp.png");

                LocalList.Add($"hero/name/{pr}.png");

                LocalList.Add($"hero/picture_sd/{pr}_SD.png");

                LocalList.Add($"hero/sd/{pr}.png");

                if (spine == true)
                {
                    LocalList.Add($"hero/spine/{pr}_anim_standby.atlas");
                    LocalList.Add($"hero/spine/{pr}_anim_standby.json");
                    LocalList.Add($"hero/spine/{pr}_anim_standby.png");
                }
            }

            JArray cfg_item = JArray.Parse(cfg1["cfg_item"].ToString());
            num = -1;
            try
            {
                for (int i = 0; i < cfg_item.Count; i++)
                {
                    if (i != 0)
                    {
                        LocalList.Add($"item/item_{cfg_item[i][num].ToString().PadLeft(5, '0')}.png");
                        LocalList.Add($"item/gacha/item_{cfg_item[i][num].ToString().PadLeft(5, '0')}.png");
                    }
                    else
                        num = JArray.Parse(cfg_item[i].ToString()).ToObject<List<string>>().IndexOf("imgs");
                }
            }
            catch (Exception ex)
            {
                if (num < 0)
                    System.Windows.MessageBox.Show($"Index : {num}, 請檢查程式", "item fail");
                else
                    System.Windows.MessageBox.Show(ex.Message.ToString(), "item fail");
                return;
            }

            JArray cfg_chapter = JArray.Parse(cfg1["cfg_chapter"].ToString());
            num = -1;
            num1 = -1;
            try
            {
                for (int i = 0; i < cfg_chapter.Count; i++)
                {
                    if (i != 0)
                    {
                        if (!String.IsNullOrEmpty(cfg_chapter[i][num].ToString()))
                            LocalList.Add($"map/{cfg_chapter[i][num].ToString()}.png");
                        if (!String.IsNullOrEmpty(cfg_chapter[i][num1].ToString()))
                            LocalList.Add($"map/{cfg_chapter[i][num1].ToString()}.png");
                    }
                    else
                    {
                        num = JArray.Parse(cfg_chapter[i].ToString()).ToObject<List<string>>().IndexOf("big_img");
                        num1 = JArray.Parse(cfg_chapter[i].ToString()).ToObject<List<string>>().IndexOf("small_img");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message.ToString(), "chapter fail");
                return;
            }

            JArray cfg_skillmain = JArray.Parse(cfg1["cfg_skillmain"].ToString());
            num = -1;
            num1 = -1;
            try
            {
                for (int i = 0; i < cfg_skillmain.Count; i++)
                {
                    if (i != 0)
                    {
                        if (!String.IsNullOrEmpty(cfg_skillmain[i][num1].ToString()) && !String.IsNullOrEmpty(cfg_skillmain[i][num].ToString()))
                            LocalList.Add($"skill/skill_{cfg_skillmain[i][num].ToString()}.png");
                    }
                    else
                    {
                        num = JArray.Parse(cfg_skillmain[i].ToString()).ToObject<List<string>>().IndexOf("icon");
                        num1 = JArray.Parse(cfg_skillmain[i].ToString()).ToObject<List<string>>().IndexOf("name");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message.ToString(), "skillmain fail");
                return;
            }

            /* CFG_3 */

            JObject cfg3 = JObject.Parse(File.ReadAllText(App.Cfg3));

            JArray cfg_hint = JArray.Parse(cfg3["cfg_hint"].ToString());
            num = -1;
            try
            {
                for (int i = 0; i < cfg_hint.Count; i++)
                {
                    if (i != 0)
                        LocalList.Add($"hint/{cfg_hint[i][num].ToString()}");
                    else
                        num = JArray.Parse(cfg_hint[i].ToString()).ToObject<List<string>>().IndexOf("img");
                }
            }
            catch (Exception ex)
            {
                if (num < 0)
                    System.Windows.MessageBox.Show($"Index : {num}, 請檢查程式", "hint fail");
                else
                    System.Windows.MessageBox.Show(ex.Message.ToString(), "hint fail");
                return;
            }

            JArray cfg_push = JArray.Parse(cfg3["cfg_push"].ToString());
            num = -1;
            num1 = -1;
            try
            {
                for (int i = 0; i < cfg_push.Count; i++)
                {
                    if (i != 0)
                    {
                        if (!String.IsNullOrEmpty(cfg_push[i][num].ToString()))
                            LocalList.Add($"push_activity/{cfg_push[i][num].ToString()}");
                        if (!String.IsNullOrEmpty(cfg_push[i][num1].ToString()))
                        {
                            LocalList.Add($"push_activity/{cfg_push[i][num1].ToString()}");
                            LocalList.Add($"push_activity/banner/{cfg_push[i][num1].ToString()}");
                        }
                    }
                    else
                    {
                        num = JArray.Parse(cfg_push[i].ToString()).ToObject<List<string>>().IndexOf("banner");
                        num1 = JArray.Parse(cfg_push[i].ToString()).ToObject<List<string>>().IndexOf("popup");
                    }
                }
            }
            catch (Exception ex)
            {
                if (num < 0 || num1 < 0)
                    System.Windows.MessageBox.Show($"Index : ({num}, {num1}), 請檢查程式", "push fail");
                else
                    System.Windows.MessageBox.Show(ex.Message.ToString(), "push fail");
                return;
            }

            /* CFG_5 */

            JObject cfg5 = JObject.Parse(File.ReadAllText(App.Cfg5));

            JArray cfg_chat = JArray.Parse(cfg5["cfg_chat"].ToString());
            num = -1;
            try
            {
                for (int i = 0; i < cfg_chat.Count; i++)
                {
                    if (i != 0)
                        LocalList.Add($"chat/{cfg_chat[i][num].ToString()}.png");
                    else
                        num = JArray.Parse(cfg_chat[i].ToString()).ToObject<List<string>>().IndexOf("chat");
                }
            }
            catch(Exception ex)
            {
                if (num < 0)
                    System.Windows.MessageBox.Show($"Index : {num}, 請檢查程式", "chat fail");
                else
                    System.Windows.MessageBox.Show(ex.Message.ToString(), "chat fail");
                return;
            }

            JArray cfg_voice = JArray.Parse(cfg5["cfg_voice"].ToString());
            num = -1;
            num1 = -1;
            num2 = -1;
            try
            {
                for (int i = 0; i < cfg_voice.Count; i++)
                {
                    if (i != 0)
                    {
                        for (int j = num; j <= num1; j++)
                        {
                            foreach (string v in cfg_voice[i][j].ToString().Split('|'))
                            {
                                LocalList.Add($"hero/voice/{pid[cfg_voice[i][num2].ToString()]}/{v}.mp3");
                            }
                        }
                    }
                    else
                    {
                        num = JArray.Parse(cfg_voice[i].ToString()).ToObject<List<string>>().IndexOf("get");
                        num1 = JArray.Parse(cfg_voice[i].ToString()).ToObject<List<string>>().IndexOf("favorable_lines");
                        num2 = JArray.Parse(cfg_voice[i].ToString()).ToObject<List<string>>().IndexOf("pid");
                    }
                }
            }
            catch (Exception ex)
            {
                if (num < 0 || num1 < 0)
                    System.Windows.MessageBox.Show($"Index : ({num}, {num1}), 請檢查程式", "voice fail");
                else
                    System.Windows.MessageBox.Show(ex.Message.ToString(), "voice fail");
                return;
            }

            JArray cfg_partner_weapon = JArray.Parse(cfg5["cfg_partner_weapon"].ToString());
            num = -1;
            try
            {
                for (int i = 0; i < cfg_partner_weapon.Count; i++)
                {
                    if (i != 0)
                    {
                        LocalList.Add($"item/partner_weapon_{cfg_partner_weapon[i][num].ToString()}.png");
                    }
                    else
                    {
                        num = JArray.Parse(cfg_partner_weapon[i].ToString()).ToObject<List<string>>().IndexOf("cfg_p_id");
                    }
                }
            }
            catch (Exception ex)
            {
                if (num < 0)
                    System.Windows.MessageBox.Show($"Index : {num}, 請檢查程式", "partner_weapon fail");
                else
                    System.Windows.MessageBox.Show(ex.Message.ToString(), "partner_weapon fail");
                return;
            }

            JArray cfg_plot = JArray.Parse(cfg5["cfg_plot"].ToString());
            List<string> DataList = new List<string>();
            num = -1;
            num1 = -1;
            num2 = -1;
            try
            {
                for (int i = 0; i < cfg_plot.Count; i++)
                {
                    if (i != 0)
                    {
                        LocalList.Add($"plot/{cfg_plot[i][num].ToString()}.png");
                        
                        if (!String.IsNullOrEmpty(cfg_plot[i][num1].ToString()))
                            DataList.Add($"scenario/{cfg_plot[i][num1].ToString()}");

                        if (!String.IsNullOrEmpty(cfg_plot[i][num2].ToString()))
                            DataList.Add($"scenario/{cfg_plot[i][num2].ToString()}");
                    }
                    else
                    {
                        num = JArray.Parse(cfg_plot[i].ToString()).ToObject<List<string>>().IndexOf("chapter_banner");
                        num1 = JArray.Parse(cfg_plot[i].ToString()).ToObject<List<string>>().IndexOf("scenario");
                        num2 = JArray.Parse(cfg_plot[i].ToString()).ToObject<List<string>>().IndexOf("r18");
                    }
                }
            }
            catch (Exception ex)
            {
                if (num < 0 || num1 < 0 || num2 < 0)
                    System.Windows.MessageBox.Show($"Index : ({num}, {num1}, {num2}), 請檢查程式", "plot fail");
                else
                    System.Windows.MessageBox.Show(ex.Message.ToString(), "plot fail");
                return;
            }

            JArray cfg_favorable = JArray.Parse(cfg5["cfg_favorable"].ToString());
            
            num = -1;
            try
            {
                for (int i = 0; i < cfg_favorable.Count; i++)
                {
                    if (i != 0)
                    {
                        if (!String.IsNullOrEmpty(cfg_favorable[i][num].ToString()))
                            DataList.Add($"scenario/{cfg_favorable[i][num].ToString()}");
                    }
                    else
                    {
                        num = JArray.Parse(cfg_favorable[i].ToString()).ToObject<List<string>>().IndexOf("scenario");
                    }
                }
            }
            catch (Exception ex)
            {
                if (num < 0)
                    System.Windows.MessageBox.Show($"Index : {num}, 請檢查程式", "favorable fail");
                else
                    System.Windows.MessageBox.Show(ex.Message.ToString(), "favorable fail");
                return;
            }

            /* Download Part 1 */

            CryptoServices cryptoServices = new CryptoServices();

            int count = 0;
            int quantity = DataList.Count;
            List<Task> tasks = new List<Task>();

            foreach (string s in DataList)
            {
                tasks.Add(DownLoadFile($"{App.resourceUrl}{cryptoServices.MD5Hash($"{s}/data.json")}.json", $"{App.Respath}{s}/data.json", cb_isCover.IsChecked == true ? true : false));
                count++;

                // 阻塞線程，等待現有工作完成再給新工作
                if ((count % pool).Equals(0) || quantity == count)
                {
                    Task t = Task.WhenAll(tasks);
                    tasks.Clear();
                    try
                    {
                        t.Wait();
                    }
                    catch { }
                }

                // 用await將線程讓給UI更新
                lb_counter.Content = $"進度(1/3) : {count} / {quantity}";
                await Task.Delay(1);
            }

            // 需要讀data.json的內容，因此等它全部載完
            Task td = Task.WhenAll(tasks);
            tasks.Clear();
            try
            {
                td.Wait();
            }
            catch { }

            foreach (string s in DataList)
            {
                if (File.Exists($"{App.Respath}{s}/data.json"))
                {
                    JObject data = JObject.Parse(File.ReadAllText($"{App.Respath}{s}/data.json"));
                    JArray commands = JArray.Parse(data["commands"].ToString());
                    foreach (JObject jo in commands)
                    {
                        if (jo.ContainsKey("content") && !String.IsNullOrEmpty(jo["content"].ToString()))
                        {
                            if (jo["cmd"].ToString() == "bgm")
                                LocalList.Add($"{s}/{jo["content"].ToString()}.mp3");
                            if (jo["cmd"].ToString() == "background" && String.IsNullOrEmpty(jo["name"].ToString()))
                                LocalList.Add($"{s}/{jo["content"].ToString()}.jpg");
                        }
                        if (jo.ContainsKey("voice") && !String.IsNullOrEmpty(jo["voice"].ToString()))
                        {
                            if (jo["cmd"].ToString() == "text")
                                LocalList.Add($"{s}/{jo["voice"].ToString()}.mp3");
                        }
                    }
                }
            }

            /* Bully */

            for (int i = 0; i <= 23; i++)
            {
                string[] em = { "login_chara.png", "login_bg.jpg", "battle_activity_logo.png", "bonusbuffbg.png", "bonusbuffcutinbg.jpg", 
                        "battle_activity_startBg.jpg", "primary_btn.png", "intermediate_btn_unch.png", "advance_btn_unch.png"};
                foreach (string s in em)
                {
                    LocalList.Add($"event_me/event_me{i.ToString().PadLeft(4, '0')}/{s}");
                }
            }

            for (int i = 1; i <= 100; i++)
            {
                LocalList.Add($"gacha/gacha_bg/back_{i.ToString().PadLeft(5, '0')}.png");

                LocalList.Add($"icon/icon_job_mini_{i}.png");
            }

            for (int i = 1; i <= 10; i++)
            {
                LocalList.Add($"plot/banner_{i}.png");
                LocalList.Add($"plot/logo_{i}.png");
            }

            for (int i = 0; i <= 300; i++)
            {
                LocalList.Add($"push_activity/icon/icon_{i.ToString().PadLeft(5, '0')}.png");
            }

            for (int i = 0; i <= 20; i++)
            {
                LocalList.Add($"vip/vip_big_{i}.png");
                LocalList.Add($"vip/vip_small_{i}.png");
            }

            /* Download Part 2 */

            count = 0;
            quantity = HotList.Count;

            foreach (string s in HotList)
            {
                tasks.Add(DownLoadFile($"{App.webClientUrl}{s}", $"{App.Hotpath}{s}", cb_isCover.IsChecked == true ? true : false));
                count++;

                // 阻塞線程，等待現有工作完成再給新工作
                if ((count % pool).Equals(0) || quantity == count)
                {
                    Task t = Task.WhenAll(tasks);
                    tasks.Clear();
                    try
                    {
                        t.Wait();
                    }
                    catch { }
                }

                // 用await將線程讓給UI更新
                lb_counter.Content = $"進度(2/3) : {count} / {quantity}";
                await Task.Delay(1);
            }

            // 遇到404的情況下重複的URL會嚴重拖累速度，因此要去重複
            LocalList = LocalList.Distinct().ToList();

            count = 0;
            int quantity1 = LocalList.Count;

            foreach (string s in LocalList)
            {
                tasks.Add(DownLoadFile($"{App.resourceUrl}{cryptoServices.MD5Hash(s)}{Path.GetExtension(s)}", $"{App.Respath}{s}", cb_isCover.IsChecked == true ? true : false));
                count++;

                // 阻塞線程，等待現有工作完成再給新工作
                if ((count % pool).Equals(0) || quantity == count)
                {
                    Task t = Task.WhenAll(tasks);
                    tasks.Clear();
                    try
                    {
                        t.Wait();
                    }
                    catch { }
                }

                // 用await將線程讓給UI更新
                lb_counter.Content = $"進度(3/3) : {count} / {quantity1}";
                await Task.Delay(1);
            }

            if (cb_Debug.IsChecked == true)
            {
                using (StreamWriter outputFile = new StreamWriter("Fail.log", false))
                {
                    foreach (string s in App.log)
                        outputFile.WriteLine(s);
                }
            }
            
            System.Windows.MessageBox.Show($"下載完成，共{App.glocount}個檔案", "Finish");
            lb_counter.Content = String.Empty;
        }

        public Task DownLoadFile(string downPath, string savePath, bool overWrite)
        {
            bool create = false;
            if (!Directory.Exists(Path.GetDirectoryName(savePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                create = true;
            }

            if (File.Exists(savePath) && overWrite == false)
                return Task.FromResult(0);

            App.glocount++;

            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.DownloadFile(downPath, savePath);
                }
                catch (Exception ex)
                {
                    App.glocount--;

                    if (create == true)
                        Directory.Delete(Path.GetDirectoryName(savePath));
                    
                    if (cb_Debug.IsChecked == true)
                        App.log.Add(downPath + Environment.NewLine + savePath + Environment.NewLine);

                    // 沒有的資源直接跳過，避免報錯。
                    //System.Windows.MessageBox.Show(ex.Message.ToString() + Environment.NewLine + downPath + Environment.NewLine + savePath);
                }
            }
            return Task.FromResult(0);
        }
    }
}
