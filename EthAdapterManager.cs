using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;

namespace EMC07
{
    public class EthAdapterManager
    {
        private const string EthernetCaption = "Сетевой адаптер";
        
        private List<EthAdapter> _adaptersList;

        private NetworkInterface[] networkInterfaces;

        public bool IsChanged;

        /// <summary>
        /// Конструктор собирает информацию по всем адаптерам
        /// и формирует список адаптеров со всеми их настройками
        /// </summary>
        public EthAdapterManager()
        {
            _adaptersList = new List<EthAdapter>();

            networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var netInterface in networkInterfaces)
            {
                if (netInterface.Supports(NetworkInterfaceComponent.IPv4) == false)
                    continue;

                if (netInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback || netInterface.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
                    continue;

                _adaptersList.Add(new EthAdapter(netInterface));
            }

            _adaptersList.Sort((adapter, ethAdapter) => adapter.SystemName.CompareTo(ethAdapter.SystemName));

            int numAdapter = 0;

            foreach (var ethAdapter in _adaptersList)
            {
                ethAdapter.CaptionAdapter = string.Format("{0} {1}", EthernetCaption, numAdapter + 1);
                numAdapter++;
            }

            IsChanged = false;
        }

        /// <summary>
        /// Свойство возвращает List<EthAdapter> со всеми сетевыми адаптерами
        /// </summary>
        public List<EthAdapter> Adapters
        {
            get { return _adaptersList; }
        }

        /// <summary>
        /// Возвращает список сетевых адаптеров с пустым элементом в верху списка
        /// </summary>
        public List<string> GetCaptionEthernetAdapters
        {
            get
            {
                List<string> sList = new List<string>();
                sList.Add("");

                foreach (var adapter in _adaptersList)
                {
                    sList.Add(adapter.CaptionAdapter);
                }

                sList.Sort();
                return sList;
            }
        }

        /// <summary>
        /// Возвращает список сетевых адаптеров без пустого элемента в верху списка
        /// </summary>
        public List<string> GetCaptionEthernetAdapters2
        {
            get
            {
                List<string> sList = new List<string>();
                
                foreach (var adapter in _adaptersList)
                {
                    sList.Add(adapter.CaptionAdapter);
                }

                sList.Sort();
                return sList;
            }
        }

        /// <summary>
        /// Возвращает название первого сетевого адаптера в списке
        /// </summary>
        public string GetFirstCaptionEthernetAdapter
        {
            get
            {
                if (_adaptersList.Count == 0)
                    return "";
                else
                    return _adaptersList[0].CaptionAdapter; //Возвращает имя "Сетевой адаптер 1"
            }
        }

        /// <summary>
        /// Получить IP адрес по названию адаптера (Сетевой адаптер 1,Сетевой адаптер 2...
        /// </summary>
        /// <param name="captionEthernet"></param>
        /// <returns></returns>
        public string GetIP_By_Caption(string captionEthernet)
        {
            return _adaptersList.Single(adapter => adapter.CaptionAdapter == captionEthernet).IpAddress;
        }

        /// <summary>
        /// Получить маску адреса по названию адаптера (Сетевой адаптер 1,Сетевой адаптер 2...
        /// </summary>
        /// <param name="captionEthernet"></param>
        /// <returns></returns>
        public string GetIPMask_By_Caption(string captionEthernet)
        {
            return _adaptersList.Single(adapter => adapter.CaptionAdapter == captionEthernet).IpMask;
        }

        /// <summary>
        /// Получить системное имя адаптера по его названию (Сетевой адаптер 1,Сетевой адаптер 2...
        /// </summary>
        /// <param name="captionEthernet"></param>
        /// <returns></returns>
        public string GetName_By_Caption(string captionEthernet)
        {
            return _adaptersList.Single(adapter => adapter.CaptionAdapter == captionEthernet).SystemName;
        }

        /// <summary>
        /// Возвращает true если OperationalStatus=Up (пр-во имен System.Net.NetworkInformation), т.е. адаптер может передавать пакеты. Введен для того чтобы можно было выставлять dhcp
        /// </summary>
        /// <param name="captionEthernet"></param>
        /// <returns></returns>
        public bool GetOperationalStatus_By_Caption(string captionEthernet)
        {
            return _adaptersList.Single(adapter => adapter.CaptionAdapter == captionEthernet).IsUpLink;
        }

        /// <summary>
        /// Возвращает случайный IP вида 192.168.xxx.xxx
        /// </summary>
        public string GetRandomIP
        {
            get
            {
                Random oct3 = new Random(DateTime.Now.Millisecond);
                Random oct4 = new Random(DateTime.Now.Millisecond / 33);

                return string.Format("192.168.{0}.{1}", oct3.Next(1, 223), oct4.Next(1, 222));
            }
        }

        /// <summary>
        /// возвращает маску по умолчанию
        /// </summary>
        public string GetDefaultIPMask
        {
            get
            {
                return "255.255.255.0";
            }
        }

        /// <summary>
        /// включает/выключает получение сетевого адреса через dhcp
        /// </summary>
        /// <param name="captionEthernet">имя сетевого адаптера</param>
        /// <param name="enable">true - включить</param>
        /// <returns></returns>
        public void SetDHCP_By_Caption(string captionEthernet, bool enable)
        {
            foreach (var ethAdapter in _adaptersList)
            {
                if (ethAdapter.CaptionAdapter == captionEthernet)
                {
                    ethAdapter.DhcpEnable = enable;
                    ethAdapter.IsStateChange = true;

                    break;
                }
            }
        }

        /// <summary>
        /// установка IP адреса сетевого адаптера по его имени
        /// </summary>
        /// <param name="captionEthernet">имя адаптера в программе</param>
        /// <param name="ipAddr">IP адрес</param>
        public void SetIP_By_Caption(string captionEthernet, string ipAddr)
        {
            foreach (var ethAdapter in _adaptersList)
            {
                if (ethAdapter.CaptionAdapter == captionEthernet)
                {
                    ethAdapter.IpAddress = ipAddr;
                    ethAdapter.IsStateChange = true;

                    break;
                }
            }
        }

        /// <summary>
        /// установка маски подсети для сетевого адаптера по его имени
        /// </summary>
        /// <param name="captionEthernet">имя адаптера в программе</param>
        /// <param name="ipMask">маска подсети</param>
        public void SetIPMask_By_Caption(string captionEthernet, string ipMask)
        {
            foreach (var ethAdapter in _adaptersList)
            {
                if (ethAdapter.CaptionAdapter == captionEthernet)
                {
                    ethAdapter.IpMask = ipMask;
                    ethAdapter.IsStateChange = true;

                    break;
                }
            }
        }

        /// <summary>
        /// проверяет задан ли данный ip адрес какому-либо адаптеру
        /// </summary>
        /// <param name="ipAddr">ip адрес</param>
        /// <returns>true если уже задан</returns>
        public bool IsIpExist(string ipAddr)
        {
            foreach (var ethAdapter in _adaptersList)
            {
                if (ethAdapter.IpAddress == ipAddr)
                {
                    Utl.MessageEr("Адаптер с заданным IP адресом уже существует!\n" +
                                  "Задайте другой адрес или включите DHCP.");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// возвращает статус dhcp сетевого адаптера
        /// </summary>
        /// <param name="captionEthernet">имя сетевого адаптеар</param>
        /// <returns>true - включен, false - выключен</returns>
        public bool IsDhcpEn(string captionEthernet)
        {
            return _adaptersList.Single(adapter => adapter.CaptionAdapter == captionEthernet).DhcpEnable;
        }

        /// <summary>
        /// перезагрузка сетевого адаптера
        /// </summary>
        /// <param name="captionEthernet">имя сетевого адаптера</param>
        public void RebootEthAdapter_By_Caption(string captionEthernet)
        {
            bool rebootSuccess =
                _adaptersList.Single(adapter => adapter.CaptionAdapter == captionEthernet).RebootAdapter();

            if (rebootSuccess)
            {
                Log.AddRecStr(Log.LogType.Config, string.Format("{0} был перезагружен", captionEthernet));
            }
        }

        /// <summary>
        /// выдает строку Выключено или Включено в зависимости от переданного знаяения
        /// </summary>
        /// <param name="V">значение состояния (0 или 1)</param>
        public void DHCPStr(ref Data.SlType V)
        {
            V.Max = 1;
            switch (V.V)
            {
                case 0: V.S = "Выключен"; break;
                case 1: V.S = "Включен"; break;
                default: V.V = 0; DHCPStr(ref V); break;
            }
        }

        public bool TryParseIPAddress(string sipAddress)
        {
            string sError;

            bool ok = TryParseIPAddress(sipAddress, out sError);

            if (!ok)
                Utl.MessageEr(sError);

            return ok;
        }

        public bool TryParseIPAddress(string sipAddress, out string sError)
        {
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            sError = "";
            StringBuilder sb = new StringBuilder();

            string[] numbers = Regex.Split(sipAddress, "\\.");

            if (numbers.Length != 4)
            {
                sError = "Ошибка формата IP-адреса";
                return false;
            }

            if (!IPAddress.TryParse(sipAddress, out ipAddress))
            {
                sError = "Ошибка ввода IP-адреса";
                return false;
            }

            //Преобразование в массив байт
            byte[] bytesIP = ipAddress.GetAddressBytes();

            //Проверка первого октета
            if (!(bytesIP[0] >= 1 && bytesIP[0] <= 223))
            {
                sb.Append(string.Format("Недопустимое значение первого октета {0}", bytesIP[0]));
                sb.Append(Environment.NewLine);
                sb.Append("Укажите значение в диапазоне 1..223");
                sError = sb.ToString();
                return false;
            }

            //Проверка четвертого октета
            if (!(bytesIP[3] >= 1 && bytesIP[3] <= 224))
            {
                sb.Append(string.Format("Недопустимое значение четвертого октета {0}", bytesIP[3]));
                sb.Append(Environment.NewLine);
                sb.Append("Укажите значение в диапазоне 1..224");
                sError = sb.ToString();
                return false;
            }

            return true;
        }

        public bool TryParseIPSubnetMask(string sipSubnetmask)
        {
            string sError;

            bool ok = TryParseIPSubnetMask(sipSubnetmask, out  sError);

            if (!ok)
                Utl.MessageEr(sError);

            return ok;


        }

        public bool TryParseIPSubnetMask(string sipSubnetmask, out string sError)
        {
            IPAddress ipSubnetMask = IPAddress.Parse("127.0.0.1");
            sError = "";
            StringBuilder sb = new StringBuilder();

            string[] numbers = Regex.Split(sipSubnetmask, "\\.");

            if (numbers.Length != 4)
            {
                sError = "Ошибка формата IP-адреса";
                return false;
            }

            if (!IPAddress.TryParse(sipSubnetmask, out ipSubnetMask))
            {
                sError = "Ошибка ввода IP-адреса";
                return false;
            }

            //Преобразование в массив байт
            byte[] bytesIP = ipSubnetMask.GetAddressBytes();
            BitArray array = new BitArray(bytesIP);


            //Проверка неразрывности макси подсети
            int length = array.Length;
            int mid = (length / 2);
            int length2;

            //1.Переворачиваем биты
            for (int k = 0; k < length / 8; k++)
            {
                mid = 4;
                length2 = 8;

                for (int i = 0; i < mid; i++)
                {
                    bool bit = array[k * 8 + i];
                    array[k * 8 + i] = array[length2 * (k + 1) - i - 1];
                    array[length2 * (k + 1) - i - 1] = bit;
                }
            }

            bool fBit = false;
            byte nCh = 0;
            //2. Проверяем неразрывности
            for (int i = 0; i < array.Length; i++)
            {
                if (i == 0)
                {
                    fBit = array[i];
                    continue;
                }
                else
                {
                    if (fBit != array[i])
                    {
                        ++nCh;
                        fBit = array[i];
                    }

                    if (nCh > 1)
                        break;
                }
            }

            if (nCh > 1)
            {
                sError = "Введена недопустимая маска подсети.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// При вызове данного метода сохраняются настройки адаптеров
        /// и меняется их состояния, если были сделаны изменения
        /// </summary>
        public void SaveAdaptersSettings()
        {
            foreach (var ethAdapter in _adaptersList)
            {
                if (ethAdapter.IsStateChange)
                {
                    ethAdapter.SaveChanges();
                }
            }

            IsChanged = false;
        }
    }
}
