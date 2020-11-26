using J_Project.FileSystem;
using PropertyChanged;

namespace J_Project.Manager
{
    [ImplementPropertyChanged]
    public class TestOption
    {
        public bool IsPassiveCtrl { get; set; }
        public bool IsFullAuto { get; set; }
        public bool IsLoadManage { get; set; }

        #region 싱글톤 패턴 구현
        private static TestOption SingleTonObj = null;

        private TestOption()
        {
            IsPassiveCtrl = false;
            IsFullAuto = false;
            IsLoadManage = false;
        }

        public static TestOption GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new TestOption();
            return SingleTonObj;
        }
        #endregion

        public static void Save()
        {
            Setting.WriteSetting(GetObj(), @"\Setting\TestOption.ini");
        }
        public static void Load()
        {
            Setting.ReadSetting(GetObj(), @"\Setting\TestOption.ini");
        }
    }
}