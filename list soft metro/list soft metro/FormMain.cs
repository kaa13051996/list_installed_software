﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace list_soft_metro
{
    public partial class FormMain : MetroFramework.Forms.MetroForm
    {
        public FormMain()
        {
            InitializeComponent();
            string name_current_user = Environment.UserName.ToString();
            label_name_user.Text = name_current_user;
            
            roundedPicBox.Image = get_picture_current_user(name_current_user);
            Regedit();
        }
        
        public void Regedit()
        {
            string[] path = list_path();

            RegistryKey[] localKey = list_localkey();

            string[][] names_key = list_names_key_path(localKey, path);

            //общий список ПО
            Dictionary<string[], RegistryKey> list_softwares = new Dictionary<string[], RegistryKey>();

            list_softwares.Add(list_software(names_key[0], localKey[0], path[0]), localKey[0]);
            list_softwares.Add(list_software(names_key[1], localKey[0], path[1]), localKey[0]);
            list_softwares.Add(list_software(names_key[2], localKey[1], path[2]), localKey[1]);
            list_softwares.Add(list_software(names_key[3], localKey[2], path[3]), localKey[2]);

            //удалить одинаковые имена программ
            //list_softwares = list_softwares.Distinct().ToList();

            List<string> list = list_parameters(list_softwares);
            cout_db(list);

        }
        static string[] list_software(String[] names_dir, RegistryKey localKey, string path)
        {            
            string[] list = new string[names_dir.Length];
            int count = 0;
            int no_display_name = 0;

            for (int i = 0; i < names_dir.Length; i++)
            {
                try
                {
                    string value = localKey.OpenSubKey(path + names_dir[i]).GetValue("DisplayName").ToString();                    
                    list[count] = path + names_dir[i];
                    count++;
                }
                catch (System.NullReferenceException)
                {
                    no_display_name++;
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
            Array.Resize(ref list, list.Length - no_display_name);
            return list;
        }

        static List<string> list_parameters(Dictionary<string[], RegistryKey> names_dir)
        {
            List<string> mass = new List<string>();
            int no_parameters = 0;
            string display_name = null, date_install = null;
            foreach (var key in names_dir.Keys)
            {
                for (int i = 0; i < key.Length; i++)
                {
                    try
                    {
                        display_name = names_dir[key].OpenSubKey(key[i]).GetValue("DisplayName").ToString();
                        mass.Add(display_name);
                        date_install = check_date(names_dir[key], key[i]);
                        mass.Add(date_install);
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                }
            }
            return mass;
        }

        //проверка наличия даты
        static string check_date(RegistryKey names_dir, string key)
        {
            string date_install = null, install_location = null;

            char[] trim = { ',', '0', '\"' };
            try
            {
                if (names_dir.OpenSubKey(key).GetValue("InstallDate") == null || names_dir.OpenSubKey(key).GetValue("InstallDate").ToString() == "")
                {
                    //проверка InstallLocation
                    if (names_dir.OpenSubKey(key).GetValue("InstallLocation") == null || names_dir.OpenSubKey(key).GetValue("InstallLocation").ToString() == "")
                    {
                        //проверка DislpayIcon
                        if (names_dir.OpenSubKey(key).GetValue("DisplayIcon") == null || names_dir.OpenSubKey(key).GetValue("DisplayIcon").ToString() == "")
                        {
                            //дальнейшие проверки
                            date_install = "null";
                        }
                        else
                        {
                            install_location = names_dir.OpenSubKey(key).GetValue("DisplayIcon").ToString().Trim(trim);
                            date_install = (System.IO.File.GetCreationTime(install_location).ToString()).Substring(0, 10);
                        }
                    }
                    else
                    {
                        install_location = names_dir.OpenSubKey(key).GetValue("InstallLocation").ToString();
                        date_install = (System.IO.File.GetCreationTime(install_location).ToString()).Substring(0, 10);
                    }
                }
                else
                {
                    date_install = names_dir.OpenSubKey(key).GetValue("InstallDate").ToString();
                    string year = date_install.Substring(0, 4);
                    string month = date_install.Substring(4, 2);
                    string day = date_install.Substring(6, 2);
                    date_install = day + "." + month + "." + year;
                }
            }
            catch (System.NullReferenceException)
            {
                date_install = "null";
            }

            return date_install;
        }

        //если появился новый путь
        static string[] list_path()
        {
            string path_HKLM = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
            string path_HKLM_2 = @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\";
            string path_HKCU = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\";
            string path_HKU = @".DEFAULT\Software\Microsoft\Windows\CurrentVersion\Uninstall\";

            string[] list = { path_HKLM, path_HKLM_2, path_HKCU, path_HKU };

            return list;
        }

        //если появился новый путь
        static string[][] list_names_key_path(RegistryKey[] localKey, string[] path)
        {
            String[] names_key_HKLM = localKey[0].OpenSubKey(path[0]).GetSubKeyNames();
            String[] names_key_HKLM_2 = localKey[0].OpenSubKey(path[1]).GetSubKeyNames();
            String[] names_key_HKCU = localKey[1].OpenSubKey(path[2]).GetSubKeyNames();
            String[] names_key_HKU = localKey[2].OpenSubKey(path[3]).GetSubKeyNames();
            string[][] list = { names_key_HKLM, names_key_HKLM_2, names_key_HKCU, names_key_HKU };
            return list;
        }

        //если добавилась ветка реестра
        static RegistryKey[] list_localkey()
        {
            RegistryKey[] localKey = new RegistryKey[3];

            if (Environment.Is64BitOperatingSystem)
            {
                localKey[0] = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                localKey[1] = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                localKey[2] = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Registry64);
            }
            else
            {
                localKey[0] = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                localKey[1] = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);
                localKey[2] = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Registry32);
            }
            return localKey;
        }

        void cout_db(List<string> mass)
        {
            dataGridView.RowHeadersVisible = false;
            dataGridView.RowCount = mass.Count / 2;
            dataGridView.ColumnCount = 2;
            int count = 0;
            for (int i = 0; i < dataGridView.RowCount; i++)
            {
                dataGridView.Rows[i].Cells[0].Value = mass[count];
                dataGridView.Rows[i].Cells[1].Value = mass[count + 1];
                count += 2;
            }
        }

        public static Bitmap get_picture_current_user(string name)
        {
            string[] args = new string[1];
            try
            {
                args[0] = Directory.GetFiles(@"C:\Users\" + name + @"\AppData\Roaming\Microsoft\Windows\AccountPictures", "*.accountpicture-ms")[0];
                string filename = Path.GetFileNameWithoutExtension(args[0]);                
            }
            catch
            {
                args[0] = @".\guest.png";
                Bitmap image = new Bitmap( Image.FromFile(@"..\..\..\guest.png"));
                return image;
            }
            Bitmap image96 = GetImage96(args[0]);
            return image96;
        }

        //преобразование FileStream в BitmapImage
        public static Bitmap GetImage96(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            long position = Seek(fs, "JFIF", 0);
            byte[] b = new byte[Convert.ToInt32(fs.Length)];
            fs.Seek(position - 6, SeekOrigin.Begin);
            fs.Read(b, 0, b.Length);
            fs.Close();
            fs.Dispose();
            return GetBitmapImage(b);
        }

        //этот метод позволяет начать чтение байтов из выбранного байта. Метод поиска:
        public static long Seek(System.IO.FileStream fs, string searchString, int startIndex)
        {
            char[] search = searchString.ToCharArray();
            long result = -1, position = 0, stored = startIndex,
            begin = fs.Position;
            int c;
            while ((c = fs.ReadByte()) != -1)
            {
                if ((char)c == search[position])
                {
                    if (stored == -1 && position > 0 && (char)c == search[0])
                    {
                        stored = fs.Position;
                    }
                    if (position + 1 == search.Length)
                    {
                        result = fs.Position - search.Length;
                        fs.Position = result;
                        break;
                    }
                    position++;
                }
                else if (stored > -1)
                {
                    fs.Position = stored + 1;
                    position = 1;
                    stored = -1;
                }
                else
                {
                    position = 0;
                }
            }

            if (result == -1)
            {
                fs.Position = begin;
            }
            return result;
        }

        //Преобразование массива байтов в BitmapImage:
        public static Bitmap GetBitmapImage(byte[] imageBytes)
        {
            //var bitmapImage = new Bitmap();
            //bitmapImage.BeginInit();
            //bitmapImage.StreamSource = new MemoryStream(imageBytes);
            //bitmapImage.EndInit();
            //return bitmapImage;
            var ms = new MemoryStream(imageBytes);
            var bitmapImage = new Bitmap(ms);
            return bitmapImage;
        }

    }
}