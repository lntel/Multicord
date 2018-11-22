using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunifu.Framework;
using System.Threading;
using System.IO;
using System.Data.SQLite;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Multicord
{
    public partial class Form1 : Form
    {
        public Thread startup_t;
        private List<Dictionary<string, string>> data;
        private List<Dictionary<string, string>> conf_d;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            startup_t = new Thread(() =>
            {
                Discord dc = new Discord();

                Config config = new Config();

                if (!dc.isActive() && config.status == -1)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        Alert alert = new Alert("Failure", "You must sign in to Discord before you start this application for the first time.", -1);
                    });
                } else
                {
                    conf_d = config.read();

                    data = config.read();

                    foreach(Dictionary<string, string> x in data)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            bunifuDropdown1.AddItem(x["email"]);
                        });
                    }
                }
            });

            startup_t.Start();
        }

        private void bunifuFlatButton1_Click(object sender, EventArgs e)
        {
            try
            {
                string email = bunifuDropdown1.selectedValue;
                string filename = null;

                if (email != "")
                {
                    foreach (Dictionary<string, string> x in conf_d)
                    {
                        if (x["email"] == email) { filename = x["filename"]; break; }
                    }

                    Discord dc = new Discord();

                    dc.Switch(filename);
                }
            }
            catch(Exception ex)
            {
                Alert alert = new Alert("Failure", "You must select an account from the dropdown menu.", -1);
            }
        }

        private void bunifuImageButton1_Click(object sender, EventArgs e)
        {
            Add add = new Add(this);
            add.Show();
        }

        public void UpdateList()
        {
            Config config = new Config();
            Discord dc = new Discord();

            conf_d = config.read();

            data = config.read();

            this.Invoke((MethodInvoker)delegate
            {
                bunifuDropdown1.Clear();
            });

            foreach (Dictionary<string, string> x in data)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    bunifuDropdown1.AddItem(x["email"]);
                });
            }
        }
    }

    public class Account
    {
        public string email { get; set; }
        public string filename { get; set; }
    }

    public class Config
    {
        public string dir_path;
        public int status;
        public Config()
        {
            dir_path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Multicord";

            if(!Directory.Exists(dir_path))
            {
                try
                {
                    Directory.CreateDirectory(dir_path);
                    Directory.CreateDirectory(string.Format(@"{0}\Accounts", dir_path));

                    File.Create(string.Format(@"{0}\app.conf", dir_path));

                    Discord dc = new Discord();

                    if(dc.isActive())
                    {
                        string content;
                        Dictionary<string, string> tmp = dc.read();

                        Guid filename = Guid.NewGuid();

                        File.Copy(dc.local, string.Format(@"{0}\Accounts\{1}", dir_path, filename));

                        content = "{ \"email\": " + tmp["email_cache"] + ", \"filename\": \"" + filename+ "\" }\r\n";

                        File.WriteAllText(string.Format(@"{0}\app.conf", dir_path), content);
                    } else
                    {
                        status = -1;
                    }
                }
                catch(Exception ex)
                {

                }
            }
        }

        public List<Dictionary<string, string>> read()
        {
            List<Dictionary<string, string>> tmp = new List<Dictionary<string, string>>();

            if(status != -1)
            {
                using (FileStream str = File.OpenRead(string.Format(@"{0}\app.conf", dir_path)))
                {
                    using (StreamReader fstr = new StreamReader(str, Encoding.UTF8, true, 128))
                    {
                        string line;
                        int index = 0;

                        while((line = fstr.ReadLine()) != null)
                        {
                            Account account = JsonConvert.DeserializeObject<Account>(line);

                            tmp.Add(new Dictionary<string, string>());

                            tmp[index].Add("email", account.email);
                            tmp[index].Add("filename", account.filename);

                            index++;
                        }
                    }
                }

                return tmp;
            }

            return null;
        }

        public void Add()
        {
            try
            {
                string content;

                Dictionary<string, string> tmp;

                Discord dc = new Discord();
                Config config = new Config();

                tmp = dc.read();

                Guid filename = Guid.NewGuid();

                File.Copy(dc.local, string.Format(@"{0}\Accounts\{1}", config.dir_path, filename));

                content = "{ \"email\": " + tmp["email_cache"] + ", \"filename\": \"" + filename + "\" }\r\n";

                File.AppendAllText(string.Format(@"{0}\app.conf", config.dir_path), content);
            }
            catch(Exception ex)
            {
                //
            }
        }

        public bool Exists()
        {
            Discord dc = new Discord();

            Dictionary<string, string> dc_data = dc.read();

            foreach(Dictionary<string, string> x in read())
            {
                if (x["email"] == dc_data["email_cache"].Replace("\"", string.Empty)) return true;
            }

            return false;
        }
    }

    public class Discord
    {
        public string local;
        public string application;

        private Thread swap_t;
        public Discord()
        {
            local = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\discord\Local Storage\https_discordapp.com_0.localstorage";
            application = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Discord\app-0.0.300\Discord.exe";
        }

        public Dictionary<string, string> read()
        {
           Dictionary<string, string> tmp = new Dictionary<string, string>();

            SQLiteConnection sql = new SQLiteConnection(string.Format("Data Source={0}", local));

            try
            {
                sql.Open();

                using (var cmd = sql.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM ItemTable";

                    var r = cmd.ExecuteReader();

                    while(r.Read())
                    {
                        byte[] value_b = (byte[])r["value"];
                        string key = r["key"].ToString();

                        var res = value_b.Where(b => b != 0).ToList();

                        tmp.Add(key, Encoding.UTF8.GetString(res.ToArray()));
                    }

                    return tmp;
                }
            }
            catch(Exception ex)
            {
                // 
                return tmp;
            }
        }

        public void Switch(string filename)
        {
            swap_t = new Thread(() =>
            {
                Config config = new Config();
                Discord dc = new Discord();

                foreach (Process x in Process.GetProcessesByName("Discord"))
                {
                    try
                    {
                        x.Kill();
                    }
                    catch(Exception ex)
                    {
                        // 
                    }
                }

                Thread.Sleep(1000);

                File.Copy(string.Format(@"{0}\Accounts\{1}", config.dir_path, filename), dc.local, true);

                Process.Start(dc.application);
            });

            swap_t.Start();
        }

        public bool isActive()
        {
            Dictionary<string, string> tmp = read();

            return (tmp.ContainsKey("token") && tmp["token"] != null);
        }
    }
}
