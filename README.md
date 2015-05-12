# EthAdapters_EMC07
Набор классов для работы с сетевыми адаптерами панельного компьютера из ПО ИМЦ-07

Данные классы помогают конфигурировать сетевые адаптеры панельного компьютера.
Через класс можно задать IP адрес и маску адаптера, включить/выключить получение сетевого адреса через DHCP.

Пример использования.

```csharp
public void Sample(){
	string captionEthernetAdapter;
	
	EthAdapterManager _adapterManager = new EthAdapterManager();
	
	captionEthernetAdapter =  _adapterManager.GetFirstCaptionEthernetAdapter;
	// turn off DHCP
	_adapterManager.SetDHCP_By_Caption(captionEthernetAdapter, false);
	_adapterManager.SetIP_By_Caption(captionEthernetAdapter, "192.168.80.105");
	_adapterManager.SetIPMask_By_Caption(captionEthernetAdapter, "255.255.255.0");
	
	_adapterManager.SaveAdaptersSettings();
}
```
