using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using J_Project.Manager;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace J_Project.FileSystem
{
    public class Setting
    {
        #region .ini File 작성을 위한 DLL 추가
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        #endregion
        
        public static bool WriteSetting(object classObj, string fileName = @"\Setting\TestSetting.ini")
        {
            Type classType = classObj.GetType();
            FieldInfo[] fieldList = classType.GetFields(Util.AllField);
            FileInfo settingFilePath = new FileInfo(Environment.CurrentDirectory + fileName);

            if (!DirectoryManager.CreateSettingFolder())
                return false;

            foreach (var fieldItem in fieldList)
            {
                if (fieldItem.FieldType == typeof(ObservableCollection<double>))
                {
                    ObservableCollection<double> temp = (ObservableCollection<double>)fieldItem.GetValue(classObj);
                    for (int i = 0; i < temp.Count; i++)
                        WritePrivateProfileString(classType.Name, fieldItem.Name + i.ToString(Util.Cultur), temp[i].ToString(Util.Cultur), settingFilePath.FullName);
                }
                else if (fieldItem.FieldType == typeof(ObservableCollection<int>))
                {
                    ObservableCollection<int> temp = (ObservableCollection<int>)fieldItem.GetValue(classObj);
                    for (int i = 0; i < temp.Count; i++)
                        WritePrivateProfileString(classType.Name, fieldItem.Name + i.ToString(Util.Cultur), temp[i].ToString(Util.Cultur), settingFilePath.FullName);
                }
                else if (fieldItem.FieldType == typeof(string))
                    WritePrivateProfileString(classType.Name, fieldItem.Name, fieldItem.GetValue(classObj).ToString(), settingFilePath.FullName);
                else if (fieldItem.FieldType == typeof(bool))
                    WritePrivateProfileString(classType.Name, fieldItem.Name, fieldItem.GetValue(classObj).ToString(), settingFilePath.FullName);
            }

            if (settingFilePath.Exists)
                return true;
            else
                return false;
        }

        public static bool ReadSetting(object classObj, string fileName = @"\Setting\TestSetting.ini")
        {
            Type classType = classObj.GetType();
            FieldInfo[] fieldList = classType.GetFields(Util.AllField);
            FileInfo settingFilePath = new FileInfo(Environment.CurrentDirectory + fileName);
            StringBuilder sb = new StringBuilder(100);

            try
            {
                foreach (FieldInfo fieldItem in fieldList)
                {
                    for(int i = 0; true ; i++)
                    {
                        if (fieldItem.FieldType == typeof(ObservableCollection<double>))
                        {
                            // ini파일로부터 데이터 추출
                            GetPrivateProfileString(classType.Name, fieldItem.Name + i.ToString(Util.Cultur), string.Empty, sb, sb.Capacity, settingFilePath.FullName);

                            if (string.IsNullOrEmpty(sb.ToString()))
                            {
                                sb.Append(string.Empty);
                                break;
                            }

                            ObservableCollection<double> temp = (ObservableCollection<double>)fieldItem.GetValue(classObj);
                            temp.Add(double.Parse(sb.ToString(), Util.Cultur));
                            temp.RemoveAt(0);
                        }
                        else if (fieldItem.FieldType == typeof(ObservableCollection<int>))
                        {
                            // ini파일로부터 데이터 추출
                            GetPrivateProfileString(classType.Name, fieldItem.Name + i.ToString(Util.Cultur), string.Empty, sb, sb.Capacity, settingFilePath.FullName);

                            if (string.IsNullOrEmpty(sb.ToString()))
                            {
                                sb.Append(string.Empty);
                                break;
                            }

                            ObservableCollection<int> temp = (ObservableCollection<int>)fieldItem.GetValue(classObj);
                            temp.Add(int.Parse(sb.ToString(), Util.Cultur));
                            temp.RemoveAt(0);
                        }
                        else if (fieldItem.FieldType == typeof(string))
                        {
                            // ini파일로부터 데이터 추출
                            GetPrivateProfileString(classType.Name, fieldItem.Name, string.Empty, sb, sb.Capacity, settingFilePath.FullName);

                            // 가져온 데이터를 클래스 필드의 자료형에 맞게 변환
                            object settingValue = Convert.ChangeType(sb.ToString(), fieldItem.FieldType, Util.Cultur);
                            fieldItem.SetValue(classObj, settingValue); // 데이터 삽입
                            break;
                        }
                        else if (fieldItem.FieldType == typeof(bool))
                        {
                            // ini파일로부터 데이터 추출
                            GetPrivateProfileString(classType.Name, fieldItem.Name, string.Empty, sb, sb.Capacity, settingFilePath.FullName);

                            // 가져온 데이터를 클래스 필드의 자료형에 맞게 변환
                            object settingValue = Convert.ChangeType(sb.ToString(), fieldItem.FieldType, Util.Cultur);
                            fieldItem.SetValue(classObj, settingValue); // 데이터 삽입
                            break;
                        }
                        else
                            break;
                    }  
                }
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}