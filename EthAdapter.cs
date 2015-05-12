using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EMC07
{
    public class EthAdapter
    {
        private string _captionAdapter;
        private string _systemName;
        private string _adapterId;
        private bool _dhcpEnable;
        private string _ipAddress;
        private string _ipMask;

        public bool IsStateChange;

        public delegate void EthAdapterDelegate();
        
        private NetworkInterface _networkInterface;
        
        /// <summary>
        /// свойство, которое возвращает объект NetworkInterface для данного адаптера
        /// </summary>
        public NetworkInterface NetInterface
        {
            get { return _networkInterface; }
            set { _networkInterface = value; }
        }

        public EthAdapter()
        {
            
        }

        /// <summary>
        /// Конструктор класса разбирает полученный объект NetworkInterface
        /// и заполняет свойства класса
        /// </summary>
        /// <param name="adapter">объект NetworkInterface</param>
        public EthAdapter(NetworkInterface adapter)
        {
            _networkInterface = adapter;

            _systemName = _networkInterface.Name;
            _adapterId = _networkInterface.Id;

            IPInterfaceProperties interfaceProperties = _networkInterface.GetIPProperties();
            UnicastIPAddressInformationCollection addressInformationCollection = interfaceProperties.UnicastAddresses;

            foreach (var uniInfo in addressInformationCollection)
            {
                if (uniInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    _ipAddress = uniInfo.Address.ToString();
                    _ipMask = uniInfo.IPv4Mask.ToString();

                    IPv4InterfaceProperties ipv4InterfaceProperties = interfaceProperties.GetIPv4Properties();
                    if (ipv4InterfaceProperties != null)
                    {
                        _dhcpEnable = ipv4InterfaceProperties.IsDhcpEnabled;
                    }
                }
            }

            IsStateChange = false;

            _captionAdapter = "";
        }

        /// <summary>
        /// Метод позволяет обновить свойства класса IP адрес и маску
        /// данные берутся с объекта NetworkInterface
        /// </summary>
        private void UpdateIpAndMask()
        {
            IPInterfaceProperties properties = _networkInterface.GetIPProperties();
            UnicastIPAddressInformationCollection information = properties.UnicastAddresses;

            foreach (var infoAddress in information)
            {
                if (infoAddress.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    _ipAddress = infoAddress.Address.ToString();
                    _ipMask = infoAddress.IPv4Mask.ToString();
                }
            }
        }

        /// <summary>
        /// Имя адаптера в программе
        /// Чаще всего Сетевой адаптер N
        /// </summary>
        public string CaptionAdapter
        {
            get { return _captionAdapter; }

            set { _captionAdapter = value; }
        }
        
        /// <summary>
        /// Свойство позволяет получить имя адаптера в системе
        /// </summary>
        public string SystemName
        {
            get { return _systemName; }
        }

        /// <summary>
        /// Свойство позволяет получить уникальный ID адаптера в системе
        /// </summary>
        public string AdapterId
        {
            get { return _adapterId; }
        }

        /// <summary>
        /// Показывает и задает статус Dhcp у адаптера
        /// </summary>
        public bool DhcpEnable
        {
            get
            {
                if (_dhcpEnable)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            set { _dhcpEnable = value; }
        }

        /// <summary>
        /// Через свойство можно получать и задавать IP адрес для соответсвующего поля класса
        /// </summary>
        public string IpAddress
        {
            get { return _ipAddress; }

            set { _ipAddress = value; }
        }

        /// <summary>
        /// Через свойство можно получать и задавать маску для соответсвующего поля класса
        /// </summary>
        public string IpMask
        {
            get { return _ipMask; }

            set { _ipMask = value; }
        }

        /// <summary>
        /// Свойство возвращает статус адаптера
        /// </summary>
        public bool IsUpLink
        {
            get
            {
                if (_networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Включение DHCP для порта
        /// </summary>
        /// <returns>true - dhcp включен, false - dhcp не запустился</returns>
        private bool EnableDHCP()
        {
            string arguments = string.Format("interface ip set address name=\"{0}\" source=dhcp", _systemName);
            ProcessStartInfo procStartInfo = new ProcessStartInfo("netsh", arguments);

            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;

            Process P = Process.Start(procStartInfo);
            P.WaitForExit();

            return P.ExitCode != 0;
        }

        /// <summary>
        /// Задание статического IP и маски на основе полей класса
        /// </summary>
        /// <returns></returns>
        private bool SetStaticIP()
        {
            try
            {
                string arguments = string.Format("interface ip set address name=\"{0}\" source=static {1} {2}", _systemName, _ipAddress, _ipMask);
                ProcessStartInfo procStartInfo = new ProcessStartInfo("netsh", arguments);

                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;

                Process P = Process.Start(procStartInfo);
                P.WaitForExit();

                return P.ExitCode != 0;
            }
            catch (Exception ex)
            {
                Utl.MessageEr("Заданный IP адрес уже существует", ex.ToString());
                return false;
            }
            
        }

        /// <summary>
        /// перезагрузка адаптера
        /// </summary>
        /// <returns>true - удачно перезагрузил, false - перезагрузка не удалась</returns>
        public bool RebootAdapter()
        {
            try
            {
                string arguments;
                ProcessStartInfo procStartInfo;
                Process P;

                Sys.waitViewModel.Set();

                #region Останавливаем адаптер
                arguments = string.Format("interface set interface name=\"{0}\" admin=disable", _systemName);
                procStartInfo = new ProcessStartInfo("netsh", arguments);

                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;

                P = Process.Start(procStartInfo);
                P.WaitForExit();
                #endregion

                #region Запускаем адаптер
                arguments = string.Format("interface set interface name=\"{0}\" admin=enable", _systemName);
                procStartInfo = new ProcessStartInfo("netsh", arguments);

                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;

                P = Process.Start(procStartInfo);
                P.WaitForExit();
                #endregion

                ////Ждем 5 сек
                System.Threading.Thread.Sleep(5000);

                Sys.waitViewModel.Reset();

                return true;
            }
            catch (Exception ex)
            {
                Sys.waitViewModel.Reset(); //Для того, чтобы не заморозить интерфейс
                Console.WriteLine("Somthing problem in RebootEthAdapter_By_Caption " + ex);
                return false;
            }
        }

        /// <summary>
        /// Сохранение внесенных изменений
        /// Учитываются включенный/выключенный Dhcp и заданный IP адрес и маска
        /// </summary>
        public void SaveChanges()
        {
            try
            {
                if (IsStateChange)
                {
                    if (_dhcpEnable)
                    {
                        if (EnableDHCP())
                        {
                            UpdateIpAndMask();
                        }
                    }
                    else
                    {
                        SetStaticIP();
                    }
                }

                IsStateChange = false;
            }
            catch (Exception ex)
            {
                //ToDo добавить вывод сообщения

                IsStateChange = false;
            }
        }
    }
}
