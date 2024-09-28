namespace App.Domain;

// Classe que representa o endereço de cada Nó, como Porta e IP
public class Address
{
    public string IP { get; set; }
    public string Port { get; set; }
    public string Key { get; set; }

    public Address(string ip, string port)
    {
        IP = ip;
        Port = port;
        Key = $"{IP}:{Port}";
    }

    public override string ToString()
    {
        return $"Address [Key={Key}]";
    }
}
