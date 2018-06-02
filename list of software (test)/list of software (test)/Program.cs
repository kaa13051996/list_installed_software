﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace list_of_software__test_
{
    class Reg
    {
        public static void Main()
        {
			string label_name_user = Environment.UserName.ToString();

            string[] path = list_path();

            RegistryKey[] localKey = list_localkey();

            string[][] names_key = list_names_key_path(localKey, path);

            //общий список ПО
            Dictionary<string[], RegistryKey> list_softwares = new Dictionary<string[], RegistryKey>();
            for (int i = 0; i < localKey.Count(); i++) list_softwares.Add(list_software(names_key[i], localKey[i], path[i]), localKey[i]);

            //удалить одинаковые имена программ
            //list_softwares = list_softwares.Distinct().ToList();

            List<string> list = list_parameters(list_softwares);
            if (list.Count == 0) Console.WriteLine("Программ в заданных ветках реестра не обнаружено!");
            else
            {
                Console.WriteLine("\n--- All programm ---");
                //foreach (KeyValuePair<string[], RegistryKey> kvp in list_softwares)
                //{
                //    foreach (var i in kvp.Key)
                //    {
                //        Console.WriteLine("Key = {0}, Value = {1}",
                //        i, kvp.Value);
                //    }
                //}

                //Console.WriteLine("\n--- Parametrs ---");
                for (int i = 0; i < list.Count / 2; i += 2)
                {
                    Console.WriteLine("Name = {0},\t Date = {1}", list[i], list[i + 1]);
                }
            }
            Console.ReadKey();
        }

        static void PrintKeys(RegistryKey rkey)
        {
            String[] names = rkey.GetSubKeyNames();

            int icount = 0;

            Console.WriteLine("Subkeys of " + rkey.Name);
            Console.WriteLine("-----------------------------------------------");
            
            foreach (String s in names)
            {
                Console.WriteLine(s);               
                icount++;                
            }
        }
                
        static string[] list_software(String[] names_dir, RegistryKey localKey, string path)
        {
            //Console.WriteLine("\n--- " + path + " ---");
            string[] list = new string[names_dir.Length];
            int count = 0;
            int no_display_name = 0;

            for (int i = 0; i < names_dir.Length; i++)
            {
                try
                {
                    string value = localKey.OpenSubKey(path + names_dir[i]).GetValue("DisplayName").ToString();
                    //Console.WriteLine(value);
                    list[count] = path + names_dir[i];
                    count++;
                }
                catch (System.NullReferenceException)
                {
                    //Console.WriteLine("-" + names_dir[i] + " no name DispalyName!");
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
                        install_location = names_dir.OpenSubKey(key).GetValue("InstallLocation").ToString().Trim(trim);
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
                return date_install;
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
            int count_localKey = localKey.Count();
            string error = null;
            string[][] list = new string[count_localKey][];
            int current = 0;
            for (int i = 0; i < count_localKey; i++)
            {
                try
                {
                    list[i] = localKey[i].OpenSubKey(path[i]).GetSubKeyNames();
                }
                catch (Exception ex)
                {
                    error += localKey[i] + path[i] + "\n";
                    list[i] = new string[0];
                }
                //current++;
            }
            if (!String.IsNullOrEmpty(error)) Console.Write("Такой ветки(-ок) в вашем реестре не существует:\n" + error);
            
            return list;
        }

        //если добавилась ветка реестра
        static RegistryKey[] list_localkey()
        {
            RegistryKey[] localKey = new RegistryKey[4];

            if (Environment.Is64BitOperatingSystem)
            {
                localKey[0] = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                localKey[1] = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                localKey[2] = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                localKey[3] = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Registry64);
            }
            else
            {
                localKey[0] = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                localKey[1] = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                localKey[2] = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);
                localKey[3] = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Registry32);
            }
            return localKey;
        }
    }
}
